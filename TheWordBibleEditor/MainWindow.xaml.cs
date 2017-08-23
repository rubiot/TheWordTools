using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;
using TheWord;

namespace TheWordBibleEditor
{
  /// <summary>
  /// Logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    BibleIndex Index = BibleIndex.Instance;
    BibleModule module1;
    BibleModule module2;

    public MainWindow()
    {
      InitializeComponent();

      module1 = new BibleModule(@"C:\Temp\AnatolicBible\Anatolic Bible 10-7-2017.ont");
      module1.OnNewVerse += OnNewVerse;

      VerseView1.DataSource = module1;

      module2 = new BibleModule(@"C:\Temp\AnatolicBible\lxxmorph-rc.ot");

      VerseView2.DataSource = module2;
      VerseView2.IsReadOnly = true;
    }

    private void OnNewVerse(object sender, NewVerseArgs e)
    {
      BibleModule bible = sender as BibleModule;

      LineTextBox.Text = $"{Index.Reference}, line {Index.Line}";
      TheWordAPI.SynchronizeRef(Index.Book, Index.Chapter, Index.Verse);
    }

    private void BtnNext_Click(object sender, RoutedEventArgs e)
    {
      Index.Next();
    }

    private void BtnPrev_Click(object sender, RoutedEventArgs e)
    {
      Index.Previous();
    }

    private void LineTextBox_KeyUp(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Return)
        if (ushort.TryParse(((TextBox)sender).Text, out ushort line))
          Index.GoTo(line);
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
      module1.Save();
    }

    private void WndMain_Closing(object sender, CancelEventArgs e)
    {
      if (module1.Modified)
      {
        MessageBoxResult result = MessageBox.Show("There are pending changes. Click Yes to save and close, No to close without saving, or Cancel to not close.",
                                                  "TheWord Bible Editor", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);

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
