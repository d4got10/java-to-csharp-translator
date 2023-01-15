using System.Globalization;
using System.Text.RegularExpressions;
using DataStructures;

namespace LexicalAnalysis
{

    public class Lexer
    {
        private string GetLine(string text, int lineNo)
        {
            string[] lines = text.Split('\n');
            return lines[lineNo];
        }
        
        private int CountLines(string text)
        {
            int count = 0;
            if (!string.IsNullOrEmpty(text))
            {
                count = text.Length - text.Replace("\n", string.Empty).Length;

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
        public IEnumerable<Token> Parse(string text)
        {
            int numLines = CountLines(text);
            List<Token> lexems = new List<Token>();
            bool isStringValueParseNow=false;
            for(int l = 0; l < numLines; l++)
            {
                var currentLine = GetLine(text, l);
                string pattern = @"([\u0022\u002F\u003D\u0025\u005B\u005D*() ,;+><!&|-}{])";
                var currentLineWords = Regex.Split(currentLine, pattern);
                string[,] words = new string[currentLineWords.Length, 3];
                for(var i = 0 ; i<currentLineWords.Length; i++)
                {
                    words[i,0] = currentLineWords[i];
                }
                int offset = 0;
                int k=0;
                for (var i = 0; i < words.GetLength(0); i++)
                {
                    if (words[i,0] == "\"" && isStringValueParseNow)
                    {
                        isStringValueParseNow = false;
                        words[k,0] += words[i,0];
                        words[i,0] = "";
                    }
                    if (words[i,0] == "\"" && !isStringValueParseNow)
                    {
                        isStringValueParseNow = true;
                        k = i;
                        words[i, 1] = l.ToString();
                        words[i, 2] = offset.ToString();
                    }
                    if (isStringValueParseNow && words[i,0]!="\"")
                    {
                        words[k,0] += words[i,0];
                        words[i,0] = "";
                        words[i, 1] = l.ToString();
                        words[i, 2] = offset.ToString();
                    }
                    
                    if (!isStringValueParseNow && words[i,0]!="\"" && words[i,0].Length>0)
                    {
                        words[i, 0] = words[i,0];
                        words[i, 1] = l.ToString();
                        words[i, 2] = offset.ToString();
                    }

                    if (i==0 && isStringValueParseNow)
                    {
                        words[i, 0] = "\"" + words[k, 0];
                    }
                    
                    if (isStringValueParseNow && i == words.GetLength(0) - 1)
                    {
                        words[k, 0] = words[k, 0]+"\"";
                    }

                    offset += words[i,0].Length;
                }

                for (int w = 0; w < words.GetLength(0); w++)
                {
                    if (words[w,0] != "" && words[w,0] != " ")
                    {
                        Token token = new Token(ChooseTokenTypeByValue(words[w, 0].Trim()), words[w, 0].Trim(), Int32.Parse(words[w, 2])+1, Int32.Parse(words[w,1])+1);
                        lexems.Add(token);
                    }
                }

                SplitIdentifiersByDots(ref lexems);
                ConcatinateMultilineStrings(ref lexems);

            }
            
            for (var i = 0; i < lexems.Count; i++)
            {
                if (i < lexems.Count - 1 && lexems[i].Value.Length>0 && lexems[i+1].Value.Length>0 && IsTwoSymbolOperator(lexems[i].Value, lexems[i+1].Value))
                {
                    lexems[i].Value = lexems[i].Value+lexems[i+1].Value;
                    lexems[i + 1].Value = "";
                }
            }

            lexems.RemoveAll(v => v.Value == "");
            lexems.RemoveAll(v => v.Value == " ");
            lexems.RemoveAll(v => v.Value == "\n");
            return lexems;
        }


        private void ConcatinateMultilineStrings(ref List<Token> lexems)
        {
            int j = 0;
            while (j<lexems.Count-1)
            {
                if (lexems[j].Type==TokenType.Value && lexems[j+1].Type==TokenType.Value && (lexems[j].Value[0]=='\"') && (lexems[j+1].Value[0]=='\"'))
                {
                    lexems[j + 1].Value = lexems[j + 1].Value.Remove(0, 1);
                    lexems[j].Value = lexems[j].Value.Remove(lexems[j].Value.Length - 1) + "\n" +
                                      lexems[j + 1].Value.Remove(0, 0);
                    lexems.RemoveAt(j+1);
                    j++;
                }
                else
                {
                    j++;
                }
                    
            }
        }
        private void SplitIdentifiersByDots(ref List<Token> lexems)
        {
            var j = 0;
            while (j<lexems.Count)
            {
                if (lexems[j].Type==TokenType.Identifier && lexems[j].Value.Contains(".") && !Double.TryParse(lexems[j].Value.Trim(), NumberStyles.Float, new CultureInfo("en-us"), out double m))
                {
                    var prevToken = lexems[j];
                    var line = prevToken.LineNumber;
                    var column = prevToken.ColumnNumber;
                    var identifiers = Regex.Split(lexems[j].Value, @"([\u002E])");
                    lexems[j].Value = identifiers[0];
                    for (var d = 1; d<identifiers.Length; d++)
                    {
                        column += identifiers[d].Length;
                        lexems.Insert(j+d, new Token(ChooseTokenTypeByValue(identifiers[d]), identifiers[d], column, line));
                    }
                    j=j+identifiers.Length;
                }
                else
                {
                    j++;
                }
            }
        }
        
        private bool IsTwoSymbolOperator(string a, string b)
        {
            foreach (var op in TokenTypeHashSets.Operators)
            {
                if (op.Length==2 && op[0]==a[0] && op[1]==b[0])
                {
                    return true;
                }
            }
            return false;
        }

        private TokenType ChooseTokenTypeByValue(string val)
        {
            if (TokenTypeHashSets.Operators.Contains(val))
            {
                return TokenType.Operator;
            }

            if (TokenTypeHashSets.AccessModifiers.Contains(val))
            {
                return TokenType.AccessModifier;
            }

            if (TokenTypeHashSets.DataTypes.Contains(val))
            {
                return TokenType.Type;
            }

            if (TokenTypeHashSets.KeyWords.Contains(val))
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

            if (val=="true" || val=="false" || (val.Length>=2 && (val[0]=='\"'||val[^1]=='\"')) ||  Double.TryParse(val.Trim(), NumberStyles.Float, new CultureInfo("en-us"), out double m))
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