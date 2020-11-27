using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot
{
    public class ChatInfo
    {
        /// <summary>
        /// Индетификатор чата
        /// </summary>
        public int id { get; set; }
        public string first_name { get; set; }
        public string username { get; set; }
        public string type { get; set; }
    }

}
