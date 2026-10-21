namespace BitCoder.Core;

public enum StepPhase
{
    Alphabet,
    Length,
    Codes,
    Message,
    Process,
    Result,
    Error,
}

/// <summary>
/// Один шаг разбора: что рассматриваем, что делаем, какие значения, что изменилось
/// (четыре обязательных пункта из задания) и состояние картинки на этом шаге.
/// Страница ничего не вычисляет — только рисует эти поля.
/// </summary>
public sealed record Step
{
    public required StepPhase Phase { get; init; }
    public required string Title { get; init; }

    // Текст шага
    public required string Looking { get; init; }
    public required string Doing { get; init; }
    public required IReadOnlyList<string> Values { get; init; }
    public required string Changed { get; init; }

    // Состояние картинки
    /// <summary>У скольких первых карточек алфавита уже показан код.</summary>
    public int CodesShown { get; init; }

    /// <summary>Подсвеченная карточка алфавита или -1.</summary>
    public int HiSymbol { get; init; } = -1;

    /// <summary>Показывать ли серые карточки свободных кодов.</summary>
    public bool ShowUnused { get; init; }

    /// <summary>Расшифровка: строка уже разрезана на группы.</summary>
    public bool Split { get; init; }

    /// <summary>Сколько колонок сообщения уже обработано целиком.</summary>
    public int Done { get; init; }

    /// <summary>Номер колонки, с которой работаем сейчас, или -1.</summary>
    public int Active { get; init; } = -1;

    /// <summary>Показан ли результат в активной колонке (false — пока ждём ответа на вопрос).</summary>
    public bool Reveal { get; init; } = true;

    /// <summary>Результат, накопленный к этому шагу (строка бит или текст).</summary>
    public string Output { get; init; } = "";

    public bool IsError { get; init; }

    /// <summary>Вопрос на этом шаге или null.</summary>
    public Question? Question { get; init; }
}
