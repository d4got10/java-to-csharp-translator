using System.Globalization;
using System.Text.RegularExpressions;
using DataStructures;

namespace LexicalAnalysis
{

    public static class Lexer
    {
        private static string GetLine(string text, int lineNo)
        {
            string[] lines = text.Split('\n');
            return lines[lineNo];
        }
        
        private static int CountLines(string text)
        {
            int count = 0;
            if (!string.IsNullOrEmpty(text))
            {
                count = text.Length - text.Replace("\n", string.Empty).Length;

                // if the last char of the string is not a newline, make sure to count that line too
                if (text[text.Length - 1] != '\n')
                {
                    ++count;
                }
            }

            return count;
        }
        /// <summary>
        /// Parse specified text for a collection of tokens
        /// </summary>
        /// <param name="text">Text to parse</param>
        /// <returns>Collection of tokens(Token)</returns>
        public static IEnumerable<Token> Parse(string text)
        {
            int numLines = CountLines(text);
            Console.WriteLine(numLines);
            List<Token> allWords = new List<Token>();
            for (int l = 0; l < numLines; l++)
            {
                var currentText = GetLine(text, l);
                string pattern = @"([\u0022\u002F\u003D\u0025\u005B\u005D*() ,;+><!&|-}{])";
                string[,] words = new string[1000, 3];
                var words1 = Regex.Split(currentText, pattern);
                for(var i = 0 ; i<words1.Length; i++)
                {
                    words[i,0] = words1[i];
                }
                int row = 0;
                int k=0;
                bool isNowString=false;

                for (var i = 0; i < words.GetLength(0); i++)
                {
                    if (words[i,0] == "\"" && isNowString)
                    {
                        isNowString = false;
                        words[k,0] += words[i,0];
                        words[i,0] = "";
                    }
                    if (words[i,0] == "\"" && !isNowString)
                    {
                        isNowString = true;
                        k = i;
                        words[i, 1] = l.ToString();
                        words[i, 2] = row.ToString();
                    }
                    if (isNowString && words[i,0]!="\"")
                    {
                        words[k,0] += words[i,0];
                        words[i,0] = "";
                    }

                    var len = 0;
                    if (words[i, 0] != null)
                    {
                        len = words[i,0].Length;
                    }

                    if (!isNowString && words[i,0]!="\"" && words[i,0]!="" && words[i,0]!=" ")
                    {
                        words[i, 0] = words[i,0];
                        words[i, 1] = l.ToString();
                        words[i, 2] = row.ToString();
                    }

                    row += len;
                }

                for (int w = 0; w < words.GetLength(0); w++)
                {
                    if (words[w,0]!=null && words[w,0] != "" && words[w,0] != " ")
                    {
                        Token token = new Token(chooseTokenTypeByValue(words[w, 0].Trim()), words[w, 0].Trim(), Int32.Parse(words[w, 2])+1, Int32.Parse(words[w,1])+1);
                        allWords.Add(token);
                    }
                }

                var j = 0;
                while (j<allWords.Count)
                {
                    if (allWords[j].Type==TokenType.Identifier && allWords[j].Value.Contains(".") && !Double.TryParse(allWords[j].Value.Trim(), NumberStyles.Float, new CultureInfo("en-us"), out double m))
                    {
                        var prevToken = allWords[j];
                        var line = prevToken.LineNumber;
                        var column = prevToken.ColumnNumber;
                        var identifiers = Regex.Split(allWords[j].Value, @"([\u002E])");
                        allWords[j].Value = identifiers[0];
                        for (var d = 1; d<identifiers.Length; d++)
                        {
                            column += identifiers[d].Length;
                            allWords.Insert(j+d, new Token(chooseTokenTypeByValue(identifiers[d]), identifiers[d], column, line));
                        }
                        j=j+identifiers.Length;
                    }
                    else
                    {
                        j++;
                    }
                    
                }
            }
            
            for (var i = 0; i < allWords.Count; i++)
            {
                if (i < allWords.Count - 1 && allWords[i].Value.Length>0 && allWords[i+1].Value.Length>0 && isBinaryOperator(allWords[i].Value, allWords[i+1].Value))
                {
                    allWords[i].Value = allWords[i].Value+allWords[i+1].Value;
                    allWords[i + 1].Value = "";
                }
            }

            allWords.RemoveAll(v => v.Value == "");
            allWords.RemoveAll(v => v.Value == " ");
            allWords.RemoveAll(v => v.Value == "\n");
            return allWords;
        }

        private static bool isBinaryOperator(string a, string b)
        {
            foreach (var op in TokenTypeHashSets.operators)
            {
                if (op.Length==2 && op[0]==a[0] && op[1]==b[0])
                {
                    return true;
                }
            }
            return false;
        }

        private static TokenType chooseTokenTypeByValue(string val)
        {
            if (TokenTypeHashSets.operators.Contains(val))
            {
                return TokenType.Operator;
            }

            if (TokenTypeHashSets.accessModifiers.Contains(val))
            {
                return TokenType.AccessModifier;
            }

            if (TokenTypeHashSets.dataTypes.Contains(val.ToLower()))
            {
                return TokenType.Type;
            }

            if (TokenTypeHashSets.keyWords.Contains(val))
            {
                return TokenType.Keyword;
            }

            if (val=="\u003B")
            {
                return TokenType.Semicolon;
            }

            if (val=="{")
            {
                return TokenType.OpenBracket;
            }

            if (val=="true" || val=="false" || (val.Length>=2 && val[0]=='\"') ||  Double.TryParse(val.Trim(), NumberStyles.Float, new CultureInfo("en-us"), out double m))
            {
                return TokenType.Value;
            }
            
            if (val=="}")
            {
                return TokenType.CloseBracket;
            }
            if (val==",")
            {
                return TokenType.Comma;
            }
            if (val==".")
            {
                return TokenType.Comma;
            }

            return TokenType.Identifier;
        }
    }
}