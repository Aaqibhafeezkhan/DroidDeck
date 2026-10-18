import 'package:web/web.dart' as web;

void clearPairingFromUrl() {
  final uri = Uri.base;
  final rest = Map<String, String>.from(uri.queryParameters)..remove('pair');
  final query = rest.isEmpty ? '' : '?${Uri(queryParameters: rest).query}';
  final fragment = uri.fragment.isEmpty ? '' : '#${uri.fragment}';
  web.window.history.replaceState(null, '', '${uri.path}$query$fragment');
}

/// Tela cheia do navegador (some a barra de endereço). Só vale depois de um toque do
/// usuário; o iPhone não tem a API — lá a saída é "Adicionar à Tela de Início".
void setBrowserFullscreen(bool on) {
  try {
    final doc = web.document;
    if (on && doc.fullscreenElement == null) {
      doc.documentElement?.requestFullscreen();
    } else if (!on && doc.fullscreenElement != null) {
      doc.exitFullscreen();
    }
  } catch (_) {}
}
