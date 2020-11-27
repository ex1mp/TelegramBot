using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using Newtonsoft.Json;
using System.Collections.Specialized;
using System.IO;

namespace TelegramBot
{
    public partial class MainForm : Form
    {
        private string token = "1416810810:AAGM84vwYkU0_6UlsdBYfea4246S00Cm_fA";
        private string baseUrl = "https://api.telegram.org/bot";
        WebClient client;
        private long LastUpdateID=0;
        private string fileLog = "BotLog.log";
        public MainForm()
        {
            InitializeComponent();
            Init();
        }
        private void Init()
        {
            client = new WebClient();
            //InitProxy();
            timergetUpdates.Enabled = true;
        }
        private void InitProxy()
        {
            WebProxy proxy = new WebProxy("192.168.0.0.1", 8080);
            proxy.Credentials = new NetworkCredential("UserName", "password");
            client.Proxy = proxy;
        }
        private void SendMessage(long chatId,string message)
        {
            string address = baseUrl + token + "/sendMessage";
            NameValueCollection collection = new NameValueCollection();
            collection.Add("chat_id", chatId.ToString());
            collection.Add("text", message);
            client.UploadValues(address, collection);
        }

        private void timergetUpdates_Tick(object sender, EventArgs e)
        {
            GetUpdates();
        }
        private void GetUpdates()
        {
            String dwLine=client.DownloadString(baseUrl + token + "/getUpdates?offset=" + (LastUpdateID+1));
            TelegramMessage nMessages = JsonConvert.DeserializeObject<TelegramMessage>(dwLine);
            if(!nMessages.ok||nMessages.result.Length==0)
            {
                return;
            }
            foreach(MessageResult result in nMessages.result)
            {
                LastUpdateID = result.update_id;
                WriteLog(result.message.from.first_name + "(" + result.message.from.id + "): " + result.message.text);
                SendAnswer(result.message.chat.id,result.message.text);

            }
        }
        private void SendAnswer(long chatID,string message)
        {
            string answer="";
            switch (message.ToLower())
            {
                case "/help": answer = @"Welcome to the bot helper. 
All supported commands are listed below:
/start - getting started with a bot
/help  - list of available commands";
                        break;
                case "/start": answer = "I'm your spy bot, you know what i can do? /help";
                    break;
                default: answer = "You write to me: '"+message+"', but i dont know what to answer";
                    break;
            }
            SendMessage( chatID,answer);
        }
        private void WriteLog(string text)
        {
            text = DateTime.Now + " " + Environment.NewLine;
            textBoxLog.Text += text;
            File.AppendAllText(fileLog,text);
        }
    }
}
