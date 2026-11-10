using System.Globalization;
namespace Math_functions.Services;
public class ExpressionEvaluator
{
    private string _s = "";
    private int _i;
    private bool _deg;

    public double Evaluate(string input, bool degrees)
    {
        _s = input
            .Replace(" ", "")
            .Replace("×", "*").Replace("÷", "/").Replace("−", "-")
            .Replace("π", "pi").Replace("√", "sqrt")
            .ToLowerInvariant();
        _i = 0;
        _deg = degrees;

        if (_s.Length == 0) throw new FormatException("Порожній вираз");
        var value = ParseExpr();
        if (_i < _s.Length) throw new FormatException($"Неочікуваний символ '{_s[_i]}'");
        if (double.IsNaN(value) || double.IsInfinity(value))
            throw new ArithmeticException("Результат не визначений");
        return value;
    }

    private char Peek() => _i < _s.Length ? _s[_i] : '\0';

    private bool Eat(char c)
    {
        if (Peek() != c) return false;
        _i++;
        return true;
    }

    private double ParseExpr()
    {
        var v = ParseTerm();
        while (true)
        {
            if (Eat('+')) v += ParseTerm();
            else if (Eat('-')) v -= ParseTerm();
            else return v;
        }
    }

    private double ParseTerm()
    {
        var v = ParseUnary();
        while (true)
        {
            if (Eat('*')) v *= ParseUnary();
            else if (Eat('/'))
            {
                var d = ParseUnary();
                if (d == 0) throw new DivideByZeroException("Ділення на нуль");
                v /= d;
            }
            else if (Eat('%'))
            {
                var d = ParseUnary();
                if (d == 0) throw new DivideByZeroException("Ділення на нуль");
                v %= d;
            }
            else return v;
        }
    }

    private double ParseUnary()
    {
        if (Eat('+')) return ParseUnary();
        if (Eat('-')) return -ParseUnary();
        return ParsePower();
    }

    private double ParsePower()
    {
        var b = ParsePostfix();
        return Eat('^') ? Math.Pow(b, ParseUnary()) : b;
    }

    private double ParsePostfix()
    {
        var v = ParsePrimary();
        while (Eat('!')) v = Factorial(v);
        return v;
    }

    private double ParsePrimary()
    {
        if (Eat('('))
        {
            var v = ParseExpr();
            if (!Eat(')')) throw new FormatException("Немає закриваючої дужки");
            return v;
        }

        if (char.IsDigit(Peek()) || Peek() == '.') return ParseNumber();
        if (char.IsLetter(Peek())) return ParseIdentifier();

        throw new FormatException(Peek() == '\0'
            ? "Вираз обірваний"
            : $"Неочікуваний символ '{Peek()}'");
    }

    private double ParseNumber()
    {
        var start = _i;
        while (char.IsDigit(Peek()) || Peek() == '.') _i++;
        var text = _s[start.._i];
        if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var v))
            throw new FormatException($"Некоректне число '{text}'");
        return v;
    }

    private double ParseIdentifier()
    {
        var start = _i;
        while (char.IsLetter(Peek())) _i++;
        var name = _s[start.._i];

        if (!Eat('('))
        {
            return name switch
            {
                "pi" => Math.PI,
                "e" => Math.E,
                _ => throw new FormatException($"Невідоме ім'я '{name}'")
            };
        }

        var args = new List<double> { ParseExpr() };
        while (Eat(',')) args.Add(ParseExpr());
        if (!Eat(')')) throw new FormatException("Немає закриваючої дужки");

        return CallFunction(name, args);
    }

    private double CallFunction(string name, List<double> a)
    {
        void Need(int n)
        {
            if (a.Count != n)
                throw new FormatException($"{name}() приймає аргументів: {n}");
        }

        double ToRad(double x) => _deg ? x * Math.PI / 180 : x;
        double FromRad(double x) => _deg ? x * 180 / Math.PI : x;

        switch (name)
        {
            case "sin": Need(1); return Math.Sin(ToRad(a[0]));
            case "cos": Need(1); return Math.Cos(ToRad(a[0]));
            case "tan":
                Need(1);
                if (Math.Abs(Math.Cos(ToRad(a[0]))) < 1e-12)
                    throw new ArithmeticException("tan не визначений");
                return Math.Tan(ToRad(a[0]));
            case "asin": Need(1); return FromRad(Math.Asin(a[0]));
            case "acos": Need(1); return FromRad(Math.Acos(a[0]));
            case "atan": Need(1); return FromRad(Math.Atan(a[0]));
            case "sinh": Need(1); return Math.Sinh(a[0]);
            case "cosh": Need(1); return Math.Cosh(a[0]);
            case "tanh": Need(1); return Math.Tanh(a[0]);
            case "sqrt":
                Need(1);
                if (a[0] < 0) throw new ArithmeticException("Корінь з від'ємного числа");
                return Math.Sqrt(a[0]);
            case "cbrt": Need(1); return Math.Cbrt(a[0]);
            case "ln":
                Need(1);
                if (a[0] <= 0) throw new ArithmeticException("ln визначений лише для x > 0");
                return Math.Log(a[0]);
            case "log":
                Need(1);
                if (a[0] <= 0) throw new ArithmeticException("log визначений лише для x > 0");
                return Math.Log10(a[0]);
            case "exp": Need(1); return Math.Exp(a[0]);
            case "abs": Need(1); return Math.Abs(a[0]);
            case "floor": Need(1); return Math.Floor(a[0]);
            case "ceil": Need(1); return Math.Ceiling(a[0]);
            case "round": Need(1); return Math.Round(a[0], MidpointRounding.AwayFromZero);
            case "pow": Need(2); return Math.Pow(a[0], a[1]);
            case "root":
                Need(2);
                if (a[1] == 0) throw new ArithmeticException("Степінь кореня не може бути 0");
                return Math.Pow(a[0], 1.0 / a[1]);
            case "min": Need(2); return Math.Min(a[0], a[1]);
            case "max": Need(2); return Math.Max(a[0], a[1]);
            default: throw new FormatException($"Невідома функція '{name}'");
        }
    }

    private static double Factorial(double x)
    {
        if (x < 0 || x != Math.Floor(x))
            throw new ArithmeticException("Факторіал лише для цілих x ≥ 0");
        if (x > 170) throw new ArithmeticException("Занадто велике число для факторіалу");
        double r = 1;
        for (var k = 2; k <= (int)x; k++) r *= k;
        return r;
    }
}