using System.Diagnostics.CodeAnalysis;

namespace BitCoder.Core;

/// <summary>
/// Таблица равномерных двоичных кодов для небольшого алфавита.
/// Код символа — его номер в алфавите (с нуля), записанный в двоичной системе
/// и дополненный нулями слева до длины <see cref="Bits"/>.
/// </summary>
public sealed class CodeTable
{
    public const int MinSymbols = 4;
    public const int MaxSymbols = 6;

    private readonly char[] _symbols;

    private CodeTable(char[] symbols)
    {
        _symbols = symbols;
        Bits = BitsFor(symbols.Length);
    }

    /// <summary>Символы алфавита в порядке номеров.</summary>
    public IReadOnlyList<char> Symbols => _symbols;

    /// <summary>Алфавит одной строкой (для адреса страницы и подписей).</summary>
    public string AlphabetText => new(_symbols);

    /// <summary>Число символов N.</summary>
    public int Count => _symbols.Length;

    /// <summary>Длина кода k в битах.</summary>
    public int Bits { get; }

    /// <summary>Сколько разных кодов длины k существует: 2^k.</summary>
    public int Capacity => 1 << Bits;

    /// <summary>
    /// Наименьшее k, при котором 2^k ≥ n. Считается целым циклом, без Math.Log2,
    /// чтобы не зависеть от погрешностей дробных чисел.
    /// </summary>
    public static int BitsFor(int symbolCount)
    {
        if (symbolCount < 1)
            throw new ArgumentOutOfRangeException(nameof(symbolCount), "Нужен хотя бы один символ.");

        int bits = 0;
        int capacity = 1;
        while (capacity < symbolCount)
        {
            capacity *= 2;
            bits++;
        }
        return bits;
    }

    /// <summary>
    /// Проверяет введённый алфавит и строит таблицу. Правила: от 4 до 6 разных букв;
    /// регистр не важен; пробелы, запятые и точки с запятой игнорируются.
    /// При ошибке возвращает false и понятное сообщение.
    /// </summary>
    public static bool TryCreate(
        string? alphabet,
        [NotNullWhen(true)] out CodeTable? table,
        out string error)
    {
        table = null;
        string cleaned = InputRules.StripSeparators(alphabet).ToUpperInvariant();

        if (cleaned.Length == 0)
        {
            error = $"Введите алфавит: от {MinSymbols} до {MaxSymbols} разных букв.";
            return false;
        }

        for (int i = 0; i < cleaned.Length; i++)
        {
            if (!char.IsLetter(cleaned[i]))
            {
                error = $"В алфавите допустимы только буквы, а «{InputRules.SymbolAt(cleaned, i)}» — не буква.";
                return false;
            }
        }

        var seen = new HashSet<char>();
        foreach (char c in cleaned)
        {
            if (!seen.Add(c))
            {
                error = $"Буква «{c}» повторяется: у каждого символа должен быть свой код.";
                return false;
            }
        }

        if (cleaned.Length < MinSymbols || cleaned.Length > MaxSymbols)
        {
            error = $"В алфавите {Ru.Count(cleaned.Length, "символ", "символа", "символов")}, " +
                    $"а нужно от {MinSymbols} до {MaxSymbols}.";
            return false;
        }

        table = new CodeTable(cleaned.ToCharArray());
        error = "";
        return true;
    }

    /// <summary>Создаёт таблицу из заведомо верной строки (для готовых примеров). Иначе — исключение.</summary>
    public static CodeTable Create(string alphabet)
    {
        if (!TryCreate(alphabet, out var table, out var error))
            throw new ArgumentException(error, nameof(alphabet));
        return table;
    }

    /// <summary>Код символа с номером <paramref name="index"/>, например «011».</summary>
    public string Code(int index)
    {
        if (index < 0 || index >= Capacity)
            throw new ArgumentOutOfRangeException(nameof(index));
        return Convert.ToString(index, 2).PadLeft(Bits, '0');
    }

    /// <summary>Номер символа в алфавите или -1, если такого символа нет.</summary>
    public int IndexOfSymbol(char symbol) => Array.IndexOf(_symbols, symbol);

    /// <summary>
    /// Номер символа, которому выдан этот код, или -1: длина группы не равна k,
    /// в ней не только 0 и 1, либо код свободен (символа с таким номером нет).
    /// </summary>
    public int IndexOfCode(string group)
    {
        if (group.Length != Bits) return -1;
        foreach (char c in group)
            if (c != '0' && c != '1') return -1;

        int value = Convert.ToInt32(group, 2);
        return value < Count ? value : -1;
    }

    /// <summary>Коды длины k, которым не досталось символа (например, 101, 110, 111 для 5 символов).</summary>
    public IReadOnlyList<string> UnusedCodes()
    {
        var result = new List<string>();
        for (int i = Count; i < Capacity; i++)
            result.Add(Code(i));
        return result;
    }
}
