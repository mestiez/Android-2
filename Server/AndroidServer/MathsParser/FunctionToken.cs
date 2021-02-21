namespace MathsParser
{
    public class FunctionToken : OperatorToken
    {
        public FunctionToken(string val) : base(TokenType.Function, int.MaxValue)
        {
            Value = val;
        }

        public string Value;
    }
}
