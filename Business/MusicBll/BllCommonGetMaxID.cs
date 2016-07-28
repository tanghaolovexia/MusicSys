using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicBll
{
    public class BllCommonGetMaxID
    {
        /// <summary>
        /// 本地歌曲存储表最大ID
        /// </summary>
        public static int tb_SaveMusicList
        {
            get
            {
                return ConnctionSqlLite.GetTableSequence("tb_SaveMusicList") + 1;
            }
        }
    }
}
