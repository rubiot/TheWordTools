using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TheWord
{
  public class SyntagmEventArgs : EventArgs
  {
    public SyntagmEventArgs(Syntagm s)
    {
      Syntagm = s;
    }
    public Syntagm Syntagm { get; set; }
  }

  class SyntagmContextMenu : ContextMenu
  {
    TextBox tagsTextBox = new TextBox();
    private Syntagm syntagm;
    public Syntagm Syntagm
    {
      get => syntagm;
      set
      {
        syntagm = value;
        tagsTextBox.Text = value.AllTags;
        if (tagsTextBox.Text.Length == 0)
          tagsTextBox.Text = "<no tags>";
      }
    }

    public SyntagmContextMenu()
    {
      var copy = new MenuItem();
      copy.Header = "Copy tags";
      copy.Click += CopyTagsClick;
      Items.Add(copy);

      var paste = new MenuItem();
      paste.Header = "Paste tags";
      paste.Click += PasteTagsClick;
      Items.Add(paste);

      var tags = new MenuItem();
      tags.Header = tagsTextBox;
      tags.Focusable = true;
      Items.Add(tags);

      tagsTextBox.KeyDown += TagsChange;
    }

    private void TagsChange(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Return)
        Syntagm?.ReplaceTags((sender as TextBox).Text);
    }

    private void PasteTagsClick(object sender, RoutedEventArgs e)
    {
      Syntagm?.ReplaceTags(Clipboard.GetText());
    }

    private void CopyTagsClick(object sender, RoutedEventArgs e)
    {
      Clipboard.SetDataObject(Syntagm?.AllTags);
    }
  }

  public class VerseView : ScrollViewer
  {
    SyntagmContextMenu contextMenu = new SyntagmContextMenu();
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

    public event EventHandler<SyntagmEventArgs> OnSyntagmClick;

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

    protected virtual void RaiseSyntagmClick(SyntagmEventArgs e)
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
      s.OnSyntagmLeftClick  += OnSyntagmBlockLeftClick;
      s.OnSyntagmRightClick += OnSyntagmBlockRightClick;
      s.ContextMenu = contextMenu;
      panel.Children.Add(s);
    }

    private void OnSyntagmBlockRightClick(object sender, SyntagmEventArgs e)
    {
      contextMenu.Syntagm = ((SyntagmBlock)sender).Syntagm;
    }

    private void OnSyntagmBlockLeftClick(object sender, SyntagmEventArgs e)
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
