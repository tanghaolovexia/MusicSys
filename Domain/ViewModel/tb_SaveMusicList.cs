using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModel
{
    /// <summary>
    /// 本地歌曲记录表
    /// </summary>
    public class tb_SaveMusicList
    {
        public int ID { get; set; }
        /// <summary>
        /// 歌曲名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 歌曲大小
        /// </summary>
        public string SingLength { get; set; }
        /// <summary>
        /// 歌手
        /// </summary>
        public string Singer { get; set; }

    }
}
