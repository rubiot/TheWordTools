using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;
using TheWord;
using System.IO;

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

      Index.GoTo(Properties.Settings.Default.line);

      if (File.Exists(Properties.Settings.Default.module1))
          VerseView1.DataSource = new BibleModule(Properties.Settings.Default.module1);
      if (File.Exists(Properties.Settings.Default.module2))
          VerseView2.DataSource = new BibleModule(Properties.Settings.Default.module2);

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
      string module1 = VerseView1.DataSource?.FilePath ?? ""; // saving file name before closing module
      string module2 = VerseView2.DataSource?.FilePath ?? ""; // saving file name before closing module

      e.Cancel = (VerseView1.DataSource != null && !VerseView1.DataSource.Close()) ||
                 (VerseView2.DataSource != null && !VerseView2.DataSource.Close());

      if (!e.Cancel)
      {
        Properties.Settings.Default["module1"] = module1;
        Properties.Settings.Default["module2"] = module2;
        Properties.Settings.Default["line"]    = Index.Line;

        Properties.Settings.Default.Save();
      }
    }
  }
}
