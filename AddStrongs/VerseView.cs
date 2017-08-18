using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace TheWord
{
  public class SyntagmClickArgs : EventArgs
  {
    public SyntagmClickArgs(Syntagm s)
    {
      Syntagm = s;
    }
    public Syntagm Syntagm { get; set; }
  }

  public class VerseView : WrapPanel
  {
    public SyntagmBlock Selected { get; set; }
    BibleModule dataSource;
    public BibleModule DataSource
    {
      get => dataSource;
      set
      {
        dataSource = value;
        dataSource.OnNewVerse += OnNewVerse;
        OnNewVerse(this, new NewVerseArgs(dataSource.Current.Syntagms));
      }
    }

    public event EventHandler<SyntagmClickArgs> OnSyntagmClick;

    public VerseView() : base()
    {
      //Margin = new Thickness(20);
    }

    protected virtual void RaiseSyntagmClick(SyntagmClickArgs e)
    {
      OnSyntagmClick?.Invoke(this, e);
    }

    private void OnNewVerse(object sender, NewVerseArgs e)
    {
      Clear();
      foreach (var syntagm in e.Syntagms)
        if (syntagm.Displayable)
          AddSyntagm(syntagm);
    }

    private void AddSyntagm(Syntagm syntagm)
    {
      var s = new SyntagmBlock(syntagm);
      s.OnSyntagmClick += OnSyntagmBlockClick;
      Children.Add(s);
    }

    private void OnSyntagmBlockClick(object sender, SyntagmClickArgs e)
    {
      SyntagmBlock syntagm = (SyntagmBlock)sender;
      Selected?.Unselect();
      syntagm.Select();
      Selected = syntagm;
      RaiseSyntagmClick(e);
    }

    public void Clear()
    {
      Children.Clear();
    }
  }
}
