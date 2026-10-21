using System.Diagnostics.CodeAnalysis;

namespace BitCoder.Core;

/// <summary>
/// Правила проверки пользовательского ввода. Вызываются на сервере при каждом запросе,
/// потому что адрес страницы можно набрать вручную и подставить что угодно.
/// </summary>
public static class InputRules
{
    /// <summary>Самое длинное сообщение для кодирования (в символах).</summary>
    public const int MaxMessageLength = 24;

    /// <summary>Самая длинная строка бит для расшифровки.</summary>
    public const int MaxBitsLength = 96;

    /// <summary>Символ в позиции i для сообщения об ошибке; эмодзи и другие «двойные» символы берутся целиком.</summary>
    public static string SymbolAt(string text, int i) =>
        char.IsHighSurrogate(text[i]) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1])
            ? text.Substring(i, 2)
            : text[i].ToString();

    /// <summary>Убирает пробелы и разделители (запятая, точка с запятой). null считается пустой строкой.</summary>
    public static string StripSeparators(string? raw)
    {
        if (string.IsNullOrEmpty(raw)) return "";
        var sb = new System.Text.StringBuilder(raw.Length);
        foreach (char c in raw)
        {
            if (char.IsWhiteSpace(c) || c == ',' || c == ';') continue;
            sb.Append(c);
        }
        return sb.ToString();
    }

    /// <summary>Сообщение для кодирования: непустое, не длиннее лимита, только символы алфавита.</summary>
    public static bool TryParseMessage(
        CodeTable table,
        string? raw,
        out string message,
        out string error)
    {
        message = StripSeparators(raw).ToUpperInvariant();

        if (message.Length == 0)
        {
            error = "Введите сообщение для кодирования.";
            return false;
        }
        if (message.Length > MaxMessageLength)
        {
            error = $"Сообщение слишком длинное: {message.Length} символов, а можно не больше {MaxMessageLength}.";
            return false;
        }
        for (int i = 0; i < message.Length; i++)
        {
            if (table.IndexOfSymbol(message[i]) < 0)
            {
                error = $"В сообщении есть «{SymbolAt(message, i)}», а в алфавите ({table.AlphabetText}) такого символа нет.";
                return false;
            }
        }
        error = "";
        return true;
    }

    /// <summary>Строка бит для расшифровки: непустая, не длиннее лимита, только 0 и 1.</summary>
    public static bool TryParseBits(string? raw, out string bits, out string error)
    {
        bits = StripSeparators(raw);

        if (bits.Length == 0)
        {
            error = "Введите строку из нулей и единиц.";
            return false;
        }
        if (bits.Length > MaxBitsLength)
        {
            error = $"Строка слишком длинная: {bits.Length} бит, а можно не больше {MaxBitsLength}.";
            return false;
        }
        for (int i = 0; i < bits.Length; i++)
        {
            if (bits[i] != '0' && bits[i] != '1')
            {
                error = $"В строке бит допустимы только 0 и 1, а «{SymbolAt(bits, i)}» — нет.";
                return false;
            }
        }
        error = "";
        return true;
    }
}
