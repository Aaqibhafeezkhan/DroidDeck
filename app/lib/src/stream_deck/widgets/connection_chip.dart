import 'package:droiddeck/core/core.dart';
import 'package:droiddeck/src/services/signalr_service.dart';
import 'package:flutter/material.dart';

/// Estado da conexão com o PC, com texto e ícone (não só cor): "Conectado ao PC-SALA",
/// "Conectando…", "Reconectando…" ou "Sem conexão". O toque explica o que fazer; os dados técnicos
/// ficam recolhidos para quem precisar.
class ConnectionChip extends StatefulWidget {
  /// Só o ícone e uma palavra (barra estreita do celular).
  final bool compact;

  const ConnectionChip({super.key, this.compact = false});

  @override
  State<ConnectionChip> createState() => _ConnectionChipState();
}

enum _Link { connected, connecting, reconnecting, offline }

class _ConnectionChipState extends State<ConnectionChip> {
  static String? _pcName;

  @override
  void initState() {
    super.initState();
    if (_pcName == null) _loadName();
  }

  Future<void> _loadName() async {
    try {
      final info = await Injector.get<DroidDeckClient>().getJson('/api/system/info');
      final name = (info is Map) ? info['name'] as String? : null;
      if (name != null && name.isNotEmpty && mounted) setState(() => _pcName = name);
    } catch (_) {}
  }

  static _Link _linkOf(String status) {
    if (status == 'Connected') return _Link.connected;
    if (status.startsWith('Connecting')) return _Link.connecting;
    // "Reconnecting..." é a reconexão automática em andamento; "Reconnecting in Ns" é a
    // espera entre tentativas, quando o PC não está respondendo.
    if (status == 'Reconnecting...') return _Link.reconnecting;
    return _Link.offline;
  }

  @override
  Widget build(BuildContext context) {
    final signalR = Injector.get<SignalRService>();
    final status = signalR.connectionStatus.watch(context);
    final url = signalR.currentUrl.watch(context);
    final link = _linkOf(status);
    final viaUsb = url.contains('127.0.0.1') && !WebMode.configurator;
    final scheme = Theme.of(context).colorScheme;

    final (IconData icon, Color color, String text) = switch (link) {
      _Link.connected => (
          viaUsb ? Icons.usb : Icons.link,
          const Color(0xFF4ADE80),
          widget.compact
              ? tr('Conectado')
              : (_pcName != null
                  ? tr('Conectado a {pc}', {'pc': _pcName})
                  : tr('Conectado')),
        ),
      _Link.connecting => (Icons.sync, const Color(0xFFFBBF24), tr('Conectando…')),
      _Link.reconnecting => (Icons.sync, const Color(0xFFFBBF24), tr('Reconectando…')),
      _Link.offline => (Icons.link_off, scheme.error, tr('Sem conexão')),
    };

    return Semantics(
      button: true,
      label: text,
      child: Tooltip(
        message: text,
        child: InkWell(
          borderRadius: BorderRadius.circular(16),
          onTap: () => _details(context, link, status, url, viaUsb),
          child: Container(
            padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 6),
            decoration: BoxDecoration(
              color: color.withValues(alpha: 0.12),
              borderRadius: BorderRadius.circular(16),
              border: Border.all(color: color.withValues(alpha: 0.45)),
            ),
            child: Row(
              mainAxisSize: MainAxisSize.min,
              children: [
                Icon(icon, size: 16, color: color),
                const SizedBox(width: 6),
                ConstrainedBox(
                  constraints: const BoxConstraints(maxWidth: 180),
                  child: Text(
                    text,
                    overflow: TextOverflow.ellipsis,
                    style: TextStyle(
                        fontSize: 13, fontWeight: FontWeight.w600, color: scheme.onSurface),
                  ),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }

  void _details(BuildContext context, _Link link, String status, String url, bool viaUsb) {
    final signalR = Injector.get<SignalRService>();
    final explain = switch (link) {
      _Link.connected => viaUsb
          ? tr('Tudo certo: este aparelho fala com o PC pelo cabo USB.')
          : tr('Tudo certo: os botões e os dados ao vivo estão chegando do PC.'),
      _Link.connecting => tr('Procurando o PC. Leva alguns segundos.'),
      _Link.reconnecting =>
        tr('A conexão caiu e está voltando sozinha. Se demorar, confira se o PC está ligado e na mesma rede.'),
      _Link.offline => tr(
          'O PC não está respondendo. Confira se o DroidDeck está aberto nele (ícone perto do relógio) '
          'e se os dois estão na mesma rede Wi-Fi. Enquanto isso, o último deck salvo continua aqui.'),
    };
    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        title: Text(tr('Conexão com o PC')),
        content: SingleChildScrollView(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              Text(explain),
              const SizedBox(height: 12),
              ExpansionTile(
                tilePadding: EdgeInsets.zero,
                title: Text(tr('Detalhes técnicos'), style: const TextStyle(fontSize: 14)),
                children: [
                  SelectableText(
                    tr('Status: {status}\n\nURL: {url}\n\nÚltimos dados:\n{data}', {
                      'status': status,
                      'url': url,
                      'data': signalR.lastDebugData.value,
                    }),
                    style: const TextStyle(fontSize: 12),
                  ),
                ],
              ),
            ],
          ),
        ),
        actions: [
          TextButton(onPressed: () => Navigator.pop(ctx), child: Text(tr('Fechar'))),
        ],
      ),
    );
  }
}
