using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot
{
    public class FromWhomMess
    {
        public int id { get; set; }
        public bool is_bot { get; set; }
        public string first_name { get; set; }
        public string username { get; set; }
        public string language_code { get; set; }
    }
}
