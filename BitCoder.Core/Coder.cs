namespace BitCoder.Core;

/// <summary>Кодирование и расшифровка. Чистые функции: одинаковый ввод — одинаковый результат.</summary>
public static class Coder
{
    /// <summary>Заменяет каждый символ его кодом. Символ не из алфавита даёт Piece с ошибкой.</summary>
    public static IReadOnlyList<Piece> Encode(CodeTable table, string message)
    {
        var pieces = new List<Piece>(message.Length);
        foreach (char c in message)
        {
            int index = table.IndexOfSymbol(c);
            pieces.Add(index < 0
                ? new Piece(-1, c, "", PieceError.UnknownSymbol)
                : new Piece(index, c, table.Code(index), PieceError.None));
        }
        return pieces;
    }

    /// <summary>
    /// Режет строку на группы по k бит слева направо и ищет каждую в таблице.
    /// Результат содержит все группы; ошибки помечены в самих Piece.
    /// </summary>
    public static IReadOnlyList<Piece> Decode(CodeTable table, string bits)
    {
        var pieces = new List<Piece>();
        for (int pos = 0; pos < bits.Length; pos += table.Bits)
        {
            int length = Math.Min(table.Bits, bits.Length - pos);
            string group = bits.Substring(pos, length);

            if (length < table.Bits)
            {
                pieces.Add(new Piece(-1, null, group, PieceError.IncompleteGroup));
                continue;
            }

            int index = table.IndexOfCode(group);
            pieces.Add(index < 0
                ? new Piece(-1, null, group, PieceError.UnknownCode)
                : new Piece(index, table.Symbols[index], group, PieceError.None));
        }
        return pieces;
    }

    /// <summary>Склеивает группы бит в одну строку.</summary>
    public static string JoinGroups(IEnumerable<Piece> pieces) =>
        string.Concat(pieces.Select(p => p.Group));

    /// <summary>Склеивает расшифрованные символы в текст.</summary>
    public static string JoinSymbols(IEnumerable<Piece> pieces) =>
        string.Concat(pieces.Where(p => p.Symbol is not null).Select(p => p.Symbol));
}
