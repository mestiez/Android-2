namespace MathsParser
{
    public class ValueToken : Token
    {
        public ValueToken(double val) : base(TokenType.Number)
        {
            Value = val;
        }

        public double Value;

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
