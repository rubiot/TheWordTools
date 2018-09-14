using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace TheWord
{
  public class TheWordAPI
  {
    [StructLayout(LayoutKind.Sequential)]
    struct GoToRefMessage
    {
      public byte span;
      public byte vi;
      public byte ci;
      public byte bi;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct COPYDATASTRUCT_DICT
    {
      public uint dwData;
      public int cbData;
      [MarshalAs(UnmanagedType.LPWStr, SizeConst = 255)]
      public string lpData;

      public COPYDATASTRUCT_DICT(uint op, string msg)
      {
        dwData = op;
        lpData = msg;
        cbData = 255;
      }
    }

    [StructLayout(LayoutKind.Sequential)]
    struct COPYDATASTRUCT_REF
    {
      public uint dwData;
      public int cbData;
      public IntPtr lpData;

      public COPYDATASTRUCT_REF(uint op, GoToRefMessage msg)
      {
        IntPtr structPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(msg));
        Marshal.StructureToPtr(msg, structPtr, false);

        dwData = op;
        lpData = structPtr;
        cbData = Marshal.SizeOf(msg);
      }
    }

    [DllImport("User32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Unicode)]
    static extern int SendMessageDict(int hWnd, int Msg, int wParam, ref COPYDATASTRUCT_DICT lParam);
    [DllImport("User32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Unicode)]
    static extern int SendMessage(int hWnd, int Msg, int wParam, ref COPYDATASTRUCT_REF lParam);
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    static extern Int32 FindWindow(String lpClassName, String lpWindowName);
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern int GetWindowText(Int32 hWnd, StringBuilder lpString, int nMaxCount);

    const string TheWordClassName        = "theWord.0f2ba8a0-906d-11e1-b0c4-0800200c9a66.UnicodeClass";
    const uint COPYDATA_OP_GOTOVERSE     = 0xffff0001;
    const uint COPYDATA_OP_DCTWORDLOOKUP = 0xffff0002;
    const int WM_COPYDATA                = 0x4a;

    static public int SynchronizeRef(byte book, byte chapter, byte verse)
    {
      int hWnd = FindWindow(TheWordClassName, null);
      GoToRefMessage msg = new GoToRefMessage() { span = 0, bi = book, ci = chapter, vi = verse };
      COPYDATASTRUCT_REF cds = new COPYDATASTRUCT_REF(COPYDATA_OP_GOTOVERSE, msg);
      return SendMessage(hWnd, WM_COPYDATA, 0, ref cds);
    }

    static public int SynchronizeDictionary(string word)
    {
      int hWnd = FindWindow(TheWordClassName, null);
      COPYDATASTRUCT_DICT cds = new COPYDATASTRUCT_DICT(COPYDATA_OP_DCTWORDLOOKUP, word);
      return SendMessageDict(hWnd, WM_COPYDATA, 0, ref cds);
    }

    static public string GetTheWordCurrentVref()
    {
      int hWnd = FindWindow(TheWordClassName, null);

      if (hWnd != 0)
      {
        var sbWindowText = new StringBuilder(1024);
        GetWindowText(hWnd, sbWindowText, sbWindowText.Capacity);
        return Regex.Replace(sbWindowText.ToString(), "\\s+-.*$", "");
      }

      return "";
    }
  }
}
