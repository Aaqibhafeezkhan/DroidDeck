import 'package:droiddeck/core/core.dart';
import 'package:flutter/material.dart';

import 'info_deck_button.dart' show usageColor;

/// Uma medida em barra: nome à esquerda, valor à direita e a barra embaixo.
class UsageRow {
  final String name;
  final double percent;

  /// Texto à direita; sem ele, a porcentagem.
  final String? value;

  /// Linha extra embaixo da barra, só em botão alto (ex.: "120 GB livres").
  final String? detail;

  const UsageRow(this.name, this.percent, {this.value, this.detail});
}

/// Medidas em barras, no mesmo molde do botão de uso de IA: título com ícone e uma barra
/// por medida (CPU e RAM, unidades de disco...). O que não cabe no botão fica de fora,
/// na ordem da lista.
class UsageBars extends StatelessWidget {
  final IconData icon;
  final String title;
  final List<UsageRow> rows;

  const UsageBars({super.key, required this.icon, required this.title, required this.rows});

  static const _rowHeight = 25.0;
  static const _headerHeight = 16.0;

  @override
  Widget build(BuildContext context) {
    return LayoutBuilder(builder: (context, c) {
      final fg = DeckTile.foregroundOf(context);
      final roomy = c.maxHeight >= 120;
      final rowHeight = _rowHeight + (roomy ? 13 : 0);
      // Título só quando sobra lugar: com muitas medidas, as barras valem mais.
      final room = c.maxHeight - 8;
      final header = rows.length * rowHeight + _headerHeight <= room || rows.length <= 2;
      final fit = ((room - (header ? _headerHeight : 0)) / rowHeight).floor().clamp(1, rows.length);
      final shown = rows.take(fit).toList();

      Widget small(String text, double alpha) => Text(text,
          maxLines: 1,
          overflow: TextOverflow.ellipsis,
          style: TextStyle(fontSize: 10, color: fg.withValues(alpha: alpha)));

      Widget row(UsageRow r) => Padding(
            padding: const EdgeInsets.symmetric(vertical: 2),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              mainAxisSize: MainAxisSize.min,
              children: [
                Row(children: [
                  Expanded(child: small(r.name, 0.75)),
                  Text(r.value ?? '${r.percent.round()}%',
                      style: const TextStyle(fontSize: 12, fontWeight: FontWeight.bold)),
                ]),
                const SizedBox(height: 2),
                ExcludeSemantics(
                  child: ClipRRect(
                    borderRadius: BorderRadius.circular(3),
                    child: LinearProgressIndicator(
                      value: (r.percent / 100).clamp(0.0, 1.0),
                      minHeight: 5,
                      backgroundColor: fg.withValues(alpha: 0.12),
                      valueColor: AlwaysStoppedAnimation(usageColor(r.percent)),
                    ),
                  ),
                ),
                if (roomy && r.detail != null)
                  Padding(
                    padding: const EdgeInsets.only(top: 1),
                    child: FittedBox(fit: BoxFit.scaleDown, child: small(r.detail!, 0.6)),
                  ),
              ],
            ),
          );

      return Padding(
        padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            if (header)
              Row(mainAxisAlignment: MainAxisAlignment.center, children: [
                Icon(icon, size: 12),
                const SizedBox(width: 4),
                Flexible(child: small(title, 0.85)),
              ]),
            if (header) const SizedBox(height: 2),
            if (rows.isEmpty) small(tr('Sem dados'), 0.7) else for (final r in shown) row(r),
          ],
        ),
      );
    });
  }
}

/// Uma medida só, em barra: ícone e nome, o número grande e a barra (a opção "Barra" dos
/// medidores de CPU, memória, GPU e disco).
class UsageBar extends StatelessWidget {
  final IconData icon;
  final String name;
  final double percent;
  final String? detail;

  const UsageBar({super.key, required this.icon, required this.name, required this.percent, this.detail});

  @override
  Widget build(BuildContext context) {
    final fg = DeckTile.foregroundOf(context);
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Row(mainAxisAlignment: MainAxisAlignment.center, children: [
            Icon(icon, size: 12),
            const SizedBox(width: 4),
            Flexible(
              child: Text(name,
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                  style: TextStyle(fontSize: 10, color: fg.withValues(alpha: 0.85))),
            ),
          ]),
          FittedBox(
            fit: BoxFit.scaleDown,
            child: Text('${percent.round()}%',
                style: const TextStyle(fontSize: 20, fontWeight: FontWeight.bold)),
          ),
          ExcludeSemantics(
            child: ClipRRect(
              borderRadius: BorderRadius.circular(3),
              child: LinearProgressIndicator(
                value: (percent / 100).clamp(0.0, 1.0),
                minHeight: 6,
                backgroundColor: fg.withValues(alpha: 0.12),
                valueColor: AlwaysStoppedAnimation(usageColor(percent)),
              ),
            ),
          ),
          if (detail != null)
            Padding(
              padding: const EdgeInsets.only(top: 3),
              child: FittedBox(
                fit: BoxFit.scaleDown,
                child: Text(detail!, style: TextStyle(fontSize: 10, color: fg.withValues(alpha: 0.7))),
              ),
            ),
        ],
      ),
    );
  }
}

/// Tamanho em disco: "931 GB", "1,8 TB" (inglês: "1.8 TB").
String formatBytes(num bytes) {
  const gb = 1024 * 1024 * 1024;
  final lang = AppLanguage.current.value;
  String f(double v) {
    var t = v < 10 ? v.toStringAsFixed(1) : v.toStringAsFixed(0);
    if (t.endsWith('.0')) t = t.substring(0, t.length - 2);
    return lang == 'en' ? t : t.replaceAll('.', ',');
  }

  final g = bytes / gb;
  if (g >= 1000) return '${f(g / 1024)} TB';
  if (g >= 1) return '${f(g)} GB';
  return '${f(bytes / (1024 * 1024))} MB';
}
