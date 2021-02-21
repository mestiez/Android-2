namespace MathsParser
{
    public abstract class Token
    {
        public TokenType Type;

        public Token(TokenType type)
        {
            Type = type;
        }

        public override string ToString()
        {
            return Type.ToString();
        }
    }
}
