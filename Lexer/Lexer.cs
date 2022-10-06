namespace LexicalAnalysis
{
    public static class Lexer
    {
        /// <summary>
        /// Parse specified text for a collection of tokens
        /// </summary>
        /// <param name="text">Text to parse</param>
        /// <returns>Collection of tokens(strings)</returns>
        public static IEnumerable<string> Parse(string text)
        {
            var words = text.Split(" ");
            for (int i = 0; i < words.Length; i++)
            {
                words[i] = words[i].Trim();
                if (!ReservedWords.Contains(words[i]))
                {
                    words[i] = "ID " + words[i];
                }
            }
            return words;
        }

        private static readonly HashSet<string> ReservedWords = new()
        {
            "for",
            "int",
            "(",
            ")",
            "=",
            "<",
            "++",
            ";"
        };
    }
}