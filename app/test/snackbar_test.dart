import 'package:droiddeck/core/core.dart';
import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';

/// "Botão removido." com "Desfazer", como o configurador mostra ao limpar um botão.
Future<void> _showUndo(WidgetTester tester, Size screen) async {
  tester.view.physicalSize = screen;
  tester.view.devicePixelRatio = 1;
  addTearDown(tester.view.reset);
  await tester.pumpWidget(MaterialApp(
    theme: ThemeData(snackBarTheme: const SnackBarThemeData(
      behavior: SnackBarBehavior.floating,
      insetPadding: EdgeInsets.fromLTRB(16, 0, 88, 16),
    )),
    builder: narrowSnackBars,
    home: Scaffold(
      body: Builder(
        builder: (context) => TextButton(
          onPressed: () => ScaffoldMessenger.of(context).showSnackBar(SnackBar(
            content: const Text('Botão removido.'),
            duration: const Duration(seconds: 6),
            persist: false,
            action: SnackBarAction(label: 'Desfazer', onPressed: () {}),
          )),
          child: const Text('limpar'),
        ),
      ),
    ),
  ));
  await tester.tap(find.text('limpar'));
  await tester.pumpAndSettle();
}

/// Largura do cartão do aviso (sem a margem em volta).
double _cardWidth(WidgetTester tester) => tester
    .getSize(find.descendant(of: find.byType(SnackBar), matching: find.byType(Material)).first)
    .width;

void main() {
  testWidgets('no navegador o aviso tem largura de aviso e some sozinho', (tester) async {
    await _showUndo(tester, const Size(1280, 800));
    expect(_cardWidth(tester), 480);
    await tester.pump(const Duration(seconds: 7));
    await tester.pumpAndSettle();
    expect(find.text('Botão removido.'), findsNothing);
  });

  testWidgets('no celular o aviso continua da largura da tela', (tester) async {
    await _showUndo(tester, const Size(400, 800));
    // Margem do tema: 16 à esquerda e 88 à direita (o botão de tela cheia fica livre).
    expect(_cardWidth(tester), 400 - 16 - 88);
  });
}
