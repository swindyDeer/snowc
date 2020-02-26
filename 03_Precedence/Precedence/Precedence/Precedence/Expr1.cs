using Parser;
using Scanner;
using static Scanner.Scan;
using System;

namespace Precedence
{
    public class Expr1
    {
        /// <summary>
        /// 全局token
        /// </summary>
        private static Token token { get; set; }

        /// <summary>
        /// 将令牌类型转换为AST类型操作
        /// </summary>
        /// <param name="tokenType"></param>
        /// <returns></returns>
        private static NodeType ConvertToNodeType(TokenType tokenType)
        {
            switch (tokenType)
            {
                case TokenType.PLUS:
                    return (NodeType.ADD);
                case TokenType.MINUS:
                    return (NodeType.SUBTRACT);
                case TokenType.STAR:
                    return (NodeType.MULTIPLY);
                case TokenType.SLASH:
                    return (NodeType.DIVIDE);
                default:
                    throw new Exception($"未知token类型{tokenType}");
            }
        }

        /// <summary>
        /// 获取ast叶节点
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private static AstNode GetAstLeafNode(Token token)
        {
            switch (token.TokenType)
            {
                //对于INTLIT令牌，为其创建叶节点
                //并扫描下一个令牌。否则，对于任何其他令牌类型输出语法错误
                case TokenType.INTLIT:
                    var node = AstTree.MkAstLeafNode(NodeType.INTLIT, token.IntValue);
                    //ScanResult = Scan.ExecScan();
                    return node;
                default:
                    throw new Exception($"token类型不是整数：{token.TokenType.ToString()}");
            }

        }

        /// <summary>
        /// 返回一个根为“ * ”或“ / ”二进制运算符的 AST 树
        /// </summary>
        /// <returns></returns>
        private static AstNode GetMultiplicativeAstTree()
        {
            AstNode left, right;
            NodeType nodeType;

            token = ExecScan();
            Console.WriteLine($"节点类型：{token?.TokenType.ToString()},节点值：{token?.IntValue}");
            //第一个节点为左节点
            left = GetAstLeafNode(token);

            //获取下一个token,正常情况应为运算符
            token = ExecScan();
            Console.WriteLine($"节点类型：{token?.TokenType.ToString()},节点值：{token?.IntValue}");

            //如果扫描结束
            if (token == null)
                return left;

            // 当token类型是*或/
            while (token.TokenType == TokenType.STAR || token.TokenType == TokenType.SLASH)
            {
                nodeType = ConvertToNodeType(token.TokenType);

                //获取下一个token
                token = ExecScan();
                Console.WriteLine($"节点类型：{token?.TokenType.ToString()},节点值：{token?.IntValue}");
                right = GetAstLeafNode(token);

                //合并左右节点
                left = AstTree.MkAstNode(nodeType, left, right, 0);

                //获取下一节点
                token = ExecScan();
                Console.WriteLine($"节点类型：{token?.TokenType.ToString()},节点值：{token?.IntValue}");
                //如果扫描结束
                if (token == null)
                    break;
            }

            return left;
        }

        /// <summary>
        /// 返回一个根为“ +”或“ - ”二进制运算符的 AST 树
        /// </summary>
        /// <returns></returns>
        private static AstNode GetAdditiveAstTree()
        {
            AstNode left, right;
            NodeType nodeType;

            //获取更高优先级的左子树
            left = GetMultiplicativeAstTree();

            //token,正常情况应为运算符+或-运算符或终止符
            //如果扫描结束
            if (token == null)
                return left;

            while (true)
            {
                nodeType = ConvertToNodeType(token.TokenType);

                //获取优先级更高的右子树，返回时可以明确是遇到了+或-运算符或终止符
                right = GetMultiplicativeAstTree();

                left = AstTree.MkAstNode(nodeType, left, right, 0);

                //token,正常情况应为运算符+或-运算符或终止符
                //如果扫描结束
                if (token == null)
                    break;
            }

            return left;
        }

        /// <summary>
        /// 返回AstTree
        /// </summary>
        /// <returns></returns>
        public static AstNode GetAstTree()
        {
            return GetAdditiveAstTree();
        }

        /// <summary>
        /// 给定AST，操作运算符并返回最终值
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static int InterpretAstTree(AstNode node)
        {
            int leftVal = 0, rightVal = 0;

            if (node.LeftNode != null)
                leftVal = InterpretAstTree(node.LeftNode);

            if (node.RightNode != null)
                rightVal = InterpretAstTree(node.RightNode);

            switch (node.NodeType)
            {
                case NodeType.ADD:
                    Console.WriteLine($"计算值:{leftVal}+{rightVal}={leftVal + rightVal}");
                    return leftVal + rightVal;
                case NodeType.SUBTRACT:
                    Console.WriteLine($"计算值:{leftVal}-{rightVal}={leftVal - rightVal}");
                    return leftVal - rightVal;
                case NodeType.MULTIPLY:
                    Console.WriteLine($"计算值:{leftVal}*{rightVal}={leftVal * rightVal}");
                    return leftVal * rightVal;
                case NodeType.DIVIDE:
                    if (rightVal != 0)
                    {
                        Console.WriteLine($"计算值:{leftVal}/{rightVal}={leftVal / rightVal}");
                        return leftVal / rightVal;
                    }
                    else
                        throw new ArgumentException(nameof(rightVal));
                case NodeType.INTLIT:
                    return node.Value;
                default:
                    throw new Exception($"未知的ast类型：{node.NodeType.ToString()}");
            }

        }
    }
}
