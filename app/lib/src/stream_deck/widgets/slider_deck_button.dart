import 'dart:async';

import 'package:droiddeck/core/core.dart';
import 'package:flutter/material.dart';

import '../buttons/colors.dart';

/// Tipos de botão controle deslizante (dynamicType).
const sliderTypes = {'volume_slider'};

/// Controle deslizante de volume: arrasta o dedo para mudar o volume do PC, do microfone
/// ou de um app. Em pé quando o botão é mais alto que largo; deitado no contrário. Um
/// botão grande (1×3, 3×1) dá mais precisão. O toque continua indo para [onTap] (editar,
/// no modo de edição) e o toque longo abre o editor.
class SliderDeckButton extends StatefulWidget {
  final DeckButton button;
  final VoidCallback onTap;
  final VoidCallback onLongPress;

  /// No configurador do PC o botão é arrastável para mudar de casa: aí o controle só mostra.
  final bool interactive;

  const SliderDeckButton({
    super.key,
    required this.button,
    required this.onTap,
    required this.onLongPress,
    this.interactive = true,
  });

  @override
  State<SliderDeckButton> createState() => _SliderDeckButtonState();
}

class _SliderDeckButtonState extends State<SliderDeckButton> {
  int? _volume;
  bool _mute = false;
  bool _dragging = false;
  Timer? _poll;
  Timer? _sendTimer;
  int? _pending;

  String get _target => widget.button.dynamicParameters?['target'] ?? 'output';
  String get _app => widget.button.dynamicParameters?['app'] ?? '';
  DroidDeckClient get _client => Injector.get<DroidDeckClient>();

  @override
  void initState() {
    super.initState();
    _refresh();
    // O volume também muda por fora (teclas do PC, outro botão): relê de tempos em tempos.
    _poll = Timer.periodic(const Duration(seconds: 3), (_) {
      if (!_dragging) _refresh();
    });
  }

  @override
  void didUpdateWidget(SliderDeckButton old) {
    super.didUpdateWidget(old);
    if (old.button.dynamicParameters != widget.button.dynamicParameters) _refresh();
  }

  @override
  void dispose() {
    _poll?.cancel();
    _sendTimer?.cancel();
    super.dispose();
  }

  Future<void> _refresh() async {
    final state = await _client.getSlider(_target, _app);
    if (!mounted || _dragging) return;
    setState(() {
      _volume = (state?['volume'] as num?)?.toInt();
      _mute = state?['mute'] == true;
    });
  }

  /// Manda no máximo ~12 vezes por segundo enquanto arrasta; o último valor sempre vai.
  void _send(int volume) {
    _pending = volume;
    if (_sendTimer?.isActive ?? false) return;
    _flush();
    _sendTimer = Timer(const Duration(milliseconds: 80), _flush);
  }

  void _flush() {
    final v = _pending;
    _pending = null;
    if (v != null) _client.setSlider(_target, _app, v);
  }

  void _update(Offset local, Size size, bool vertical) {
    final fraction = vertical
        ? 1 - local.dy / size.height
        : local.dx / size.width;
    final v = (fraction.clamp(0.0, 1.0) * 100).round();
    if (v == _volume) return;
    setState(() => _volume = v);
    _send(v);
  }

  @override
  Widget build(BuildContext context) {
    final color = parseHexColor(widget.button.backgroundColor);
    final label = widget.button.label?.isNotEmpty == true
        ? widget.button.label!
        : switch (_target) {
            'input' => tr('Microfone'),
            'app' => _app,
            _ => tr('Volume'),
          };
    return DeckTile(
      color: color,
      onTap: widget.onTap,
      onLongPress: widget.onLongPress,
      child: LayoutBuilder(builder: (context, c) {
        final size = Size(c.maxWidth, c.maxHeight);
        final vertical = size.height > size.width;
        final fraction = (_volume ?? 0) / 100;
        final fg = DeckTile.foregroundOf(context);
        final fill = Theme.of(context).colorScheme.primary.withValues(alpha: _mute ? 0.25 : 0.55);
        final content = Stack(
          children: [
            Positioned.fill(
              child: Align(
                alignment: vertical ? Alignment.bottomCenter : Alignment.centerLeft,
                child: FractionallySizedBox(
                  heightFactor: vertical ? fraction : 1,
                  widthFactor: vertical ? 1 : fraction,
                  child: DecoratedBox(
                    decoration: BoxDecoration(
                      color: fill,
                      borderRadius: BorderRadius.circular(8),
                    ),
                  ),
                ),
              ),
            ),
            Center(
              child: Padding(
                padding: const EdgeInsets.all(6),
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    Icon(
                      _mute || _volume == 0
                          ? (_target == 'input' ? Icons.mic_off : Icons.volume_off)
                          : (_target == 'input' ? Icons.mic : Icons.volume_up),
                      color: fg,
                    ),
                    Text(
                      _volume == null ? '—' : '$_volume%',
                      style: TextStyle(color: fg, fontWeight: FontWeight.w700),
                    ),
                    Text(label,
                        maxLines: 1,
                        overflow: TextOverflow.ellipsis,
                        style: TextStyle(color: fg, fontSize: 11)),
                  ],
                ),
              ),
            ),
          ],
        );
        if (!widget.interactive) return content;
        void start(Offset p) {
          _dragging = true;
          _update(p, size, vertical);
        }

        void end() {
          _dragging = false;
          _sendTimer?.cancel();
          _flush();
        }

        // Só o eixo do controle: arrastar no outro sentido continua trocando de página.
        return GestureDetector(
          behavior: HitTestBehavior.opaque,
          onVerticalDragStart: vertical ? (d) => start(d.localPosition) : null,
          onVerticalDragUpdate: vertical ? (d) => _update(d.localPosition, size, vertical) : null,
          onVerticalDragEnd: vertical ? (_) => end() : null,
          onHorizontalDragStart: vertical ? null : (d) => start(d.localPosition),
          onHorizontalDragUpdate: vertical ? null : (d) => _update(d.localPosition, size, vertical),
          onHorizontalDragEnd: vertical ? null : (_) => end(),
          child: content,
        );
      }),
    );
  }
}
