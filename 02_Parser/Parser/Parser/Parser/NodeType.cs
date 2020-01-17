using System;
using System.Collections.Generic;
using System.Text;

namespace Parser
{
    /// <summary>
    /// ast节点类型
    /// </summary>
    public enum NodeType
    {
        /// <summary>
        /// +
        /// </summary>
        ADD,

        /// <summary>
        /// -
        /// </summary>
        SUBTRACT, 

        /// <summary>
        /// *
        /// </summary>
        MULTIPLY, 

        /// <summary>
        /// /
        /// </summary>
        DIVIDE, 

        /// <summary>
        /// 整数
        /// </summary>
        INTLIT
    }
}
