using Precedence;
using Scanner;
using System;

namespace Assembly
{
    class Program
    {
        static void Main(string[] args)
        {
            var path = "./input1.txt";
            Scan.Init(path);
            var tree = Expr1.GetAstTree();
            Expr.GenCode(tree);
        }
    }
}
