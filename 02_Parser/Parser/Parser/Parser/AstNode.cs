using System;
using System.Collections.Generic;
using System.Text;

namespace Parser
{
    /// <summary>
    /// ast节点
    /// </summary>
    public class AstNode
    {
        /// <summary>
        /// 节点类型
        /// </summary>
        public NodeType NodeType { get; set; }

        /// <summary>
        /// 左节点
        /// </summary>
        public AstNode LeftNode { get; set; }

        /// <summary>
        /// 右节点
        /// </summary>
        public AstNode RightNode { get; set; }

        /// <summary>
        /// 整数数值
        /// </summary>
        public int Value { get; set; }
    }
}
