import 'package:droiddeck/core/core.dart';
import 'package:droiddeck/src/connection/constants.dart';
import 'package:flutter/material.dart';
import 'package:shared_preferences/shared_preferences.dart';

/// Deck no navegador sem chave: explica como parear pelo QR "Abrir no navegador" do PC.
/// Também aceita a chave digitada, para quem não tem câmera à mão.
class BrowserPairPage extends StatefulWidget {
  /// A chave guardada foi recusada pelo PC (trocada ou de outro PC).
  final bool rejected;

  const BrowserPairPage({super.key, this.rejected = false});

  @override
  State<BrowserPairPage> createState() => _BrowserPairPageState();
}

class _BrowserPairPageState extends State<BrowserPairPage> {
  final _key = TextEditingController();
  bool _busy = false;
  String? _error;

  @override
  void initState() {
    super.initState();
    if (widget.rejected) {
      _error = tr('O PC recusou a chave salva neste navegador. Pareie de novo.');
    }
  }

  @override
  void dispose() {
    _key.dispose();
    super.dispose();
  }

  Future<void> _useKey() async {
    final key = _key.text.trim();
    if (key.isEmpty) return;
    setState(() {
      _busy = true;
      _error = null;
    });
    final client = Injector.get<DroidDeckClient>();
    client.setApiKey(key);
    if (!await client.checkKey()) {
      if (mounted) {
        setState(() {
          _busy = false;
          _error = tr('O PC não aceitou essa chave. Confira se copiou ela inteira, sem espaços.');
        });
      }
      return;
    }
    await (await SharedPreferences.getInstance()).setString(Constants.apiKeyKey, key);
    if (!mounted) return;
    // O splash refaz a conexão (SignalR) com a chave nova.
    Navigator.of(context).pushReplacementNamed('/');
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final muted = theme.textTheme.bodyMedium?.copyWith(
      color: theme.colorScheme.onSurface.withValues(alpha: 0.7),
    );
    return Scaffold(
      body: SafeArea(
        child: Center(
          child: ConstrainedBox(
            constraints: const BoxConstraints(maxWidth: 480),
            child: SingleChildScrollView(
              padding: const EdgeInsets.fromLTRB(24, 32, 24, 24),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  Image.asset('assets/images/logo.png', height: 72),
                  const SizedBox(height: 20),
                  Text(
                    tr('Usar este aparelho como deck'),
                    textAlign: TextAlign.center,
                    style: theme.textTheme.headlineSmall
                        ?.copyWith(fontWeight: FontWeight.bold),
                  ),
                  const SizedBox(height: 12),
                  Text(
                    tr('No PC, clique com o botão direito no ícone do DroidDeck → '
                        '"Parear dispositivo (QR)" → "Abrir no navegador" e aponte a '
                        'câmera deste aparelho para o código.'),
                    textAlign: TextAlign.center,
                    style: muted,
                  ),
                  if (_error != null) ...[
                    const SizedBox(height: 16),
                    Text(_error!,
                        textAlign: TextAlign.center,
                        style: TextStyle(color: theme.colorScheme.error)),
                  ],
                  const SizedBox(height: 28),
                  Text(tr('Ou digite a chave mostrada na janela de pareamento:'),
                      style: muted),
                  const SizedBox(height: 8),
                  TextField(
                    controller: _key,
                    autocorrect: false,
                    enableSuggestions: false,
                    decoration: InputDecoration(
                      border: const OutlineInputBorder(),
                      labelText: tr('Chave'),
                      helperText: tr('O texto depois de "Chave:" na janela de pareamento do PC.'),
                      helperMaxLines: 2,
                    ),
                    onSubmitted: (_) => _useKey(),
                  ),
                  const SizedBox(height: 12),
                  FilledButton(
                    onPressed: _busy ? null : _useKey,
                    child: Text(tr('Conectar')),
                  ),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }
}
