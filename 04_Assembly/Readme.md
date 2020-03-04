# Part 4 真正的编译器

是时候编写一个编译器了。 因此，在这部分中，我们将用生成 x86-64汇编代码的代码来替换程序中的解释器 。

## 修改解释器

在此之前，我梦有必要重新研究InterpretAstTree中的解释器代码：

```c#
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
```

 	函数的作用是: 首先对给定的 AST 树进行深度遍历（ 后序遍历——按照“左孩子-右孩子-根节点”的顺序进行访问  ）。 它先计算任何左子树，然后计算右子树。 最后，它使用当前树底部的 op 值对这些子节点进行操作 。

​	如果运算值是四个数学运算符之一，那么这个数学运算就被执行了。 如果 op 值指示节点只是一个整数文本，则返回文本值。 

 	该函数返回该树的最终值。 而且，由于是递归的，它将一次计算整棵树的一个子树的最终值。 

### 更改为汇编代码生成

 我们将编写一个通用的汇编代码生成器 

```c#
//给定AST，生成递归汇编代码
public static int GenAST(AstNode node)
{
    int leftVal = 0, rightVal = 0;

    if (node.LeftNode != null)
        leftVal = GenAST(node.LeftNode);

    if (node.RightNode != null)
        rightVal = GenAST(node.RightNode);

    switch (node.NodeType)
    {
        case NodeType.ADD:
            return CgAdd(leftVal,rightVal);
        case NodeType.SUBTRACT:
            return CgSub(leftVal,rightVal);
        case NodeType.MULTIPLY:
            return CgMul(leftVal,rightVal);
        case NodeType.DIVIDE:
            if (rightVal != 0)
            {
                return CgDiv(leftVal,rightVal);;
            }
            else
                throw new ArgumentException(nameof(rightVal));
        case NodeType.INTLIT:
            return CgLoad(node.Value);
        default:
            throw new Exception($"未知的ast类型：{node.NodeType.ToString()}");
    }

}
```

 看起来跟InterpretAstTree很像，我们正在做相同的深度优先树遍历。 这一次 

* INTLIT：为文本值加载一个寄存器
* 其它操作符： 对保存左值和右值的两个寄存器执行一个数学函数 

 GenAst ()中的代码不传递值，而是传递寄存器标识符。 例如，CgLoad ()将一个值加载到一个寄存器中，并使用加载的值返回寄存器的标识。

这样， GenAst ()本身返回的是保存树的最终值的寄存器的标识。  

### 调用GenAst

 GenAst ()只计算给它的表达式的值。 我们需要打印出最后的计算结果。 我们还需要将我们生成的汇编代码包装成一些前导代码和尾随代码。 

```C#
public static void GenerateCode(AstNode node)
{
    int reg;
    //打印出初始化汇编代码
    CgPreamble();
    reg= GenAST(node);
    //打印出使用的寄存器
    CgPrintint(reg);
    //打印出生成的汇编代码
    CgPostamble(reg);
}
```

###   X86-64汇编代码生成器 

 这就是通用代码生成器。 现在我们需要看看一些真正的汇编代码的生成方法。 目前，我们的目标是 x86-64 CPU，运行在 Linux 平台。

#### 分配寄存器

 任何 CPU 的寄存器数量都是有限的。 我们必须分配一个寄存器来保存整数字面值，并对它们执行相关计算。 一旦我们使用了一整数字面值后，我们通常可以丢弃这个值，从而**释放保存它的寄存器**。 然后我们可以为另一个值重新分配使用这个寄存器。  

 有三个函数处理寄存器分配 ：

*  FreeAllRegisters() ：将所有寄存器设为可用 
*  AllocRegister()：分配可用寄存器 
*   FreeRegister() ： 释放已经分配的寄存器

 该代码适用于通用寄存器: r0、 r1、 r2和 r3。 有一个包含实际寄存器名称的字符串表: 

```C#
/// <summary>
/// 寄存器列表
/// </summary>
private static Dictionary<string, int> RegList = new Dictionary<string, int>
{
    { "r8",0},
    { "r9",0},
    { "r10",0},
    { "r11",0}
};

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

    Console.WriteLine("没有可用的寄存器");
    return null;
}

/// <summary>
/// 释放已分配的寄存器
/// </summary>
public void FreeRegister(string key)
{
    if (RegList.Keys.Contains(key))
        RegList[key] = 0;
}
```

#### 加载寄存器

 分配一个寄存器，然后一个 movq 指令将一个文本值加载到分配的寄存器中 ：

```c#
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
```

#### 两个寄存器相加

 CgAdd ()接受两个寄存器编号，并生成将它们相加的代码。 结果保存在两个寄存器中的一个中，然后释放另一个以备将来使用: 

```C#
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
```

 注意，加法这里是把 r1加到 r2，返回寄存器r2的标识 。


 #### 两个寄存器相乘

```c#
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
```

#### 两个寄存器相减

```C#
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
```

 减法是不可交换的: 我们必须正确排序。第一个寄存器中减去 第二个寄存器，所以我们返回第一个寄存器，释放第二个寄存器 

#### 两个寄存器相除

 除法也是不可交换的，所以前面的注释适用。 在 x86-64上，情况更加复杂。 我们需要将 r1复制给% rax 。 这需要将 cqo 扩展到八个字节。 然后，idivq 将% rax 除以 r2中的除数，将商留在% rax 中，因此我们需要将它复制到 r1或 r2中。 然后我们可以释放另一个寄存器 ：

```C#
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
```

####  打印一个寄存器

 没有任何x86-64指令可将寄存器输出为十进制数字。 为了解决此问题，汇编程序的序言包含一个名为printint（）的函数，该函数带有一个寄存器参数，并将寄存器中的数值以十进制形式将其打印出来。 

```C#
/// <summary>
/// 对给定的寄存器调用 打印函数printint ()
/// </summary>
/// <param name="r"></param>
public void CgPrintInt(string r)
{
    CodeContents.Append($"\tmovq\t{RegList[r]}, %rdi\n");
    CodeContents.Append("\tcall\tprintint\n");
    FreeRegister(r);
}
```



