using System.Collections.Generic;
using System.Text;

namespace TheWord
{
  public class Verse
  {
    BibleModule parent;
    Parser parser = new Parser();
    List<Syntagm> syntagms = new List<Syntagm>();
    public string Text
    {
      get
      {
        var result = new StringBuilder();
        foreach (var syntagm in syntagms)
        {
          result.Append(syntagm.Text);
          result.Append(syntagm.AllTags);
        }
        return result.ToString();
      }
      set
      {
        parser.Parse(value);
        syntagms.Clear();
        foreach (var s in parser.GetSyntagms())
          syntagms.Add(s);
        parent.RaiseNewVerse(new NewVerseArgs(Syntagms));
      }
    }

    public List<Syntagm> Syntagms
    {
      get => syntagms;
    }

    public Verse(BibleModule _parent)
    {
      parent = _parent;
    }
  }
}
