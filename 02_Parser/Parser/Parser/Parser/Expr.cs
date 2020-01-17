using Scanner;
using System;
using static Scanner.Scan;

namespace Parser
{
    public class Expr
    {
        /// <summary>
        /// 将令牌转换为AST操作
        /// </summary>
        /// <param name="tokenType"></param>
        /// <returns></returns>
        public static NodeType ConvertToNodeType(TokenType tokenType)
        {
            switch(tokenType)
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
        public static AstNode GetAstLeafNode(Token token)
        {
            switch(token.TokenType)
            {
                //对于INTLIT令牌，为其创建叶节点
                //并扫描下一个令牌。否则，对于任何其他令牌类型输出语法错误
                case TokenType.INTLIT:
                    var node = AstTree.MkAstLeafNode(NodeType.INTLIT,token.IntValue);
                    //ScanResult = Scan.ExecScan();
                    return node;
                default:
                    throw new Exception($"token类型不是整数：{token.TokenType.ToString()}");
            }

        }

        /// <summary>
        /// 获取Ast树
        /// </summary>
        /// <returns></returns>
        public static AstNode GetAstTree()
        {
            AstNode node, left, right;
            NodeType nodeType;

            var token = ExecScan();
            //第一个节点为左节点
            left = GetAstLeafNode(token);

            //获取下一个token
            token = ExecScan();

            //如果扫描结束
            if (token == null)
                return left;

            //记录根节点类型
            nodeType = ConvertToNodeType(token.TokenType);

            //获取下一个token
            token = ExecScan();

            //递归得到右子树
            right = GetAstTree();

            //合并左右子树
            node = AstTree.MkAstNode(nodeType,left,right,0);

            return node;
        }

    }
}
