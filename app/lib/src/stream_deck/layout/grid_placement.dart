import 'package:droiddeck/core/core.dart';

/// Onde cada botão fica na grade, com os botões grandes (que ocupam várias casas).
class GridPlacement {
  final DeckButton button;
  final int row;
  final int column;
  final int rowSpan;
  final int columnSpan;

  const GridPlacement(this.button, this.row, this.column, this.rowSpan, this.columnSpan);
}

class GridLayout {
  final int rows;
  final int columns;
  final List<GridPlacement> placements;

  /// Casas cobertas por algum botão ("linha,coluna").
  final Set<String> covered;

  GridLayout._(this.rows, this.columns, this.placements, this.covered);

  static String key(int row, int column) => '$row,$column';

  bool isFree(int row, int column) => !covered.contains(key(row, column));

  /// Botões fora da grade somem; o tamanho é cortado na borda. Um botão cujo canto cai
  /// dentro de um botão grande anterior (lendo de cima para baixo) fica escondido, e um
  /// botão grande não cobre casas já ocupadas: ele encolhe até caber.
  factory GridLayout.of(List<DeckButton> buttons, int rows, int columns) {
    final sorted = [...buttons]
      ..sort((a, b) => a.row != b.row ? a.row.compareTo(b.row) : a.column.compareTo(b.column));
    final covered = <String>{};
    final placements = <GridPlacement>[];
    // Os botões de uma casa só reservam o lugar primeiro: um vizinho grande não os engole.
    final anchors = {for (final b in sorted) key(b.row, b.column)};
    for (final b in sorted) {
      if (b.row < 0 || b.column < 0 || b.row >= rows || b.column >= columns) continue;
      if (covered.contains(key(b.row, b.column))) continue;
      var colSpan = b.columnSpan.clamp(1, columns - b.column);
      var rowSpan = b.rowSpan.clamp(1, rows - b.row);
      bool blocked(int r, int c) =>
          (r != b.row || c != b.column) &&
          (covered.contains(key(r, c)) || anchors.contains(key(r, c)));
      // Encolhe na largura e depois na altura até não pegar casa de outro botão.
      while (colSpan > 1 &&
          [for (var r = b.row; r < b.row + rowSpan; r++) blocked(r, b.column + colSpan - 1)].any((x) => x)) {
        colSpan--;
      }
      while (rowSpan > 1 &&
          [for (var c = b.column; c < b.column + colSpan; c++) blocked(b.row + rowSpan - 1, c)].any((x) => x)) {
        rowSpan--;
      }
      for (var r = b.row; r < b.row + rowSpan; r++) {
        for (var c = b.column; c < b.column + colSpan; c++) {
          covered.add(key(r, c));
        }
      }
      placements.add(GridPlacement(b, b.row, b.column, rowSpan, colSpan));
    }
    return GridLayout._(rows, columns, placements, covered);
  }
}
