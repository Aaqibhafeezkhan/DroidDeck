import 'package:droiddeck/core/core.dart';
import 'package:flutter/material.dart';

import 'package:droiddeck/src/services/signalr_service.dart';

import 'remote_options_field.dart';

/// "Mostrar no botão": monitor do PC ou informação (relógio, clima, comando, URL...),
/// com os campos de cada tipo. Quem chama guarda o tipo e os parâmetros.
class InfoSection extends StatelessWidget {
  final String? type;
  final Map<String, String> params;

  /// Tipo gravado no botão antes da edição (para saber se está travado no celular).
  final String? savedType;

  /// Editando no celular (comando e URL só mudam no configurador do PC).
  final bool remote;

  final void Function(String? type, Map<String, String> params) onChanged;

  const InfoSection({
    super.key,
    required this.type,
    required this.params,
    required this.savedType,
    required this.remote,
    required this.onChanged,
  });

  static const labels = {
    'cpu_monitor': 'Uso da CPU',
    'memory_monitor': 'Uso da memória',
    'gpu_monitor': 'Uso da GPU',
    'network_monitor': 'Rede',
    'system_monitor': 'CPU e memória',
    'disk_monitor': 'Espaço em disco',
    'clock': 'Relógio',
    'weather': 'Clima',
    'now_playing': 'Tocando agora',
    'command': 'Saída de um comando (PC)',
    'url': 'Valor de uma URL (PC)',
    'volume_slider': 'Controle deslizante (volume, brilho, música)',
    'notifications': 'Notificações do Windows',
    'ai_usage': 'Uso de IA (Claude, Codex)',
  };

  /// Comando e URL rodam no PC: como os atalhos, só mudam no configurador.
  static const privileged = {'command', 'url'};

  bool get _locked => remote && privileged.contains(savedType);

  String _p(String key, [String fallback = '']) => params[key] ?? fallback;

  void _set(String key, String value) {
    final next = Map.of(params);
    if (value.isEmpty) {
      next.remove(key);
    } else {
      next[key] = value;
    }
    onChanged(type, next);
  }

  @override
  Widget build(BuildContext context) {
    final hint = TextStyle(fontSize: 12, color: Theme.of(context).colorScheme.onSurfaceVariant);
    final options = labels.entries
        .where((e) => !remote || !privileged.contains(e.key) || e.key == type)
        .toList();
    final hasType = type != null && type!.isNotEmpty;
    return Card(
      margin: EdgeInsets.zero,
      child: ExpansionTile(
        initiallyExpanded: hasType,
        shape: const Border(),
        leading: const Icon(Icons.insights_outlined),
        title: Text(tr('Mostrar no botão')),
        subtitle: Text(
          hasType ? (labels[type] != null ? tr(labels[type]!) : type!) : tr('Opcional'),
          style: const TextStyle(fontSize: 12),
        ),
        childrenPadding: const EdgeInsets.fromLTRB(12, 0, 12, 12),
        children: [
          DropdownButtonFormField<String?>(
            initialValue: hasType ? type : null,
            decoration: InputDecoration(
              labelText: tr('Informação'),
              border: const OutlineInputBorder(),
            ),
            items: [
              DropdownMenuItem(value: null, child: Text(tr('Nenhuma'))),
              ...options.map((e) => DropdownMenuItem(value: e.key, child: Text(tr(e.value)))),
            ],
            // Trocar o tipo zera os parâmetros do anterior.
            onChanged: _locked ? null : (v) => onChanged(v, v == type ? params : {}),
          ),
          if (_locked)
            Padding(
              padding: const EdgeInsets.only(top: 8),
              child: Text(
                tr('Botões que rodam um comando ou leem uma URL no PC só mudam no '
                    'editor do PC: ícone do DroidDeck na bandeja → "Abrir no navegador".'),
                style: hint,
              ),
            ),
          ..._fields(),
          if (hasType)
            Padding(
              padding: const EdgeInsets.only(top: 12),
              child: Text(tr('O toque continua executando a ação do botão, se houver uma.'),
                  style: hint),
            ),
        ],
      ),
    );
  }

  Widget _note(String text) => Padding(
        padding: const EdgeInsets.only(top: 12),
        child: Text(text, style: const TextStyle(fontSize: 13)),
      );

  Widget _field(String key, String label,
      {String? hint, String? help, bool number = false, int maxLines = 1}) {
    return Padding(
      padding: const EdgeInsets.only(top: 12),
      child: TextFormField(
        key: ValueKey('info_${type}_$key'),
        initialValue: _p(key),
        enabled: !_locked,
        keyboardType: number ? TextInputType.number : null,
        maxLines: maxLines,
        decoration: InputDecoration(
          labelText: label,
          hintText: hint,
          helperText: help,
          helperMaxLines: 3,
          border: const OutlineInputBorder(),
        ),
        onChanged: (v) => _set(key, v.trim()),
      ),
    );
  }

  Widget _switch(String key, String label, bool def, {String on = 'true', String off = 'false'}) {
    return SwitchListTile(
      contentPadding: EdgeInsets.zero,
      dense: true,
      title: Text(label),
      value: _p(key, def ? on : off) == on,
      onChanged: (v) => _set(key, v ? on : off),
    );
  }

  /// Anel (padrão) ou barra.
  Widget _style() => Padding(
        padding: const EdgeInsets.only(top: 12),
        child: SegmentedButton<String>(
          segments: [
            ButtonSegment(value: 'ring', label: Text(tr('Anel')), icon: const Icon(Icons.donut_large)),
            ButtonSegment(value: 'bars', label: Text(tr('Barra')), icon: const Icon(Icons.align_horizontal_left)),
          ],
          selected: {_p('style', 'ring')},
          onSelectionChanged: (v) => _set('style', v.first == 'ring' ? '' : v.first),
        ),
      );

  List<Widget> _fields() {
    switch (type) {
      case 'cpu_monitor':
      case 'memory_monitor':
      case 'gpu_monitor':
        return [_style()];
      case 'system_monitor':
        return [
          _switch('gpu', tr('Mostrar a GPU também'), false),
          _switch('net', tr('Mostrar a rede também (download e upload)'), false),
          _note(tr('Uma barra para cada medida; a rede mostra a velocidade numa linha, sem barra. Em botão '
              'alto, a memória mostra quanto está em uso. O que não cabe no botão fica de fora: '
              'aumente a altura em "Tamanho".')),
        ];
      case 'disk_monitor':
        final drive = _p('drive');
        final disks = Injector.get<SignalRService>().systemStats.value.disks;
        return [
          Padding(
            padding: const EdgeInsets.only(top: 12),
            child: DropdownButtonFormField<String>(
              key: ValueKey('disk_drive_$drive'),
              initialValue: drive,
              decoration: InputDecoration(
                labelText: tr('Unidade'),
                border: const OutlineInputBorder(),
              ),
              items: [
                DropdownMenuItem(value: '', child: Text(tr('Todas (uma barra para cada)'))),
                for (final d in disks)
                  DropdownMenuItem(
                    value: d.name,
                    child: Text(d.label.isNotEmpty ? '${d.name} ${d.label}' : d.name),
                  ),
                // Unidade gravada que não aparece agora (HD desligado): continua na lista.
                if (drive.isNotEmpty && !disks.any((d) => d.name == drive))
                  DropdownMenuItem(value: drive, child: Text(drive)),
              ],
              onChanged: (v) => _set('drive', v ?? ''),
            ),
          ),
          if (drive.isNotEmpty) _style(),
          _note(tr('Mostra quanto da unidade está ocupado e quanto ainda está livre. '
              'Atualiza a cada 30 segundos.')),
        ];
      case 'clock':
        return [
          _switch('format24', tr('24 horas'), true),
          _switch('showSeconds', tr('Mostrar os segundos'), false),
          _switch('showDate', tr('Mostrar a data'), true),
        ];
      case 'weather':
        return [
          _field('city', tr('Cidade'), hint: 'São Paulo'),
          _switch('units', tr('Em Fahrenheit'), false, on: 'f', off: 'c'),
        ];
      case 'command':
        return [
          _field('command', tr('Comando'),
              hint: 'powershell -c "(Get-Date).DayOfYear"',
              help: tr('Roda no cmd do PC; o botão mostra a saída (até 10 s).'),
              maxLines: 2),
          _field('format', tr('Modelo'),
              hint: '{value}', help: tr('{value} é a saída. Ex.: "Dia {value}"')),
          _field('interval', tr('Atualizar a cada (segundos)'), hint: '60', number: true),
        ];
      case 'url':
        return [
          _field('url', tr('URL'), hint: 'https://api.exemplo.com/status'),
          _field('path', tr('Campo do JSON'),
              hint: 'data.items[0].name', help: tr('Vazio = a resposta inteira.')),
          _field('format', tr('Modelo'), hint: '{value}', help: tr('{value} é o valor lido.')),
          _field('interval', tr('Atualizar a cada (segundos)'), hint: '60', number: true),
        ];
      case 'notifications':
        return [
          _field('app', tr('Só do app (opcional)'),
              hint: 'Discord',
              help: tr('Parte do nome como aparece nas notificações do Windows. Vazio = todas.')),
        ];
      case 'ai_usage':
        final provider = _p('provider', 'claude');
        final show = _p('show', 'plan');
        return [
          Padding(
            padding: const EdgeInsets.only(top: 12),
            child: DropdownButtonFormField<String>(
              key: ValueKey('ai_provider_$provider'),
              initialValue: provider,
              decoration: InputDecoration(
                labelText: tr('Ferramenta'),
                border: const OutlineInputBorder(),
              ),
              items: const [
                DropdownMenuItem(value: 'claude', child: Text('Claude (Claude Code)')),
                DropdownMenuItem(value: 'codex', child: Text('Codex (OpenAI)')),
              ],
              onChanged: (v) => _set('provider', v ?? 'claude'),
            ),
          ),
          Padding(
            padding: const EdgeInsets.only(top: 12),
            child: SegmentedButton<String>(
              segments: [
                ButtonSegment(value: 'plan', label: Text(tr('Limites do plano'))),
                ButtonSegment(value: 'tokens', label: Text(tr('Tokens'))),
              ],
              selected: {show},
              onSelectionChanged: (v) => _set('show', v.first),
            ),
          ),
          _note(show == 'plan'
              ? (provider == 'codex'
                  ? tr('Quanto já foi usado da janela de 5 horas e da semana, pelo último registro '
                      'do Codex neste PC. Atualiza quando o Codex roda.')
                  : tr('Quanto já foi usado da janela de 5 horas e da semana, como no /usage. Usa '
                      'o login do Claude Code deste PC; nada sai do PC além da consulta à Anthropic.'))
              : tr('Tokens de hoje e dos últimos 7 dias, somados dos registros que a ferramenta '
                  'guarda no PC (entrada, saída e cache).')),
        ];
      case 'volume_slider':
        final target = _p('target', 'output');
        // Trocar o alvo limpa os campos do alvo anterior (uma lâmpada não serve ao OBS).
        void setTarget(String? v) => onChanged(type, {'target': v ?? 'output'});
        Widget remote(String key, String label, String url, {String? help}) => RemoteOptionsField(
            url: url, label: label, value: params[key], help: help, onChanged: (v) => _set(key, v));
        return [
          Padding(
            padding: const EdgeInsets.only(top: 12),
            child: DropdownButtonFormField<String>(
              key: ValueKey('slider_target_$target'),
              initialValue: target,
              isExpanded: true,
              decoration: InputDecoration(
                labelText: tr('Controla'),
                border: const OutlineInputBorder(),
              ),
              items: [
                DropdownMenuItem(value: 'output', child: Text(tr('Som do PC (saída padrão)'))),
                DropdownMenuItem(value: 'input', child: Text(tr('Microfone (entrada padrão)'))),
                DropdownMenuItem(value: 'app', child: Text(tr('Um aplicativo'))),
                DropdownMenuItem(value: 'obs', child: Text(tr('Volume de uma fonte do OBS'))),
                DropdownMenuItem(value: 'discord_user', child: Text(tr('Volume de uma pessoa no Discord'))),
                DropdownMenuItem(value: 'monitor', child: Text(tr('Brilho do monitor'))),
                DropdownMenuItem(value: 'media', child: Text(tr('Posição da música'))),
                DropdownMenuItem(value: 'tuya', child: Text(tr('Brilho de lâmpada Smart Life (Tuya)'))),
                DropdownMenuItem(value: 'homeassistant', child: Text(tr('Brilho de luz do Home Assistant'))),
                DropdownMenuItem(value: 'hue', child: Text(tr('Brilho de luz Philips Hue'))),
                DropdownMenuItem(value: 'govee', child: Text(tr('Brilho de luz Govee'))),
                DropdownMenuItem(value: 'keylight', child: Text(tr('Brilho de Elgato Key Light'))),
              ],
              onChanged: setTarget,
            ),
          ),
          if (target == 'app')
            _field('app', tr('Processo do aplicativo'),
                hint: 'Spotify', help: tr('Nome do processo, sem .exe (Spotify, chrome, Discord).')),
          if (target == 'obs') remote('inputName', tr('Fonte de áudio'), '/api/obs/audio-inputs'),
          if (target == 'discord_user')
            remote('userId', tr('Pessoa'), '/api/slider/options?target=discord_user',
                help: tr('Mostra quem está na call agora. 100% é o volume normal; vai até 200%.')),
          if (target == 'tuya')
            remote('deviceId', tr('Lâmpada'), '/api/slider/options?target=tuya'),
          if (target == 'homeassistant')
            remote('entityId', tr('Luz'),
                '/api/integrations/homeassistant/options/entities?operation=brightness'),
          if (target == 'hue') ...[
            Padding(
              padding: const EdgeInsets.only(top: 12),
              child: SegmentedButton<String>(
                segments: [
                  ButtonSegment(value: 'light', label: Text(tr('Uma luz'))),
                  ButtonSegment(value: 'group', label: Text(tr('Um cômodo'))),
                ],
                selected: {_p('hueTarget', 'light')},
                onSelectionChanged: (v) =>
                    onChanged(type, {'target': 'hue', 'hueTarget': v.first}),
              ),
            ),
            remote('id', _p('hueTarget', 'light') == 'group' ? tr('Cômodo') : tr('Luz'),
                '/api/integrations/hue/options/items?target=${_p('hueTarget', 'light')}'),
          ],
          if (target == 'govee') remote('ip', tr('Luz'), '/api/govee/devices'),
          if (target == 'keylight')
            _field('host', tr('Endereço da Key Light'), hint: '192.168.0.50'),
          if (target == 'monitor')
            _note(tr('Muda todos os monitores juntos. Monitor externo precisa do DDC/CI '
                'ligado no menu dele.')),
          if (target == 'media')
            _note(tr('Arraste para avançar ou voltar na faixa que está tocando no PC.')),
          Padding(
            padding: const EdgeInsets.only(top: 8),
            child: Text(
              tr('Arraste o dedo no botão. Deixe o botão mais largo ou mais alto em "Tamanho" '
                  'para ter mais precisão.'),
              style: TextStyle(fontSize: 12, color: Colors.grey[500]),
            ),
          ),
        ];
      default:
        return const [];
    }
  }
}
