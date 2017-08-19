using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
      VerseView2.DataSource = module2 = new BibleModule(@"C:\Temp\AnatolicBible\lxxmorph-rc.ot");
      VerseView2.OnSyntagmClick += OnSyntagmClick;
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
  }
}
