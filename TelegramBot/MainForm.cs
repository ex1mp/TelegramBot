using Newtonsoft.Json;
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace TelegramBot
{
    public enum BotState
    {
        Wait,
        KillProc,
        StartProc
    }
    public partial class MainForm : Form
    {
        private readonly string token = "1416810810:AAGM84vwYkU0_6UlsdBYfea4246S00Cm_fA";
        private readonly string baseUrl = "https://api.telegram.org/bot";
        private WebClient client;
        private long LastUpdateID = 0;
        private readonly string fileLog = "BotLog.log";
        private BotState botState = BotState.Wait;
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
        private void SendMessage(long chatId, string message)
        {
            string address = baseUrl + token + "/sendMessage";
            NameValueCollection collection = new NameValueCollection
            {
                { "chat_id", chatId.ToString() },
                { "text", message }
            };
            client.UploadValues(address, collection);
        }

        private void timergetUpdates_Tick(object sender, EventArgs e)
        {
            GetUpdates();
        }
        private void GetUpdates()
        {
            string dwLine = client.DownloadString(baseUrl + token + "/getUpdates?offset=" + (LastUpdateID + 1));
            TelegramMessage nMessages = JsonConvert.DeserializeObject<TelegramMessage>(dwLine);
            if (!nMessages.ok || nMessages.result.Length == 0)
            {
                return;
            }
            foreach (MessageResult result in nMessages.result)
            {
                LastUpdateID = result.update_id;
                WriteLog(result.message.from.first_name + "(" + result.message.from.id + "): " + result.message.text);
                switch (botState)
                {
                    case BotState.KillProc:
                        if (CloseProcess(result.message.text))
                        {
                            SendMessage(result.message.chat.id, "Process is stopped.  ");
                            WriteLog("Process " + result.message.text + " is closed");
                        }
                        else
                        {
                            SendMessage(result.message.chat.id, "Error, no such process exists. ");
                        }
                        break;
                    case BotState.StartProc:
                        if (StartProcess(result.message.text))
                        {
                            SendMessage(result.message.chat.id, "Application is started.  ");
                            WriteLog("Application " + result.message.text + " is started");
                        }
                        else
                        {
                            SendMessage(result.message.chat.id, "Error, no such application exists. ");
                        }
                        break;
                    default:
                        SendAnswer(result.message.chat.id, result.message.text);
                        break;
                }

            }
        }
        private void SendAnswer(long chatID, string message)
        {
            string answer = "";
            switch (message.ToLower())
            {
                case "/help":
                    answer = @"Welcome to the bot helper. 
All supported commands are listed below:
/start - getting started with a bot
/help - list of available commands
/getlog - send you log file data
/screenshot - send screenshot of your desktop
/taskmanager- shows a list of processes on the server computer
/stopprocess- stops the selected process
/startprocess- starts the selected process";

                    break;
                case "/getlog":
                    answer = RetLog();
                    break;
                case "/start":
                    answer = "I'm your spy bot, you know what i can do? /help";
                    break;
                case "/screenshot":
                    SendPrintScreen(chatID);
                    return;
                case "/taskmanager":
                    answer = GetRunningProcesses();
                    break;
                case "/stopprocess":
                    answer = GetRunningProcesses() + "\r\n Which one?";
                    botState = BotState.KillProc;
                    break;
                case "/startprocess":
                    answer = "\r\n Which one?";
                    botState = BotState.StartProc;
                    break;
                default:
                    answer = "You write to me: '" + message + "', but i dont know what to answer";
                    break;
            }
            SendMessage(chatID, answer);
        }
        private void WriteLog(string text)
        {
            text = DateTime.Now + " " + text + ";" + Environment.NewLine;
            textBoxLog.Text += text;
            File.AppendAllText(fileLog, text);
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
                for (int i = fileLogLenght; i < readingFileLog.Length; i++)
                {
                    answer += readingFileLog[i] + Environment.NewLine;
                }
            }
            return answer;
        }
       
        private void HttpUploadScreen(string url, string file, string paramName, string contentType, NameValueCollection nvc)
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
            string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";

            foreach (string key in nvc.Keys)
            {
                rs.Write(boundaryBytes, 0, boundaryBytes.Length);
                string formitem = string.Format(formdataTemplate, key, nvc[key]);
                byte[] formitembytes = Encoding.UTF8.GetBytes(formitem);
                rs.Write(formitembytes, 0, formitembytes.Length);
            }

            rs.Write(boundaryBytes, 0, boundaryBytes.Length);
            string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
            string header = string.Format(headerTemplate, paramName, file, contentType);
            byte[] headerBytes = Encoding.UTF8.GetBytes(header);
            rs.Write(headerBytes, 0, headerBytes.Length);
            //image file processing
            Bitmap screen = GetPrintScreen();
            MemoryStream fileStream = new MemoryStream();
            screen.Save(fileStream, ImageFormat.Png);
            fileStream.Position = 0;
            byte[] buffer = new byte[4096];
            int bytesRead = 0;
            //writing the image in to the stream
            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
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
                if (wResp != null)
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
            NameValueCollection nvc = new NameValueCollection
            {
                { "chat_id", chatID.ToString() }
            };
            //HttpUploadFile(address, "NuGet.png", "photo", "image/png", nvc);
            HttpUploadScreen(address, ".png", "photo", "image/png", nvc);
        }
        private Bitmap GetPrintScreen()
        {
            //Bitmap screen = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            //If scaling in Windows is set to any value other than 100 %,
            //for some reason the methods stop correctly determining the screen resolution 
            Bitmap screen = new Bitmap(1920, 1080);
            Graphics gr = Graphics.FromImage(screen);
            gr.CopyFromScreen(0, 0, 0, 0, screen.Size);
            return ResizeImg(screen, 1280, 720);
        }
        private Bitmap ResizeImg(Bitmap b, int nWidth, int nHeight)
        {
            Bitmap result = new Bitmap(nWidth, nHeight);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(b, 0, 0, nWidth, nHeight);
            }
            return result;
        }
        private Bitmap ResizeImg(Bitmap b, int numberOfcompression)
        {
            return ResizeImg(b, b.Width / numberOfcompression, b.Height / numberOfcompression);
        }

        private string GetRunningProcesses()
        {
            Process[] procList = Process.GetProcesses();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("List of Processes: ");
            sb.AppendLine(new string('=', 25));
            foreach (Process p in procList)
            {
                if (!string.IsNullOrEmpty(p.MainWindowTitle))
                {
                    sb.AppendLine(p.StartTime + ": " + p.ProcessName + " - " + p.MainWindowTitle);
                }
            }
            sb.AppendLine(new string('=', 25));
            return sb.ToString();
        }
        private bool CloseProcess(string nameProc)
        {
            botState = BotState.Wait;
            Process[] procList = Process.GetProcesses();
            foreach (Process p in procList)
            {
                if (p.ProcessName == nameProc)
                {
                    Process.GetProcessesByName(nameProc)[0].Kill();
                    return true;
                }
            }

            return false;
        }
        private bool StartProcess(string path)
        {
            botState = BotState.Wait;
            if (File.Exists(path))
            {
                Process.Start(path);
                return true;
            }
            else
            {
                return false;
            }

        }
    }
}
