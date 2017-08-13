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
  /// Interação lógica para MainWindow.xam
  /// </summary>
  public partial class MainWindow : Window
  {
    BibleModule module1;
    BibleModule module2;

    public MainWindow()
    {
      InitializeComponent();
      VerseView1.DataSource = module1 = new BibleModule(@"C:\Temp\AnatolicBible\lxxmorph-rc.ot");
      VerseView2.DataSource = module2 = new BibleModule(@"C:\Temp\AnatolicBible\Anatolic Bible 10-7-2017.ont");
    }

    private void BtnNext_Click(object sender, RoutedEventArgs e)
    {
      module1.NextVerse();
      module2.NextVerse();
      TbLine.Text = "line " + module1.Line.ToString();
    }

    private void BtnPrev_Click(object sender, RoutedEventArgs e)
    {
      module1.PreviousVerse();
      module2.PreviousVerse();
      TbLine.Text = "line " + module1.Line.ToString();
    }
  }
}
