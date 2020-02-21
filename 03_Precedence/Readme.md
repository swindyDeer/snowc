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
* **用运算符优先级表影响现有解析器** 

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

