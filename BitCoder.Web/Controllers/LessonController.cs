using BitCoder.Core;
using BitCoder.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace BitCoder.Web.Controllers;

/// <summary>
/// Страница пошагового разбора. Сервер ничего не помнит между запросами:
/// алфавит, режим, ввод и номер шага приходят в адресе страницы, а разбор строится заново.
/// Поэтому обновление страницы показывает тот же шаг, а смена данных (форма без step) начинает с нуля.
/// </summary>
public sealed class LessonController : Controller
{
    // GET /lesson?a=АБВГД&mode=enc&in=ГАДБ&step=3&ans=011
    public IActionResult Index(
        string? a,
        string? mode,
        [FromQuery(Name = "in")] string? input,
        string? step,
        string? ans)
    {
        // 1. Пустой адрес — показываем основной пример.
        if (a is null && mode is null && input is null)
        {
            a = Presets.Main.Alphabet;
            mode = ModeCodes.ToCode(Presets.Main.Mode);
            input = Presets.Main.Input;
        }

        string modeCode = mode == ModeCodes.Decode ? ModeCodes.Decode : ModeCodes.Encode;
        var form = new DataFormModel { Alphabet = a ?? "", ModeCode = modeCode, Input = input ?? "" };

        // 2. Проверка ввода на сервере. Любая ошибка — сообщение и форма, без разбора.
        if (mode is not null && mode != ModeCodes.Encode && mode != ModeCodes.Decode)
            return View(new LessonViewModel { Form = form, Error = "Неизвестный режим: выберите «Закодировать» или «Расшифровать»." });

        if (!CodeTable.TryCreate(a, out var table, out string error))
            return View(new LessonViewModel { Form = form, Error = error });

        Mode m = ModeCodes.FromCode(modeCode);
        string cleaned;
        bool inputOk = m == Mode.Encode
            ? InputRules.TryParseMessage(table, input, out cleaned, out error)
            : InputRules.TryParseBits(input, out cleaned, out error);
        if (!inputOk)
            return View(new LessonViewModel { Form = form, Error = error });

        // 3. Расчёты: полностью в BitCoder.Core.
        Lesson lesson = LessonBuilder.Build(table, m, cleaned);

        // 4. Номер шага из адреса: не число или вне диапазона — ближайший допустимый.
        int last = lesson.Steps.Count - 1;
        int stepIndex = int.TryParse(step, out int parsed) ? Math.Clamp(parsed, 0, last) : 0;
        Step current = lesson.Steps[stepIndex];

        // 5. Ответ на вопрос шага (если он есть и ответ пришёл).
        QuestionResult? verdict = null;
        string? answer = ans is { Length: > 40 } ? ans[..40] : ans;
        if (answer is not null && current.Question is { } question)
            verdict = question.Check(answer);

        // 6. Ссылки для кнопок.
        string alphabet = table.AlphabetText;
        string StepUrl(int s) => Url.LessonUrl(alphabet, m, cleaned, s);

        string? reverseUrl = null;
        if (stepIndex == last && lesson.Result is not null)
        {
            reverseUrl = m == Mode.Encode
                ? Url.LessonUrl(alphabet, Mode.Decode, lesson.Result)
                : Url.LessonUrl(alphabet, Mode.Encode, lesson.Result);
        }

        return View(new LessonViewModel
        {
            Form = new DataFormModel { Alphabet = alphabet, ModeCode = modeCode, Input = cleaned },
            Lesson = lesson,
            StepIndex = stepIndex,
            Step = current,
            FirstUrl = StepUrl(0),
            PrevUrl = stepIndex > 0 ? StepUrl(stepIndex - 1) : null,
            NextUrl = stepIndex < last ? StepUrl(stepIndex + 1) : null,
            ReverseUrl = reverseUrl,
            QuestionAnswer = answer,
            QuestionResult = verdict,
        });
    }
}
