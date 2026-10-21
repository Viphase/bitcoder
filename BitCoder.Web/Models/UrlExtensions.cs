using BitCoder.Core;
using Microsoft.AspNetCore.Mvc;

namespace BitCoder.Web.Models;

/// <summary>Ссылки на страницу разбора. Всё состояние разбора живёт в параметрах адреса.</summary>
public static class UrlExtensions
{
    public static string LessonUrl(this IUrlHelper url, string alphabet, Mode mode, string input, int step = 0) =>
        url.Action("Index", "Lesson", new { a = alphabet, mode = ModeCodes.ToCode(mode), @in = input, step })!;

    public static string LessonUrl(this IUrlHelper url, Preset preset) =>
        url.LessonUrl(preset.Alphabet, preset.Mode, preset.Input);
}
