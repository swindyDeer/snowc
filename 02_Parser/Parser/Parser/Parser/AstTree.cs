namespace Parser
{
    public class AstTree
    {
        /// <summary>
        /// 创建节点
        /// </summary>
        /// <param name="nodeType"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static AstNode MkAstNode(NodeType nodeType,AstNode left,AstNode right,int value)
        {
            var node = new AstNode();

            node.NodeType = nodeType;
            node.LeftNode = left;
            node.RightNode = right;
            node.Value = value;

            return node;
        }

        /// <summary>
        /// 创建无子节点的叶子节点
        /// </summary>
        /// <param name="nodeType"></param>
        /// <param name="left"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static AstNode MkAstLeafNode(NodeType nodeType,int value)
        {
            return MkAstNode(nodeType,null,null,value);
        }

        /// <summary>
        /// 创建只有一个子节点的节点（子节点默认为左节点）
        /// </summary>
        /// <param name="nodeType"></param>
        /// <param name="left"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static AstNode MkAstUnary(NodeType nodeType, AstNode left, int value)
        {
            return MkAstNode(nodeType,left,null,value);
        }

    }
}
