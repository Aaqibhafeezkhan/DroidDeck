import 'package:droiddeck/core/core.dart';
import 'package:flutter/material.dart';

/// Automações: um acontecimento no PC (horário, app abrindo ou fechando, volta da
/// suspensão, DroidDeck iniciando) aperta um botão do deck sozinho.
class TriggersPage extends StatefulWidget {
  const TriggersPage({super.key});

  @override
  State<TriggersPage> createState() => _TriggersPageState();
}

class _TriggersPageState extends State<TriggersPage> {
  final _client = Injector.get<DroidDeckClient>();
  List<Map<String, dynamic>> _triggers = [];
  List<DeckProfile> _profiles = [];
  bool _loading = true;
  String? _error;

  static Map<String, String> get events => {
        'schedule': tr('Num horário'),
        'app_start': tr('Quando um app abrir'),
        'app_exit': tr('Quando um app fechar'),
        'resume': tr('Quando o PC voltar da suspensão'),
        'startup': tr('Quando o DroidDeck iniciar'),
      };

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    try {
      final list = await _client.getJson('/api/triggers') as List;
      final profiles = await _client.getProfiles();
      if (!mounted) return;
      setState(() {
        _triggers = list.map((e) => Map<String, dynamic>.from(e as Map)).toList();
        _profiles = profiles;
        _loading = false;
      });
    } catch (e) {
      if (mounted) {
        setState(() {
          _loading = false;
          _error = tr('O PC não respondeu.');
        });
      }
    }
  }

  Future<void> _save() async {
    try {
      await _client.putJson('/api/triggers', _triggers);
    } catch (_) {
      if (mounted) _snack(tr('Não deu para salvar.'));
    }
  }

  void _snack(String m) =>
      ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(m)));

  String _buttonName(Map<String, dynamic> t) {
    final p = _profiles.where((p) => p.id == t['profileId']).firstOrNull;
    final b = p?.buttons.where((b) => b.id == t['buttonId']).firstOrNull;
    if (p == null || b == null) return tr('(botão apagado)');
    return '${p.name} · ${b.label?.isNotEmpty == true ? b.label : tr('sem rótulo')}';
  }

  String _describe(Map<String, dynamic> t) {
    final event = t['event'] as String? ?? 'schedule';
    final when = switch (event) {
      'schedule' => '${t['time'] ?? '--:--'}'
          '${(t['days'] as String?)?.isNotEmpty == true ? ' (${t['days']})' : ''}',
      'app_start' || 'app_exit' => '${events[event]}: ${t['app'] ?? ''}',
      _ => events[event] ?? event,
    };
    return '$when → ${_buttonName(t)}';
  }

  Future<void> _edit([Map<String, dynamic>? existing]) async {
    final result = await showDialog<Map<String, dynamic>>(
      context: context,
      builder: (_) => _TriggerDialog(
        initial: existing ?? {'event': 'schedule', 'enabled': true},
        profiles: _profiles,
      ),
    );
    if (result == null) return;
    setState(() {
      if (existing != null) {
        _triggers[_triggers.indexOf(existing)] = result;
      } else {
        _triggers.add(result);
      }
    });
    await _save();
  }

  Future<void> _run(Map<String, dynamic> t) async {
    try {
      await _client.postJson('/api/triggers/${t['id']}/run');
      _snack(tr('Botão apertado.'));
    } catch (_) {
      _snack(tr('Salve a automação e confira se o botão ainda existe.'));
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: Text(tr('Automações'))),
      floatingActionButton: _loading || _error != null
          ? null
          : FloatingActionButton.extended(
              onPressed: () => _edit(),
              icon: const Icon(Icons.add),
              label: Text(tr('Nova automação')),
            ),
      body: _loading
          ? const Center(child: CircularProgressIndicator())
          : _error != null
              ? Center(child: Text(_error!))
              : ListView(
                  padding: const EdgeInsets.fromLTRB(16, 16, 16, 96),
                  children: [
                    Text(
                      tr('Uma automação aperta um botão do deck sozinha quando algo acontece '
                          'no PC. Crie antes o botão com a ação desejada.'),
                      style: Theme.of(context).textTheme.bodySmall,
                    ),
                    const SizedBox(height: 12),
                    if (_triggers.isEmpty)
                      Padding(
                        padding: const EdgeInsets.only(top: 32),
                        child: Center(child: Text(tr('Nenhuma automação ainda.'))),
                      ),
                    for (final t in _triggers)
                      Card(
                        child: ListTile(
                          leading: Switch(
                            value: t['enabled'] != false,
                            onChanged: (v) {
                              setState(() => t['enabled'] = v);
                              _save();
                            },
                          ),
                          title: Text((t['name'] as String?)?.isNotEmpty == true
                              ? t['name'] as String
                              : events[t['event']] ?? ''),
                          subtitle: Text(_describe(t)),
                          onTap: () => _edit(t),
                          trailing: PopupMenuButton<String>(
                            onSelected: (v) async {
                              if (v == 'run') _run(t);
                              if (v == 'delete') {
                                setState(() => _triggers.remove(t));
                                await _save();
                              }
                            },
                            itemBuilder: (_) => [
                              PopupMenuItem(value: 'run', child: Text(tr('Testar agora'))),
                              PopupMenuItem(value: 'delete', child: Text(tr('Excluir'))),
                            ],
                          ),
                        ),
                      ),
                  ],
                ),
    );
  }
}

class _TriggerDialog extends StatefulWidget {
  final Map<String, dynamic> initial;
  final List<DeckProfile> profiles;

  const _TriggerDialog({required this.initial, required this.profiles});

  @override
  State<_TriggerDialog> createState() => _TriggerDialogState();
}

class _TriggerDialogState extends State<_TriggerDialog> {
  late final Map<String, dynamic> _t = Map.of(widget.initial);

  String get _event => _t['event'] as String? ?? 'schedule';

  /// "perfil|botão" de todos os botões com ação.
  Map<String, String> get _buttons => {
        for (final p in widget.profiles)
          for (final b in p.buttons.where((b) => b.action != null && b.action!.type != 'none'))
            '${p.id}|${b.id}':
                '${p.name} · ${b.label?.isNotEmpty == true ? b.label : tr('sem rótulo')}',
      };

  Widget _text(String key, String label, {String? hint, String? help}) => Padding(
        padding: const EdgeInsets.only(top: 12),
        child: TextFormField(
          initialValue: _t[key] as String? ?? '',
          decoration: InputDecoration(
            labelText: label,
            hintText: hint,
            helperText: help,
            helperMaxLines: 2,
            border: const OutlineInputBorder(),
          ),
          onChanged: (v) => _t[key] = v.trim(),
        ),
      );

  @override
  Widget build(BuildContext context) {
    final buttons = _buttons;
    final selected = '${_t['profileId']}|${_t['buttonId']}';
    final valid = buttons.containsKey(selected);
    return AlertDialog(
      title: Text(tr('Automação')),
      content: SizedBox(
        width: 420,
        child: SingleChildScrollView(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              _text('name', tr('Nome (opcional)')),
              const SizedBox(height: 12),
              DropdownButtonFormField<String>(
                initialValue: _event,
                isExpanded: true,
                decoration: InputDecoration(
                    labelText: tr('Quando'), border: const OutlineInputBorder()),
                items: [
                  for (final e in _TriggersPageState.events.entries)
                    DropdownMenuItem(value: e.key, child: Text(e.value)),
                ],
                onChanged: (v) => setState(() => _t['event'] = v ?? 'schedule'),
              ),
              if (_event == 'schedule') ...[
                _text('time', tr('Horário'), hint: '09:00'),
                _text('days', tr('Dias (opcional)'),
                    hint: '1-5', help: tr('0 = domingo … 6 = sábado. Vazio = todo dia.')),
              ],
              if (_event == 'app_start' || _event == 'app_exit')
                _text('app', tr('Processo do aplicativo'),
                    hint: 'obs64', help: tr('Nome do processo, sem .exe (Spotify, chrome, Discord).')),
              const SizedBox(height: 12),
              DropdownButtonFormField<String>(
                initialValue: valid ? selected : null,
                isExpanded: true,
                decoration: InputDecoration(
                    labelText: tr('Apertar o botão'), border: const OutlineInputBorder()),
                items: [
                  for (final e in buttons.entries)
                    DropdownMenuItem(
                        value: e.key, child: Text(e.value, overflow: TextOverflow.ellipsis)),
                ],
                onChanged: (v) => setState(() {
                  final parts = (v ?? '|').split('|');
                  _t['profileId'] = parts[0];
                  _t['buttonId'] = parts[1];
                }),
              ),
            ],
          ),
        ),
      ),
      actions: [
        TextButton(
            onPressed: () => Navigator.pop(context), child: Text(tr('Cancelar'))),
        FilledButton(
          onPressed: valid ? () => Navigator.pop(context, _t) : null,
          child: Text(tr('Salvar')),
        ),
      ],
    );
  }
}
