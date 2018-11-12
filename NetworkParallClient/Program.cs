#define Multithread
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

#if Multithread
        static string SendMsg(NetworkStream stream, string msg)
        {

            // преобразуем сообщение в массив байтов
            byte[] data = Encoding.Unicode.GetBytes(msg);
            // отправка сообщения
            stream.Write(data, 0, data.Length);

            // получаем ответ
            data = new byte[2048]; // буфер для получаемых данных
            StringBuilder builder = new StringBuilder();
            int bytes = 0;
            do
            {
                bytes = stream.Read(data, 0, data.Length);
                builder.Append(Encoding.UTF8.GetString(data, 0, bytes));
            }
            while (stream.DataAvailable);

            msg = builder.ToString();
            return msg;
        }
#endif

#if Multithread
        const int port = 8888;
        const string address = /*"192.168.0.83"*/"127.0.0.1";
        static void Main(string[] args)
        {
            TcpClient client = null;
            try
            {
                client = new TcpClient(address, port);
                NetworkStream stream = client.GetStream();

                /*
                * Message types:
                * dvc_firstConn - first connection
                * sndAnsw - return result
                * CountPrc - number of parcels
                * 67_1 - percent of work
                */
                //получаем код программы
                string UserSrcCode = SendMsg(stream, "dvc_firstConn");
                //номер этой машины
                char number = UserSrcCode[0]; UserSrcCode = UserSrcCode.Substring(1);
                //отправлять ответ?
                bool sndRes = (SendMsg(stream, "sndAnsw") == "sndRes") ? true : false;
                //получаем количество посылок
                string argss = SendMsg(stream, "CountPrc");


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
                    Arguments = argss,
                    WorkingDirectory = Path.GetDirectoryName("C:\\11\\Foo.EXE"),
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                };
                //run de process
                execProg.StartInfo = msi;
                execProg.Start();

                while (true)
                {
                    string output = execProg.StandardOutput.ReadLine();
                    if (output == "Done")
                    {
                        // преобразуем сообщение в массив байтов
                        byte[] data = Encoding.UTF8.GetBytes("<TheEnd>");
                        // отправка сообщения
                        stream.Write(data, 0, data.Length);
                        break;
                    }
                    if (output == null) break;
                    SendMsg(stream, output + "_" + number);
                    Console.WriteLine(output);
                }

                //execProg.WaitForExit();
#if false
            //Console.WriteLine("\nОтвет от сервера: {0}\n\n", Encoding.UTF8.GetString(bytes, 0, bytesRec));
            // Используем рекурсию для неоднократного вызова SendMessageFromSocket()
            //if (message.IndexOf("<TheEnd>") == -1)
            //SendMessageFromSocket(port);
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
#endif                
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
#endif
    }
}
