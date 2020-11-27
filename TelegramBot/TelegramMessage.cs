using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot
{
    //{"ok":true,"result":[{"update_id":80837508,"message":{"message_id":5,"from":{"id":919055225,"is_bot":false,"first_name":"Niko","username":"ex1mp","language_code":"ru"},"chat":{ "id":919055225,"first_name":"Niko","username":"ex1mp","type":"private"},"date":1605556761,"text":"2332"}}]}

    class TelegramMessage
    {
        /// <summary>
        /// Все ли хорошо
        /// </summary>
        public bool ok { get; set; }
        /// <summary>
        /// Результат сообщения
        /// </summary>
        public MessageResult[] result { get; set; }
    }
}
