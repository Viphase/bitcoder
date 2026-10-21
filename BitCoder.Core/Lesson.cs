namespace BitCoder.Core;

/// <summary>Готовый разбор: таблица, колонки сообщения, список шагов и итог.</summary>
public sealed class Lesson
{
    public Lesson(
        CodeTable table,
        Mode mode,
        string input,
        IReadOnlyList<Piece> pieces,
        IReadOnlyList<Step> steps,
        string? result)
    {
        Table = table;
        Mode = mode;
        Input = input;
        Pieces = pieces;
        Steps = steps;
        Result = result;
    }

    public CodeTable Table { get; }
    public Mode Mode { get; }

    /// <summary>Сообщение (кодирование) или строка бит (расшифровка) после нормализации.</summary>
    public string Input { get; }

    /// <summary>Колонки сообщения: по одной на символ (кодирование) или на группу бит (расшифровка).</summary>
    public IReadOnlyList<Piece> Pieces { get; }

    public IReadOnlyList<Step> Steps { get; }

    /// <summary>Итог (строка бит или текст). null, если по дороге была ошибка.</summary>
    public string? Result { get; }

    public Piece? FirstError => Pieces.FirstOrDefault(p => p.IsError);
}
