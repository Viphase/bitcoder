using BitCoder.Core;
using BitCoder.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace BitCoder.Web.Controllers;

/// <summary>Стартовая страница и страница ошибки.</summary>
public sealed class HomeController : Controller
{
    public IActionResult Index()
    {
        Preset demo = Presets.Intro;
        var table = CodeTable.Create(demo.Alphabet);

        var model = new HomeViewModel
        {
            Presets = Presets.All,
            Demo = LessonBuilder.Build(table, demo.Mode, demo.Input),
            Form = new DataFormModel
            {
                Alphabet = Presets.Main.Alphabet,
                ModeCode = ModeCodes.ToCode(Presets.Main.Mode),
                Input = Presets.Main.Input,
            },
        };
        return View(model);
    }

    /// <summary>Единая страница ошибок: 404 и необработанные исключения.</summary>
    public IActionResult Error(int? code)
    {
        int status = code ?? Response.StatusCode;
        if (status < 400) status = 500;

        var model = status == 404
            ? new ErrorViewModel
            {
                Code = 404,
                Title = "Такой страницы нет",
                Text = "Проверьте адрес или начните с главной страницы.",
            }
            : new ErrorViewModel
            {
                Code = status,
                Title = "Что-то пошло не так",
                Text = "Сервер не смог обработать запрос. Вернитесь на главную и попробуйте ещё раз.",
            };

        Response.StatusCode = model.Code;
        return View(model);
    }
}
