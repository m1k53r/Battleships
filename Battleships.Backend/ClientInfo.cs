using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battleships.Backend
{
    internal class ClientInfo
    {
        public string uuid { get; set; }
        public string username { get; set; }
        public bool isPlaying { get; set; }
        public ClientInfo(string uuid, string username, bool isPlaying)
        {
            this.uuid = uuid;
            this.username = username;
            this.isPlaying = isPlaying;
        }
    }
}
