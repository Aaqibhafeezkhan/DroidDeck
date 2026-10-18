import 'package:droiddeck/src/connection/browser_url.dart';
import 'package:droiddeck/src/connection/pc_store.dart';
import 'package:droiddeck/src/connection/usb_route.dart';
import 'package:droiddeck/src/connection/constants.dart';
import 'package:droiddeck/src/services/signalr_service.dart';
import 'package:droiddeck/core/core.dart';
import 'package:droiddeck/src/welcome/browser_pair_page.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:shared_preferences/shared_preferences.dart';

class SplashPage extends StatefulWidget {
  const SplashPage({super.key});

  @override
  SplashPageState createState() => SplashPageState();
}

class SplashPageState extends State<SplashPage> {
  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) => _checkConfig());
  }

  Future<void> _checkConfig() async {
    if (kIsWeb) {
      // O web é servido pelo próprio PC: no PC a chave vem pelo loopback (sem QR); de
      // outro aparelho, do QR "Abrir no navegador" (?pair=) ou do que ficou salvo.
      final client = Injector.get<DroidDeckClient>();
      final signalR = Injector.get<SignalRService>();
      final origin = Uri.base.origin;
      client.setBaseUrl(origin);
      String? key;
      try {
        final resp = await Dio().get('$origin/api/pairing/local-key');
        key = (resp.data is Map) ? resp.data['key'] as String? : null;
      } catch (_) {}
      var rejected = false;
      if (key == null && WebMode.deck) {
        final sp = await SharedPreferences.getInstance();
        final fromUrl = Uri.base.queryParameters['pair'];
        if (fromUrl != null && fromUrl.isNotEmpty) {
          clearPairingFromUrl();
          await sp.setString(Constants.apiKeyKey, fromUrl);
        }
        key = sp.getString(Constants.apiKeyKey);
        if (key != null) {
          client.setApiKey(key);
          if (!await client.checkKey()) {
            await sp.remove(Constants.apiKeyKey);
            key = null;
            rejected = true;
          }
        }
        if (key == null) {
          if (!mounted) return;
          Navigator.of(context).pushReplacement(MaterialPageRoute(
              builder: (_) => BrowserPairPage(rejected: rejected)));
          return;
        }
      }
      client.setApiKey(key);
      await signalR.init(origin, apiKey: key);
      if (mounted) Navigator.of(context).pushReplacementNamed('/home');
      return;
    }
    // Lista de PCs pareados (migra o PC único das versões antigas).
    await PcStore.load();
    final sp = await SharedPreferences.getInstance();
    final ip = sp.getString(Constants.ipAddrKey);
    final port = sp.getString(Constants.portKey) ?? Constants.defaultPort;
    final apiKey = sp.getString(Constants.apiKeyKey);

    if (ip != null && ip.isNotEmpty && mounted) {
      // Pelo cabo USB quando o PC responde nele; senão pela rede.
      final baseUrl = await UsbRoute.pick('http://$ip:$port', apiKey: apiKey);
      if (!mounted) return;

      final client = Injector.get<DroidDeckClient>();
      client.setBaseUrl(baseUrl);
      client.setApiKey(apiKey);

      // Initialize SignalR with the configured URL (use local variable, not client.baseUrl)
      final signalR = Injector.get<SignalRService>();
      await signalR.init(baseUrl, apiKey: apiKey);

      if (!mounted) return; // o widget pode ter sido descartado durante o await
      Navigator.of(context).pushReplacementNamed('/home');
    } else if (mounted) {
      // Ainda não pareou: explica que precisa do DroidDeck no PC antes de pedir o IP.
      Navigator.of(context).pushReplacementNamed('/welcome');
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Image.asset(
              'assets/images/logo.png',
              width: 150,
              height: 150,
            ),
            const SizedBox(height: 16),
            const Text(
              'DroidDeck',
              style: TextStyle(
                fontSize: 32,
                fontWeight: FontWeight.w800,
                letterSpacing: 0.5,
              ),
            ),
            const SizedBox(height: 200),
            LoadingAnimationWidget.staggeredDotsWave(
              color: Theme.of(context).colorScheme.primary,
              size: 50,
            ),
          ],
        ),
      ),
    );
  }
}
