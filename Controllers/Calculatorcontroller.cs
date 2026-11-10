using System.Globalization;
using Math_functions.Services;
using Microsoft.AspNetCore.Mvc;

namespace Math_functions.Controllers;

public record CalcRequest(string? Expression, bool Degrees);

public class CalculatorController : Controller
{
    private readonly ExpressionEvaluator _evaluator = new();

    [HttpGet]
    public IActionResult Index() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Calculate([FromBody] CalcRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Expression))
            return Json(new { ok = false, error = "Введіть вираз" });

        try
        {
            var value = _evaluator.Evaluate(request.Expression, request.Degrees);
            // прибираємо похибку типу 0.1+0.2 = 0.30000000000000004
            var text = Math.Round(value, 12).ToString("G15", CultureInfo.InvariantCulture);
            return Json(new { ok = true, result = text });
        }
        catch (Exception ex) when (ex is FormatException or ArithmeticException)
        {
            return Json(new { ok = false, error = ex.Message });
        }
    }
}