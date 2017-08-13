using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheWord
{
  public class Syntagm
  {
    public string word;
    public List<string> tags;
  }

  class Parser
  {
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
            break;
          case TokenType.Space:
            yield return new Syntagm() { word = tokenList[i].Value };
            break;
          case TokenType.Word:
            Syntagm syntagm = new Syntagm()
            {
              word = tokenList[i].Value,
              tags = new List<string>()
            };
            int t;
            for (t = i + 1; t < tokenList.Count && new[] { TokenType.Strong, TokenType.Morpho }.Contains(tokenList[t].Type); t++)
              syntagm.tags.Add(tokenList[t].Value);
            i = t - 1;
            yield return syntagm;
            break;
          case TokenType.Strong:
          case TokenType.Morpho:
            throw new Exception("Strong and morphology tag must follow a word");
          default:
            throw new Exception($"Unhandled token type: {tokenList[i].Type}");
        }
      }
    }
  }
}
