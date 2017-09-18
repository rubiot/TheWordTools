using System;
using System.Collections.Generic;
using System.Linq;

namespace TheWord
{
  class Parser
  {
    public static readonly Parser Instance = new Parser();

    static TokenType[] TagTokens = new[] { TokenType.Strong, TokenType.Morpho, TokenType.ReviewTag };

    Tokenizer tokenizer = new Tokenizer();

    static Parser() { }
    private Parser() { }

    public void ParseVerse(string verse)
    {
      tokenizer.Text = verse;
    }

    public IEnumerable<string> ParseTags(string tags, bool ignore_invalid = true)
    {
      tokenizer.Text = tags;
      foreach (var token in tokenizer.GetTokens())
      {
        if (TagTokens.Contains(token.Type))
          yield return token.Value;
        else if (ignore_invalid == false)
          throw new Exception($"<{token.Value}> is not a valid tag");
      }
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
            yield return new Syntagm() { Text = tokenList[i].Value, Selectable = false };
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
          case TokenType.ReviewTag:
            throw new Exception($"This tag type must follow a word ({tokenList[i].Type}/\"{tokenList[i].Value}\")");
          default:
            throw new Exception($"Unhandled token type: {tokenList[i].Type}");
        }
      }
    }
  }
}
