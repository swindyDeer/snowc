using Scanner;
using System;

namespace Precedence
{
    class Program
    {
        static void Main(string[] args)
        {
            var path = "./input1.txt";
            Scan.Init(path);
            var tree = Expr1.GetAstTree();
            var val = Expr1.InterpretAstTree(tree);
            Console.WriteLine($"计算结果{val}");
        }
    }
}
