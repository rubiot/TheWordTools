using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Win32;

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
    public MenuItem MakeMenuItem(object header, RoutedEventHandler click = null)
    {
      var item = new MenuItem() { Header = header };
      if (click != null)
        item.Click += click;
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
      Items.Add(MakeMenuItem("Open...", OpenClick));
      Items.Add(MakeMenuItem("Save",    SaveClick));
      Items.Add(MakeMenuItem("Close",   CloseClick));
      Items.Add(MakeMenuItem("<module path>", null));
    }

    private void OnOpened(object sender, RoutedEventArgs e)
    {
      // TODO: Too fragile...
      (Items[1] as MenuItem).IsEnabled = Verse.DataSource != null && Verse.DataSource.Modified;
      (Items[2] as MenuItem).IsEnabled = Verse.DataSource != null;
      (Items[3] as MenuItem).Header = Verse.DataSource?.FilePath;
      (Items[3] as MenuItem).IsEnabled = false;
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

    public SyntagmContextMenu() : base()
    {
      Items.Add(MakeMenuItem("Copy tags", CopyTagsClick));
      Items.Add(MakeMenuItem("Paste tags", PasteTagsClick));
      Items.Add(MakeMenuItem("<Mark for review>", null));
      Items.Add(MakeMenuItem(tagsTextBox));

      Opened += OnOpened;
      tagsTextBox.KeyDown += TagsChange;
    }

    public void SetReadOnlyOption(bool value)
    {
      // TODO: This is too fragile. Make it safer
      (Items[1] as MenuItem).IsEnabled = !value;
      ((Items[3] as MenuItem).Header as TextBox).IsReadOnly = value;
    }

    private void OnOpened(object sender, RoutedEventArgs e)
    {
      if (syntagm.HasTag("<?>"))
      {
        (Items[2] as MenuItem).Header = "Mark as reviewed";
        (Items[2] as MenuItem).Click -= MarkForReviewClick;
        (Items[2] as MenuItem).Click += MarkAsReviewedClick;
      }
      else
      {
        (Items[2] as MenuItem).Header = "Mark for review";
        (Items[2] as MenuItem).Click -= MarkAsReviewedClick;
        (Items[2] as MenuItem).Click += MarkForReviewClick;
      }
      Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(delegate () { tagsTextBox.Focus(); }));
      tagsTextBox.SelectAll();
    }

    private void MarkAsReviewedClick(object sender, RoutedEventArgs e)
    {
      syntagm.RemoveTag("<?>");
    }

    private void MarkForReviewClick(object sender, RoutedEventArgs e)
    {
      syntagm.AddTag("<?>");
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
      set { isReadOnly = value; contextMenu.SetReadOnlyOption(value); }
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
