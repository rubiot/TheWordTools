using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TheWord
{
  public class NewVerseArgs : EventArgs
  {
    public NewVerseArgs(IEnumerable<Syntagm> syntagms)
    {
      Syntagms = syntagms;
    }

    public IEnumerable<Syntagm> Syntagms { get; set; }
  }

  public class BibleModule
  {
    string file;
    const int maxLine = 32767;
    long[] offsets = new long[maxLine + 1];
    StreamReader stream;
    Dictionary<int, string> changes = new Dictionary<int, string>();
    Verse current;
    public Verse Current { get => current; }
    int line = 1;
    public int Line { get => line; set => GoToLine(value); }
    public bool Modified { get; set; }

    public event EventHandler<NewVerseArgs> OnNewVerse;
    public event EventHandler<NewVerseArgs> OnChange;

    public BibleModule(string _file)
    {
      if (!File.Exists(_file))
        throw new FileNotFoundException("File not found", _file);

      current = new Verse(this);
      current.OnChange += OnVerseChange;
      Open(_file);
    }

    private void OnVerseChange(object sender, NewVerseArgs e)
    {
      Modified = true;
      changes[line] = current.Text;
      RaiseOnChange(e);
    }

    protected virtual void RaiseOnChange(NewVerseArgs e)
    {
      OnChange?.Invoke(this, e);
    }

    private void Open(string _file)
    {
      file = _file;
      stream = new StreamReader(file);
      IndexModule();
      changes.Clear();
      Line = line;
    }

    static public long GetActualPosition(StreamReader reader)
    {
      // source: https://stackoverflow.com/questions/5404267/streamreader-and-seeking
      System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetField;
      // The current buffer of decoded characters
      char[] charBuffer = (char[])reader.GetType().InvokeMember("charBuffer", flags, null, reader, null);
      // The index of the next char to be read from charBuffer
      int charPos = (int)reader.GetType().InvokeMember("charPos", flags, null, reader, null);
      // The number of decoded chars presently used in charBuffer
      int charLen = (int)reader.GetType().InvokeMember("charLen", flags, null, reader, null);
      // The current buffer of read bytes (byteBuffer.Length = 1024; this is critical).
      byte[] byteBuffer = (byte[])reader.GetType().InvokeMember("byteBuffer", flags, null, reader, null);
      // The number of bytes read while advancing reader.BaseStream.Position to (re)fill charBuffer
      int byteLen = (int)reader.GetType().InvokeMember("byteLen", flags, null, reader, null);
      // The number of bytes the remaining chars use in the original encoding.
      int numBytesLeft = reader.CurrentEncoding.GetByteCount(charBuffer, charPos, charLen - charPos);

      // For variable-byte encodings, deal with partial chars at the end of the buffer
      int numFragments = 0;
      if (byteLen > 0 && !reader.CurrentEncoding.IsSingleByte)
      {
        if (reader.CurrentEncoding.CodePage == 65001) // UTF-8
        {
          byte byteCountMask = 0;
          while ((byteBuffer[byteLen - numFragments - 1] >> 6) == 2) // if the byte is "10xx xxxx", it's a continuation-byte
            byteCountMask |= (byte)(1 << ++numFragments); // count bytes & build the "complete char" mask
          if ((byteBuffer[byteLen - numFragments - 1] >> 6) == 3) // if the byte is "11xx xxxx", it starts a multi-byte char.
            byteCountMask |= (byte)(1 << ++numFragments); // count bytes & build the "complete char" mask
                                                          // see if we found as many bytes as the leading-byte says to expect
          if (numFragments > 1 && ((byteBuffer[byteLen - numFragments] >> 7 - numFragments) == byteCountMask))
            numFragments = 0; // no partial-char in the byte-buffer to account for
        }
        else if (reader.CurrentEncoding.CodePage == 1200) // UTF-16LE
        {
          if (byteBuffer[byteLen - 1] >= 0xd8) // high-surrogate
            numFragments = 2; // account for the partial character
        }
        else if (reader.CurrentEncoding.CodePage == 1201) // UTF-16BE
        {
          if (byteBuffer[byteLen - 2] >= 0xd8) // high-surrogate
            numFragments = 2; // account for the partial character
        }
      }
      return reader.BaseStream.Position - numBytesLeft - numFragments;
    }

    private void IndexModule()
    {
      int _line = 0;
      stream.BaseStream.Seek(0, SeekOrigin.Begin);
      stream.DiscardBufferedData();
      while (stream.Peek() > -1)
      {
        //offsets[line++] = stream.Position; fails to consider buffers!!
        //                                   StreamReader should implement its own Position
        offsets[++_line] = GetActualPosition(stream);
        stream.ReadLine();
      }
    }

    public void NextVerse()
    {
      if (line < maxLine)
        GoToLine(line + 1);
    }

    private void ReadVerse()
    {
      current.Reset(changes.ContainsKey(line) ? changes[line] : ReadLine());
      RaiseNewVerse(new NewVerseArgs(current.Syntagms));
    }

    public void PreviousVerse()
    {
      if (line > 1)
        GoToLine(line - 1);
    }

    private void GoToLine(int _line)
    {
      line = _line;
      stream.BaseStream.Seek(offsets[line], SeekOrigin.Begin);
      stream.DiscardBufferedData();
      ReadVerse();
    }

    public void Save()
    {
      if (!Modified)
        return;

      string tmpFile = $"{file}.saving";
      using (var tmpModule = new StreamWriter(tmpFile, false, new UTF8Encoding(true)))
      {
        stream.BaseStream.Seek(0, SeekOrigin.Begin);
        stream.DiscardBufferedData();
        int _line = 0;
        while (stream.Peek() > -1 && ++_line < maxLine)
        {
          string buffer = stream.ReadLine();
          if (changes.ContainsKey(_line))
            buffer = changes[_line];
          tmpModule.WriteLine(buffer);
        }
      }

      stream.Dispose();

      if (File.Exists(file))
        File.Delete(file);
      File.Move(tmpFile, file);

      Open(file);
      Modified = false;
    }

    public virtual void RaiseNewVerse(NewVerseArgs e)
    {
      OnNewVerse?.Invoke(this, e);
    }

    private string ReadLine()
    {
      if (stream.Peek() == -1)
        throw new EndOfStreamException("There is no more verses");
      return stream.ReadLine();
    }
  }
}
