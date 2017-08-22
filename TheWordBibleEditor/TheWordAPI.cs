using System;
using System.Runtime.InteropServices;

namespace TheWord
{
  public class TheWordAPI
  {
    [StructLayout(LayoutKind.Sequential)]
    struct Message
    {
      public byte span;
      public byte vi;
      public byte ci;
      public byte bi;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct COPYDATASTRUCT
    {
      public uint dwData;
      public int cbData;
      public IntPtr lpData;

      public COPYDATASTRUCT(uint op, Message msg)
      {
        IntPtr structPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(msg));
        Marshal.StructureToPtr(msg, structPtr, false);

        dwData = op;
        lpData = structPtr;
        cbData = Marshal.SizeOf(msg);
      }
    }

    [DllImport("User32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
    static extern int SendMessage(int hWnd, int Msg, int wParam, ref COPYDATASTRUCT lParam);
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    static extern Int32 FindWindow(String lpClassName, String lpWindowName);

    const string TheWordClassName        = "theWord.0f2ba8a0-906d-11e1-b0c4-0800200c9a66.UnicodeClass";
    const uint COPYDATA_OP_GOTOVERSE     = 0xffff0001;
    const uint COPYDATA_OP_DCTWORDLOOKUP = 0xffff0002;
    const int WM_COPYDATA                = 0x4a;

    static public int SynchronizeRef(byte book, byte chapter, byte verse)
    {
      int hWnd = FindWindow(TheWordClassName, null);
      Message msg = new Message() { span = 0, bi = book, ci = chapter, vi = verse };
      COPYDATASTRUCT cds = new COPYDATASTRUCT(COPYDATA_OP_GOTOVERSE, msg);
      return SendMessage(hWnd, WM_COPYDATA, 0, ref cds);
    }
  }
}
