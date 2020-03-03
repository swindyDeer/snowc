using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assembly
{
    /// <summary>
    /// 汇编代码生成器
    /// </summary>
    public class CodeGenerate
    {
        /// <summary>
        /// 寄存器列表
        /// </summary>
        private static Dictionary<string, int> RegList = new Dictionary<string, int>
        {
            { "%r8",0},
            { "%r9",0},
            { "%r10",0},
            { "%r11",0}
        };

        /// <summary>
        /// 生成代码
        /// </summary>
        private static StringBuilder CodeContents = new StringBuilder();

        /// <summary>
        /// 将所有寄存器设为可用
        /// </summary>
        public void FreeAllRegisters()
        {
            foreach(var key in RegList.Keys)
            {
                RegList[key] = 0;
            }
        }

        /// <summary>
        /// 分配一个可用的寄存器
        /// </summary>
        /// <returns></returns>
        public string AllocRegister()
        {
            foreach (var key in RegList.Keys)
            {
                if (RegList[key] == 0)
                    return key;
            }

            throw new Exception("没有可用的寄存器");
        }

        /// <summary>
        /// 释放已分配的寄存器
        /// </summary>
        public void FreeRegister(string key)
        {
            if (RegList.Keys.Contains(key))
                RegList[key] = 0;
        }

        /// <summary>
        /// 将整数文字值加载到寄存器中
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public void LoadReg(int value)
        {
            var reg = AllocRegister();
            CodeContents.Append($"\tmovq\t{value},{reg}\n");
        }

        /// <summary>
        /// 相加
        /// </summary>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <returns></returns>
        public string CgAdd(string r1,string r2)
        {
            CodeContents.Append($"\taddq\t{RegList[r1]},{RegList[r2]}\n");
            FreeRegister(r1);
            return r2;
        }

        /// <summary>
        /// 相乘
        /// </summary>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <returns></returns>
        public string CgMul(string r1, string r2)
        {
            CodeContents.Append($"\timulq\t{RegList[r1]},{RegList[r2]}\n");
            FreeRegister(r1);
            return r2;
        }

        /// <summary>
        /// 相减
        /// </summary>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <returns></returns>
        public string CgSub(string r1, string r2)
        {
            CodeContents.Append($"\tsubq\t{RegList[r2]},{RegList[r1]}\n");
            FreeRegister(r2);
            return r1;
        }

        /// <summary>
        /// 相除
        /// </summary>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <returns></returns>
        public string CgDiv(string r1, string r2)
        {
            CodeContents.Append($"\tmovq\t{RegList[r1]},%rax\n");
            CodeContents.Append("\tcqo\n");
            CodeContents.Append($"\tidivq\t{RegList[r2]}\n");
            CodeContents.Append($"\tmovq\t%rax,{RegList[r1]}\n");
            FreeRegister(r2);
            return r1;
        }
    }
}
