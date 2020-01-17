using Scanner;
using System;

namespace Parser
{
    class Program
    {
        static void Main(string[] args)
        {
            var path = "./input1.txt";
            Scan.Init(path);
            var tree = Expr.GetAstTree();
            var val=Expr.InterpretAstTree(tree);
            Console.WriteLine($"计算结果{val}");
        }
    }
}
