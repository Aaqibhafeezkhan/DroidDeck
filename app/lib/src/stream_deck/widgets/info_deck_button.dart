import 'dart:async';
import 'dart:convert';

import 'package:droiddeck/core/core.dart';
import 'package:droiddeck/src/services/signalr_service.dart';
import 'package:flutter/material.dart';

/// Tipos de botão de informação (dynamicType) desenhados por [InfoDeckButton].
const infoTypes = {
  'clock',
  'weather',
  'now_playing',
  'command',
  'url',
  'notifications',
  'ai_usage',
};

/// Botão que mostra uma informação ao vivo: relógio, clima, o que está tocando, a saída
/// de um comando ou um valor de uma URL. O relógio é calculado aqui; o resto vem do PC
/// (ReceiveInfoState / ReceiveNowPlaying). O toque executa a ação do botão, se houver.
class InfoDeckButton extends StatefulWidget {
  final DeckButton button;
  final VoidCallback onTap;
  final VoidCallback onLongPress;
  final Color? color;

  const InfoDeckButton({
    super.key,
    required this.button,
    required this.onTap,
    required this.onLongPress,
    this.color,
  });

  @override
  State<InfoDeckButton> createState() => _InfoDeckButtonState();
}

class _InfoDeckButtonState extends State<InfoDeckButton> {
  Timer? _clock;

  String get _type => widget.button.dynamicType ?? '';
  Map<String, String> get _params => widget.button.dynamicParameters ?? const {};

  @override
  void initState() {
    super.initState();
    if (_type == 'clock') {
      _clock = Timer.periodic(const Duration(seconds: 1), (_) {
        if (mounted) setState(() {});
      });
    }
  }

  @override
  void dispose() {
    _clock?.cancel();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return DeckTile(
      color: widget.color,
      onTap: widget.onTap,
      onLongPress: widget.onLongPress,
      child: switch (_type) {
        'clock' => _buildClock(),
        'now_playing' => Watch((_) => _buildNowPlaying(Injector.get<SignalRService>()
            .stateOf('ReceiveNowPlaying'))),
        _ => Watch((_) {
            final all = Injector.get<SignalRService>().stateOf('ReceiveInfoState');
            final v = all[widget.button.id];
            return _buildValue(v is Map ? Map<String, dynamic>.from(v) : null);
          }),
      },
    );
  }

  String? get _label =>
      (widget.button.label?.isNotEmpty ?? false) ? widget.button.label : null;

  Widget _small(String text, {double alpha = 0.7, int maxLines = 1}) => Builder(
        builder: (context) => Text(
          text,
          textAlign: TextAlign.center,
          maxLines: maxLines,
          overflow: TextOverflow.ellipsis,
          style: TextStyle(
            fontSize: 10,
            color: DeckTile.foregroundOf(context).withValues(alpha: alpha),
          ),
        ),
      );

  Widget _big(String text, {int maxLines = 1}) => FittedBox(
        fit: BoxFit.scaleDown,
        child: Text(
          text,
          textAlign: TextAlign.center,
          maxLines: maxLines,
          style: const TextStyle(fontSize: 22, fontWeight: FontWeight.bold),
        ),
      );

  // ---- Relógio ----

  static const _weekdays = ['seg', 'ter', 'qua', 'qui', 'sex', 'sáb', 'dom'];

  Widget _buildClock() {
    final now = DateTime.now();
    final h24 = _params['format24'] != 'false';
    final seconds = _params['showSeconds'] == 'true';
    final date = _params['showDate'] != 'false';
    String two(int n) => n.toString().padLeft(2, '0');
    final hour = h24 ? now.hour : (now.hour % 12 == 0 ? 12 : now.hour % 12);
    var time = '${h24 ? two(hour) : hour}:${two(now.minute)}';
    if (seconds) time += ':${two(now.second)}';
    if (!h24) time += now.hour < 12 ? ' AM' : ' PM';
    return Padding(
      padding: const EdgeInsets.all(4),
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          if (_label != null) _small(_label!),
          _big(time),
          if (date)
            // Dia/mês em português e espanhol, mês/dia em inglês.
            _small('${tr(_weekdays[now.weekday - 1])}, ' +
                (AppLanguage.current.value == 'en'
                    ? '${two(now.month)}/${two(now.day)}'
                    : '${two(now.day)}/${two(now.month)}')),
        ],
      ),
    );
  }

  // ---- Clima, comando e URL ----

  Widget _buildValue(Map<String, dynamic>? v) {
    if (v == null) {
      return Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          const SizedBox(
              width: 18, height: 18, child: CircularProgressIndicator(strokeWidth: 2)),
          if (_label != null) ...[const SizedBox(height: 6), _small(_label!)],
        ],
      );
    }
    final text = '${v['text'] ?? ''}';
    final sub = v['sub'] as String?;
    final error = v['error'] == true;
    if (_type == 'weather' && !error) {
      return Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Icon(weatherIcon(v['code'] as int?, v['isDay'] != false), size: 26),
          _big(text),
          _small(_label ?? sub ?? ''),
        ],
      );
    }
    if (_type == 'ai_usage' && !error && v['rows'] is List) {
      return _buildAiUsage((v['rows'] as List).whereType<Map>().toList());
    }
    if (_type == 'notifications' && !error) {
      final count = int.tryParse(text) ?? 0;
      return Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Badge(
            isLabelVisible: count > 0,
            label: Text(count > 99 ? '99+' : '$count'),
            child: Icon(
                count > 0 ? Icons.notifications_active : Icons.notifications_none,
                size: 28),
          ),
          const SizedBox(height: 4),
          _small(_label ?? sub ?? tr('Notificações')),
        ],
      );
    }
    return Padding(
      padding: const EdgeInsets.all(4),
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          if (_label != null) _small(_label!),
          if (error) ...[
            const Icon(Icons.error_outline, size: 22, color: Colors.amber),
            if (sub != null) Flexible(child: _small(sub, maxLines: 3)),
          ] else
            Flexible(
              child: text.length <= 8
                  ? _big(text)
                  : Text(
                      text,
                      textAlign: TextAlign.center,
                      maxLines: 3,
                      overflow: TextOverflow.ellipsis,
                      style: const TextStyle(fontSize: 12, fontWeight: FontWeight.w600),
                    ),
            ),
        ],
      ),
    );
  }

  // ---- Uso de IA ----

  /// Limites do plano (uma barra por janela, com a hora em que zera se couber) ou tokens de
  /// hoje e da semana. As linhas vêm do PC com chave e número; nome e formato são daqui.
  Widget _buildAiUsage(List<Map> rows) {
    final provider = _params['provider'] == 'codex' ? 'Codex' : 'Claude';
    final title = _label ?? provider;
    if (_params['show'] == 'tokens') {
      int count(String key) =>
          (rows.firstWhere((r) => r['key'] == key, orElse: () => const {})['count'] as num?)
              ?.toInt() ??
          0;
      return Padding(
        padding: const EdgeInsets.all(4),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            _small(title),
            _big(compactCount(count('today'))),
            _small(tr('hoje')),
            const SizedBox(height: 2),
            _small(tr('7 dias: {n}', {'n': compactCount(count('week_tokens'))}), alpha: 0.85),
          ],
        ),
      );
    }
    return LayoutBuilder(builder: (context, c) {
      final roomy = c.maxHeight >= 120;
      final fg = DeckTile.foregroundOf(context);
      Widget row(Map r) {
        final pct = (r['percent'] as num?)?.toDouble() ?? 0;
        final resets = DateTime.tryParse('${r['resetsAt'] ?? ''}')?.toLocal();
        final name = r['key'] == 'session' ? tr('5 horas') : tr('Semana');
        return Padding(
          padding: const EdgeInsets.symmetric(vertical: 2),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            mainAxisSize: MainAxisSize.min,
            children: [
              Row(children: [
                Expanded(
                  child: Text(name,
                      maxLines: 1,
                      overflow: TextOverflow.ellipsis,
                      style: TextStyle(fontSize: 10, color: fg.withValues(alpha: 0.75))),
                ),
                Text('${pct.round()}%',
                    style: const TextStyle(fontSize: 12, fontWeight: FontWeight.bold)),
              ]),
              const SizedBox(height: 2),
              ClipRRect(
                borderRadius: BorderRadius.circular(3),
                child: LinearProgressIndicator(
                  value: (pct / 100).clamp(0.0, 1.0),
                  minHeight: 5,
                  backgroundColor: fg.withValues(alpha: 0.12),
                  valueColor: AlwaysStoppedAnimation(usageColor(pct)),
                ),
              ),
              if (roomy && resets != null)
                Padding(
                  padding: const EdgeInsets.only(top: 1),
                  child: _small(tr('zera {quando}', {'quando': resetLabel(resets)}), alpha: 0.6),
                ),
            ],
          ),
        );
      }

      return Padding(
        padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Row(mainAxisAlignment: MainAxisAlignment.center, children: [
              const Icon(Icons.auto_awesome, size: 12),
              const SizedBox(width: 4),
              Flexible(child: _small(title, alpha: 0.85)),
            ]),
            const SizedBox(height: 2),
            if (rows.isEmpty) _small(tr('Sem dados'))
            else
              for (final r in rows) row(r),
          ],
        ),
      );
    });
  }

  // ---- Tocando agora ----

  Widget _buildNowPlaying(Map<String, dynamic> np) {
    final title = np['title'] as String?;
    final artist = np['artist'] as String?;
    var thumb = np['thumbnailBase64'] as String?;
    if (thumb != null && thumb.startsWith('data:')) {
      thumb = thumb.substring(thumb.indexOf(',') + 1);
    }
    if (title == null || title.isEmpty) {
      return Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          const Icon(Icons.music_off, size: 28),
          const SizedBox(height: 4),
          _small(_label ?? tr('Nada tocando')),
        ],
      );
    }
    Widget? cover;
    if (thumb != null && thumb.isNotEmpty) {
      try {
        cover = Image.memory(base64Decode(thumb),
            fit: BoxFit.cover,
            gaplessPlayback: true,
            errorBuilder: (_, __, ___) => const Icon(Icons.music_note));
      } catch (_) {}
    }
    return Padding(
      padding: const EdgeInsets.all(4),
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Flexible(
            child: LayoutBuilder(builder: (context, c) {
              // Botão grande (2×1) não é quadrado: a imagem não passa da altura disponível.
        final w = c.maxWidth.isFinite ? c.maxWidth * 0.62 : 48.0;
        final side = c.maxHeight.isFinite && c.maxHeight < w ? c.maxHeight : w;
              return ClipRRect(
                borderRadius: BorderRadius.circular(6),
                child: SizedBox(
                  width: side,
                  height: side,
                  child: Stack(fit: StackFit.expand, children: [
                    cover ?? const Icon(Icons.music_note, size: 28),
                    if (np['playing'] != true)
                      Container(
                        color: Colors.black45,
                        child: const Icon(Icons.pause, color: Colors.white),
                      ),
                  ]),
                ),
              );
            }),
          ),
          const SizedBox(height: 2),
          Text(title,
              textAlign: TextAlign.center,
              maxLines: 1,
              overflow: TextOverflow.ellipsis,
              style: const TextStyle(fontSize: 10, fontWeight: FontWeight.w600)),
          if (artist != null && artist.isNotEmpty) _small(artist),
        ],
      ),
    );
  }
}

/// Ícone do tempo pelo código WMO do Open-Meteo.
IconData weatherIcon(int? code, bool isDay) {
  if (code == null) return Icons.thermostat;
  if (code == 0) return isDay ? Icons.wb_sunny : Icons.nightlight_round;
  if (code <= 2) return isDay ? Icons.wb_cloudy : Icons.nights_stay;
  if (code == 3) return Icons.cloud;
  if (code == 45 || code == 48) return Icons.foggy;
  if (code >= 51 && code <= 67) return Icons.grain;
  if (code >= 71 && code <= 77) return Icons.ac_unit;
  if (code >= 80 && code <= 82) return Icons.umbrella;
  if (code == 85 || code == 86) return Icons.ac_unit;
  if (code >= 95) return Icons.thunderstorm;
  return Icons.cloud;
}

/// Cor da barra de uso: verde, laranja a partir de 60%, vermelho a partir de 85%.
Color usageColor(double pct) {
  if (pct > 85) return Colors.redAccent;
  if (pct > 60) return Colors.orangeAccent;
  return Colors.greenAccent;
}

/// 1234 → "1,2 mil"; 3456789 → "3,5 mi" (inglês: "3.5M"; espanhol: "3,5 M").
String compactCount(int n) {
  final lang = AppLanguage.current.value;
  final (k, m, b) = switch (lang) {
    'en' => ('K', 'M', 'B'),
    'es' => (' mil', ' M', ' mil M'),
    _ => (' mil', ' mi', ' bi'),
  };
  String f(double v) {
    var t = v < 10 ? v.toStringAsFixed(1) : v.toStringAsFixed(0);
    if (t.endsWith('.0')) t = t.substring(0, t.length - 2);
    return lang == 'en' ? t : t.replaceAll('.', ',');
  }

  if (n >= 1000000000) return f(n / 1e9) + b;
  if (n >= 1000000) return f(n / 1e6) + m;
  if (n >= 1000) return f(n / 1e3) + k;
  return '$n';
}

/// Quando um limite zera: só a hora se for hoje; senão o dia da semana e a hora.
String resetLabel(DateTime at, {DateTime? now}) {
  now ??= DateTime.now();
  // A Anthropic manda 20:39:59.9; mostra o minuto cheio.
  at = at.add(const Duration(seconds: 30));
  String two(int n) => n.toString().padLeft(2, '0');
  final time = '${two(at.hour)}:${two(at.minute)}';
  final sameDay = at.year == now.year && at.month == now.month && at.day == now.day;
  if (sameDay) return time;
  const days = ['seg', 'ter', 'qua', 'qui', 'sex', 'sáb', 'dom'];
  return '${tr(days[at.weekday - 1])} $time';
}
