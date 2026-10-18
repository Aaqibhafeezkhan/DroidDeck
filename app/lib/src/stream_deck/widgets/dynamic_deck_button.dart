import 'package:droiddeck/src/stream_deck/widgets/deck_grid_button.dart';
import 'package:droiddeck/src/services/signalr_service.dart';
import 'package:droiddeck/core/core.dart';
import 'package:flutter/material.dart';

import '../buttons/colors.dart';
import 'info_deck_button.dart';
import 'editor/info_section.dart';
import 'slider_deck_button.dart';

class DynamicDeckButton extends StatelessWidget {
  final DeckButton button;
  final VoidCallback onTap;
  final VoidCallback onLongPress;

  const DynamicDeckButton({
    super.key,
    required this.button,
    required this.onTap,
    required this.onLongPress,
  });


  /// O que o leitor de tela diz: o rótulo, ou o tipo do botão quando ele não tem rótulo.
  String _semanticLabel() {
    final label = button.label?.trim() ?? '';
    if (label.isNotEmpty) return label;
    final type = button.dynamicType;
    if (type != null && InfoSection.labels[type] != null) return tr(InfoSection.labels[type]!);
    return switch (type) {
      'cpu_monitor' => tr('Uso da CPU'),
      'memory_monitor' => tr('Uso da memória'),
      'gpu_monitor' => tr('Uso da GPU'),
      'network_monitor' => tr('Rede'),
      _ => button.action?.type == 'open_profile' ? tr('Pasta') : tr('Botão'),
    };
  }

  @override
  Widget build(BuildContext context) {
    return Semantics(
      button: true,
      label: _semanticLabel(),
      child: _build(context),
    );
  }

  Widget _build(BuildContext context) {
    // If no dynamic type, render standard button
    if (button.dynamicType == null ||
        button.dynamicType!.isEmpty ||
        button.dynamicType == 'none') {
      return DeckGridButton(
        button: button,
        onTap: onTap,
        onLongPress: onLongPress,
      );
    }

    // Controle deslizante de volume.
    if (sliderTypes.contains(button.dynamicType)) {
      return SliderDeckButton(
        button: button,
        onTap: onTap,
        onLongPress: onLongPress,
        interactive: !WebMode.configurator,
      );
    }

    // Relógio, clima, tocando agora, comando e URL.
    if (infoTypes.contains(button.dynamicType)) {
      return InfoDeckButton(
        button: button,
        onTap: onTap,
        onLongPress: onLongPress,
        color: parseHexColor(button.backgroundColor),
      );
    }

    // Dynamic Rendering
    final signalR = Injector.get<SignalRService>();
    final stats = signalR.systemStats.watch(context);

    return DeckTile(
      color: parseHexColor(button.backgroundColor),
      onTap: onTap,
      onLongPress: onLongPress,
      child: _buildDynamicContent(context, stats, button.dynamicType!),
    );
  }

  Widget _buildDynamicContent(
    BuildContext context,
    SystemStats stats,
    String type,
  ) {
    switch (type) {
      case 'cpu_monitor':
        return _buildCpuMonitor(stats);
      case 'memory_monitor':
        return _buildMemoryMonitor(stats);
      case 'gpu_monitor':
        return _buildGpuMonitor(stats);
      case 'network_monitor':
        return _buildNetworkMonitor(stats);
      default:
        return Center(child: Text(tr('Desconhecido: {type}', {'type': type})));
    }
  }

  Widget _buildCpuMonitor(SystemStats stats) {
    double pct = stats.cpuUsage;
    if (pct.isNaN || pct.isInfinite) pct = 0;
    return _gauge(
      percent: pct,
      icon: Icons.memory,
      value: '${pct.toStringAsFixed(0)}%',
      label: 'CPU',
    );
  }

  Widget _buildMemoryMonitor(SystemStats stats) {
    double pct = stats.ramUsedPercent;
    if (pct.isNaN || pct.isInfinite) pct = 0;
    final detail = stats.ramTotal > 0
        ? '${stats.ramUsedGb.toStringAsFixed(1)}/${stats.ramTotalGb.toStringAsFixed(1)}G'
        : 'RAM';
    return _gauge(
      percent: pct,
      icon: Icons.memory_outlined,
      value: '${pct.toStringAsFixed(0)}%',
      label: detail,
    );
  }

  Widget _buildGpuMonitor(SystemStats stats) {
    double pct = stats.gpuUsage;
    if (pct.isNaN || pct.isInfinite) pct = 0;
    return _gauge(
      percent: pct,
      icon: Icons.videogame_asset,
      value: '${pct.toStringAsFixed(0)}%',
      label: 'GPU',
    );
  }

  Widget _buildNetworkMonitor(SystemStats stats) {
    // FittedBox: com a unidade (KB/s) o texto não pode estourar num botão pequeno.
    return Center(
      child: FittedBox(
        fit: BoxFit.scaleDown,
        child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          const Icon(Icons.swap_vert, size: 18),
          Text(
            '↓ ${_fmtRate(stats.netDown)}',
            style: const TextStyle(
              color: Colors.greenAccent,
              fontWeight: FontWeight.bold,
              fontSize: 11,
            ),
          ),
          Text(
            '↑ ${_fmtRate(stats.netUp)}',
            style: const TextStyle(
              color: Colors.lightBlueAccent,
              fontWeight: FontWeight.bold,
              fontSize: 11,
            ),
          ),
          const Opacity(
            opacity: 0.7,
            child: Text('NET', style: TextStyle(fontSize: 10)),
          ),
        ],
        ),
      ),
    );
  }

  String _fmtRate(double kbps) {
    if (kbps >= 1024) return '${(kbps / 1024).toStringAsFixed(1)} MB/s';
    return '${kbps.toStringAsFixed(0)} KB/s';
  }

  Color _usageColor(double pct) {
    if (pct > 85) return Colors.redAccent;
    if (pct > 60) return Colors.orangeAccent;
    return Colors.greenAccent;
  }

  /// Medidor circular: anel colorido pela faixa de uso + ícone/valor/label no centro.
  Widget _gauge({
    required double percent,
    required IconData icon,
    required String value,
    required String label,
  }) {
    final p = (percent / 100).clamp(0.0, 1.0);
    final color = _usageColor(percent);
    return LayoutBuilder(
      builder: (context, c) {
        final fg = DeckTile.foregroundOf(context);
        final small = c.maxWidth < 70 || c.maxHeight < 70;
        return Stack(
          alignment: Alignment.center,
          children: [
            Positioned.fill(
              child: Padding(
                padding: const EdgeInsets.all(6),
                child: CircularProgressIndicator(
                  value: p,
                  strokeWidth: small ? 4 : 6,
                  backgroundColor: fg.withValues(alpha: 0.12),
                  valueColor: AlwaysStoppedAnimation<Color>(color),
                ),
              ),
            ),
            // Encolhe o texto para caber dentro do anel em botões pequenos.
            Padding(
              padding: EdgeInsets.all(small ? 10 : 14),
              child: FittedBox(
                fit: BoxFit.scaleDown,
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    Icon(icon, size: small ? 14 : 18),
                    Text(
                      value,
                      style: TextStyle(
                        fontWeight: FontWeight.bold,
                        fontSize: small ? 12 : 15,
                      ),
                    ),
                    Text(
                      label,
                      style: TextStyle(
                        color: fg.withValues(alpha: 0.7),
                        fontSize: small ? 8 : 9,
                      ),
                      maxLines: 1,
                      overflow: TextOverflow.clip,
                    ),
                  ],
                ),
              ),
            ),
          ],
        );
      },
    );
  }
}
