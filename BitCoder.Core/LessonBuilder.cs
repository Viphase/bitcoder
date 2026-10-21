namespace BitCoder.Core;

/// <summary>
/// Превращает таблицу и сообщение в список шагов. Все числа и формулы в текстах шагов
/// берутся из реальных данных, поэтому при смене алфавита или сообщения разбор пересчитывается целиком.
/// </summary>
public static class LessonBuilder
{
    public static Lesson Build(CodeTable table, Mode mode, string input)
    {
        IReadOnlyList<Piece> pieces = mode == Mode.Encode
            ? Coder.Encode(table, input)
            : Coder.Decode(table, input);

        var builder = new StepBuilder(table, mode, input, pieces);
        builder.Run();

        string? result = pieces.Any(p => p.IsError)
            ? null
            : mode == Mode.Encode ? Coder.JoinGroups(pieces) : Coder.JoinSymbols(pieces);

        return new Lesson(table, mode, input, pieces, builder.Steps, result);
    }
}

internal sealed class StepBuilder
{
    private readonly CodeTable _t;
    private readonly Mode _mode;
    private readonly string _input;
    private readonly IReadOnlyList<Piece> _pieces;
    private readonly List<Step> _steps = new();

    // Состояние картинки: меняется по ходу и копируется в каждый шаг.
    private int _codesShown;
    private bool _showUnused;
    private bool _split;
    private int _done;
    private int _active = -1;
    private bool _reveal = true;
    private string _output = "";

    public StepBuilder(CodeTable table, Mode mode, string input, IReadOnlyList<Piece> pieces)
    {
        _t = table;
        _mode = mode;
        _input = input;
        _pieces = pieces;
    }

    public IReadOnlyList<Step> Steps => _steps;

    public void Run()
    {
        AlphabetStep();
        LengthSteps();
        CodeSteps();
        UnusedStep();
        if (_mode == Mode.Encode) EncodeSteps(); else DecodeSteps();
    }

    private void Push(
        StepPhase phase,
        string title,
        string looking,
        string doing,
        IReadOnlyList<string> values,
        string changed,
        int hiSymbol = -1,
        bool isError = false,
        Question? question = null)
    {
        _steps.Add(new Step
        {
            Phase = phase,
            Title = title,
            Looking = looking,
            Doing = doing,
            Values = values,
            Changed = changed,
            CodesShown = _codesShown,
            HiSymbol = hiSymbol,
            ShowUnused = _showUnused,
            Split = _split,
            Done = _done,
            Active = _active,
            Reveal = _reveal,
            Output = _output,
            IsError = isError,
            Question = question,
        });
    }

    // ---------- Общая часть: алфавит → длина кода → таблица ----------

    private static string Symbols(int n) => Ru.Count(n, "символ", "символа", "символов");
    private static string Bits(int n) => Ru.Count(n, "бит", "бита", "бит");

    private static string Codes(int n) => Ru.Count(n, "код", "кода", "кодов");

    /// <summary>Родительный падеж: «из 3 бит», «до 1 бита», «не хватает 2 бит».</summary>
    private static string BitsOf(int n) => Ru.Count(n, "бита", "бит", "бит");

    private void AlphabetStep()
    {
        int n = _t.Count;
        Push(StepPhase.Alphabet, "Алфавит",
            $"Алфавит: {string.Join(", ", _t.Symbols)} — всего {Symbols(n)}.",
            "Каждому символу нужно выдать свой двоичный код. Все коды будут одной длины — такой код называют равномерным.",
            [$"N = {n}"],
            "Пока ни у одного символа кода нет.");
    }

    private void LengthSteps()
    {
        int n = _t.Count;
        int k = _t.Bits;

        Push(StepPhase.Length, "Сколько бит нужно?",
            $"Сколько бит k нужно на один символ, если N = {n}?",
            "Один бит различает 2 варианта, k бит — 2ᵏ разных кодов. " +
            "Кодов должно хватить всем символам: 2ᵏ ≥ N.",
            [$"N = {n}", $"{Ru.Pow2(1)} = 2,  {Ru.Pow2(2)} = 4,  {Ru.Pow2(3)} = 8,  {Ru.Pow2(4)} = 16"],
            "Длина k пока не выбрана — ответьте на вопрос.",
            question: LengthQuestion());

        var lines = new List<string>();
        for (int j = 1; j <= k; j++)
        {
            int cap = 1 << j;
            lines.Add(cap >= n
                ? $"k = {j}: {Ru.Pow2(j)} = {cap} ≥ {n} — хватает"
                : $"k = {j}: {Ru.Pow2(j)} = {cap} < {n} — мало");
        }

        Push(StepPhase.Length, "Выбираем длину кода",
            "Подбираем наименьшее k.",
            "Пробуем k = 1, 2, 3… и останавливаемся, когда 2ᵏ становится не меньше N.",
            lines,
            $"Длина кода k = {k}. Всего {Codes(_t.Capacity)}, а символов {n}.");
    }

    private void CodeSteps()
    {
        int k = _t.Bits;
        for (int i = 0; i < _t.Count; i++)
        {
            char symbol = _t.Symbols[i];
            string binary = Convert.ToString(i, 2);
            string code = _t.Code(i);
            _codesShown = i + 1;

            Push(StepPhase.Codes, $"Код для «{symbol}»",
                $"Символ «{symbol}» — номер {i} в алфавите (счёт идёт с нуля).",
                $"Записываем число {i} в двоичной системе и дополняем нулями слева до {BitsOf(k)}.",
                [$"{i} в двоичной системе: {binary}", $"дополняем до {BitsOf(k)}: {code}"],
                $"У «{symbol}» теперь код {code}.",
                hiSymbol: i);
        }
    }

    private void UnusedStep()
    {
        var unused = _t.UnusedCodes();
        if (unused.Count == 0) return;

        _showUnused = true;
        Push(StepPhase.Codes, "Свободные коды",
            $"Кодов из {BitsOf(_t.Bits)} всего {_t.Capacity}, а символов {_t.Count}.",
            "Коды, которым не досталось символа, остаются свободными.",
            [$"{_t.Capacity} − {_t.Count} = {unused.Count}", $"Свободны: {string.Join(", ", unused)}"],
            "Если такая группа встретится при расшифровке, превратить её в символ нельзя — это будет ошибка.");
    }

    // ---------- Кодирование ----------

    private void EncodeSteps()
    {
        int len = _input.Length;
        int k = _t.Bits;
        _done = 0;
        _active = -1;
        _output = "";

        Push(StepPhase.Message, "Берём сообщение",
            $"Сообщение: {_input} — {Symbols(len)}.",
            "Каждый символ заменяем его кодом из таблицы. Коды идут подряд без разделителей; " +
            "промежутки между группами на экране нужны только для удобства чтения.",
            [$"Длина сообщения: {len}"],
            "Результат пока пустой.");

        for (int i = 0; i < _pieces.Count; i++)
        {
            Piece p = _pieces[i];
            if (p.IsError)
            {
                ErrorStep(i);
                return;
            }

            char c = p.Symbol!.Value;

            if (i == 0)
            {
                _done = 0;
                _active = 0;
                _reveal = false;
                Push(StepPhase.Process, "Ищем код символа",
                    $"Символ №1: «{c}».",
                    "Найдите его в таблице: какой код ему выдан?",
                    [$"«{c}» → ?"],
                    "Ждём ответа: код ещё не записан в результат.",
                    hiSymbol: p.Index,
                    question: EncodeCodeQuestion(p));
            }

            _done = i;
            _active = i;
            _reveal = true;
            _output = Coder.JoinGroups(_pieces.Take(i + 1));
            Push(StepPhase.Process, $"Символ №{i + 1}",
                $"Символ №{i + 1}: «{c}».",
                "Находим символ в таблице и записываем его код в результат.",
                [$"«{c}» → {p.Group}"],
                $"К результату добавлено {p.Group}. Сейчас: {_output}",
                hiSymbol: p.Index);
        }

        _done = _pieces.Count;
        _active = -1;
        string result = Coder.JoinGroups(_pieces);
        _output = result;

        Push(StepPhase.Process, "Считаем длину",
            "Все символы заменены кодами.",
            "Каждый символ занимает ровно k бит. Сколько бит в результате?",
            [$"Символов: {len}", $"Бит на символ: {k}"],
            "Длину результата пока не посчитали — ответьте на вопрос.",
            question: EncodedLengthQuestion());

        Push(StepPhase.Result, "Результат",
            "Все коды записаны подряд.",
            "Склеиваем группы в одну строку.",
            [$"{len} × {k} = {len * k} бит", $"{_input} → {result}"],
            $"Итог: {result}");
    }

    // ---------- Расшифровка ----------

    private void DecodeSteps()
    {
        int len = _input.Length;
        int k = _t.Bits;
        int full = len / k;
        int rest = len % k;
        _done = 0;
        _active = -1;
        _split = false;
        _output = "";

        Push(StepPhase.Message, "Берём строку бит",
            $"Строка из {BitsOf(len)}: {_input}.",
            $"Каждая группа — ровно {Bits(k)}. Режем строку на группы слева направо.",
            rest == 0
                ? [$"{len} ÷ {k} = {full}"]
                : [$"{len} ÷ {k} = {full}, остаток {rest}"],
            "Строка ещё не разрезана.");

        _split = true;
        Push(StepPhase.Message, "Режем на группы",
            $"Отделяем группы по {Bits(k)}.",
            "Идём слева направо и отсчитываем по k бит.",
            [string.Join(" | ", _pieces.Select(p => p.Group))],
            rest == 0
                ? $"Получилось {Ru.Count(full, "группа", "группы", "групп")} по {Bits(k)}."
                : $"Получилось {Ru.Count(full, "полная группа", "полные группы", "полных групп")} " +
                  $"по {Bits(k)} и {Bits(rest)} в конце.");

        for (int i = 0; i < _pieces.Count; i++)
        {
            Piece p = _pieces[i];
            if (p.IsError)
            {
                ErrorStep(i);
                return;
            }

            char s = p.Symbol!.Value;

            if (i == 0)
            {
                _done = 0;
                _active = 0;
                _reveal = false;
                Push(StepPhase.Process, "Ищем символ по коду",
                    $"Группа №1: {p.Group}.",
                    "Найдите этот код в таблице: какому символу он выдан?",
                    [$"{p.Group} → ?"],
                    "Ждём ответа: символ ещё не записан в результат.",
                    question: DecodeSymbolQuestion(p));
            }

            _done = i;
            _active = i;
            _reveal = true;
            _output = Coder.JoinSymbols(_pieces.Take(i + 1));
            Push(StepPhase.Process, $"Группа №{i + 1}",
                $"Группа №{i + 1}: {p.Group}.",
                "Ищем её в таблице и записываем найденный символ.",
                [$"{p.Group} → {s}"],
                $"К тексту добавлена «{s}». Сейчас: {_output}",
                hiSymbol: p.Index);
        }

        _done = _pieces.Count;
        _active = -1;
        string text = Coder.JoinSymbols(_pieces);
        _output = text;

        Push(StepPhase.Process, "Считаем символы",
            "Все группы расшифрованы.",
            $"Каждая группа — один символ. Сколько символов в сообщении?",
            [$"Бит в строке: {len}", $"Бит в группе: {k}"],
            "Число символов пока не найдено — ответьте на вопрос.",
            question: DecodedLengthQuestion());

        Push(StepPhase.Result, "Результат",
            "Все группы превращены в символы.",
            "Склеиваем символы подряд.",
            [$"{len} ÷ {k} = {len / k} {Ru.Plural(len / k, "символ", "символа", "символов")}", $"{_input} → {text}"],
            $"Итог: {text}");
    }

    // ---------- Ошибки (штатные случаи урока) ----------

    private void ErrorStep(int i)
    {
        Piece p = _pieces[i];
        int k = _t.Bits;
        _done = i;
        _active = i;
        _reveal = true;

        switch (p.Error)
        {
            case PieceError.UnknownSymbol:
                Push(StepPhase.Error, "Ошибка: нет символа",
                    $"Символ №{i + 1}: «{p.Symbol}».",
                    "Ищем его в таблице.",
                    [$"«{p.Symbol}» → в алфавите такого символа нет"],
                    "ОШИБКА: у этого символа нет кода. Дальше разбор остановлен — исправьте сообщение или алфавит.",
                    isError: true);
                break;

            case PieceError.UnknownCode:
                Push(StepPhase.Error, "Ошибка: кода нет в таблице",
                    $"Группа №{i + 1}: {p.Group}.",
                    "Ищем её в таблице.",
                    [
                        $"{p.Group} → в таблице такого кода нет",
                        $"Выданы коды от {_t.Code(0)} до {_t.Code(_t.Count - 1)}, свободны: {string.Join(", ", _t.UnusedCodes())}",
                    ],
                    "ОШИБКА: этому коду не выдан ни один символ, расшифровать группу нельзя. " +
                    "Дальше разбор остановлен — исправьте строку и начните сначала.",
                    isError: true);
                break;

            default: // IncompleteGroup
                Push(StepPhase.Error, "Ошибка: неполная группа",
                    $"Остался хвост: {p.Group} — {Bits(p.Group.Length)}.",
                    $"Группа должна состоять из {BitsOf(k)}, а здесь их меньше.",
                    [$"{p.Group.Length} < {k}", $"Не хватает {BitsOf(k - p.Group.Length)}"],
                    "ОШИБКА: неполная группа — символ из неё получить нельзя. " +
                    "Дальше разбор остановлен — допишите недостающие биты или удалите хвост.",
                    isError: true);
                break;
        }
    }

    // ---------- Вопросы ----------

    private Question LengthQuestion()
    {
        int n = _t.Count;
        int k = _t.Bits;
        return new Question(
            1,
            $"Сколько бит k нужно на каждый символ, если в алфавите {Symbols(n)}?",
            QuestionKind.Number,
            k.ToString(),
            $"k бит дают {Ru.Pow2(k)} = {Codes(1 << k)}. При k = {k - 1} получится только {Codes(1 << (k - 1))}, а символов {n}: не хватает.",
            given =>
            {
                int g = int.Parse(given);
                if (g <= 0) return "Код должен состоять хотя бы из одного бита.";
                if (g > 16) return "Это слишком много. Подумайте, сколько кодов реально нужно.";
                if (g < k) return $"{Bits(g)} дают {Ru.Pow2(g)} = {Codes(1 << g)}, а символов {n}: кодов не хватит.";
                return "Так кодов хватит, но код получится длиннее нужного. Есть ли меньшее k, при котором 2ᵏ всё ещё не меньше N?";
            });
    }

    private Question EncodeCodeQuestion(Piece p)
    {
        char c = p.Symbol!.Value;
        int k = _t.Bits;
        string binary = Convert.ToString(p.Index, 2);
        return new Question(
            2,
            $"Какой код у символа «{c}»?",
            QuestionKind.Bits,
            p.Group,
            $"Номер «{c}» в алфавите — {p.Index}; в двоичной системе это {binary}, дополняем до {BitsOf(k)}: {p.Group}.",
            given =>
            {
                if (given.Length != k) return $"Код должен состоять ровно из {BitsOf(k)}, а вы ввели {given.Length}.";
                int other = _t.IndexOfCode(given);
                if (other < 0) return "Такой код никому не выдан. Посмотрите на карточку символа.";
                return $"Это код символа «{_t.Symbols[other]}», а нужен код «{c}».";
            });
    }

    private Question EncodedLengthQuestion()
    {
        int len = _input.Length;
        int k = _t.Bits;
        return new Question(
            3,
            "Сколько всего бит в закодированном сообщении?",
            QuestionKind.Number,
            (len * k).ToString(),
            $"{len} × {k} = {len * k} бит.",
            given => int.Parse(given) == len
                ? "Это число символов, а бит больше: каждый символ занимает k бит."
                : "Каждый символ занимает ровно k бит. Умножьте число символов на k.");
    }

    private Question DecodeSymbolQuestion(Piece p)
    {
        char s = p.Symbol!.Value;
        return new Question(
            2,
            $"Какому символу выдан код {p.Group}?",
            QuestionKind.Symbol,
            s.ToString(),
            $"{p.Group} в двоичной системе — число {p.Index}; символ с номером {p.Index} — «{s}».",
            given =>
            {
                int other = _t.IndexOfSymbol(given[0]);
                if (other < 0) return $"Буквы «{given[0]}» нет в алфавите ({_t.AlphabetText}).";
                return $"У «{given[0]}» код {_t.Code(other)}, а нужен {p.Group}. Найдите карточку с кодом {p.Group}.";
            });
    }

    private Question DecodedLengthQuestion()
    {
        int len = _input.Length;
        int k = _t.Bits;
        return new Question(
            3,
            "Сколько символов в расшифрованном сообщении?",
            QuestionKind.Number,
            (len / k).ToString(),
            $"{len} ÷ {k} = {len / k}: на каждый символ ушло {Bits(k)}.",
            _ => "Каждая группа — один символ. Сколько групп по k бит помещается в строке?");
    }
}
