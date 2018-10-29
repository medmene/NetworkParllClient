﻿#define Multithread
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.IO;
using System.Diagnostics;

namespace SocketClient
{
    class Program
    {
#if Multithread
        const int port = 8888;
        const string address = "127.0.0.1";
        static void Main(string[] args)
        {
            //Console.Write("Введите свое имя:");
            string userName = Console.ReadLine();
            TcpClient client = null;
            try
            {
                client = new TcpClient(address, port);
                NetworkStream stream = client.GetStream();
 
                while (true)
                {
                    Console.Write(userName + ": ");
                    // ввод сообщения
                    string message = Console.ReadLine();
                    message = String.Format("{0}: {1}", userName, message);
                    // преобразуем сообщение в массив байтов
                    byte[] data = Encoding.Unicode.GetBytes(message);
                    // отправка сообщения
                    stream.Write(data, 0, data.Length);
 
                    // получаем ответ
                    data = new byte[64]; // буфер для получаемых данных
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (stream.DataAvailable);
 
                    message = builder.ToString();
                    Console.WriteLine("Сервер: {0}", message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                client.Close();
            }
        }
#else
        static void Main(string[] args)
        {
            try
            {
                SendMessageFromSocket(11000);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                Console.ReadLine();
            }
        }

        static string SendMsg(string msg)
        {
            // Буфер для входящих данных
            byte[] bytes = new byte[2048];

            // Соединяемся с удаленным устройством

            // Устанавливаем удаленную точку для сокета
            IPHostEntry ipHost = Dns.GetHostEntry("localhost");
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, 11000);

            Socket sender = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // Соединяем сокет с удаленной точкой
            sender.Connect(ipEndPoint);

            string message = msg;

            //Console.WriteLine("Сокет соединяется с {0} ", sender.RemoteEndPoint.ToString());
            byte[] msga = Encoding.UTF8.GetBytes(message);

            // Отправляем данные через сокет
            int bytesSent = sender.Send(msga);

            // Получаем ответ от сервера
            int bytesRec = sender.Receive(bytes);

            string answ = Encoding.UTF8.GetString(bytes, 0, bytesRec);

            //Освобождаем сокет
            sender.Shutdown(SocketShutdown.Both);
            sender.Close();
            return answ;
        }

        static void CompileFile(string UserSrcCode)
        {
            /////////////////////////////////////////////
            ////////////GENERATE execute FILE////////////
            /////////////////////////////////////////////
            Dictionary<string, string> providerOptions = new Dictionary<string, string>
                {
                    {"CompilerVersion", "v3.5"}
                };
            CSharpCodeProvider provider = new CSharpCodeProvider(providerOptions);

            //создание директории,если её нет
            if (!Directory.Exists("C:\\11")) { Directory.CreateDirectory("C:\\11"); }

            CompilerParameters compilerParams = new CompilerParameters
            { OutputAssembly = "C:\\11\\Foo.EXE", GenerateExecutable = true };
            compilerParams.ReferencedAssemblies.Add("System.Core.Dll");

            // Компиляция 
            CompilerResults results = provider.CompileAssemblyFromSource(compilerParams, UserSrcCode);

            // Выводим информацию об ошибках 
            using (StreamWriter outputFile = new StreamWriter("CompileLog.txt"))
            {
                outputFile.WriteLine("Number of Errors: {0}", results.Errors.Count);
                foreach (CompilerError err in results.Errors)
                {
                    //richTextBox1.Text += "ERROR " + err.ErrorText + "\n";
                    outputFile.WriteLine("ERROR {0}", err.ErrorText);
                }
            }
            /////////////////////////////////////////////
            ////////////GENERATE execute FILE////////////
            /////////////////////////////////////////////
        }

        static void SendMessageFromSocket(int port)
        {
            /*
            * Message types:
            * dvc_firstConn - first connection
            * sndAnsw - return result
            * CountPrc - number of parcels
            * 67_1 - percent of work
            */
            //получаем код программы
            string UserSrcCode = SendMsg("dvc_firstConn"); 
            //номер этой машины
            char number = UserSrcCode[0]; UserSrcCode = UserSrcCode.Substring(1);
            //отправлять ответ?
            bool sndRes = (SendMsg("sndAnsw") == "sndRes") ? true : false;
            //получаем количество посылок
            string args = SendMsg("CountPrc");
            
            if (sndRes) UserSrcCode = "#define result\n" + UserSrcCode;
            
            //Компиляция файла
            CompileFile(UserSrcCode);

            Process execProg = new Process();
            ProcessStartInfo msi = new ProcessStartInfo("C:\\11\\Foo.EXE")
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                Arguments = args,
                WorkingDirectory = Path.GetDirectoryName("C:\\11\\Foo.EXE"),
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };
            //run de process
            execProg.StartInfo = msi;
            execProg.Start();

            bool local = false;
            string res="";
            while (true)
            {
                string output = execProg.StandardOutput.ReadLine();
                SendMsg(output + "_" + number);
                Console.WriteLine(output);
                if (local) res += output + "\n";
                if (output == "Done") local = true;
                if (output == null) break;
            }

            execProg.WaitForExit();
#if false
            //Console.WriteLine("\nОтвет от сервера: {0}\n\n", Encoding.UTF8.GetString(bytes, 0, bytesRec));
            // Используем рекурсию для неоднократного вызова SendMessageFromSocket()
            //if (message.IndexOf("<TheEnd>") == -1)
            //SendMessageFromSocket(port);
#endif

        }

#endif
    }
}