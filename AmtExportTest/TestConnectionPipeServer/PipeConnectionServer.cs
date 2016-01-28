using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.IO.Pipes;
using Modem.Amt.Export.Utility;

namespace TestConnectionPipeServer
{
    public class PipeConnectionServer
    {
        static void Main(string[] args)
        {
            NamedPipeServerStream pipeServer = new NamedPipeServerStream("PipeConnection", PipeDirection.InOut, 4, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
            pipeServer.WaitForConnection();
            Console.WriteLine("Connected");
            Console.WriteLine("Enter data in format - %d %d %d ... (or \"end\" to finish input)");
            StreamString ss = new StreamString(pipeServer);
            string input = Console.ReadLine();
            while (!input.Equals("end"))
            {
                ss.WriteString(input);
                Console.WriteLine("Data sent");
                input = Console.ReadLine();
                //Thread.Sleep(1000);
            }
            ss.WriteString("end");
            pipeServer.Close();
        }
    }
}
