# Part 2：解析简介 

 在我们的编译器编写过程的这一部分中，我将介绍解析器的基础知识。正如我在第一部分中提到的那样，解析器的工作是识别输入的语法和结构元素，并确保它们符合语言的语法。 

 我们已经有几种可以扫描的语言元素，即令牌 :

* 四个基本数学运算符：*，/，+和-
* 具有一个或多个0~9元素组成的十进制数

 现在让我们为解析器可以识别的语言定义一个语法 

## BNF

 如果您要处理计算机语言，则有时会遇到BNF的使用。我将在这里介绍足够多的BNF语法来表达我们想要识别的语法。 

 我们希望语法能用整数表达数学表达式。这是语法的BNF描述： 

```C#
expression: number
          | expression '*' expression
          | expression '/' expression
          | expression '+' expression
          | expression '-' expression
          ;

number:  T_INTLIT
    	 ;
```

 竖线将语法中的选项分开，因此上面的内容表示：

* 表达式可以只是一个数字，或者
* 一个表达式是两个由'*'标记分隔的表达式，或者
* 一个表达式是两个由'/'标记分隔的表达式，或者
* 一个表达式是两个由'+'标记分隔的表达式，或者
* 一个表达式是两个由'-'标记分隔的表达式
* 数字始终是T_INTLIT令牌  

 显然，语法的BNF定义是递归的：通过引用其他表达式来定义一个表达式。但是有一种方法可以“自下而上”地进行递归：当一个表达式原来是一个数字时，它总是一个T_INTLIT令牌，因此不是递归的 。

 在BNF中，我们说“表达式”和“数字”是非终结符，因为它们是由语法规则产生的。但是，T_INTLIT是终端符号，因为它没有任何规则定义。相反，它是该语言中已经被认可的令牌。同样，四个数学运算符是终端符号。 

## 递归下降解析

鉴于我们语言的语法是递归的，因此尝试递归解析它是有意义的。我们需要做的是读入令牌，然后向前看下一个令牌。根据下一个标记是什么，然后我们可以决定解析输入所需要的路径。这可能需要我们递归调用已被调用的函数。

在我们的例子中，任何表达式中的第一个标记都是数字，数学运算符可以跟在后面。之后，可能只有一个数字，或者可能是一个全新表达式的开始。我们如何递归解析呢？

我们可以编写如下的伪代码：

```C#
function expression() {
  扫描并检查第一个令牌是一个数字,如果不是则报错
  获取下一个令牌
  如果我们到达输入的末尾 返回
  其它情况, 调用 expression()
}
```

 让我们在输入2 + 3-5 T_EOF上运行此函数，其中T_EOF是反映输入结束的标记。我将对每个对expression（）的调用进行编号。 

```C#
expression0:
  扫描 2, 这是一个数字
  获取下一个令牌, +, 不是结束标志 T_EOF
  调用 expression()

    expression1:
      扫描 3, 这是一个数字
      获取下一个令牌, -, 不是结束标志 T_EOF
      调用 expression()

        expression2:
          扫描 5, 这是一个数字
          获取下一个令牌, 是结束标志T_EOF, 
		  因此返回 expression2

    返回 expression1
              
返回 expression0

```

 可以看到，该函数能够递归解析输入2 + 3-5 T_EOF 。

当然，我们对输入没有做任何事情，但这不是解析器的工作。解析器的工作是识别输入，并警告所有语法错误。输入的语义分析交给了编译器的其它部分，即理解并执行该输入的含义。

> 稍后，您将看到实际上并非如此。将语法分析和语义分析交织在一起通常是有意义的。 

## 抽象语法树

 要进行语义分析，我们需要解释输入的代码，或将其翻译为其他格式，例如汇编代码。在旅程的这一部分中，我们将为输入内容构建一个解释器。但是要到达那里，我们首先将输入转换为抽象语法树，也称为AST。

我强烈建议阅读AST的简短说明：

[用AST提升解析游戏的水平](https://medium.com/basecs/leveling-up-ones-parsing-game-with-asts-d7a6fc2400ff)

这篇文章写得很好，确实有助于解释AST的目的和结构。

AST中每个节点的结构在如下：

```C#
// AST 节点类型
enum {
  A_ADD, A_SUBTRACT, A_MULTIPLY, A_DIVIDE, A_INTLIT
};

// 抽象语法树结构
class ASTnode {
  int op;                               // 将在此树上执行的“操作”
  ASTnode left;                 // 左右子树
  ASTnode right;
  int intvalue;                         // 对于A_INTLIT类型，是整数值
};
```

某些AST节点（例如具有op值A_ADD和A_SUBTRACT的AST节点）具有左右两个指向的子AST。稍后，我们将添加或减去子树的值。

或者，具有op值A_INTLIT的AST节点表示一个整数值。它没有子树后代，只有intvalue字段中的一个值。

## 建立AST节点和树

```C#
    public class AstTree
    {
        /// <summary>
        /// 创建节点
        /// </summary>
        /// <param name="nodeType"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public AstNode MkAstNode(NodeType nodeType,AstNode left,AstNode right,int value)
        {
            var node = new AstNode();

            node.NodeType = nodeType;
            node.LeftNode = left;
            node.RightNode = right;
            node.Value = value;

            return node;
        }
```

 在这种情况下，我们可以编写更具体的函数来创建叶AST节点（即一个没有子节点的AST节点），并创建一个有单个子节点的AST节点 

```C#
        /// <summary>
        /// 创建无子节点的节点（默认为左节点）
        /// </summary>
        /// <param name="nodeType"></param>
        /// <param name="left"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public AstNode MkAstLeftNode(NodeType nodeType,int value)
        {
            return MkAstNode(nodeType,null,null,value);
        }

        /// <summary>
        /// 创建只有一个子节点的节点（子节点默认为左节点）
        /// </summary>
        /// <param name="nodeType"></param>
        /// <param name="left"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public AstNode MkAstUnary(NodeType nodeType, AstNode left, int value)
        {
            return MkAstNode(nodeType,left,null,value);
        }
```

## Ast的目的

我们将使用AST来存储我们认识的每个表达式，以便以后可以递归遍历它以计算表达式的最终值。我们确实想处理数学运算符的优先级。这里有一个例子。

考虑表达式2 * 3 + 4 *5。现在，乘法比加法具有更高的优先级。因此，我们希望将乘法操作数绑定在一起，并在进行加法之前执行这些操作。

如果我们生成的AST树看起来像这样：

```markdown
          +
         / \
        /   \
       /     \
      *       *
     / \     / \
    2   3   4   5
```

然后，当遍历树时，我们将首先执行2 * 3，然后执行4 * 5。获得这些结果后，便可以将它们传递到树的根部以执行加法。

## 一个天真的表达解析器

现在，我们可以将扫描器中的令牌值重新用作AST节点操作值，但是我想将令牌和AST节点的概念分开。因此，首先，我将具有一个将令牌值映射到AST节点操作值的函数。这以及解析器的其余部分：

```C#
/// <summary>
/// 将令牌转换为AST操作
/// </summary>
/// <param name="tokenType"></param>
/// <returns></returns>
public static NodeType ConvertToNodeType(TokenType tokenType)
{
    switch(tokenType)
    {
        case TokenType.PLUS:
            return (NodeType.ADD);
        case TokenType.MINUS:
            return (NodeType.SUBTRACT);
        case TokenType.STAR:
            return (NodeType.MULTIPLY);
        case TokenType.SLASH:
            return (NodeType.DIVIDE);
        default:
            throw new Exception($"未知token类型{tokenType}");
    }
}
```

当我们无法将给定令牌转换为AST节点类型时，将触发switch语句中的默认语句。这将成为解析器中语法检查的一部分。

我们需要一个函数来检查下一个标记是否为整数文字，并构建一个AST节点来保存文字值。这里是：

```C#
/// <summary>
/// 获取ast叶节点
/// </summary>
/// <param name="token"></param>
/// <returns></returns>
public static AstNode GetAstLeafNode(Token token)
{
    switch(token.TokenType)
    {
            //对于INTLIT令牌，为其创建叶节点
            //并扫描下一个令牌。否则，对于任何其他令牌类型输出语法错误
        case TokenType.INTLIT:
            var node = AstTree.MkAstLeafNode(NodeType.INTLIT,token.IntValue);
            //ScanResult = Scan.ExecScan();
            return node;
        default:
            throw new Exception($"token类型不是整数：{token.TokenType.ToString()}");
    }

}
```

现在我们可以为解析器编写代码:

```C#
/// <summary>
/// 获取Ast树
/// </summary>
/// <returns></returns>
public static AstNode GetAstTree()
{
    AstNode node, left, right;
    NodeType nodeType;

    var token = ExecScan();
    //第一个节点为左节点
    left = GetAstLeafNode(token);

    //获取下一个token
    token = ExecScan();

    //如果扫描结束
    if (token == null)
        return left;

    //记录根节点类型
    nodeType = ConvertToNodeType(token.TokenType);

    //获取下一个token
    token = ExecScan();

    //递归得到右子树
    right = GetAstTree();

    //合并左右子树
    node = AstTree.MkAstNode(nodeType,left,right,0);

    return node;
}
```

请注意，在此朴素的解析器代码中，没有任何地方可以处理不同的运算符优先级。就目前而言，该代码将所有运算符都视为具有相同的优先级。如果您在解析表达式2 * 3 + 4 * 5时遵循该代码，则会看到它构建了此AST：

```markdown
     *
    / \
   2   +
      / \
     3   *
        / \
       4   5
```

这绝对是不正确的。它将乘以4 * 5得到20，然后执行3 + 20得到23，而不是进行2 * 3得到6。那我为什么要这样做呢？我想向您展示，编写一个简单的解析器很容易，但是要使其同时进行语义分析也很困难。

## 解释树



