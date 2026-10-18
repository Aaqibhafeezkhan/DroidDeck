import 'package:flutter/foundation.dart' show kIsWeb;

/// Os dois jeitos de o app rodar no navegador.
///
/// - Configurador: aberto no próprio PC (localhost). Layout de três painéis para montar
///   o deck, chave de API pega pelo loopback.
/// - Deck no navegador: aberto de outro aparelho da rede (iPhone, tablet, outro PC) ou
///   com `?deck` no PC. Funciona como o app do celular; a chave vem do QR "Abrir no
///   navegador" da janela de pareamento.
class WebMode {
  WebMode._();

  static bool get _loopback {
    final host = Uri.base.host;
    return host == 'localhost' || host == '127.0.0.1' || host == '::1' || host == '[::1]';
  }

  /// Navegador no PC, montando o deck.
  static final bool configurator =
      kIsWeb && _loopback && !Uri.base.queryParameters.containsKey('deck');

  /// Navegador usado como deck (no lugar do app de celular).
  static final bool deck = kIsWeb && !configurator;

  /// Roda como deck (celular ou navegador): toca os botões, guarda o deck offline etc.
  static bool get isDevice => !configurator;
}
