using Parser;
using System;

namespace Assembly
{
    public class Expr
    {
        //给定AST，生成递归汇编代码
        public static string GenAST(AstNode node)
        {
            string leftReg=null, rightReg=null;

            if (node.LeftNode != null)
                leftReg = GenAST(node.LeftNode);

            if (node.RightNode != null)
                rightReg = GenAST(node.RightNode);

            switch (node.NodeType)
            {
                case NodeType.ADD:
                    return CodeGenerate.CgAdd(leftReg, rightReg);
                case NodeType.SUBTRACT:
                    return CodeGenerate.CgSub(leftReg, rightReg);
                case NodeType.MULTIPLY:
                    return CodeGenerate.CgMul(leftReg, rightReg);
                case NodeType.DIVIDE:
                    if (rightReg != null)
                    {
                        return CodeGenerate.CgDiv(leftReg, rightReg); ;
                    }
                    else
                        throw new ArgumentException(nameof(rightReg));
                case NodeType.INTLIT:
                    return CodeGenerate.LoadReg(node.Value);
                default:
                    throw new Exception($"未知的ast类型：{node.NodeType.ToString()}");
            }

        }

        /// <summary>
        /// 生成代码
        /// </summary>
        /// <param name="node"></param>
        public static void GenCode(AstNode node)
        {
            CodeGenerate.CgPreamble();
            var reg = GenAST(node);
            CodeGenerate.CgPrintInt(reg);
            CodeGenerate.CgPostamble();
            CodeGenerate.SaveCodeFile(null);
        }
    }
}
