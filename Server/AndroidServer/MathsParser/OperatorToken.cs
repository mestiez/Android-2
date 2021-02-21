namespace MathsParser
{
    public class OperatorToken : Token
    {
        public OperatorToken(TokenType type, int predence) : base(type)
        {
            Predence = predence;
        }

        public int Predence;
    }
}
