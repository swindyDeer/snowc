using System;
using System.IO;
using System.Text;

namespace Scanner
{
    class Program
    {
        static void Main(string[] args)
        {
            var path = "./input5.txt";
            Scan.Init(path);
            Scan.ScanPrint();
            Console.WriteLine("exit");
        }
    }
}
