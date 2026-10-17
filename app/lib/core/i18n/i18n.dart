import 'dart:ui' show PlatformDispatcher;

import 'package:shared_preferences/shared_preferences.dart';
import 'package:signals_flutter/signals_flutter.dart';

import 'en/en.dart';
import 'es/es.dart';

/// Idioma da interface: o texto-fonte é português e [tr] devolve a tradução quando o
/// idioma é inglês ou espanhol. Texto sem tradução em espanhol cai no inglês; sem
/// tradução nenhuma aparece em português (nunca some).
///
/// Placeholders entre chaves: `tr('Conectado — {account}', {'account': nome})`.
class AppLanguage {
  AppLanguage._();

  static const _prefKey = 'app_language';

  /// Escolha do usuário: 'auto' (idioma do aparelho), 'pt', 'en' ou 'es'.
  static final choice = signal('auto');

  /// Idioma em uso: 'pt', 'en' ou 'es'.
  static final current = computed(() => resolve(choice.value));

  static String resolve(String choice) {
    if (choice == 'pt' || choice == 'en' || choice == 'es') return choice;
    final device = PlatformDispatcher.instance.locale.languageCode;
    return device == 'pt' || device == 'es' ? device : 'en';
  }

  static Future<void> load() async {
    try {
      final prefs = await SharedPreferences.getInstance();
      choice.value = prefs.getString(_prefKey) ?? 'auto';
    } catch (_) {}
  }

  static Future<void> set(String value) async {
    choice.value = value;
    try {
      await (await SharedPreferences.getInstance()).setString(_prefKey, value);
    } catch (_) {}
  }

  /// Valor do cabeçalho Accept-Language para o PC traduzir o que manda (schema, erros).
  static String get header => switch (current.value) {
        'en' => 'en',
        'es' => 'es',
        _ => 'pt-BR',
      };
}

String tr(String pt, [Map<String, Object?> args = const {}]) {
  var s = switch (AppLanguage.current.value) {
    'en' => enStrings[pt] ?? pt,
    'es' => esStrings[pt] ?? enStrings[pt] ?? pt,
    _ => pt,
  };
  args.forEach((k, v) => s = s.replaceAll('{$k}', '${v ?? ''}'));
  return s;
}
