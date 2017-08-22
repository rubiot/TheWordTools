using System;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace TheWord
{
  public class SyntagmBlock : TextBlock
  {
    Run run;
    Syntagm syntagm;
    public Syntagm Syntagm { get => syntagm; set => syntagm = value; }
    public event EventHandler<SyntagmEventArgs> OnSyntagmLeftClick;
    public event EventHandler<SyntagmEventArgs> OnSyntagmRightClick;

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
      SetSyntagmStyle();
    }

    private void MakeSyntagmRun()
    {
      run = new Run(syntagm.Text) { Focusable = true, FontSize = 16 };
      if (syntagm.Selectable)
      {
        run.MouseEnter           += Syntagm_MouseEnter;
        run.MouseLeave           += Syntagm_MouseLeave;
        run.MouseLeftButtonUp    += Syntagm_MouseLeftButtonUp;
        run.MouseRightButtonDown += Syntagm_MouseRightButtonDown;
      }
      SetSyntagmStyle();
      Inlines.Add(run);
    }

    private void Syntagm_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
      RaiseSyntagmRightClick(new SyntagmEventArgs(syntagm));
    }

    private void SetSyntagmStyle()
    {
      if (syntagm.Tags.Count > 0)
        run.ToolTip = syntagm.AllTags;
      run.Foreground = syntagm.Tags.Count > 0 ? Brushes.Black : Brushes.Gray;
    }

    public void Select()
    {
      run.Background = Brushes.Yellow;
    }

    public void Unselect()
    {
      run.Background = Brushes.LightYellow;
    }

    protected virtual void RaiseSyntagmLeftClick(SyntagmEventArgs e)
    {
      OnSyntagmLeftClick?.Invoke(this, e);
    }

    protected virtual void RaiseSyntagmRightClick(SyntagmEventArgs e)
    {
      OnSyntagmRightClick?.Invoke(this, e);
    }

    private void Syntagm_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
      RaiseSyntagmLeftClick(new SyntagmEventArgs(syntagm));
    }

    private void Syntagm_MouseEnter(object sender, MouseEventArgs e)
    {
      ((Run)sender).Foreground = Brushes.Magenta;
    }

    private void Syntagm_MouseLeave(object sender, MouseEventArgs e)
    {
      SetSyntagmStyle();
    }
  }
}
