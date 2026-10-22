import 'dart:convert';

import 'package:droiddeck/core/core.dart';
import 'package:droiddeck/src/services/signalr_service.dart';
import 'package:droiddeck/src/stream_deck/widgets/deck_grid_button.dart';
import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:get_it/get_it.dart';

void main() {
  testWidgets('botão Tuya mostra a luz acesa', (tester) async {
    final sr = SignalRService();
    GetIt.I.registerSingleton<SignalRService>(sr);
    addTearDown(GetIt.I.reset);

    // O que o hub manda (JSON de verdade, como chega no celular).
    sr.tuyaState.value = Map<String, dynamic>.from(jsonDecode('''
      {"connected": true, "push": true, "paired": true, "devices": [
        {"id": "dev1", "name": "Luz", "category": "tdq", "online": true,
         "status": {"switch_1": true, "switch_type": "flip"},
         "functions": {"switch_1": {"type": "Boolean", "values": "{}"}}}]}''') as Map);

    final button = DeckButton.fromJson(jsonDecode('''
      {"id": "b1", "row": 0, "column": 0, "label": "Quarto", "activeColor": "#B71C1C",
       "action": {"type": "tuya", "parameters":
         {"operation": "toggle", "deviceId": "dev1", "code": "switch_1"}}}''') as Map<String, dynamic>);

    await tester.pumpWidget(MaterialApp(
      home: SizedBox(
        width: 100,
        height: 100,
        child: DeckGridButton(button: button, onTap: () {}, onLongPress: () {}),
      ),
    ));
    expect(find.byIcon(Icons.toggle_on), findsOneWidget);
  });
}
