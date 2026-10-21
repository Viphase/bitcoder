using BitCoder.Core;
using BitCoder.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace BitCoder.Web.Controllers;

/// <summary>Задача для самостоятельного решения: условие, проверка, объяснение ошибки, повторная попытка.</summary>
public sealed class PracticeController : Controller
{
    // GET /practice?id=1&ans=100001000011100
    public IActionResult Index(string? id, string? ans)
    {
        int taskId = int.TryParse(id, out int parsed) ? parsed : 1;
        PracticeTask task = PracticeTask.Get(taskId);

        string? answer = ans is { Length: > 120 } ? ans[..120] : ans;
        PracticeResult? result = answer is null ? null : task.Check(answer);

        return View(new PracticeViewModel
        {
            Task = task,
            AllTasks = PracticeTask.All,
            Answer = answer,
            Result = result,
        });
    }
}
