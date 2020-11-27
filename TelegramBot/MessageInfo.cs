using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot
{
    public class MessageInfo
    {
        /// <summary>
        /// Индетификатор сообщения
        /// </summary>
        public int message_id { get; set; }
        /// <summary>
        /// От какого пользователя сообшение
        /// </summary>
        public FromWhomMess from { get; set; }
        /// <summary>
        /// Описание чата
        /// </summary>
        public ChatInfo chat { get; set; }
        /// <summary>
        /// Дата в формате UNIX
        /// </summary>
        public int date { get; set; }
        /// <summary>
        /// Текст сообщения
        /// </summary>
        public string text { get; set; }
    }
}
