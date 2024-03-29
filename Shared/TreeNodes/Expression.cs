﻿namespace Shared.TreeNodes;

public class Expression : Node
{
}

public class ValueExpression : Expression
{
    public DataNode Value;
}

public class UnaryExpression : Expression
{
    public DataNode Value;
    public DataNode Operator;
}

public class BinaryExpression : Expression
{
    public Expression Left;
    public Expression Right;
    public DataNode Operator;
}