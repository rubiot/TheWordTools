using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TheWord
{
  public enum TokenType
  {
    NotDefined,
    Space,
    Word,
    Strong,
    Morpho,
    ReviewTag, // <?> indicates that the word should be revised
    Meta // irrelevant tags
  }

  class Tokenizer
  {
    private int index = 0;
    string text;
    public string Text
    {
      get => text;
      set { text = value; index = 0; }
    }

    List<TokenDefinition> tokenDefinitions = new List<TokenDefinition>();

    public Tokenizer()
    {
      InitTokenDefinitions();
    }

    public IEnumerable<Token> GetTokens()
    {
      while (GetToken(out Token token))
        yield return token;
    }

    bool GetToken(out Token token)
    {
      Match m;
      foreach (var t in tokenDefinitions)
      {
        m = t.TryMatch(Text, index);
        if (!m.Success || m.Index > index) // TryMatch only matches ^ when index == 0
          continue;

        index += m.Length;
        token = new Token()
        {
          Type = t.Type,
          Value = m.Value
        };
        return true;
      }
      token = null;
      return false;
    }

    void InitTokenDefinitions()
    {
      tokenDefinitions.Add(new TokenDefinition(TokenType.Morpho, "<WT[^> ]+( l(emma)?=\"[^\"]+\")>"));
      tokenDefinitions.Add(new TokenDefinition(TokenType.Strong, "<W[HG][^>]+>"));
      tokenDefinitions.Add(new TokenDefinition(TokenType.ReviewTag, @"<\?>"));
      tokenDefinitions.Add(new TokenDefinition(TokenType.Meta,   @"<RF.*?<Rf>|<TS\d*>.*?<Ts>|<[^>]+>"));
      tokenDefinitions.Add(new TokenDefinition(TokenType.Space,  @"[\s.,!?:;·—]+"));
      tokenDefinitions.Add(new TokenDefinition(TokenType.Word,   "[^< .,!?:;·—]+"));
    }
  }

  public class Token
  {
    public TokenType Type { get; set; }
    public string Value { get; set; }
  }

  class TokenDefinition
  {
    private Regex regex;
    public TokenType Type { get; set; }

    public TokenDefinition(TokenType _type, string _pattern)
    {
      //regex = new Regex($"^{_pattern}", RegexOptions.Compiled);
      regex = new Regex(_pattern, RegexOptions.Compiled);
      Type = _type;
    }

    public Match TryMatch(string input, int index)
    {
      return regex.Match(input, index);
    }
  }
}
