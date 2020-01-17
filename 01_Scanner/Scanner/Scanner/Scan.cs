using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Scanner
{
    public class Scan
    {
        /// <summary>
        /// 行号
        /// </summary>
        protected static int Line { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        protected static string Content { get; set; }

        /// <summary>
        /// 索引
        /// </summary>
        protected static int Index { get; set; }

        /// <summary>
        /// 整数字符串
        /// </summary>
        protected static string IntStr = "0123456789";

        /// <summary>
        /// 流
        /// </summary>
        protected static StreamReader StreamReader { get; set; }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="path"></param>
        public static void Init(string path)
        {
            var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            StreamReader = new StreamReader(fs, Encoding.Default);
            Content = StreamReader.ReadToEnd();
            Line = 0;
            Index = 0;
        }

        /// <summary>
        /// 获取下一个字符
        /// </summary>
        /// <returns></returns>
        private static char? Next()
        {
            //读取结束
            if (Index == Content.Length)
            {
                return null;
            }

            var n = Content[Index++];
            if (n == '\n')
            {
                Line++;
            }

            return n;
        }

        /// <summary>
        /// 跳过空白字符
        /// </summary>
        /// <returns></returns>
        private static char? Skip()
        {
            var c = Next();
            while (' ' == c || '\t' == c || '\n' == c || '\r' == c || '\f' == c)
            {
                c = Next();
            }
            return c;
        }

        /// <summary>
        /// 执行扫描
        /// </summary>
        /// <returns>isEffect 是否有效token</returns>
        public static Token ExecScan()
        {
            var c = Skip();

            if (c == null)
                return null;

            switch (c)
            {
                case ' ':
                    return null;
                case '+':
                    return new Token() { TokenType = TokenType.PLUS };
                case '-':
                    return new Token() { TokenType = TokenType.MINUS };
                case '*':
                    return new Token() { TokenType = TokenType.STAR };
                case '/':
                    return new Token() { TokenType = TokenType.SLASH };
                default:
                    if(CheckInt(c))
                    {
                        var val = ScanInt(c);
                        return  new Token() { TokenType = TokenType.INTLIT,IntValue=val };
                    }
                    else
                    {
                        Console.WriteLine($"未解析字符{c}");
                        return new Token();
                    }
                    
            }
        }

        /// <summary>
        /// 是否为整数
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private static bool CheckInt(char? c)
        {
            if (c == null)
                return false;
            
            return IntStr.Contains(c.ToString());
        }

        /// <summary>
        /// 整数扫描
        /// </summary>
        /// <returns></returns>
        public static int ScanInt(char? c)
        {
            int k, val = 0;
            while(CheckInt(c))
            {
                k = Convert.ToInt32(c.ToString());
                val = val * 10 + k;
                c = Next();
            }

            return val;
        }

        /// <summary>
        /// 扫描打印
        /// </summary>
        public static void ScanPrint()
        {
            var token = ExecScan();
            while(token != null)
            {
                
                if(token.TokenType==TokenType.INTLIT)
                    Console.WriteLine($"Token: { token.TokenType.ToString()},value: {token.IntValue}");
                else
                    Console.WriteLine($"Token: { token.TokenType.ToString()}");
                token = ExecScan();
            }

        }

    }
}

