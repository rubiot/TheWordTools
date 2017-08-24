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

    public MainWindow()
    {
      InitializeComponent();

      Index.OnIndexChange += OnIndexChange;

      VerseView1.DataSource = new BibleModule(@"C:\Temp\AnatolicBible\Anatolic Bible 10-7-2017.ont");
      VerseView2.IsReadOnly = true;
    }

    private void OnIndexChange(object sender, EventArgs e)
    {
      LineTextBox.Text = $"{Index.Reference}, line {Index.Line}";
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

    private void WndMain_Closing(object sender, CancelEventArgs e)
    {
      e.Cancel = !VerseView1.DataSource.Close() || !VerseView2.DataSource.Close();
    }
  }
}
