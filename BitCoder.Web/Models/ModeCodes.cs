using BitCoder.Core;

namespace BitCoder.Web.Models;

/// <summary>Перевод режима в короткий код для адреса страницы и обратно: enc / dec.</summary>
public static class ModeCodes
{
    public const string Encode = "enc";
    public const string Decode = "dec";

    public static string ToCode(Mode mode) => mode == Mode.Encode ? Encode : Decode;

    public static Mode FromCode(string code) => code == Decode ? Mode.Decode : Mode.Encode;
}
