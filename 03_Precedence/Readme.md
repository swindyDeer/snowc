# Part 3: 运算符优先级

我们在编译器的编写过程的上一部分中看到，解析器不一定强制执行我们语言的语义。它仅强制执行语法的语法和结构规则。我们最后得到的代码计算出2 * 3 + 4 * 5等表达式的错误值，因为该代码创建了一个Ast树，看起来像

```markdown
     *
    / \
   2   +
      / \
     3   *
        / \
       4   5
```

而不是：

```markdown
          +
         / \
        /   \
       /     \
      *       *
     / \     / \
    2   3   4   5
```

为了解决这个问题，我们必须向解析器添加代码以执行运算符优先级。有（至少）两种方法:

* **在语言的语法中明确运算符的优先级**
* **用运算符优先级表来影响现有解析器** 

## 明确运算符优先级

这是上一节最后一部分的语法：

```markdown
expression: number
          | expression '*' expression
          | expression '/' expression
          | expression '+' expression
          | expression '-' expression
          ;

number:  T_INTLIT
         ;
```

请注意，这四个数学运算符之间没有区别。让我们调整语法，以便有所不同:

```markdown
expression: additive_expression
    ;

additive_expression:
      multiplicative_expression
    | additive_expression '+' multiplicative_expression
    | additive_expression '-' multiplicative_expression
    ;

multiplicative_expression:
      number
    | number '*' multiplicative_expression
    | number '/' multiplicative_expression
    ;

number:  T_INTLIT
         ;
```

以上内容表示：

* 一个表达式是加法表达式（表达式都是加法表达式）
* 一个加法表达式可以是乘法表达式，或者
* 一个加法表达式可以是加法表达式+乘法表达式，或者
* 一个加法表达式可以是加法表达式-乘法表达式
* 一个乘法法表达式可以是数字，或者
* 一个乘法法表达式可以是数字*乘法表达式，或者
* 一个乘法法表达式可以是数字/乘法表达式
* 数字始终是T_INTLIT令牌

现在，我们有两种类型的表达式：加法表达式和乘法表达式。

请注意，语法现在强制数字仅作为乘法表达式的一部分。这迫使'*'和'/'运算符更紧密地绑定到任一侧的数字，因此具有更高的优先级。

任何加法表达式实际上要么本身就是乘法表达式，要么是加法（即乘法）表达式，后跟“ +”或“-”运算符，然后是另一个乘法表达式。现在，加法表达式的出现率比乘法表达式低得多。

## 在递归下降解析器中执行上述操作

我们使用GetMultiplicativeAstTree来处理*与/运算符，GetAdditiveAstTree来处理+与-运算符。新的解析器使用了expr1类。

这两个函数都将读入某个token。 然后，当有下列优先级相同的运算符时，每个函数将解析更多的输入并将左右两半与第一个运算符结合起来 组成子树。

```c#
/// <summary>
/// 返回一个根为“ +”或“ - ”二进制运算符的 AST 树
/// </summary>
/// <returns></returns>
private static AstNode GetAdditiveAstTree()
{
    AstNode left, right;
    NodeType nodeType;

    //获取更高优先级的左子树
    left = GetMultiplicativeAstTree();

    //token,正常情况应为运算符+或-运算符或终止符
    //如果扫描结束
    if (token == null)
        return left;

    while (true)
    {
        //+或-运算符
        nodeType = ConvertToNodeType(token.TokenType);

        //获取优先级更高的右子树，返回时可以明确是遇到了+或-运算符或终止符
        right = GetMultiplicativeAstTree();

        left = AstTree.MkAstNode(nodeType, left, right, 0);

        //token,正常情况应为运算符+或-运算符或终止符
        //如果扫描结束
        if (token == null)
            break;
    }

    return left;
}
```

```c#
/// <summary>
/// 返回一个根为“ * ”或“ / ”二进制运算符的 AST 树
/// </summary>
/// <returns></returns>
private static AstNode GetMultiplicativeAstTree()
{
    AstNode left, right;
    NodeType nodeType;

    token = ExecScan();
    Console.WriteLine($"节点类型：{token?.TokenType.ToString()},节点值：{token?.IntValue}");
    //第一个节点为左节点
    left = GetAstLeafNode(token);

    //获取下一个token,正常情况应为运算符
    token = ExecScan();
    Console.WriteLine($"节点类型：{token?.TokenType.ToString()},节点值：{token?.IntValue}");

    //如果扫描结束
    if (token == null)
        return left;

    // 当token类型是*或/
    while (token.TokenType == TokenType.STAR || token.TokenType == TokenType.SLASH)
    {
        nodeType = ConvertToNodeType(token.TokenType);

        //获取下一个token
        token = ExecScan();
        Console.WriteLine($"节点类型：{token?.TokenType.ToString()},节点值：{token?.IntValue}");
        right = GetAstLeafNode(token);

        //合并左右节点
        left = AstTree.MkAstNode(nodeType, left, right, 0);

        //获取下一节点
        token = ExecScan();
        Console.WriteLine($"节点类型：{token?.TokenType.ToString()},节点值：{token?.IntValue}");
        //如果扫描结束
        if (token == null)
            break;
    }

    return left;
}
```

与上一节不同的地方是，Expr1类中使用了一个全局token，来处理while循环结束后最后获取的token类型无法获取到的问题。

正常情况下，算术表达式的第一个字符应为数字，我们在GetAdditiveAstTree方法中使用GetMultiplicativeAstTree方法来处理，GetMultiplicativeAstTree方法回值遇到+-运算符或表达式终止时时返回。

在GetAdditiveAstTree方法中，当我们执行 while 循环时，我们知道有一个“ + ”或“-”操作符。 执行循环，获取乘法表达式，结合左右子树为新的左子树。

GetMultiplicativeAstTree方法将首先创建一个左节点，并读取下一个token，如果符合循环条件会创建一个右节点，结合左右子树为新的左子树，并继续循环。

构建的树：

```markdown
      -
     / \
    /   \
   /     \
  +       /
 / \     / \
2   *   8   3
   / \      
  3   5 
```

## 递归下降解析的缺点

1. 效率低下：使用显式操作符优先级构造递归下降解析器，所有的函数调用都需要达到正确的优先级，**随着操作符的优先级的增加，递归调用的层级也会增加。
2. 难以扩展： 使用不同函数来处理每一级的运算符优先级，因此每次增加优先级都必须增加对应的处理函数，同时由于存在递归调用，每次都会影响下级或下级调用。

## ## 使用运算符优先级表

接下来我们尝试另一种方法， 使用 普拉特（Pratt） 解析器，它有一个与每个令牌相关联的优先级值表。 

Pratt语法分析实现在expr2中。

首先我们需要确定每个令牌的优先级：在C中我们可以使用数字，在C#中使用字典显然会更合适。

```c#
/// <summary>
/// 运算符优先级表
/// </summary>
public static Dictionary<NodeType, int> PrecedenceDic = new Dictionary<NodeType, int>
{
    { NodeType.INTLIT, 0 },
    { NodeType.ADD, 10 },
    { NodeType.SUBTRACT, 10 },
    { NodeType.MULTIPLY, 20 },
    { NodeType.DIVIDE, 20 },
};
```

表中有3个优先级 ，数值大的优先级高。