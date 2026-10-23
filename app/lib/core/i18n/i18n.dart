import 'dart:convert';
import 'dart:ui' show PlatformDispatcher;

import 'package:flutter/services.dart' show AssetManifest, rootBundle;
import 'package:shared_preferences/shared_preferences.dart';
import 'package:signals_flutter/signals_flutter.dart';

/// Um idioma disponível: um arquivo `assets/i18n/<código>.json`.
class LanguageInfo {
  /// Código do arquivo ('en', 'es', 'hi'...). É o que vai para o PC.
  final String code;

  /// Nome no próprio idioma ("Español"), como aparece na lista de idiomas.
  final String name;

  /// Separador decimal ("1,5" ou "1.5").
  final String decimal;

  const LanguageInfo(this.code, this.name, {this.decimal = '.'});
}

/// Idioma da interface. Cada idioma é um arquivo em `assets/i18n/` (o mesmo que o PC usa):
/// pôr um arquivo novo lá basta para ele aparecer na lista e ser escolhido sozinho quando o
/// aparelho estiver nesse idioma.
///
/// O texto no código é português e serve de chave: `tr('Salvar')`. Texto sem tradução no
/// idioma escolhido sai em inglês, o idioma padrão. Entradas que começam com `@` são dados do
/// idioma (nome, separador decimal), não textos.
///
/// Placeholders entre chaves: `tr('Não deu certo: {error}', {'error': e})`.
class AppLanguage {
  AppLanguage._();

  static const _prefKey = 'app_language';
  static const _folder = 'assets/i18n/';

  /// Idioma em que o código está escrito: as chaves já são o texto dele.
  static const source = 'pt';

  /// Idioma padrão: aparelho em idioma sem arquivo e texto sem tradução ficam nele.
  static const fallback = 'en';

  static final Map<String, Map<String, String>> _strings = {};
  static List<LanguageInfo> _available = const [
    LanguageInfo(source, 'Português', decimal: ','),
    LanguageInfo(fallback, 'English'),
  ];

  /// Idiomas com arquivo, em ordem alfabética do código.
  static List<LanguageInfo> get available => _available;

  /// Escolha do usuário: 'auto' (idioma do aparelho) ou o código de um idioma.
  static final choice = signal('auto');

  // Muda quando chegam os arquivos: o idioma do aparelho pode ter acabado de ganhar um.
  static final _installed = signal(0);

  /// Código do idioma em uso.
  static final current = computed(() {
    _installed.value;
    return resolve(choice.value);
  });

  static bool _has(String code) => _available.any((l) => l.code == code);

  static String resolve(String choice) {
    if (choice != 'auto' && _has(choice)) return choice;
    final device = PlatformDispatcher.instance.locale.languageCode;
    return _has(device) ? device : fallback;
  }

  static LanguageInfo get info =>
      _available.firstWhere((l) => l.code == current.value, orElse: () => _available.first);

  /// Separador decimal do idioma em uso.
  static String get decimal => info.decimal;

  /// Lê os arquivos de idioma (o app inclui todo `assets/i18n/*.json`) e a escolha salva.
  static Future<void> load() async {
    try {
      final manifest = await AssetManifest.loadFromAssetBundle(rootBundle);
      final files = <String, Map<String, dynamic>>{};
      for (final path in manifest.listAssets()) {
        if (!path.startsWith(_folder) || !path.endsWith('.json')) continue;
        final code = path.substring(_folder.length, path.length - '.json'.length);
        files[code] = jsonDecode(await rootBundle.loadString(path)) as Map<String, dynamic>;
      }
      install(files);
    } catch (_) {
      // Sem os arquivos o app segue em português (as chaves).
    }
    try {
      final prefs = await SharedPreferences.getInstance();
      choice.value = prefs.getString(_prefKey) ?? 'auto';
    } catch (_) {}
  }

  /// Usa estes idiomas (código → conteúdo do JSON). Os testes chamam com os arquivos do disco.
  static void install(Map<String, Map<String, dynamic>> files) {
    if (files.isEmpty) return;
    _strings.clear();
    final available = <LanguageInfo>[];
    for (final MapEntry(key: code, value: json) in files.entries) {
      _strings[code] = {
        for (final e in json.entries)
          if (!e.key.startsWith('@') && e.value is String) e.key: e.value as String,
      };
      available.add(LanguageInfo(
        code,
        json['@language'] as String? ?? code,
        decimal: json['@decimal'] as String? ?? '.',
      ));
    }
    available.sort((a, b) => a.code.compareTo(b.code));
    _available = available;
    _installed.value++;
  }

  /// Textos de um idioma (vazio se não houver arquivo). Os testes usam.
  static Map<String, String> stringsOf(String code) => _strings[code] ?? const {};

  static Future<void> set(String value) async {
    choice.value = value;
    try {
      await (await SharedPreferences.getInstance()).setString(_prefKey, value);
    } catch (_) {}
  }

  /// Valor do cabeçalho Accept-Language para o PC traduzir o que manda (schema, erros).
  static String get header => current.value;
}

String tr(String pt, [Map<String, Object?> args = const {}]) {
  final language = AppLanguage.current.value;
  var s = language == AppLanguage.source
      ? pt
      : AppLanguage._strings[language]?[pt] ?? AppLanguage._strings[AppLanguage.fallback]?[pt] ?? pt;
  args.forEach((k, v) => s = s.replaceAll('{$k}', '${v ?? ''}'));
  return s;
}

/// Número com o separador decimal do idioma.
String decimalText(String formatted) =>
    AppLanguage.decimal == '.' ? formatted : formatted.replaceAll('.', AppLanguage.decimal);
