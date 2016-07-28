using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using ViewModel;
using ToolKit;

namespace MusicPlayWinfrom
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPlay_Click(object sender, EventArgs e)
        {
            folderBrowserDialogLocalMusic.SelectedPath = "c:\\";
            if (folderBrowserDialogLocalMusic.ShowDialog() == DialogResult.OK)
            {
                string path = folderBrowserDialogLocalMusic.SelectedPath;


                //ScanLocalMusic music = new ScanLocalMusic();
                


                DirectoryInfo dinfo = new DirectoryInfo(path);


                //是否进行下一步
                bool isNext = true;

                DirectoryInfo[] dinfos = dinfo.GetDirectories();
                foreach (DirectoryInfo dinfotwo in dinfos)
                {
                    InsertSaveMusicList(dinfotwo);
                    isNext = false;
                }
                if (isNext)
                {
                    InsertSaveMusicList(dinfo);
                }
            }
        }


        private void ScanHandlerEvg(string direpath)
        {

        }


        /// <summary>
        /// 返回除数结果
        /// </summary>
        /// <param name="num">除数</param>
        /// <param name="factor">被除数因子</param>
        /// <returns></returns>
        private double GetNumber(long num, int factor)
        {
            double iresult = 0d;
            double x = double.Parse(num.ToString());
            double y = double.Parse(factor.ToString());
            iresult = Math.Round((double)(x / y), 2);
            return iresult;
        }

        /// <summary>
        /// 本地歌曲列表插入数据库操作
        /// </summary>
        /// <param name="dinfo"></param>
        private void InsertSaveMusicList(DirectoryInfo dinfo)
        {
            //初始化操作SQLLITE类,必须要初始化,这样SQLLITE才会开启
            DoReadWriteSqlLite write = new DoReadWriteSqlLite();
            //用来保存歌曲类集合
            List<tb_SaveMusicList> list = new List<tb_SaveMusicList>();
            int i = 0;
            //遍历文件夹中文件
            foreach (FileInfo info in dinfo.GetFiles())
            {
                //文件类型
                string musicType = string.Empty;
                if (info.Name.Contains("."))
                {
                    musicType = info.Name.Substring(info.Name.LastIndexOf("."));
                }
                //如果为歌曲则将歌曲保存
                if (musicType == "mp3")
                {
                    tb_SaveMusicList music = new tb_SaveMusicList();
                    music.ID = i <= 0 ? MusicBll.BllCommonGetMaxID.tb_SaveMusicList : MusicBll.BllCommonGetMaxID.tb_SaveMusicList + i;
                    if (info.Name.Contains("-"))
                    {
                        string[] musicNames = info.Name.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                        music.Singer = musicNames[0].Trim().ToString();
                        music.Name = musicNames[1].Trim().ToString();
                    }
                    music.Name = info.Name.Trim().ToString();
                    double c = GetNumber(info.Length, 1024 * 1024);
                    music.SingLength = c.ToString() + "MB";
                    list.Add(music);
                    i++;
                }
            }

            bool result = write.InsertLocalMusicList(list);
        }


    }
}
