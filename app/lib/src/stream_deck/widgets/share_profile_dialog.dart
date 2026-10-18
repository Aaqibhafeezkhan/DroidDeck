import 'package:droiddeck/core/core.dart';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:share_plus/share_plus.dart';

import '../../connection/browser_url.dart';

/// Compartilhar um perfil: link do site (que mostra o perfil e importa no PC de quem
/// abrir) e o mesmo link em QR.
class ShareProfileDialog extends StatefulWidget {
  final DeckProfile profile;

  const ShareProfileDialog({super.key, required this.profile});

  static Future<void> show(BuildContext context, DeckProfile profile) => showDialog(
        context: context,
        builder: (_) => ShareProfileDialog(profile: profile),
      );

  @override
  State<ShareProfileDialog> createState() => _ShareProfileDialogState();
}

class _ShareProfileDialogState extends State<ShareProfileDialog> {
  final _client = Injector.get<DroidDeckClient>();
  Map<String, dynamic>? _share;
  List<int>? _qr;
  String? _error;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    try {
      final share = Map<String, dynamic>.from(
          await _client.getJson('/api/share/${widget.profile.id}') as Map);
      List<int>? qr;
      if (share['qr'] == true) qr = await _client.getBytes('/api/share/${widget.profile.id}/qr');
      if (!mounted) return;
      setState(() {
        _share = share;
        _qr = qr;
      });
    } catch (_) {
      if (mounted) setState(() => _error = tr('O PC não respondeu.'));
    }
  }

  @override
  Widget build(BuildContext context) {
    final url = _share?['url'] as String?;
    final muted = Theme.of(context).textTheme.bodySmall;
    return AlertDialog(
      title: Text(tr('Compartilhar "{name}"', {'name': widget.profile.name})),
      content: SizedBox(
        width: 380,
        child: _error != null
            ? Text(_error!)
            : url == null
                ? const SizedBox(height: 120, child: Center(child: CircularProgressIndicator()))
                : SingleChildScrollView(
                    child: Column(
                      mainAxisSize: MainAxisSize.min,
                      crossAxisAlignment: CrossAxisAlignment.stretch,
                      children: [
                        Text(tr('Quem abrir o link vê os botões e importa o perfil no próprio '
                            'PC com um clique.')),
                        if (_qr != null) ...[
                          const SizedBox(height: 16),
                          Center(
                            child: Container(
                              color: Colors.white,
                              padding: const EdgeInsets.all(8),
                              child: Image.memory(Uint8List.fromList(_qr!), width: 220, height: 220),
                            ),
                          ),
                        ],
                        if (_share?['images'] == true) ...[
                          const SizedBox(height: 12),
                          Text(
                              tr('Imagens próprias não vão no link (ficam os ícones). Para '
                                  'levar tudo, use o backup.'),
                              style: muted),
                        ],
                      ],
                    ),
                  ),
      ),
      actions: [
        TextButton(onPressed: () => Navigator.pop(context), child: Text(tr('Fechar'))),
        if (url != null)
          OutlinedButton.icon(
            onPressed: () => SharePlus.instance.share(ShareParams(
                text: tr('Meu perfil do DroidDeck: {url}', {'url': url}))),
            icon: const Icon(Icons.share, size: 18),
            label: Text(tr('Enviar')),
          ),
        if (url != null)
          FilledButton.icon(
            onPressed: () async {
              await Clipboard.setData(ClipboardData(text: url));
              if (context.mounted) {
                ScaffoldMessenger.of(context)
                    .showSnackBar(SnackBar(content: Text(tr('Link copiado.'))));
              }
            },
            icon: const Icon(Icons.link, size: 18),
            label: Text(tr('Copiar link')),
          ),
      ],
    );
  }
}

/// Aberto pelo link de um perfil compartilhado (?import=...): pergunta e importa.
/// [onImported] recebe o id do perfil novo.
Future<void> importSharedProfileFromUrl(
    BuildContext context, ValueChanged<String> onImported) async {
  final data = Uri.base.queryParameters['import'];
  if (data == null || data.isEmpty) return;
  clearUrlParam('import');
  final ok = await showDialog<bool>(
    context: context,
    builder: (ctx) => AlertDialog(
      title: Text(tr('Importar perfil compartilhado?')),
      content: Text(tr('O perfil do link entra como um perfil novo; nada do seu deck é apagado.')),
      actions: [
        TextButton(onPressed: () => Navigator.pop(ctx, false), child: Text(tr('Cancelar'))),
        FilledButton(onPressed: () => Navigator.pop(ctx, true), child: Text(tr('Importar'))),
      ],
    ),
  );
  if (ok != true || !context.mounted) return;
  final messenger = ScaffoldMessenger.of(context);
  try {
    final r = Map<String, dynamic>.from(
        await Injector.get<DroidDeckClient>().postJson('/api/share/import', {'data': data}) as Map);
    final stripped = (r['stripped'] as num?)?.toInt() ?? 0;
    messenger.showSnackBar(SnackBar(
      content: Text([
        tr('Perfil "{name}" importado.', {'name': r['name']}),
        if (stripped > 0)
          tr('{n} botões de atalho ou programa ficaram sem ação: crie-os no configurador do PC.',
              {'n': stripped}),
      ].join(' ')),
    ));
    onImported(r['profileId'] as String);
  } on DioException catch (e) {
    final err = e.response?.data;
    messenger.showSnackBar(SnackBar(
        content: Text(err is Map && err['error'] is String
            ? err['error'] as String
            : tr('O PC não respondeu.'))));
  }
}
