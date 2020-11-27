using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot
{
    public class MessageResult
    {
        /// <summary>
        /// Индетификатор сообщения
        /// </summary>
        public int update_id { get; set; }
        /// <summary>
        /// Само сообщение  
        /// </summary>
        public MessageInfo message { get; set; }
    }
}
