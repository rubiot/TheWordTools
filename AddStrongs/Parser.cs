using System;
using System.Collections.Generic;
using System.Linq;

namespace TheWord
{
  class Parser
  {
    static TokenType[] TagTokens = new[] { TokenType.Strong, TokenType.Morpho };

    Tokenizer tokenizer = new Tokenizer();

    public void Parse(string verse)
    {
      tokenizer.Text = verse;
    }

    public IEnumerable<Syntagm> GetSyntagms()
    {
      var tokenList = tokenizer.GetTokens().ToList<Token>();

      for (int i = 0; i < tokenList.Count; i++)
      {
        switch (tokenList[i].Type)
        {
          case TokenType.Meta:
            yield return new Syntagm() { Text = tokenList[i].Value, Displayable = false };
            break;
          case TokenType.Space:
            yield return new Syntagm() { Text = tokenList[i].Value };
            break;
          case TokenType.Word:
            Syntagm syntagm = new Syntagm() { Text = tokenList[i].Value };
            int t;
            for (t = i + 1; t < tokenList.Count && TagTokens.Contains(tokenList[t].Type); t++)
              syntagm.AddTag(tokenList[t].Value);
            i = t - 1;
            yield return syntagm;
            break;
          case TokenType.Strong:
          case TokenType.Morpho:
            throw new Exception("Strong and morphology tags must follow a word");
          default:
            throw new Exception($"Unhandled token type: {tokenList[i].Type}");
        }
      }
    }
  }
}
