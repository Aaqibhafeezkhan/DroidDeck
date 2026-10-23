import 'dart:async';
import 'dart:convert';
import 'dart:io';

import 'package:droiddeck/core/i18n/i18n.dart';

/// Os idiomas vêm dos mesmos arquivos que o app empacota, e os testes procuram os textos em
/// português: fixa o idioma, senão valeria o do sistema.
Future<void> testExecutable(FutureOr<void> Function() testMain) async {
  AppLanguage.install({
    for (final f in Directory('assets/i18n').listSync().whereType<File>())
      if (f.path.endsWith('.json'))
        f.uri.pathSegments.last.replaceAll('.json', ''):
            jsonDecode(f.readAsStringSync()) as Map<String, dynamic>,
  });
  AppLanguage.choice.value = 'pt';
  await testMain();
}
