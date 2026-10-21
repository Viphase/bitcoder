namespace BitCoder.Core;

public enum PracticeVerdict { InputError, Correct, Wrong }

/// <summary>Результат проверки: вердикт, главное сообщение и построчное объяснение ошибки.</summary>
public sealed record PracticeResult(PracticeVerdict Verdict, string Message, IReadOnlyList<string> Details);

/// <summary>
/// Задача для самостоятельного решения. Правильный ответ не хранится «ключом» —
/// он вычисляется теми же функциями (<see cref="Coder"/>), что и разбор,
/// поэтому задача и разбор не могут разойтись.
/// </summary>
public sealed class PracticeTask
{
    private PracticeTask(int id, string title, string statement, CodeTable table, Mode mode, string input)
    {
        Id = id;
        Title = title;
        Statement = statement;
        Table = table;
        Mode = mode;
        Input = input;
        Expected = mode == Mode.Encode
            ? Coder.JoinGroups(Coder.Encode(table, input))
            : Coder.JoinSymbols(Coder.Decode(table, input));
    }

    public int Id { get; }
    public string Title { get; }
    public string Statement { get; }
    public CodeTable Table { get; }
    public Mode Mode { get; }

    /// <summary>Исходное сообщение (кодирование) или строка бит (расшифровка).</summary>
    public string Input { get; }

    /// <summary>Правильный ответ.</summary>
    public string Expected { get; }

    public static IReadOnlyList<PracticeTask> All { get; } =
    [
        new PracticeTask(
            1, "Закодировать сообщение",
            "Алфавит: Л, Е, Т, О, М. Закодируйте сообщение «МЕЛОМ» двоичным кодом одинаковой длины. " +
            "Коды выдаются по порядку: первому символу алфавита — число 0, второму — 1 и так далее; " +
            "число записывается в двоичной системе ровно в k бит, где k — наименьшая возможная длина. " +
            "Ответ запишите одной строкой из нулей и единиц.",
            CodeTable.Create("ЛЕТОМ"), Mode.Encode, "МЕЛОМ"),

        new PracticeTask(
            2, "Расшифровать строку",
            "Алфавит: С, Е, Т, К, А — коды выданы по тому же правилу. " +
            "Расшифруйте строку 010001000010. Ответ запишите буквами.",
            CodeTable.Create("СЕТКА"), Mode.Decode, "010001000010"),
    ];

    /// <summary>Задача по номеру; неизвестный номер даёт первую задачу.</summary>
    public static PracticeTask Get(int id) => All.FirstOrDefault(t => t.Id == id) ?? All[0];

    public PracticeResult Check(string? raw) =>
        Mode == Mode.Encode ? CheckEncode(raw) : CheckDecode(raw);

    // ---------- Кодирование ----------

    private PracticeResult CheckEncode(string? raw)
    {
        if (!InputRules.TryParseBits(raw, out string answer, out string error))
            return new PracticeResult(PracticeVerdict.InputError, error, []);

        int k = Table.Bits;
        var pieces = Coder.Encode(Table, Input);

        if (answer == Expected)
        {
            var good = pieces.Select(p => $"«{p.Symbol}» → {p.Group}").ToList();
            return new PracticeResult(PracticeVerdict.Correct,
                $"Верно! Для {Ru.Count(Table.Count, "символа", "символов", "символов")} нужно {Ru.Count(k, "бит", "бита", "бит")} на символ.",
                good);
        }

        var details = new List<string>();
        int expectedLength = Expected.Length;

        if (answer.Length != expectedLength)
        {
            if (answer.Length % Input.Length == 0)
            {
                int given = answer.Length / Input.Length;
                details.Add(given < k
                    ? $"У вас по {Ru.Count(given, "биту", "бита", "бит")} на символ. Но {Ru.Count(given, "бит", "бита", "бит")} дают только " +
                      $"{Ru.Pow2(given)} = {Ru.Count(1 << given, "код", "кода", "кодов")}, а в алфавите {Table.Count} символов — кодов не хватит."
                    : $"У вас по {Ru.Count(given, "биту", "бита", "бит")} на символ, а достаточно {k}: это наименьшее k, при котором " +
                      $"{Ru.Pow2(k)} = {1 << k} ≥ {Table.Count}.");
            }
            else
            {
                details.Add($"В ответе {Ru.Count(answer.Length, "бит", "бита", "бит")}, а должно быть " +
                            $"{Input.Length} × {k} = {expectedLength}: у каждого из {Input.Length} символов ровно {k} бит.");
            }
            return new PracticeResult(PracticeVerdict.Wrong, "Пока неверно. Длина ответа не сходится.", details);
        }

        for (int i = 0; i < pieces.Count; i++)
        {
            string mine = answer.Substring(i * k, k);
            if (mine == pieces[i].Group) continue;

            int other = Table.IndexOfCode(mine);
            string note = other >= 0
                ? $" (это код «{Table.Symbols[other]}»)"
                : " (такой код никому не выдан)";
            details.Add($"Символ №{i + 1} «{pieces[i].Symbol}»: у вас {mine}{note}, а в таблице у него {pieces[i].Group}.");
        }
        return new PracticeResult(PracticeVerdict.Wrong,
            $"Пока неверно: ошибок в группах — {details.Count}.", details);
    }

    // ---------- Расшифровка ----------

    private PracticeResult CheckDecode(string? raw)
    {
        string answer = InputRules.StripSeparators(raw).ToUpperInvariant();
        if (answer.Length == 0)
            return new PracticeResult(PracticeVerdict.InputError, "Введите ответ буквами.", []);
        foreach (char c in answer)
        {
            if (!char.IsLetter(c))
                return new PracticeResult(PracticeVerdict.InputError, $"В ответе должны быть только буквы, а «{c}» — не буква.", []);
        }

        var pieces = Coder.Decode(Table, Input);

        if (answer == Expected)
        {
            var good = pieces.Select(p => $"{p.Group} → {p.Symbol}").ToList();
            return new PracticeResult(PracticeVerdict.Correct, "Верно! Все группы найдены в таблице.", good);
        }

        var details = new List<string>();

        if (answer.Length != pieces.Count)
        {
            details.Add($"В строке {Ru.Count(Input.Length, "бит", "бита", "бит")}, в группе {Table.Bits}: " +
                        $"{Input.Length} ÷ {Table.Bits} = {pieces.Count}. Групп {pieces.Count}, значит и символов {pieces.Count}, а у вас {answer.Length}.");
            return new PracticeResult(PracticeVerdict.Wrong, "Пока неверно. Число символов не сходится.", details);
        }

        for (int i = 0; i < pieces.Count; i++)
        {
            if (answer[i] == pieces[i].Symbol) continue;
            details.Add($"Группа №{i + 1} ({pieces[i].Group}): у вас «{answer[i]}», но {pieces[i].Group} — это число " +
                        $"{pieces[i].Index}, а под номером {pieces[i].Index} в алфавите стоит «{pieces[i].Symbol}».");
        }
        return new PracticeResult(PracticeVerdict.Wrong,
            $"Пока неверно: ошибок — {details.Count}.", details);
    }
}
