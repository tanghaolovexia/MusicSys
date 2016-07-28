using System.Collections.Generic;

namespace ToolKit
{
    public class ScanRelayStation
    {
        public bool isRead { get; set; }
        public string path { get; set; }
        public int level { get; set; }
        /// <summary>
        /// 是否结束
        /// </summary>
        public static bool isOver { get; set; }

        public List<ScanRelayStation> listScanRStation = new List<ScanRelayStation>();
    }
}
