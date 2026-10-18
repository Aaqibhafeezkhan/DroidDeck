import 'package:dio/dio.dart';

import '../i18n/i18n.dart';

/// Mensagem de erro para gente, não para desenvolvedor: diz o que aconteceu e o que fazer.
/// Se o PC mandou um texto próprio ({"error": "..."}), ele vale (já vem traduzido).
String friendlyError(Object error) {
  if (error is DioException) {
    final data = error.response?.data;
    if (data is Map && data['error'] is String) return data['error'] as String;
    final code = error.response?.statusCode;
    if (code == 401 || code == 403) {
      return tr('O PC recusou a chave deste aparelho. Refaça o pareamento.');
    }
    switch (error.type) {
      case DioExceptionType.connectionTimeout:
      case DioExceptionType.receiveTimeout:
      case DioExceptionType.sendTimeout:
        return tr('O PC demorou demais para responder. Tente de novo.');
      case DioExceptionType.connectionError:
        return tr('O PC não respondeu. Confira se o DroidDeck está aberto nele e se os dois estão na mesma rede.');
      default:
        break;
    }
    if (code != null && code >= 500) {
      return tr('O DroidDeck do PC teve um problema. Tente de novo; se repetir, reinicie o DroidDeck no PC.');
    }
  }
  return tr('Algo deu errado. Tente de novo.');
}
