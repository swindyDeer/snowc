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

