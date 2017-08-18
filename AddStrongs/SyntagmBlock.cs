using System;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace TheWord
{
  public class SyntagmBlock : TextBlock
  {
    Syntagm syntagm;
    Run run;
    public event EventHandler<SyntagmClickArgs> OnSyntagmClick;

    public SyntagmBlock(Syntagm _syntagm) : base()
    {
      syntagm = _syntagm;
      MakeSyntagmRun();
      syntagm.OnChange += OnSyntagmChanged;
    }

    private void OnSyntagmChanged(object sender, EventArgs e)
    {
      Syntagm syntagm = (Syntagm)sender;
      run.Text = syntagm.Text;
      run.ToolTip = String.Join("", syntagm.AllTags);
    }

    private void MakeSyntagmRun()
    {
      run = new Run(syntagm.Text) { Focusable = true, FontSize = 16 };
      run.MouseEnter        += Syntagm_MouseEnter;
      run.MouseLeave        += Syntagm_MouseLeave;
      run.MouseLeftButtonUp += Syntagm_MouseLeftButtonUp;
      if (syntagm.AllTags.Length > 0)
        run.ToolTip = String.Join("", syntagm.AllTags);
      Inlines.Add(run);
    }

    public void Select()
    {
      run.Background = Brushes.Gray;
    }

    public void Unselect()
    {
      run.Background = Brushes.LightYellow; //SystemColors.InfoBrushKey ??
    }

    protected virtual void RaiseSyntagmClick(SyntagmClickArgs e)
    {
      OnSyntagmClick?.Invoke(this, e);
    }

    private void Syntagm_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
      RaiseSyntagmClick(new SyntagmClickArgs(syntagm));
    }

    private void Syntagm_MouseEnter(object sender, MouseEventArgs e)
    {
      ((Run)sender).Foreground = Brushes.Magenta;
    }

    private void Syntagm_MouseLeave(object sender, MouseEventArgs e)
    {
      ((Run)sender).Foreground = Brushes.Black;
    }
  }
}
