namespace BitCoder.Core;

public enum Mode
{
    /// <summary>Сообщение → строка бит.</summary>
    Encode,

    /// <summary>Строка бит → сообщение.</summary>
    Decode,
}

public enum PieceError
{
    None,

    /// <summary>При кодировании: символа нет в алфавите.</summary>
    UnknownSymbol,

    /// <summary>При расшифровке: группа нужной длины, но такой код никому не выдан.</summary>
    UnknownCode,

    /// <summary>При расшифровке: в конце осталась группа короче k бит.</summary>
    IncompleteGroup,
}

/// <summary>
/// Одна «колонка» сообщения: символ и соответствующая ему группа бит.
/// Ошибки — это обычный результат (<see cref="Error"/>), а не исключение:
/// для этой темы «код отсутствует» и «неполная группа» — штатные случаи.
/// </summary>
/// <param name="Index">Номер символа в алфавите или -1.</param>
/// <param name="Symbol">Символ (при расшифровке null, если расшифровать не удалось).</param>
/// <param name="Group">Группа бит (при кодировании пустая, если символ неизвестен).</param>
/// <param name="Error">Что пошло не так.</param>
public sealed record Piece(int Index, char? Symbol, string Group, PieceError Error)
{
    public bool IsError => Error != PieceError.None;
}
