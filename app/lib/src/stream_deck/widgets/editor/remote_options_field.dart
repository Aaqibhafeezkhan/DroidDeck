import 'package:droiddeck/core/core.dart';
import 'package:droiddeck/src/stream_deck/actions/action_schema.dart';
import 'package:flutter/material.dart';

/// Lista de escolhas que vem do PC ([url] devolve [{value, label}], [{id}] ou [{name}]):
/// fontes do OBS, pessoas na call, lâmpadas... Sem lista (integração desligada, ninguém na
/// call), deixa digitar e diz por quê.
class RemoteOptionsField extends StatefulWidget {
  final String url;
  final String label;
  final String? value;
  final String? help;
  final ValueChanged<String> onChanged;

  const RemoteOptionsField({
    super.key,
    required this.url,
    required this.label,
    required this.value,
    required this.onChanged,
    this.help,
  });

  @override
  State<RemoteOptionsField> createState() => _RemoteOptionsFieldState();
}

class _RemoteOptionsFieldState extends State<RemoteOptionsField> {
  List<FieldOption>? _options;
  String? _error;
  bool _loading = false;

  @override
  void initState() {
    super.initState();
    _load();
  }

  @override
  void didUpdateWidget(RemoteOptionsField old) {
    super.didUpdateWidget(old);
    if (old.url != widget.url) _load();
  }

  Future<void> _load() async {
    setState(() {
      _loading = true;
      _error = null;
    });
    try {
      final data = await Injector.get<DroidDeckClient>().getJson(widget.url);
      final list = (data as List? ?? const [])
          .whereType<Map>()
          .map((m) => FieldOption.fromJson(Map<String, dynamic>.from(m)))
          .toList();
      if (mounted) setState(() => _options = list);
    } catch (e) {
      if (mounted) {
        setState(() {
          _options = const [];
          _error = tr('O PC não trouxe a lista (a integração está conectada?).');
        });
      }
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final options = _options ?? const <FieldOption>[];
    final value = widget.value ?? '';
    final refresh = _loading
        ? const Padding(
            padding: EdgeInsets.all(12),
            child: SizedBox(width: 18, height: 18, child: CircularProgressIndicator(strokeWidth: 2)),
          )
        : IconButton(
            tooltip: tr('Recarregar a lista'),
            icon: const Icon(Icons.refresh),
            onPressed: _load,
          );

    if (options.isEmpty && !_loading) {
      return Padding(
        padding: const EdgeInsets.only(top: 12),
        child: TextFormField(
          key: ValueKey('remote_text_${widget.url}'),
          initialValue: value,
          decoration: InputDecoration(
            labelText: widget.label,
            border: const OutlineInputBorder(),
            helperText: _error ?? tr('Lista vazia. Digite o valor ou recarregue.'),
            helperMaxLines: 3,
            suffixIcon: refresh,
          ),
          onChanged: widget.onChanged,
        ),
      );
    }

    final known = options.any((o) => o.value == value);
    return Padding(
      padding: const EdgeInsets.only(top: 12),
      child: DropdownButtonFormField<String>(
        key: ValueKey('remote_${widget.url}_$value'),
        initialValue: value.isEmpty ? null : value,
        isExpanded: true,
        decoration: InputDecoration(
          labelText: widget.label,
          border: const OutlineInputBorder(),
          helperText: widget.help,
          helperMaxLines: 3,
          suffixIcon: refresh,
        ),
        items: [
          for (final o in options)
            DropdownMenuItem(value: o.value, child: Text(o.label, overflow: TextOverflow.ellipsis)),
          // Valor salvo que a lista não traz agora (OBS fechado, pessoa fora da call).
          if (value.isNotEmpty && !known)
            DropdownMenuItem(
                value: value,
                child: Text(tr('{value} (salvo)', {'value': value}), overflow: TextOverflow.ellipsis)),
        ],
        onChanged: (v) {
          if (v != null) widget.onChanged(v);
        },
      ),
    );
  }
}
