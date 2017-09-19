using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Windows.Documents;

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

  class ContexMenuHelper : ContextMenu
  {
    public Dictionary<string, MenuItem> ItemsMap = new Dictionary<string, MenuItem>();
 
    public MenuItem MakeMenuItem(string key, object header, RoutedEventHandler click = null)
    {
      var item = new MenuItem() { Header = header };
      if (click != null)
        item.Click += click;
      ItemsMap[key] = item;

      return item;
    }
  }

  class VerseContextMenu : ContexMenuHelper
  {
    public VerseView Verse { get; set; }

    public VerseContextMenu(VerseView verse)
    {
      Verse = verse;
      Opened += OnOpened;
      Items.Add(MakeMenuItem("open", "Open...", OpenClick));
      Items.Add(MakeMenuItem("save", "Save",    SaveClick));
      Items.Add(MakeMenuItem("close", "Close",   CloseClick));
      Items.Add(new Separator());
      Items.Add(MakeMenuItem("path", "<module path>"));
      ItemsMap["path"].IsEnabled = false;
    }

    private void OnOpened(object sender, RoutedEventArgs e)
    {
      ItemsMap["save"].IsEnabled = Verse.DataSource != null && Verse.DataSource.Modified;
      ItemsMap["close"].IsEnabled = Verse.DataSource != null;
      ItemsMap["path"].Header = new Run(Verse.DataSource?.FilePath) { FontWeight = FontWeights.DemiBold };
    }

    private void CloseClick(object sender, RoutedEventArgs e)
    {
      if (Verse.DataSource != null && Verse.DataSource.Close())
      {
        Verse.DataSource = null;
        Verse.Clear();
      }
    }

    private void SaveClick(object sender, RoutedEventArgs e)
    {
      Verse.DataSource.Save();
    }

    private void OpenClick(object sender, RoutedEventArgs e)
    {
      if (Verse.DataSource == null || Verse.DataSource.Close())
      {
        var dlg = new OpenFileDialog() { Filter = "TheWord bible modules (*.ot, *.nt, *.ont)|*.ot;*.nt;*.ont" };
        Nullable<bool> result = dlg.ShowDialog();
        if (result == true)
          Verse.DataSource = new BibleModule(dlg.FileName);
      }
    }
  }

  class SyntagmContextMenu : ContexMenuHelper
  {
    const string NoTagsText = "<no tags>";
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
          tagsTextBox.Text = NoTagsText;
      }
    }

    public SyntagmContextMenu() : base()
    {
      Items.Add(MakeMenuItem("word", "<word>"));
      ItemsMap["word"].IsEnabled = false;
      Items.Add(new Separator());
      Items.Add(MakeMenuItem("copy", "Copy tags", CopyTagsClick));
      Items.Add(MakeMenuItem("paste", "Paste tags", PasteTagsClick));
      Items.Add(MakeMenuItem("review", "<Mark for review>", ReviewMarkClick));
      Items.Add(MakeMenuItem("tags", tagsTextBox));

      Opened += OnOpened;
      tagsTextBox.KeyDown += TagsChange;
    }

    private void ReviewMarkClick(object sender, RoutedEventArgs e)
    {
      syntagm.ToggleReviewMark();
    }

    public void SetReadOnlyOption(bool value)
    {
      ItemsMap["copy"].IsEnabled = !value;
      ItemsMap["paste"].IsEnabled = !value;
      ItemsMap["review"].IsEnabled = !value;
      (ItemsMap["tags"].Header as TextBox).IsReadOnly = value;
    }

    private void OnOpened(object sender, RoutedEventArgs e)
    {
      ItemsMap["word"].Header = new Run(syntagm.Text) { FontSize = 14, FontWeight = FontWeights.DemiBold };

      if (syntagm.Review)
        ItemsMap["review"].Header = "Mark as reviewed";
      else
        ItemsMap["review"].Header = "Mark for review";

      Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(delegate () { tagsTextBox.Focus(); }));
      tagsTextBox.SelectAll();
    }

    private void TagsChange(object sender, KeyEventArgs e)
    {
      var textBox = (sender as TextBox);
      if (e.Key == Key.Return && textBox.Text != NoTagsText)
        Syntagm?.ReplaceTags(textBox.Text);
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
        if (dataSource != null)
        {
          dataSource.OnNewVerse += OnNewVerse;
          dataSource.OnChange += OnNewVerse;
          OnNewVerse(this, new NewVerseArgs(dataSource.Current.Syntagms));
        }
      }
    }
    private bool isReadOnly = false;
    public bool IsReadOnly
    {
      get => isReadOnly;
      set
      {
        isReadOnly = value;
        contextMenu.SetReadOnlyOption(value);
        editBox.IsReadOnly = value;
      }
    }

    public event EventHandler<SyntagmEventArgs> OnSyntagmClick;

    public VerseView() : base()
    {
      VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
      Content = panel = new WrapPanel();
      ContextMenu = new VerseContextMenu(this);
      MouseDoubleClick += OnMouseDoubleClick;

      editBox = new TextBox { TextWrapping = TextWrapping.Wrap };
      editBox.KeyUp += EditBox_KeyUp;
    }

    private void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
      if (e.ChangedButton == MouseButton.Left)
      {
        editBox.Text = dataSource?.Current.Text;
        Content = editBox;
        Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(delegate () { editBox.Focus(); }));
      }
    }

    private void EditBox_KeyUp(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Escape)
      {
        if (!IsReadOnly)
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
