using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Pipes;
using Modem.Amt.Export.Utility;

namespace Modem.Amt.Export
{
    public class DataProcess
    {
        public static decimal[] TestConnectionProcess()
        {
            var random = new Random();
            var randomArray = new decimal[5];
            for (int i = 0; i < 5; i++) randomArray[i] = random.Next(0, 100);
            return randomArray;
        }

        public static decimal[] PipeConnectionProcess(NamedPipeClientStream pipeClient)
        {
            StreamString reader = new StreamString(pipeClient);
            var s = reader.ReadString();
            if (s.Equals("end")) return null;
            List<string> parsedNumbers = s.Split(' ').ToList();
            List<decimal> numbers = parsedNumbers.Select(x => Decimal.Parse(x)).ToList();
            return numbers.ToArray();
        }
    }
}
