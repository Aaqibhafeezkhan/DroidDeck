import 'dart:io';

import 'package:droiddeck/core/i18n/i18n.dart';
import 'package:flutter_test/flutter_test.dart';

/// Chaves literais de `tr('...')` no código do app (texto montado com `$` fica de fora).
Set<String> _literalKeys() {
  const lit = r'''(?:r?'(?:[^'\\\n]|\\.)*'|r?"(?:[^"\\\n]|\\.)*")''';
  final call = RegExp(r'\btr\(\s*((?:' + lit + r'\s*)+)\s*[,)]');
  final part = RegExp(lit);
  final keys = <String>{};
  for (final f in Directory('lib').listSync(recursive: true).whereType<File>()) {
    if (!f.path.endsWith('.dart')) continue;
    for (final m in call.allMatches(f.readAsStringSync())) {
      var key = StringBuffer();
      for (final p in part.allMatches(m[1]!)) {
        var t = p[0]!;
        final raw = t.startsWith('r');
        if (raw) t = t.substring(1);
        var body = t.substring(1, t.length - 1);
        if (!raw) {
          if (RegExp(r'(?<!\\)\$').hasMatch(body)) {
            key = StringBuffer('\u0000');
            break;
          }
          body = body.replaceAllMapped(
              RegExp(r'\\(.)'), (e) => switch (e[1]) { 'n' => '\n', 't' => '\t', final c => c! });
        }
        key.write(body);
      }
      if (!key.toString().startsWith('\u0000')) keys.add(key.toString());
    }
  }
  return keys;
}

void main() {
  tearDown(() => AppLanguage.choice.value = 'pt');

  final en = AppLanguage.stringsOf('en');
  final es = AppLanguage.stringsOf('es');

  test('os idiomas vêm da pasta assets/i18n', () {
    final codes = AppLanguage.available.map((l) => l.code);
    expect(codes, containsAll(['en', 'es', 'hi', 'pt']));
    expect(AppLanguage.available.firstWhere((l) => l.code == 'es').name, 'Español');
  });

  test('português devolve a chave; os outros, a tradução', () {
    AppLanguage.choice.value = 'pt';
    expect(tr('Salvar'), 'Salvar');
    AppLanguage.choice.value = 'en';
    expect(tr('Salvar'), 'Save');
    AppLanguage.choice.value = 'es';
    expect(tr('Salvar'), 'Guardar');
    AppLanguage.choice.value = 'hi';
    expect(tr('Salvar'), 'सहेजें');
  });

  test('sem tradução no idioma, sai em inglês', () {
    AppLanguage.choice.value = 'hi';
    final untranslated = en.keys.firstWhere((k) => !AppLanguage.stringsOf('hi').containsKey(k));
    expect(tr(untranslated), en[untranslated]);
  });

  test('idioma sem arquivo cai no inglês', () {
    // O aparelho de teste está em inglês (en_US).
    expect(AppLanguage.resolve('xx'), 'en');
    expect(AppLanguage.resolve('auto'), 'en');
  });

  test('cabeçalho Accept-Language é o código do idioma', () {
    for (final code in ['pt', 'en', 'es', 'hi']) {
      AppLanguage.choice.value = code;
      expect(AppLanguage.header, code);
    }
  });

  test('placeholders e texto sem tradução', () {
    AppLanguage.choice.value = 'en';
    expect(tr('Texto que não existe {x}', {'x': 1}), 'Texto que não existe 1');
    AppLanguage.choice.value = 'hi';
    expect(tr('Não deu certo: {error}', {'error': 'x'}), 'यह काम नहीं किया: x');
  });

  test('separador decimal vem do arquivo', () {
    AppLanguage.choice.value = 'pt';
    expect(decimalText('1.5'), '1,5');
    AppLanguage.choice.value = 'en';
    expect(decimalText('1.5'), '1.5');
  });

  test('todo texto do app tem inglês e espanhol', () {
    final keys = _literalKeys();
    expect(keys.length, greaterThan(300));
    final missing = keys.where((k) => !en.containsKey(k)).toList()..sort();
    expect(missing, isEmpty, reason: 'sem inglês em assets/i18n/en.json: $missing');
    final noEs = keys.where((k) => !es.containsKey(k)).toList()..sort();
    expect(noEs, isEmpty, reason: 'sem espanhol em assets/i18n/es.json: $noEs');
  });

  test('inglês e espanhol têm as mesmas chaves', () {
    expect(en.keys.where((k) => !es.containsKey(k)).toList(), isEmpty);
    expect(es.keys.where((k) => !en.containsKey(k)).toList(), isEmpty);
  });

  test('nenhum idioma traduz chave que o inglês não tem', () {
    for (final l in AppLanguage.available) {
      final extra = AppLanguage.stringsOf(l.code).keys.where((k) => !en.containsKey(k)).toList();
      expect(extra, isEmpty, reason: 'chave em ${l.code}.json que não existe em en.json: $extra');
    }
  });

  test('toda tradução mantém os mesmos placeholders da chave', () {
    final re = RegExp(r'\{(\w+)\}');
    for (final l in AppLanguage.available) {
      for (final e in AppLanguage.stringsOf(l.code).entries) {
        final a = re.allMatches(e.key).map((m) => m[1]).toSet();
        final b = re.allMatches(e.value).map((m) => m[1]).toSet();
        expect(b, a, reason: 'placeholders diferentes em ${l.code}.json: "${e.key}"');
      }
    }
  });
}
