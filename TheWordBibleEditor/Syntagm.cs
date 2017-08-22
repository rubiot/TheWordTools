using System;
using System.Collections.Generic;

namespace TheWord
{
  public class Syntagm
  {
    private string text;
    private List<string> tags = new List<string>();

    public string Text
    {
      get => text;
      set { text = value; RaiseSytagmChanged(); }
    }

    //public List<string>.Enumerator Tags { get => tags.GetEnumerator(); }
    // TODO: make Tags readonly!!!
    public List<string> Tags { get => tags; }
    public string AllTags    { get => string.Join("", tags); }
    public bool Displayable  { get; set; } = true;
    public bool Selectable   { get; set; } = true;

    public event EventHandler OnChange;

    protected virtual void RaiseSytagmChanged()
    {
      OnChange?.Invoke(this, null);
    }

    public void AddTag(string tag)
    {
      tags.Add(tag);
      RaiseSytagmChanged();
    }

    public void RemoveTag(string tag)
    {
      if (tags.Count > 0)
      {
        tags.Remove(tag);
        RaiseSytagmChanged();
      }
    }

    public void ReplaceTags(string new_tags)
    {
      if (new_tags == AllTags)
        return;

      if (tags.Count > 0)
        tags.Clear();

      // TODO: Parse tags and add them split
      if (new_tags.Length > 0)
        AddTag(new_tags);

      RaiseSytagmChanged();
    }

    public void ClearTags()
    {
      if (tags.Count > 0)
      {
        tags.Clear();
        RaiseSytagmChanged();
      }
    }

    public bool HasTag(string tag)
    {
      foreach (var t in tags)
        if (t.Contains(tag))
          return true;
      return false;
    }
  }
}
