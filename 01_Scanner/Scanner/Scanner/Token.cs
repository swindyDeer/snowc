using System;
using System.Collections.Generic;
using System.Text;

namespace Scanner
{
    /// <summary>
    /// token
    /// </summary>
    public class Token
    {
        public TokenType TokenType { get; set; }

        public int IntValue { get; set; }
    }

    /// <summary>
    /// token类型
    /// </summary>
    public enum TokenType
    {
        /// <summary>
        /// +
        /// </summary>
        PLUS,

        /// <summary>
        /// -
        /// </summary>
        MINUS,

        /// <summary>
        /// *
        /// </summary>
        STAR,

        /// <summary>
        /// /
        /// </summary>
        SLASH,

        /// <summary>
        /// 数字
        /// </summary>
        INTLIT
    }
}
