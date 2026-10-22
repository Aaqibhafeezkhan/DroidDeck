import 'package:droiddeck/core/core.dart';
import 'package:droiddeck/src/services/signalr_service.dart';
import 'package:droiddeck/src/stream_deck/widgets/usage_bars.dart';
import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';

void main() {
  const gb = 1024 * 1024 * 1024;

  test('formatBytes mostra GB e TB no formato do idioma', () {
    AppLanguage.choice.value = 'pt';
    expect(formatBytes(931 * gb), '931 GB');
    expect(formatBytes(2.5 * gb), '2,5 GB');
    expect(formatBytes(1863 * gb), '1,8 TB');
    expect(formatBytes(300 * 1024 * 1024), '300 MB');
    AppLanguage.choice.value = 'en';
    expect(formatBytes(1863 * gb), '1.8 TB');
    AppLanguage.choice.value = 'pt';
  });

  test('DiskStat lê o JSON do PC e calcula o uso', () {
    final s = SystemStats.fromJson({
      'cpuUsage': 10,
      'disks': [
        {'name': 'C:', 'label': '', 'totalBytes': 100 * gb, 'freeBytes': 25 * gb},
      ],
    });
    expect(s.disks.single.name, 'C:');
    expect(s.disks.single.usedPercent, closeTo(75, 0.01));
    // PC antigo, sem "disks": lista vazia.
    expect(SystemStats.fromJson({'cpuUsage': 1}).disks, isEmpty);
  });

  testWidgets('UsageBars deixa de fora o que não cabe no botão', (tester) async {
    final rows = [for (var i = 0; i < 6; i++) UsageRow('D$i:', 50)];
    await tester.pumpWidget(MaterialApp(
      home: Center(
        child: SizedBox(
          width: 80,
          height: 80,
          child: UsageBars(icon: Icons.storage, title: 'Discos', rows: rows),
        ),
      ),
    ));
    expect(tester.takeException(), isNull);
    expect(find.text('D0:'), findsOneWidget);
    expect(find.text('D5:'), findsNothing);
  });

  testWidgets('Linha só de texto (rede) não tem barra', (tester) async {
    await tester.pumpWidget(MaterialApp(
      home: Center(
        child: SizedBox(
          width: 80,
          height: 160,
          child: UsageBars(icon: Icons.memory, title: 'PC', rows: const [
            UsageRow('CPU', 20),
            UsageRow.text('Rede', '14 KB/s'),
            UsageRow.custom(Text('↓ 1.2 MB/s  ↑ 300 KB/s')),
          ]),
        ),
      ),
    ));
    expect(tester.takeException(), isNull);
    expect(find.text('14 KB/s'), findsOneWidget);
    expect(find.byType(LinearProgressIndicator), findsOneWidget);
  });
}
