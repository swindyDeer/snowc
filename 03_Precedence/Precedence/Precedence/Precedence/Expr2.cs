using Parser;
using Scanner;
using System;
using System.Collections.Generic;
using static Scanner.Scan;
using System.Text;

namespace Precedence
{
    public class Expr2
    {
        /// <summary>
        /// 全局token
        /// </summary>
        private static Token token { get; set; }

        /// <summary>
        /// 运算符优先级表
        /// </summary>
        private static Dictionary<NodeType, int> PrecedenceDic = new Dictionary<NodeType, int>
        {
            { NodeType.INTLIT, 0 },
            { NodeType.ADD, 10 },
            { NodeType.SUBTRACT, 10 },
            { NodeType.MULTIPLY, 20 },
            { NodeType.DIVIDE, 20 },
        };


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
                case TokenType.INTLIT:
                    return (NodeType.INTLIT);
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
                    return node;
                default:
                    throw new Exception($"token类型不是整数：{token.TokenType.ToString()}");
            }

        }

        /// <summary>
        /// 返回AstTree
        /// </summary>
        /// <param name="ptp">优先级</param>
        /// <returns></returns>
        public static AstNode GetAstTree(int ptp = 0)
        {
            AstNode left, right;
            NodeType nodeType;

            token = ExecScan();
            Console.WriteLine($"节点类型：{token?.TokenType.ToString()},节点值：{token?.IntValue}");

            //第一个节点为左节点
            left = GetAstLeafNode(token);

            token = ExecScan();
            Console.WriteLine($"节点类型：{token?.TokenType.ToString()},节点值：{token?.IntValue}");

            //如果扫描结束
            if (token == null)
                return left;

            //运算符节点类型
            nodeType = ConvertToNodeType(token.TokenType);

            while (PrecedenceDic[nodeType] > ptp)
            {

                //优先级高的构建为右子树，遇到低优先级的运算符返回
                right = GetAstTree(PrecedenceDic[nodeType]);

                //合并子树
                left = AstTree.MkAstNode(nodeType, left, right, 0);

                //如果扫描结束
                if (token == null)
                    break;

                //更新当前令牌的详细信息,即低优先级令牌
                nodeType = ConvertToNodeType(token.TokenType);
                Console.WriteLine($"低优先级令牌:{PrecedenceDic[nodeType]}");
            }

            return left;
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
