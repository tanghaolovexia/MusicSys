using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace ToolKit
{
    /// <summary>
    /// 本地歌曲文件操作
    /// </summary>
    public class MainScan : ScanRelayStation
    {
        /// <summary>
        /// 选中文件夹地址
        /// </summary>
        private string m_mainpath = string.Empty;


        public MainScan(string mainpath)
        {
            m_mainpath = mainpath;
        }
        public void DoMain()
        {
            #region 获取选中文件夹歌曲
            ScanLocalMusic.m_path = m_mainpath;
            ScanLocalMusic.SearchDirectory();
            List<string> dirs = ScanLocalMusic.dirs1;
            ScanLocalMusic.GetMusicInfo();
            List<string> listmusic = ScanLocalMusic.listmusic;
            #endregion


            Thread thread = new Thread(new ThreadStart(ScanLocalMusic.GetMusicInfo));
            thread.Start();
            while (!ScanRelayStation.isOver)
            {
                System.Windows.Forms.Application.DoEvents();
                if (thread.ThreadState != ThreadState.Running)
                {
                    thread.Start();
                }
            }
            if(ScanRelayStation.isOver)
            {

            }
        }
    }
}
