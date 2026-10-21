namespace BitCoder.Core;

public enum PresetKind { Intro, Main, Special }

/// <summary>Готовый пример: набор исходных данных, с которого можно начать разбор.</summary>
public sealed record Preset(
    string Id,
    PresetKind Kind,
    string Title,
    string Description,
    string Alphabet,
    Mode Mode,
    string Input);

/// <summary>Три обязательных примера (вводный, основной, особый случай) плюс второй особый случай.</summary>
public static class Presets
{
    public static readonly Preset Intro = new(
        "intro", PresetKind.Intro,
        "Четыре символа — два бита",
        "Самый простой случай: кодов ровно столько, сколько символов, свободных нет. Кодируем слово «БАВ».",
        "АБВГ", Mode.Encode, "БАВ");

    public static readonly Preset Main = new(
        "main", PresetKind.Main,
        "Пять символов — три бита",
        "Двух бит уже мало, нужно три — и три кода остаются свободными. Кодируем «ГАДБ»; расшифровать результат можно в конце разбора.",
        "АБВГД", Mode.Encode, "ГАДБ");

    public static readonly Preset SpecialUnknownCode = new(
        "special-code", PresetKind.Special,
        "Код, которого нет в таблице",
        "Расшифровываем 000111: первая группа — А, а вот 111 не выдан ни одному символу.",
        "АБВГД", Mode.Decode, "000111");

    public static readonly Preset SpecialIncompleteGroup = new(
        "special-tail", PresetKind.Special,
        "Неполная группа в конце",
        "Расшифровываем 0110: после группы 011 остаётся один лишний бит.",
        "АБВГД", Mode.Decode, "0110");

    public static IReadOnlyList<Preset> All { get; } =
        [Intro, Main, SpecialUnknownCode, SpecialIncompleteGroup];
}
