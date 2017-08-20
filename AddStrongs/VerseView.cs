using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

  public class VerseView : ScrollViewer
  {
    TextBox editBox; // used for text-mode edit
    WrapPanel panel;
    public SyntagmBlock Selected { get; set; }
    BibleModule dataSource;
    public BibleModule DataSource
    {
      get => dataSource;
      set
      {
        dataSource = value;
        dataSource.OnNewVerse += OnNewVerse;
        dataSource.OnChange   += OnNewVerse;
        OnNewVerse(this, new NewVerseArgs(dataSource.Current.Syntagms));
      }
    }

    public event EventHandler<SyntagmClickArgs> OnSyntagmClick;

    public VerseView() : base()
    {
      VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
      Content = panel = new WrapPanel();
      MouseDoubleClick += OnMouseDoubleClick;
      editBox = new TextBox
      {
        TextWrapping = TextWrapping.Wrap
      };
      editBox.KeyUp += EditBox_KeyUp;
      //Margin = new Thickness(20);
    }

    private void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
      if (e.ChangedButton == MouseButton.Left)
      {
        editBox.Text = dataSource.Current.Text;
        Content = editBox;
        //Keyboard.Focus(editBox);
      }
    }

    private void EditBox_KeyUp(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Escape)
      {
        dataSource.Current.ChangeText(editBox.Text);
        Content = panel;
      }
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
      panel.Children.Add(s);
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
      panel.Children.Clear();
    }
  }
}
