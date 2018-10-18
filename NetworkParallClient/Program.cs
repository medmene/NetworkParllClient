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
        

        static void SendMessageFromSocket(int port)
        {            
            // Буфер для входящих данных
            byte[] bytes = new byte[2048];

            // Соединяемся с удаленным устройством

            // Устанавливаем удаленную точку для сокета
            IPHostEntry ipHost = Dns.GetHostEntry("localhost");
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, port);

            Socket sender = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // Соединяем сокет с удаленной точкой
            sender.Connect(ipEndPoint);

            //Console.Write("Введите сообщение: ");
            string message = "dvc_firstConn";

            Console.WriteLine("Сокет соединяется с {0} ", sender.RemoteEndPoint.ToString());
            byte[] msg = Encoding.UTF8.GetBytes(message);

            // Отправляем данные через сокет
            int bytesSent = sender.Send(msg);

            // Получаем ответ от сервера
            int bytesRec = sender.Receive(bytes);

            string UserSrcCode = Encoding.UTF8.GetString(bytes, 0, bytesRec);

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

            // Компиляция 
            CompilerResults results = provider.CompileAssemblyFromSource(compilerParams, UserSrcCode);
            //richTextBox1.Text += "Compiled\n";

            // Выводим информацию об ошибках 
            using (StreamWriter outputFile = new StreamWriter("CompileLog.txt"))
            {
                outputFile.WriteLine("Number of Errors: {0}", results.Errors.Count);
                Console.WriteLine("Number of Errors: {0}", results.Errors.Count);
                foreach (CompilerError err in results.Errors)
                {
                    //richTextBox1.Text += "ERROR " + err.ErrorText + "\n";
                    outputFile.WriteLine("ERROR {0}", err.ErrorText);
                    Console.WriteLine("ERROR {0}", err.ErrorText);
                }
            }
            /////////////////////////////////////////////
            ////////////GENERATE execute FILE////////////
            /////////////////////////////////////////////

            Process execProg = new Process();
            ProcessStartInfo msi = new ProcessStartInfo("C:\\11\\Foo.EXE")
            {
                UseShellExecute = false,
                CreateNoWindow = true, //false
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                //Arguments = String.Join(" ", args),
                WorkingDirectory = Path.GetDirectoryName("C:\\11\\Foo.EXE"),
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };
            //run de process
            execProg.EnableRaisingEvents = true;
            execProg.StartInfo = msi;
            execProg.Start();
            if (msi.RedirectStandardOutput) execProg.BeginOutputReadLine();
            if (msi.RedirectStandardError) execProg.BeginErrorReadLine();

#if false
            //Console.WriteLine("\nОтвет от сервера: {0}\n\n", Encoding.UTF8.GetString(bytes, 0, bytesRec));
            // Используем рекурсию для неоднократного вызова SendMessageFromSocket()
            //if (message.IndexOf("<TheEnd>") == -1)
            //SendMessageFromSocket(port);
#endif

            // Освобождаем сокет
            sender.Shutdown(SocketShutdown.Both);
            sender.Close();


            //while (true)
            //{

            //}
        }
        
    }
}