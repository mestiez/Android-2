using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MathsParser
{
    public class Lexer
    {
        private static readonly Random rand = new Random();

        public static List<Token> Analyse(string input)
        {
            var tokens = new List<Token>();

            int i = 0;

            while (true)
            {
                var eaten = EatInput(input, i, out var token);
                if (token != null)
                {
                    tokens.Add(token);
                    //Console.WriteLine("{0}\tadded as {1}", input.Substring(i, eaten).Trim(), token.Type);
                }
                i += eaten;

                if (i >= input.Length)
                    break;
            }

            return tokens;
        }

        private static int EatInput(string input, int startIndex, out Token token)
        {
            Token lastToken = null;
            string current = string.Empty;
            int eaten = 0;

            for (int i = startIndex; i < input.Length; i++)
            {
                current += input[i];
                current = current.Trim();

                if (GetToken(current, out var t))
                {
                    lastToken = t;
                    eaten = i - startIndex + 1;
                }
            }

            if (lastToken != null)
            {
                token = lastToken;
                return eaten;
            }

            token = default;
            return 1;
        }

        private static bool GetToken(string input, out Token token)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                token = null;
                return false;
            }

            switch (input)
            {
                case "+":
                    token = new OperatorToken(TokenType.Addition, 2);
                    return true;
                case "-":
                    token = new OperatorToken(TokenType.Subtraction, 2);
                    return true;
                case "*":
                    token = new OperatorToken(TokenType.Multiplication, 3);
                    return true;
                case "/":
                    token = new OperatorToken(TokenType.Division, 3);
                    return true;
                case "(":
                    token = new OperatorToken(TokenType.OpenPar, 2);
                    return true;
                case ")":
                    token = new OperatorToken(TokenType.ClosePar, 2);
                    return true;
            }

            if (!input.StartsWith('-') && !input.StartsWith('+') && double.TryParse(input, System.Globalization.NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var d))
            {
                token = new ValueToken(d);
                return true;
            }

            if (input.All(c => char.IsLetter(c)))
            {
                switch (input)
                {
                    case "inf":
                    case "infinity":
                        token = new ValueToken(double.PositiveInfinity);
                        break;
                    case "e":
                        token = new ValueToken(Math.E);
                        break;
                    case "tau":
                    case "τ":
                        token = new ValueToken(Math.Tau);
                        break;
                    case "φ":
                        token = new ValueToken(1.6180339887498948482d);
                        break;
                    case "pi":
                    case "π":
                        token = new ValueToken(Math.PI);
                        break;                   
                    case "rand":
                    case "random":
                        token = new ValueToken(rand.NextDouble());
                        break;
                    default:
                        token = new FunctionToken(input);
                        break;
                }

                return true;
            }

            token = null;
            return false;
        }
    }
}
