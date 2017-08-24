using System;
using System.Windows;
using System.Windows.Controls;

namespace TheWord
{
  public class BibleNavigator : ScrollViewer
  {
    StackPanel panel = new StackPanel();
    BibleIndex Index = BibleIndex.Instance;

    public BibleNavigator()
    {
      Content = panel;
      for (short b = 1; b <= 66 ; b++)
      {
        var e = new Expander
        {
          Header = new TextBlock() { Text = BibleIndex.BookNames[b] },
          Content = MakeChapterGrid(b)
        };
        panel.Children.Add(e);
      }
    }

    private object MakeChapterGrid(short book)
    {
      var grid = new Grid();
      var chapters = BibleIndex.ChaptersPerBook[book];

      int columns = chapters < 10 ? chapters : 10;
      for (int i = 0; i < columns; i++)
        grid.ColumnDefinitions.Add(new ColumnDefinition());

      int rows = (int)Math.Ceiling(chapters / 10.0);
      for (int i = 0; i < rows; i++)
        grid.RowDefinitions.Add(new RowDefinition());

      for (int i = 0; i < chapters; i++)
      {
        var button = new Button
        {
          Content = i + 1,
          Tag = book
        };
        button.Click += OnChapterClick;
        Grid.SetColumn(button, i % 10);
        Grid.SetRow(button, i / 10);
        grid.Children.Add(button);
      }

      return grid;
    }

    private void OnChapterClick(object sender, RoutedEventArgs e)
    {
      Button btn = sender as Button;
      short book = (short)btn.Tag;
      short chapter = short.Parse(btn.Content.ToString());
      Index.GoTo(book, chapter, 1);
    }
  }
}
