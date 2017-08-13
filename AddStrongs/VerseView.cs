using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace TheWord
{
  public class VerseView : WrapPanel
  {
    BibleModule dataSource;
    public BibleModule DataSource
    {
      get => dataSource;
      set
      {
        dataSource = value;
        dataSource.SubscribeView(this);
      }
    }

    public VerseView() : base()
    {
      Margin = new Thickness(20);
    }

    ~VerseView()
    {
      dataSource?.UnsubscribeView(this);
    }

    private void AddSyntagm(Syntagm syntagm)
    {
      Children.Add(MakeTextBlock(syntagm));
    }

    public void Clear()
    {
      Children.Clear();
    }

    public void Update(IEnumerable<Syntagm> syntagms)
    {
      Clear();
      foreach (var syntagm in syntagms)
        AddSyntagm(syntagm);
    }

    private TextBlock MakeTextBlock(Syntagm syntagm)
    {
      var run = new Run(syntagm.word) { Focusable = true };
      run.MouseEnter += Word_MouseEnter;
      run.MouseLeave += Word_MouseLeave;
      if (syntagm?.tags?.Count > 0)
        run.ToolTip = String.Join("", syntagm.tags);
      return new TextBlock(run) { Tag = syntagm };
    }

    private void Word_MouseEnter(object sender, MouseEventArgs e)
    {
      ((Run)sender).Foreground = Brushes.Brown;
    }

    private void Word_MouseLeave(object sender, MouseEventArgs e)
    {
      ((Run)sender).Foreground = Brushes.Black;
    }

  }
}
