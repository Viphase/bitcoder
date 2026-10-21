namespace BitCoder.Core;

public enum QuestionKind
{
    /// <summary>Ответ — целое число.</summary>
    Number,

    /// <summary>Ответ — группа из нулей и единиц.</summary>
    Bits,

    /// <summary>Ответ — одна буква.</summary>
    Symbol,
}

public enum Verdict
{
    /// <summary>Ответ не получилось прочитать (пусто, не число, не 0/1). Попыткой не считается.</summary>
    InputError,
    Correct,
    Wrong,
}

public sealed record QuestionResult(Verdict Verdict, string Message);

/// <summary>
/// Вопрос внутри разбора. Правильный ответ и проверка живут здесь, в расчётной части:
/// страница только показывает вопрос и вердикт.
/// </summary>
public sealed class Question
{
    private readonly Func<string, string> _hintForWrongAnswer;

    public Question(
        int number,
        string text,
        QuestionKind kind,
        string answer,
        string explanation,
        Func<string, string> hintForWrongAnswer)
    {
        Number = number;
        Text = text;
        Kind = kind;
        Answer = answer;
        Explanation = explanation;
        _hintForWrongAnswer = hintForWrongAnswer;
    }

    public int Number { get; }
    public string Text { get; }
    public QuestionKind Kind { get; }

    /// <summary>Правильный ответ в нормализованном виде (число, 0/1 или буква в верхнем регистре).</summary>
    public string Answer { get; }

    /// <summary>Объяснение, которое показывается после верного ответа.</summary>
    public string Explanation { get; }

    /// <summary>Подсказка для поля ввода.</summary>
    public string Placeholder => Kind switch
    {
        QuestionKind.Number => "число",
        QuestionKind.Bits => "например, 010",
        _ => "одна буква",
    };

    /// <summary>Проверяет ответ пользователя. Верный — с объяснением, неверный — с подсказкой без готового ответа.</summary>
    public QuestionResult Check(string? raw)
    {
        string given = InputRules.StripSeparators(raw).ToUpperInvariant();

        if (given.Length == 0)
            return new QuestionResult(Verdict.InputError, "Введите ответ в поле и нажмите «Проверить».");
        if (given.Length > 12)
            return new QuestionResult(Verdict.InputError, "Ответ слишком длинный.");

        switch (Kind)
        {
            case QuestionKind.Number:
                if (!int.TryParse(given, out int number))
                    return new QuestionResult(Verdict.InputError, "Здесь нужен ответ целым числом, например 3.");
                given = number.ToString();
                break;

            case QuestionKind.Bits:
                if (given.Any(c => c != '0' && c != '1'))
                    return new QuestionResult(Verdict.InputError, "Код записывается только нулями и единицами, например 010.");
                break;

            default:
                if (given.Length != 1 || !char.IsLetter(given[0]))
                    return new QuestionResult(Verdict.InputError, "Нужна одна буква.");
                break;
        }

        return given == Answer
            ? new QuestionResult(Verdict.Correct, "Верно! " + Explanation)
            : new QuestionResult(Verdict.Wrong, "Пока неверно. " + _hintForWrongAnswer(given));
    }
}
