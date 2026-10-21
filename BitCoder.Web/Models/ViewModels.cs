using BitCoder.Core;

namespace BitCoder.Web.Models;

/// <summary>Содержимое формы «свои данные» (она есть на главной и на странице разбора).</summary>
public sealed class DataFormModel
{
    public string Alphabet { get; init; } = "";
    public string ModeCode { get; init; } = ModeCodes.Encode;
    public string Input { get; init; } = "";
}

public sealed class HomeViewModel
{
    public required IReadOnlyList<Preset> Presets { get; init; }

    /// <summary>Разбор для крупной картинки на главной (пример «БАВ»).</summary>
    public required Lesson Demo { get; init; }

    public required DataFormModel Form { get; init; }
}

/// <summary>Всё, что нужно странице разбора. Расчётов здесь нет — только готовые результаты и ссылки.</summary>
public sealed class LessonViewModel
{
    public required DataFormModel Form { get; init; }

    /// <summary>Сообщение об ошибке ввода. Если оно есть, разбора нет.</summary>
    public string? Error { get; init; }

    public Lesson? Lesson { get; init; }
    public int StepIndex { get; init; }
    public Step? Step { get; init; }

    public string? FirstUrl { get; init; }
    public string? PrevUrl { get; init; }
    public string? NextUrl { get; init; }

    /// <summary>Ссылка «обратная операция»: расшифровать полученный результат или закодировать его снова.</summary>
    public string? ReverseUrl { get; init; }

    public string? QuestionAnswer { get; init; }
    public QuestionResult? QuestionResult { get; init; }
}

public sealed class PracticeViewModel
{
    public required PracticeTask Task { get; init; }
    public required IReadOnlyList<PracticeTask> AllTasks { get; init; }
    public string? Answer { get; init; }
    public PracticeResult? Result { get; init; }
}

public sealed class ErrorViewModel
{
    public required int Code { get; init; }
    public required string Title { get; init; }
    public required string Text { get; init; }
}
