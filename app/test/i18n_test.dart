import 'package:droiddeck/core/i18n/en/en.dart';
import 'package:droiddeck/core/i18n/es/es.dart';
import 'package:droiddeck/core/i18n/i18n.dart';
import 'package:flutter_test/flutter_test.dart';

void main() {
  tearDown(() => AppLanguage.choice.value = 'pt');

  test('português devolve a chave; inglês devolve a tradução', () {
    AppLanguage.choice.value = 'pt';
    expect(tr('Salvar'), 'Salvar');
    AppLanguage.choice.value = 'en';
    expect(tr('Salvar'), 'Save');
    AppLanguage.choice.value = 'es';
    expect(tr('Salvar'), 'Guardar');
  });

  test('cabeçalho Accept-Language segue o idioma', () {
    AppLanguage.choice.value = 'pt';
    expect(AppLanguage.header, 'pt-BR');
    AppLanguage.choice.value = 'en';
    expect(AppLanguage.header, 'en');
    AppLanguage.choice.value = 'es';
    expect(AppLanguage.header, 'es');
    AppLanguage.choice.value = 'hi';
    expect(AppLanguage.header, 'hi-IN');
  });

  test('Hindi is selectable and uses translated strings with English fallback', () {
    AppLanguage.choice.value = 'hi';
    expect(AppLanguage.resolve('hi'), 'hi');
    expect(tr('Salvar'), 'सहेजें');
    expect(tr('Texto que não existe'), 'Texto que não existe');
  });

  test('toda chave traduzida para inglês também tem espanhol', () {
    final missing = enStrings.keys.where((k) => !esStrings.containsKey(k)).toList();
    expect(missing, isEmpty, reason: 'sem espanhol: $missing');
    final extra = esStrings.keys.where((k) => !enStrings.containsKey(k)).toList();
    expect(extra, isEmpty, reason: 'chave espanhola que não existe em inglês: $extra');
  });

  test('placeholders e texto sem tradução', () {
    AppLanguage.choice.value = 'en';
    expect(tr('Texto que não existe {x}', {'x': 1}), 'Texto que não existe 1');
    AppLanguage.choice.value = 'hi';
    expect(tr('Conectado — {account}', {'account': 'Aaqib'}), 'कनेक्ट किया गया — Aaqib');
  });

  test('toda tradução mantém os mesmos placeholders da chave', () {
    final re = RegExp(r'\{(\w+)\}');
    for (final e in [...enStrings.entries, ...esStrings.entries]) {
      final a = re.allMatches(e.key).map((m) => m[1]).toSet();
      final b = re.allMatches(e.value).map((m) => m[1]).toSet();
      expect(b, a, reason: 'placeholders diferentes em "${e.key}"');
    }
  });
}
