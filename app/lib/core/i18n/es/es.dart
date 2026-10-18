import 'config.dart';
import 'core.dart';
import 'deck.dart';
import 'editors.dart';
import 'local_integrations.dart';

/// Traduções para espanhol, nas mesmas áreas e com as mesmas chaves de en/en.dart (texto
/// em português, exatamente como aparece no código). Falta de chave cai no inglês.
final Map<String, String> esStrings = {
  ...esCore,
  ...esDeck,
  ...esEditors,
  ...esConfig,
  ...esLocalIntegrations,
};
