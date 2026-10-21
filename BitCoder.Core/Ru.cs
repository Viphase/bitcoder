namespace BitCoder.Core;

/// <summary>Мелкие помощники для русских подписей: склонение и надстрочные цифры.</summary>
public static class Ru
{
    /// <summary>Выбирает форму слова по числу: 1 символ, 2 символа, 5 символов.</summary>
    public static string Plural(int n, string one, string few, string many)
    {
        int abs = Math.Abs(n);
        int last2 = abs % 100;
        int last1 = abs % 10;
        if (last2 is >= 11 and <= 14) return many;
        if (last1 == 1) return one;
        if (last1 is >= 2 and <= 4) return few;
        return many;
    }

    /// <summary>«3 бита», «5 символов» — число вместе со словом.</summary>
    public static string Count(int n, string one, string few, string many) =>
        $"{n} {Plural(n, one, few, many)}";

    private const string Superscripts = "⁰¹²³⁴⁵⁶⁷⁸⁹";

    /// <summary>Степень двойки с надстрочным показателем: Pow2(3) = «2³».</summary>
    public static string Pow2(int exponent)
    {
        var sb = new System.Text.StringBuilder("2");
        foreach (char digit in exponent.ToString())
            sb.Append(Superscripts[digit - '0']);
        return sb.ToString();
    }
}
