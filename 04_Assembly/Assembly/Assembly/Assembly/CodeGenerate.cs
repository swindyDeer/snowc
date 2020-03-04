using System;
using System.Collections.Generic;
using System.IO;
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
        private static Dictionary<string, int?> RegList = new Dictionary<string, int?>
        {
            { "%r8",null},
            { "%r9",null},
            { "%r10",null},
            { "%r11",null}
        };



        /// <summary>
        /// 生成代码
        /// </summary>
        private static StringBuilder CodeContents = new StringBuilder();

        /// <summary>
        /// 将所有寄存器设为可用
        /// </summary>
        public static void FreeAllRegisters()
        {
            var keys = RegList.Keys.ToArray();
            for(int i=0;i<keys.Length;i++)
            {
                var key = keys[i];
                RegList[key] = null;
            }
        }

        /// <summary>
        /// 分配一个可用的寄存器
        /// </summary>
        /// <returns></returns>
        public static string AllocRegister()
        {
            foreach (var key in RegList.Keys)
            {
                if (RegList[key] == null)
                    return key;
            }

            throw new Exception("没有可用的寄存器");
        }

        /// <summary>
        /// 释放已分配的寄存器
        /// </summary>
        public static void FreeRegister(string key)
        {
            if (RegList.Keys.Contains(key))
                RegList[key] = null;
        }

        /// <summary>
        /// 将整数文字值加载到寄存器中
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string LoadReg(int value)
        {
            var reg = AllocRegister();
            //设为已经占用
            RegList[reg] = value;
            CodeContents.Append($"\tmovq\t{value},{reg}\n");
            return reg;
        }

        /// <summary>
        /// 相加
        /// </summary>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <returns></returns>
        public static string CgAdd(string r1,string r2)
        {
            CodeContents.Append($"\taddq\t{r1},{r2}\n");
            FreeRegister(r1);
            return r2;
        }

        /// <summary>
        /// 相乘
        /// </summary>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <returns></returns>
        public static string CgMul(string r1, string r2)
        {
            CodeContents.Append($"\timulq\t{r1},{r2}\n");
            FreeRegister(r1);
            return r2;
        }

        /// <summary>
        /// 相减
        /// </summary>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <returns></returns>
        public static string CgSub(string r1, string r2)
        {
            CodeContents.Append($"\tsubq\t{r2},{r1}\n");
            FreeRegister(r2);
            return r1;
        }

        /// <summary>
        /// 相除
        /// </summary>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <returns></returns>
        public static string CgDiv(string r1, string r2)
        {
            CodeContents.Append($"\tmovq\t{r1},%rax\n");
            CodeContents.Append("\tcqo\n");
            CodeContents.Append($"\tidivq\t{r2}\n");
            CodeContents.Append($"\tmovq\t%rax,{r1}\n");
            FreeRegister(r2);
            return r1;
        }

        /// <summary>
        /// 对给定的寄存器调用 打印函数printint ()
        /// </summary>
        /// <param name="r"></param>
        public static void CgPrintInt(string r)
        {
            CodeContents.Append($"\tmovq\t{r}, %rdi\n");
            CodeContents.Append("\tcall\tprintint\n");
            FreeRegister(r);
        }

        /// <summary>
        /// 添加汇编代码前文
        /// </summary>
        public static void CgPreamble()
        {
            FreeAllRegisters();
            CodeContents.Append("\t.text\n");
            CodeContents.Append(".LC0:\n");

            CodeContents.Append("\t.string\t\"%d\\n\"\n");
            CodeContents.Append("printint:\n");
            CodeContents.Append("\tpushq\t%rbp\n");

            CodeContents.Append("\tmovq\t%rsp, %rbp\n");
            CodeContents.Append("\tsubq\t$16, %rsp\n");
            CodeContents.Append("\tmovl\t%edi, -4(%rbp)\n");

            CodeContents.Append("\tmovl\t-4(%rbp), %eax\n");
            CodeContents.Append("\tmovl\t%eax, %esi\n");
            CodeContents.Append("\tleaq	.LC0(%rip), %rdi\n");

            CodeContents.Append("\tmovl	$0, %eax\n");
            CodeContents.Append("\tcall	printf@PLT\n");
            CodeContents.Append("\tnop\n");

            CodeContents.Append("\tleave\n");
            CodeContents.Append("\tret\n");
            CodeContents.Append("\n");

            CodeContents.Append("\t.globl\tmain\n");
            CodeContents.Append("\t.type\tmain, @function\n");
            CodeContents.Append("main:\n");

            CodeContents.Append("\tpushq\t%rbp\n");
            CodeContents.Append("\tmovq	%rsp, %rbp\n");
        }

        /// <summary>
        /// 添加汇编代码后文
        /// </summary>
        public static void CgPostamble()
        {
            CodeContents.Append("\tmovl	$0, %eax\n");
            CodeContents.Append("\tpopq	%rbp\n");
            CodeContents.Append("\tret\n");
        }

        /// <summary>
        /// 保存生成的汇编文件
        /// </summary>
        /// <param name="path"></param>
        public static void SaveCodeFile(string path)
        {
            var fileName = $"{DateTime.Now.ToString("yyMMddHHmmss")}.asm";
            path = string.IsNullOrWhiteSpace(path) ? Environment.CurrentDirectory+"/" + fileName : path + fileName;
            File.WriteAllText(path, CodeContents.ToString());
        }
    }
}
