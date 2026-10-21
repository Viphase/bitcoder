using System.Text.Json;
using BitCoder.Core;

// Мини-набор проверок. Для каждой проверки печатается «ожидание» и «факт»,
// эту таблицу можно переносить в отчёт о проверках.

var results = new List<(string Name, string Expected, string Actual, bool Ok)>();

void Case(string name, string expected, string actual) =>
    results.Add((name, expected, actual, expected == actual));

string Show(IEnumerable<Piece> pieces) =>
    string.Join(" ", pieces.Select(p => p.IsError ? $"[{p.Group}:{p.Error}]" : $"{p.Symbol}={p.Group}"));

CodeTable Table(string alphabet) => CodeTable.Create(alphabet);

// ===== 5 обязательных проверок из задания =====

// 1. Обычные данные
var t4 = Table("АБВГ");
Case("1. Обычные данные: АБВГ, «БАВ» → строка бит",
     "010010",
     Coder.JoinGroups(Coder.Encode(t4, "БАВ")));
Case("1б. Обычные данные: расшифровка обратно",
     "БАВ",
     Coder.JoinSymbols(Coder.Decode(t4, "010010")));

// 2. Особый случай
var t5 = Table("АБВГД");
Case("2. Особый случай: АБВГД, 000111 → А, затем неизвестный код 111",
     "А=000 [111:UnknownCode]",
     Show(Coder.Decode(t5, "000111")));
Case("2б. Особый случай: АБВГД, 0110 → Г, затем неполная группа",
     "Г=011 [0:IncompleteGroup]",
     Show(Coder.Decode(t5, "0110")));

// 3. Неправильный ввод
string Try(string? alphabet) =>
    CodeTable.TryCreate(alphabet, out _, out string e) ? "ok" : e;

Case("3. Неправильный ввод: алфавит из 2 букв",
     "В алфавите 2 символа, а нужно от 4 до 6.", Try("АБ"));
Case("3б. Неправильный ввод: 7 букв",
     "В алфавите 7 символов, а нужно от 4 до 6.", Try("АБВГДЕЖ"));
Case("3в. Неправильный ввод: повтор буквы",
     "Буква «А» повторяется: у каждого символа должен быть свой код.", Try("ААБВ"));
Case("3г. Неправильный ввод: цифра в алфавите",
     "В алфавите допустимы только буквы, а «1» — не буква.", Try("АБ1Г"));
Case("3д. Неправильный ввод: эмодзи в алфавите",
     "В алфавите допустимы только буквы, а «😀» — не буква.", Try("АБВ😀"));
Case("3е. Неправильный ввод: пустой алфавит",
     "Введите алфавит: от 4 до 6 разных букв.", Try(null));

string TryMsg(string? msg) =>
    InputRules.TryParseMessage(t4, msg, out _, out string e) ? "ok" : e;
Case("3ж. Неправильный ввод: чужая буква в сообщении",
     "В сообщении есть «Д», а в алфавите (АБВГ) такого символа нет.", TryMsg("АБД"));
Case("3з. Неправильный ввод: пустое сообщение",
     "Введите сообщение для кодирования.", TryMsg("  "));

string TryBits(string? bits) =>
    InputRules.TryParseBits(bits, out _, out string e) ? "ok" : e;
Case("3и. Неправильный ввод: двойка в строке бит",
     "В строке бит допустимы только 0 и 1, а «2» — нет.", TryBits("0120"));

// 4. Изменение параметров
Case("4. Изменение параметров: бит на символ при 4, 5, 6, 8, 9 символах",
     "2 3 3 3 4",
     string.Join(" ", new[] { 4, 5, 6, 8, 9 }.Select(CodeTable.BitsFor)));
Case("4б. Изменение параметров: добавили Д — код Г удлинился",
     "11 → 011",
     $"{Table("АБВГ").Code(3)} → {Table("АБВГД").Code(3)}");
Case("4в. Изменение параметров: число шагов разбора для АБВГ и АБВГД («БАВ»)",
     "разное",
     LessonBuilder.Build(t4, Mode.Encode, "БАВ").Steps.Count ==
     LessonBuilder.Build(t5, Mode.Encode, "БАВ").Steps.Count ? "одинаковое" : "разное");

// 5. Повторный запуск
string Fingerprint(Lesson l) => JsonSerializer.Serialize(l.Steps);
Case("5. Повторный запуск: те же данные дают тот же разбор",
     "True",
     (Fingerprint(LessonBuilder.Build(t5, Mode.Decode, "000111")) ==
      Fingerprint(LessonBuilder.Build(t5, Mode.Decode, "000111"))).ToString());

// ===== Дополнительные проверки =====

// Туда-обратно для всех сообщений длины до 4
{
    bool allOk = true;
    foreach (string alphabet in new[] { "АБВГ", "АБВГД", "АБВГДЕ" })
    {
        var t = Table(alphabet);
        var syms = t.Symbols.ToArray();
        for (int len = 1; len <= 4; len++)
        {
            int total = (int)Math.Pow(syms.Length, len);
            for (int n = 0; n < total; n++)
            {
                var chars = new char[len];
                int x = n;
                for (int i = 0; i < len; i++) { chars[i] = syms[x % syms.Length]; x /= syms.Length; }
                string msg = new(chars);
                string back = Coder.JoinSymbols(Coder.Decode(t, Coder.JoinGroups(Coder.Encode(t, msg))));
                if (back != msg) allOk = false;
            }
        }
    }
    Case("6. Туда-обратно: Decode(Encode(m)) = m для всех сообщений до 4 символов", "True", allOk.ToString());
}

Case("7. Регистр и пробелы в алфавите не важны: «а б, в;г» = АБВГ",
     "АБВГ",
     CodeTable.TryCreate("а б, в;г", out var lower, out _) ? lower.AlphabetText : "ошибка");

Case("8. Свободные коды для 5 символов",
     "101 110 111",
     string.Join(" ", t5.UnusedCodes()));
Case("8б. Свободных кодов нет для 4 символов",
     "0", t4.UnusedCodes().Count.ToString());

// Каждый разбор заканчивается ожидаемым шагом, а вопросов не меньше двух
foreach (var preset in Presets.All)
{
    var table = Table(preset.Alphabet);
    var lesson = LessonBuilder.Build(table, preset.Mode, preset.Input);
    int questions = lesson.Steps.Count(s => s.Question is not null);
    Case($"9. Пример «{preset.Id}»: вопросов в разборе не меньше двух", "True", (questions >= 2).ToString());
    bool last = lesson.Steps[^1].IsError == (lesson.Result is null);
    Case($"9б. Пример «{preset.Id}»: последний шаг соответствует итогу", "True", last.ToString());
}

Case("10. Итог основного примера «ГАДБ»", "011000100001",
     LessonBuilder.Build(t5, Mode.Encode, "ГАДБ").Result ?? "нет");

// Вопросы
{
    var lesson = LessonBuilder.Build(t5, Mode.Encode, "ГАДБ");
    var qs = lesson.Steps.Where(s => s.Question is not null).Select(s => s.Question!).ToList();
    Case("11. Вопрос 1: ответ 3 верный", "Correct", qs[0].Check("3").Verdict.ToString());
    Case("11б. Вопрос 1: ответ 2 неверный", "Wrong", qs[0].Check("2").Verdict.ToString());
    Case("11в. Вопрос 1: «abc» не число", "InputError", qs[0].Check("abc").Verdict.ToString());
    Case("11г. Вопрос 2: код Г = 011", "Correct", qs[1].Check("011").Verdict.ToString());
    Case("11д. Вопрос 2: чужой код", "Wrong", qs[1].Check("100").Verdict.ToString());
    Case("11е. Вопрос 3: 4 × 3 = 12", "Correct", qs[2].Check("12").Verdict.ToString());
}

// Задачи для самостоятельного решения
{
    var t1 = PracticeTask.Get(1);
    var t2 = PracticeTask.Get(2);
    Case("12. Задача 1: правильный ответ", "100001000011100", t1.Expected);
    Case("12б. Задача 1: верный ответ принимается (с пробелами)", "Correct", t1.Check("100 001 000 011 100").Verdict.ToString());
    Case("12в. Задача 1: два бита на символ — объяснение про нехватку кодов", "Wrong",
         t1.Check("1000100111").Verdict.ToString());
    Case("12г. Задача 1: буква вместо бит — ошибка ввода", "InputError", t1.Check("МЕЛОМ").Verdict.ToString());
    Case("12д. Задача 1: ошибка в одной группе указана по номеру", "True",
         t1.Check("100001000011101").Details.Any(d => d.StartsWith("Символ №5")).ToString());
    Case("12е. Задача 2: правильный ответ", "ТЕСТ", t2.Expected);
    Case("12ж. Задача 2: неверный ответ", "Wrong", t2.Check("ТЕСА").Verdict.ToString());
    Case("12з. Задача 2: число вместо букв — ошибка ввода", "InputError", t2.Check("123").Verdict.ToString());
    Case("12и. Неизвестный номер задачи — первая задача", "1", PracticeTask.Get(99).Id.ToString());
}

// ===== Вывод =====
int failed = 0;
Console.OutputEncoding = System.Text.Encoding.UTF8;
foreach (var (name, expected, actual, ok) in results)
{
    Console.WriteLine($"{(ok ? "OK  " : "FAIL")} {name}");
    if (!ok)
    {
        failed++;
        Console.WriteLine($"       ожидание: {expected}");
        Console.WriteLine($"       факт:     {actual}");
    }
}
Console.WriteLine();
Console.WriteLine($"Проверок: {results.Count}, успешно: {results.Count - failed}, неудачно: {failed}");
return failed == 0 ? 0 : 1;
