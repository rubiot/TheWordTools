using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TheWord;

namespace AddStrongs
{
  /// <summary>
  /// Logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    BibleModule module1;
    BibleModule module2;

    public MainWindow()
    {
      InitializeComponent();
      VerseView1.DataSource = module1 = new BibleModule(@"C:\Temp\AnatolicBible\Anatolic Bible 10-7-2017.ont");
      VerseView1.OnSyntagmClick += OnSyntagmClick;
      module1.OnNewVerse += OnNewVerse;
      module1.OnChange += OnModuleChange;
      VerseView2.DataSource = module2 = new BibleModule(@"C:\Temp\AnatolicBible\lxxmorph-rc.ot");
      VerseView2.OnSyntagmClick += OnSyntagmClick;
    }

    private void OnModuleChange(object sender, EventArgs e)
    {
      BtnSave.IsEnabled = true;
    }

    private void OnNewVerse(object sender, NewVerseArgs e)
    {
      LineTextBox.Text = $"line {((BibleModule)sender).Line}";
    }

    private void OnSyntagmClick(object sender, SyntagmClickArgs e)
    {
      TagsTextBox.Text = e.Syntagm.AllTags;
    }

    private void BtnNext_Click(object sender, RoutedEventArgs e)
    {
      module1.NextVerse();
      module2.NextVerse();
      TheWordAPI.SynchronizeRef(42, 3, 16);
    }

    private void BtnPrev_Click(object sender, RoutedEventArgs e)
    {
      module1.PreviousVerse();
      module2.PreviousVerse();
    }

    private void LineTextBox_KeyUp(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Return)
      {
        ushort line;
        if (ushort.TryParse(((TextBox)sender).Text, out line))
          module1.Line = module2.Line = line;
      }
    }

    private void TagsTextBox_KeyUp(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Return)
        VerseView1.Selected.Syntagm.ReplaceTags(TagsTextBox.Text);
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
      module1.Save();
      BtnSave.IsEnabled = false;
    }

    private void WndMain_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      if (module1.Modified)
      {
        MessageBoxResult result = MessageBox.Show("There are pending changes. Click Yes to save and close, No to close without saving, or Cancel to not close.",
                                                  "AddStrongs", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);

        switch (result)
        {
          case MessageBoxResult.Cancel:
            e.Cancel = true;
            break;
          case MessageBoxResult.Yes:
            module1.Save();
            e.Cancel = false;
            break;
          case MessageBoxResult.No:
            e.Cancel = false;
            break;
          default:
            break;
        }
      }
    }
  }
}
