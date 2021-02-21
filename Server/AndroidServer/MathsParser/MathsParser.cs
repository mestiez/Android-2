using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace MathsParser
{
    public static class MathsParser
    {
        public static double Parse(string input)
        {
            var tokens = Lexer.Analyse(input).ToImmutableList();
            var output = new Queue<Token>();
            var operators = new Stack<OperatorToken>();

            foreach (var item in tokens)
            {
                if (item is ValueToken)
                    output.Enqueue(item);
                else if (item is FunctionToken i)
                    operators.Push(i);
                else if (item is OperatorToken o)
                {
                    switch (o.Type)
                    {
                        case TokenType.OpenPar:
                            operators.Push(o);
                            break;
                        case TokenType.ClosePar:
                            {
                                OperatorToken top;

                                while (operators.TryPeek(out top) && top.Type != TokenType.OpenPar)
                                    output.Enqueue(operators.Pop());

                                if (operators.TryPeek(out top) && top.Type == TokenType.OpenPar)
                                    operators.Pop();

                                if (operators.TryPeek(out top) && top.Type == TokenType.Function)
                                    output.Enqueue(operators.Pop());
                            }
                            break;
                        default:
                            {
                                while (operators.TryPeek(out var top) && top.Type != TokenType.OpenPar && top.Predence >= o.Predence)
                                    output.Enqueue(operators.Pop());

                                operators.Push(o);
                            }
                            break;
                    }
                }
            }

            while (operators.TryPop(out var r))
                output.Enqueue(r);


            var numbers = new Stack<double>();
            while (output.TryDequeue(out var v))
            {
                if (v is ValueToken n)
                    numbers.Push(n.Value);
                else if (v is FunctionToken f)
                {
                    try
                    {
                        PerformFunction(numbers, f);
                    }
                    catch (Exception)
                    {
                        throw new Exception("Syntax error");
                    }
                }
                else if (v is OperatorToken o)
                {
                    var rhs = numbers.Pop();
                    numbers.TryPop(out double lhs);

                    switch (o.Type)
                    {
                        case TokenType.Addition:
                            numbers.Push(lhs + rhs);
                            break;
                        case TokenType.Subtraction:
                            numbers.Push(lhs - rhs);
                            break;
                        case TokenType.Multiplication:
                            numbers.Push(lhs * rhs);
                            break;
                        case TokenType.Division:
                            if (rhs < float.Epsilon)
                                throw new NaNException();
                            numbers.Push(lhs / rhs);
                            break;
                    }
                }
            }

            if (numbers.Count != 1)
                throw new Exception("Syntax error");

            var result = numbers.Pop();
            if (double.IsNaN(result))
                throw new NaNException();
            if (double.IsInfinity(result))
                throw new InfinityException();
            return result;
        }

        private static void PerformFunction(Stack<double> numbers, FunctionToken f)
        {
            switch (f.Value)
            {
                case "lerp":
                case "mix":
                    {
                        var t = numbers.Pop();
                        var b = numbers.Pop();
                        var a = numbers.Pop();
                        numbers.Push(a * (1 - t) + b * t);
                    }
                    break;
                case "atan2":
                    {
                        var rhs = numbers.Pop();
                        var lhs = numbers.Pop();
                        numbers.Push(Math.Atan2(rhs, lhs));
                    }
                    break;
                case "max":
                    {
                        var rhs = numbers.Pop();
                        var lhs = numbers.Pop();
                        numbers.Push(Math.Max(rhs, lhs));
                    }
                    break;
                case "min":
                    {
                        var rhs = numbers.Pop();
                        var lhs = numbers.Pop();
                        numbers.Push(Math.Min(rhs, lhs));
                    }
                    break;
                case "sq":
                case "square":
                    {
                        var vv = numbers.Pop();
                        numbers.Push(Math.Pow(vv, 2));
                    }
                    break;
                case "pow":
                    {
                        var rhs = numbers.Pop();
                        var lhs = numbers.Pop();
                        numbers.Push(Math.Pow(lhs, rhs));
                    }
                    break;
                case "saturate":
                    {
                        var vv = numbers.Pop();
                        numbers.Push(Math.Clamp(vv, 0, 1d));
                    }
                    break;
                case "round":
                    {
                        var vv = numbers.Pop();
                        numbers.Push(Math.Round(vv));
                    }
                    break;
                case "log":
                    {
                        var vv = numbers.Pop();
                        numbers.Push(Math.Log10(vv));
                    }
                    break;
                case "logn":
                    {
                        var rhs = numbers.Pop();
                        var lhs = numbers.Pop();
                        numbers.Push(Math.Log(lhs, rhs));
                    }
                    break;
                case "ceil":
                    {
                        var vv = numbers.Pop();
                        numbers.Push(Math.Ceiling(vv));
                    }
                    break;
                case "floor":
                    {
                        var vv = numbers.Pop();
                        numbers.Push(Math.Floor(vv));
                    }
                    break;
                case "exp":
                    {
                        var vv = numbers.Pop();
                        numbers.Push(Math.Exp(vv));
                    }
                    break;
                case "sqrt":
                    {
                        var vv = numbers.Pop();
                        numbers.Push(Math.Sqrt(vv));
                    }
                    break;
                case "sin":
                case "sine":
                    {
                        var vv = numbers.Pop();
                        numbers.Push(vv % Math.PI == 0 ? 0 : Math.Sin(vv));
                    }
                    break;
                case "cos":
                case "cosine":
                    {
                        var vv = numbers.Pop();
                        numbers.Push(Math.Cos(vv));
                    }
                    break;
                case "tan":
                case "tangent":
                    {
                        var vv = numbers.Pop();
                        numbers.Push(Math.Tan(vv));
                    }
                    break;
                case "tanh":
                    {
                        var vv = numbers.Pop();
                        numbers.Push(Math.Tanh(vv));
                    }
                    break;
                case "atan":
                    {
                        var vv = numbers.Pop();
                        numbers.Push(Math.Atan(vv));
                    }
                    break;
                case "atanh":
                    {
                        var vv = numbers.Pop();
                        numbers.Push(Math.Atanh(vv));
                    }
                    break;
                case "frac":
                case "fract":
                    {
                        var vv = numbers.Pop();
                        numbers.Push(vv - Math.Floor(vv));
                    }
                    break;
            }
        }
    }

    public class InfinityException : Exception
    {
        public override string Message => "Result was infinity";
    }

    public class NaNException : Exception
    {
        public override string Message => "Result was NaN";
    }
}
