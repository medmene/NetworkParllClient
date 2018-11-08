#define Multithread
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
////////////////////////////
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.Threading;

namespace NetworkParall
{
    public partial class Form1 : Form
    {
        string filepath = "";
        public string UserSrcCode = "";
        public int numberClt = 1;
        public int progVal2 = 0;
        public int progVal3 = 0;
        public int progVal4 = 0;
        bool work = true;
        

        public Form1() { InitializeComponent(); timer1.Enabled = true; }

        private void button1_Click(object sender, EventArgs e)
        {
#if Multithread
            if (UserSrcCode != "")
            {
                Thread t = new Thread(new ThreadStart(GetClients));
                t.Start();
                button1.Enabled = false;
#if false
                const int port = 8888;
                TcpListener listener = null;
                try
                {
                    listener = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
                    listener.Start();
                    while (true)
                    {
                        TcpClient client = listener.AcceptTcpClient();
                        numberClt++;
                        ClientObject clientObject = new ClientObject(client, numberClt+1, this, textBox1.Text);
                        // создаем новый поток для обслуживания нового клиента
                        Thread clientThread = new Thread(new ThreadStart(clientObject.Process));
                        clientThread.Start();
                    }
                }
                catch (Exception ex) { Console.WriteLine(ex.Message); }
                finally { if (listener != null) listener.Stop(); }
#endif
            }
            else { MessageBox.Show("UserCode is empty!"); }          
#else
                if (UserSrcCode != "")
            {

                // Устанавливаем для сокета локальную конечную точку
                IPHostEntry ipHost = Dns.GetHostEntry("localhost");
                IPAddress ipAddr = ipHost.AddressList[0];
                IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, 11000);
                progressBar1.Enabled = true;
                progressBar2.Enabled = true;
                progressBar3.Enabled = true;

                // Создаем сокет Tcp/Ip
                sListener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                // Назначаем сокет локальной конечной точке и слушаем входящие сокеты
                try
                {
                    sListener.Bind(ipEndPoint);
                    sListener.Listen(10);
                    int someInt = 0; //
                    int curUser = 1; //выделение номера пользователю
                    // Начинаем слушать соединения
                    while (true)
                    {
                        // Программа приостанавливается, ожидая входящее соединение
                        Socket handler = sListener.Accept();
                        string data = null;

                        // Буфер для входящих данных
                        byte[] bytes = new byte[2048];

                        // Мы дождались клиента, пытающегося с нами соединиться
                        int bytesRec = handler.Receive(bytes);

                        data += Encoding.UTF8.GetString(bytes, 0, bytesRec);
                        /////////////////////////////////////////////////////
                        /////////////////////////////////////////////////////
                        /////////////////////////////////////////////////////
                        //первое присоединение
                        if (data == "dvc_firstConn")
                        {
                            byte[] msg = Encoding.UTF8.GetBytes(curUser.ToString() + UserSrcCode);
                            curUser++;
                            handler.Send(msg);
                        }
                        //отправлять результат
                        else if (data == "sndAnsw")
                        {
                            if (checkBox1.Checked == true)
                            {
                                byte[] msg = Encoding.UTF8.GetBytes("sndRes");
                                handler.Send(msg);
                            }
                            else
                            {
                                byte[] msg = Encoding.UTF8.GetBytes("DontSndRes");
                                handler.Send(msg);
                            }
                        }
                        //количество посылок
                        else if (data == "CountPrc")
                        {
                            if (textBox1.Text != "" && Int32.TryParse(textBox1.Text, out someInt))
                            {
                                byte[] msg = Encoding.UTF8.GetBytes(textBox1.Text);
                                handler.Send(msg);
                            }
                            else
                            {
                                byte[] msg = Encoding.UTF8.GetBytes("100000");
                                handler.Send(msg);
                            }
                        }
                        //процент выполнения
                        else {
                            string[] ss = data.Split('_');
                            if (Int32.TryParse(ss[0], out someInt))
                            {
                                if (ss[1] == "2") progressBar1.Value = someInt;
                                if (ss[1] == "3") progressBar2.Value = someInt;
                                if (ss[1] == "4") progressBar3.Value = someInt;
                                byte[] msg = Encoding.UTF8.GetBytes("good");
                                handler.Send(msg);
                            }
                            //отключение
                            else if (data.IndexOf("<TheEnd>") > -1)
                            {
                                richTextBox1.Text += "Сервер завершил соединение с клиентом." + data[data.Length - 1];
                                break;
                            }
                        }

                        handler.Shutdown(SocketShutdown.Both);
                        handler.Close();
                    }
                }
                catch (Exception ex) { richTextBox1.Text += (ex.ToString()); }
            }
            else richTextBox1.Text += "User code is empty!\n";
#endif
        }

        void GetClients()
        {
            const int port = 8888;
            TcpListener listener = null;
            try
            {
                listener = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
                listener.Start();

                while (work)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    //numberClt++;
                    ClientObject clientObject = new ClientObject(client, numberClt + 1, this, textBox1.Text);

                    // создаем новый поток для обслуживания нового клиента
                    Thread clientThread = new Thread(new ThreadStart(clientObject.Process));
                    clientThread.Start();
                    if (numberClt == 0) { work = false; button1.Enabled = true; }
                }
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
            finally { if (listener != null) listener.Stop(); }
        }

        private void button2_Click(object sender, EventArgs e){
            OpenFileDialog od = new OpenFileDialog();
            if (od.ShowDialog() == DialogResult.OK){
                filepath = od.FileName;
                using (StreamReader sr = new StreamReader(filepath)){
                    UserSrcCode= sr.ReadToEnd();
                }
                richTextBox1.Text += "Source code loaded!\n";
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            UserCode uc = new UserCode();
            Data.Value = "";
            uc.ShowDialog();
            if (Data.Value != ""){
                UserSrcCode = Data.Value;
                richTextBox1.Text += "Source code loaded!\n";
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            progressBar2.Value = progVal2;
            progressBar3.Value = progVal3;
            progressBar4.Value = progVal4;
        }
    }
}
