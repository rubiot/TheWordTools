using System;
using System.Collections.Generic;
using System.Text;

namespace TheWord
{
  public class Verse
  {
    BibleModule parent;
    Parser parser = Parser.Instance;
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
        parser.ParseVerse(value);
        syntagms.Clear();
        foreach (var s in parser.GetSyntagms())
        {
          s.OnChange += OnSyntagmChange;
          syntagms.Add(s);
        }
      }
    }
    public List<Syntagm> Syntagms { get => syntagms; }
    public event EventHandler<NewVerseArgs> OnChange;

    public Verse(BibleModule _parent)
    {
      parent = _parent;
    }

    public void Reset(string text)
    {
      Text = text;
    }

    public void ChangeText(string text)
    {
      if (text != Text)
      {
        Text = text;
        RaiseOnChange(new NewVerseArgs(syntagms));
      }
    }

    private void OnSyntagmChange(object sender, EventArgs e)
    {
      RaiseOnChange(new NewVerseArgs(syntagms));
    }

    protected virtual void RaiseOnChange(NewVerseArgs e)
    {
      OnChange?.Invoke(this, e);
    }
  }
}
