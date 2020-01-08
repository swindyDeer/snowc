using System;
using System.Collections.Generic;
using System.Text;

namespace Scanner
{
    /// <summary>
    /// token
    /// </summary>
    public struct Token
    {
        public TokenType TokenType { get; set; }

        public int IntValue { get; set; }
    }

    /// <summary>
    /// token类型
    /// </summary>
    public enum TokenType
    {
        PLUS,//+ 
        MINUS,//- 
        STAR,//* 
        SLASH,/// 
        INTLIT//数字
    }
}
