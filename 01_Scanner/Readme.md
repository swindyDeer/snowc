# Part 1: 词法扫描简介 

我们从一个简单的词法扫描器开始我们的编译器编写过程。扫描器的工作是识别输入语言中的词法元素或记号 。

 我们将从只有五个词法元素的语言开始： 

* 四个基本数学运算符：*，/，+和-
* 具有一个或多个0~9元素组成的十进制数（显然是一类词法元素，这里我们先将这一类当成一个）

每次扫描，都会将扫描的结果保存到令牌中，我们定义令牌的格式如下：

```C#
public class Token
{
    public TokenType TokenType { get; set; }

    public int IntValue { get; set; }
}
```

 令牌字段可以是以下值之一 :

```C#
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
```

 如果令牌是INTLIT（即整数文字），则intvalue字段将保存我们在其中扫描的整数的值 

 scan.cs文件包含我们的词法扫描器的功能。我们将从输入文件中一次读取一个字符。 

## 读取文件

内容保存到字符串Content中，并初始化索引Index与行号Line

```C#
public static void Init(string path)
{
    var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
    StreamReader = new StreamReader(fs, Encoding.Default);
    Content = StreamReader.ReadToEnd();
    Line = 0;
    Index = 0;
}
```

## 每次读取一个字符

```C#
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
```

**这里默认返回null时，文本读取结束。即null为文本结束标志**

## 跳过空白字符

 我们需要一个函数，该函数读取并以静默方式跳过空白字符，直到获得非空白字符，然后将其返回 。

```C#
private static char? Skip()
{
    var c = Next();
    while (' ' == c || '\t' == c || '\n' == c || '\r' == c || '\f' == c)
    {
        c = Next();
    }
    return c;
}
```

## 扫描字符并返回令牌

 现在我们可以在跳过空格的同时读取字符了 ，对于读取到的字符，我们需要将其封装为token返回，同时我们需要知道返回的token是否可用，因为文件中可能包含无法解析的未知字符。

```C#
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
```

 简单的单字符令牌就是这样：对于每个识别的字符，将其转换为令牌。你可能会问：为什么不只将识别的字符放入令牌？答案是，稍后我们将需要识别多字符标记（例如==）和关键字（例如if和while）。因此，使用枚举值将更加轻松。 

##  整数文字值 

 实际上，我们已经这种情况做了处理，用于识别3827和87731之类的整数文字值 ，在default块中： 一旦我们击中了一个十进制数字字符，我们就用第一个字符调用辅助函数scanint（） 。

```C#
public static int ScanInt(char c)
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
```

 我们从零值开始。每次我们从0到9的集合中获得一个字符时，都会其转换为int值。我们将val增大10倍，然后向其添加新数字 。

 例如，如果我们有字符3、2、8，则执行：

* val = 0 * 10 + 3，即3
* val = 3 * 10 + 2，即32
* val = 32 * 10 + 8，即328 

## 打印扫描结果

```C#
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
```

在main函数中指定测试文件：

```C#
static void Main(string[] args)
{
    var path = "./input5.txt";
    Scan.Init(path);
    Scan.ScanPrint();
    Console.WriteLine("exit");
}
```



 ## 结论和下一步

我们有了一个小开始，我们有一个简单的词法扫描器，它可以识别四个主要的数学运算符以及整数文字值。我们看到，单字符令牌很容易扫描，但多字符令牌要难一些。 

 在我们的编译器编写过程的下一部分中，我们将构建一个递归下降解析器来解释输入文件的语法，并计算并打印出每个文件的最终值 。