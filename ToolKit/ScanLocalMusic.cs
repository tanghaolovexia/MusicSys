using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ToolKit
{
    /// <summary>
    /// 扫描本地音乐
    /// </summary>
    public class ScanLocalMusic : ScanRelayStation
    {
        public static string m_path = string.Empty;
        public static int i = 0;
        public static Stack<string> dirs = new Stack<string>(20);
        public static List<string> dirs1 = new List<string>();
        public static List<string> listmusic = new List<string>();
        public static void GetMusicInfo()
        {
            foreach (string path in dirs1)
            {
                try
                {
                    DirectoryInfo dinfo = new DirectoryInfo(path);
                    foreach (FileInfo info in dinfo.GetFiles())
                    {
                        //文件类型
                        string musicType = string.Empty;
                        if (info.Name.Contains(".mp3"))
                        {
                            listmusic.Add(info.Name);
                        }
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    continue;
                }
                catch (DirectoryNotFoundException)
                {
                    continue;
                }
            }
        }

        public static void SearchDirectory()
        {

            if (!Directory.Exists(m_path))
            {
                throw new ArgumentException();
            }
            dirs.Push(m_path);

            while (dirs.Count > 0)
            {
                string currentDir = dirs.Pop();
                string[] subDirs;
                try
                {
                    subDirs = Directory.GetDirectories(currentDir);
                }
                catch (UnauthorizedAccessException e)
                {
                    continue;
                }
                catch (DirectoryNotFoundException e)
                {
                    continue;
                }
                foreach (string str in subDirs)
                {
                    dirs.Push(str);
                    dirs1.Add(str);
                }
            }
        }

    }
}
