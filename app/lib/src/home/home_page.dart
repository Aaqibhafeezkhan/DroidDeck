import 'package:droiddeck/core/core.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';

import 'package:droiddeck/src/connection/browser_url.dart';
import 'package:droiddeck/src/connection/constants.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'home_controller.dart';
import 'audio_page.dart';
import '../stream_deck/stream_deck_page.dart';

class HomePage extends StatefulWidget {
  const HomePage({super.key});

  @override
  HomePageState createState() => HomePageState();
}

class HomePageState extends State<HomePage> {
  final controller = HomeController();
  // Abre no Deck (é para isso que se abre o app); lembra a última aba usada.
  static const _tabKey = 'home_tab';
  final PageController _pageController = PageController(initialPage: 1);
  int selectedIndex = 1;
  void Function()? _deckOnlyCleanup;

  @override
  void initState() {
    super.initState();
    // Modo só deck abre em tela cheia (e sai dela quando é desligado). Roda já na
    // primeira vez, com o valor carregado no startup.
    _deckOnlyCleanup = effect(() {
      final deckOnly = ThemeController.instance.deckOnly.value;
      untracked(() => controller.isFullscreen.value = deckOnly);
    });
    // No web o foco é configurar o deck; não precisa do polling de mixer.
    if (WebMode.isDevice) controller.startPolling();
    SharedPreferences.getInstance().then((sp) {
      final tab = sp.getInt(_tabKey);
      if (tab != null && tab != selectedIndex && mounted) {
        setState(() => selectedIndex = tab);
        if (_pageController.hasClients) _pageController.jumpToPage(tab);
      }
    });
  }

  @override
  void dispose() {
    _deckOnlyCleanup?.call();
    controller.dispose();
    _pageController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    // No navegador (configurador no PC), abre direto no editor de deck.
    if (WebMode.configurator) {
      return StreamDeckPage(
        onToggleFullscreen: () {},
        isFullscreen: false,
      );
    }
    final body = Watch((context) {
        final deckOnly = ThemeController.instance.deckOnly.value;
        // O deck tem carregamento e modo offline próprios; a espera aqui é só do áudio.
        if (!deckOnly &&
            controller.isLoading.value &&
            controller.outputs.value.isEmpty) {
          return const Center(child: CircularProgressIndicator());
        }
        final err = controller.error.value ?? '';
        // 401/403 = pareamento invalido: retry so falha de novo. Oferece voltar
        // ao pareamento pra nao prender o usuario nesta tela. Outros erros (PC
        // desligado) nao bloqueiam: o deck abre com o ultimo perfil salvo.
        final isAuthError = err.contains('401') || err.contains('403');
        if (isAuthError) {
          return Center(
            child: Padding(
              padding: const EdgeInsets.all(24),
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Text(
                    isAuthError
                        ? tr('O PC não reconheceu este aparelho (a chave mudou ou é de outro PC).\nPareie de novo pelo QR.')
                        : tr('O PC não respondeu. Confira se o DroidDeck está aberto nele e se os dois estão na mesma rede.'),
                    textAlign: TextAlign.center,
                  ),
                  const SizedBox(height: 16),
                  if (!isAuthError)
                    ElevatedButton(
                        onPressed: controller.fetchOutputs,
                        child: Text(tr('Tentar novamente'))),
                  TextButton.icon(
                    onPressed: () async {
                      // No navegador, refazer é esquecer a chave e voltar à tela do QR.
                      if (kIsWeb) {
                        await (await SharedPreferences.getInstance())
                            .remove(Constants.apiKeyKey);
                      }
                      if (!context.mounted) return;
                      Navigator.of(context)
                          .pushReplacementNamed(kIsWeb ? '/' : '/config');
                    },
                    icon: const Icon(Icons.qr_code_scanner),
                    label: Text(tr('Refazer pareamento')),
                  ),
                ],
              ),
            ),
          );
        }

        // Toggle System UI based on fullscreen state
        if (kIsWeb) setBrowserFullscreen(controller.isFullscreen.value);
        if (controller.isFullscreen.value) {
          SystemChrome.setEnabledSystemUIMode(SystemUiMode.immersiveSticky);
        } else {
          SystemChrome.setEnabledSystemUIMode(SystemUiMode.edgeToEdge);
        }

        if (deckOnly) {
          return StreamDeckPage(
            onToggleFullscreen: controller.toggleFullscreen,
            isFullscreen: controller.isFullscreen.value,
          );
        }

        return PageView(
          controller: _pageController,
          onPageChanged: (index) {
            setState(() {
              selectedIndex = index;
            });
            SharedPreferences.getInstance().then((sp) => sp.setInt(_tabKey, index));
          },
          children: [
            Column(
              children: [
                if (err.isNotEmpty)
                  MaterialBanner(
                    leading: const Icon(Icons.wifi_off),
                    content: Text(tr(
                        'O PC não respondeu. Confira se o DroidDeck está aberto nele.')),
                    actions: [
                      TextButton(
                        onPressed: controller.fetchOutputs,
                        child: Text(tr('Tentar novamente')),
                      ),
                    ],
                  ),
                Expanded(
                  child: AudioPage(
                    controller: controller,
                    isFullscreen: controller.isFullscreen.value,
                    onToggleFullscreen: controller.toggleFullscreen,
                  ),
                ),
              ],
            ),
            StreamDeckPage(
              onToggleFullscreen: controller.toggleFullscreen,
              isFullscreen: controller.isFullscreen.value,
            ),
          ],
        );
      });
    final hideNav = Watch((context) {
        if (controller.isFullscreen.value ||
            ThemeController.instance.deckOnly.value) {
          return const SizedBox.shrink();
        }
        return BottomNavigationBar(
          items: <BottomNavigationBarItem>[
            BottomNavigationBarItem(
              icon: const Icon(Icons.tune),
              label: tr('Áudio'),
            ),
            BottomNavigationBarItem(
              icon: const Icon(Icons.grid_view),
              label: tr('Deck'),
            ),
          ],
          currentIndex: selectedIndex,
          onTap: _goToTab,
        );
      });
    // Celular deitado: a navegação vai para uma faixa lateral (o Android recomenda o
    // "rail" quando falta altura), e o deck ganha a altura que a barra de baixo comia.
    final size = MediaQuery.sizeOf(context);
    final landscape = size.width > size.height && size.height < 600;
    if (!landscape) {
      return Scaffold(body: body, bottomNavigationBar: hideNav);
    }
    return Scaffold(
      body: Row(
        children: [
          Watch((context) {
            if (controller.isFullscreen.value ||
                ThemeController.instance.deckOnly.value) {
              return const SizedBox.shrink();
            }
            return SafeArea(
              right: false,
              child: NavigationRail(
                selectedIndex: selectedIndex,
                onDestinationSelected: _goToTab,
                labelType: NavigationRailLabelType.all,
                destinations: [
                  NavigationRailDestination(
                    icon: const Icon(Icons.tune),
                    label: Text(tr('Áudio')),
                  ),
                  NavigationRailDestination(
                    icon: const Icon(Icons.grid_view),
                    label: Text(tr('Deck')),
                  ),
                ],
              ),
            );
          }),
          Expanded(child: body),
        ],
      ),
    );
  }

  void _goToTab(int val) {
    _pageController.animateToPage(
      val,
      duration: const Duration(milliseconds: 300),
      curve: Curves.easeInOut,
    );
    setState(() => selectedIndex = val);
  }
}
