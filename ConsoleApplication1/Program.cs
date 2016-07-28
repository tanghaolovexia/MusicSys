using System;
using System.Runtime.InteropServices;

namespace ConsoleApplication1
{
    class Program
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        static void Main(string[] args)
        {
            String winTitle = "聊天标题";
            IntPtr hwnd = FindWindow(null, winTitle);
            QqWindowHelper a = new QqWindowHelper(hwnd, winTitle);
            System.IO.File.WriteAllText("D:\\1.txt", a.GetContent());
        }
    }
}
