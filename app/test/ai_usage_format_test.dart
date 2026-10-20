import 'package:droiddeck/src/stream_deck/widgets/info_deck_button.dart';
import 'package:flutter_test/flutter_test.dart';

void main() {
  test('compactCount abrevia em português', () {
    expect(compactCount(950), '950');
    expect(compactCount(1234), '1,2 mil');
    expect(compactCount(186000000), '186 mi');
    expect(compactCount(1087064877), '1,1 bi');
    expect(compactCount(2000000), '2 mi');
  });

  test('resetLabel mostra só a hora hoje e o dia nos outros', () {
    final now = DateTime(2026, 10, 20, 15);
    expect(resetLabel(DateTime(2026, 10, 20, 17, 39, 59, 900), now: now), '17:40');
    expect(resetLabel(DateTime(2026, 10, 26, 10), now: now), 'seg 10:00');
  });
}
