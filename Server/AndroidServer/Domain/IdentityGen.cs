using shortid;
using shortid.Configuration;

namespace AndroidServer.Domain
{
    /// <summary>
    /// Identity generator
    /// </summary>
    public struct IdentityGen
    {
        /// <summary>
        /// Generate a unique ID string
        /// </summary>
        public static string Generate()
        {
            return ShortId.Generate(new GenerationOptions
            {
                Length = 14,
                UseSpecialCharacters = false,
                UseNumbers = true
            });
        }
    }
}
