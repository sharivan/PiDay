using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace Pfz.Math.Parser;

/// <summary>
/// This class is responsible for parsing math expressions and generating
/// the "math evaluator".
/// It actually achieve this by generating a LINQ expression that represents
/// the math operation and by compiling such expression, generating a delegate
/// dedicated to evaluate the given expression as many times as needed.
/// Such re-evaluation is very useful when the only thing that change are the
/// contents of the input variables.
/// </summary>
public sealed class MathParser
{
    #region StaticRegisterFunction - Many, many overloads
    private struct _FunctionWithParameterCount
    {
        internal _FunctionWithParameterCount(Delegate function, int parameterCount)
        {
            _function = function;
            _parameterCount = parameterCount;
        }

        internal readonly Delegate _function;
        internal readonly int _parameterCount;
    }

    private static readonly ConcurrentDictionary<string, _FunctionWithParameterCount> _staticFunctions = new();
    public static void StaticRegisterFunction(Func<BigDecimal> function)
    {
        StaticRegisterFunction(null, function);
    }
    public static void StaticRegisterFunction(Func<BigDecimal, BigDecimal> function)
    {
        StaticRegisterFunction(null, function);
    }
    public static void StaticRegisterFunction(Func<BigDecimal, BigDecimal, BigDecimal> function)
    {
        StaticRegisterFunction(null, function);
    }
    public static void StaticRegisterFunction(Func<BigDecimal, BigDecimal, BigDecimal, BigDecimal> function)
    {
        StaticRegisterFunction(null, function);
    }
    public static void StaticRegisterFunction(Delegate function)
    {
        StaticRegisterFunction(null, function);
    }

    public static void StaticRegisterFunction(string? name, Func<BigDecimal> function)
    {
        StaticRegisterFunction(name, (Delegate) function);
    }
    public static void StaticRegisterFunction(string? name, Func<BigDecimal, BigDecimal> function)
    {
        StaticRegisterFunction(name, (Delegate) function);
    }
    public static void StaticRegisterFunction(string? name, Func<BigDecimal, BigDecimal, BigDecimal> function)
    {
        StaticRegisterFunction(name, (Delegate) function);
    }
    public static void StaticRegisterFunction(string? name, Func<BigDecimal, BigDecimal, BigDecimal, BigDecimal> function)
    {
        StaticRegisterFunction(name, (Delegate) function);
    }
    public static void StaticRegisterFunction(string? name, Delegate function)
    {
        if (function == null)
            throw new ArgumentNullException("function");

        var method = function.Method;
        if (name == null)
            name = method.Name;

        int nameLength = name.Length;
        if (nameLength == 0)
            throw new ArgumentException("name can't be empty.", "name");

        char firstChar = name[0];
        if (firstChar < 'A' || firstChar > 'Z')
            throw new ArgumentException("name must start with an uppercase english letter.", "name");

        for (int i = 1; i < nameLength; i++)
            if (_IsInvalidNameCharacter(name[i]))
                throw new ArgumentException("name must only have english letters or numbers from 0 to 9.");

        if (method.ReturnType != typeof(BigDecimal))
            throw new ArgumentException($"function must have a return type of {nameof(BigDecimal)}.", "function");

        // In some situations (optimizations of static methods) the number of parameters of the method 
        // may be different than the number of parameters of the delegate. So, we validate the delegate itself.
        var invoke = function.GetType().GetMethod("Invoke");
        if (invoke == null)
            throw new InvalidProgramException("The required Invoke method could not be found.");

        var invokeParameters = invoke.GetParameters();
        foreach (var parameter in invokeParameters)
            if (parameter.ParameterType != typeof(BigDecimal))
                throw new ArgumentException($"The parameter {parameter.Name} of the given function isn't of type {nameof(BigDecimal)}.", "function");

        // If the method pointed by the delegate has a different number of parameters than the delegate itself,
        // we create a delegate to the current delegate's Invoke method. Yeah, we can create a delegate to a
        // delegate, and this solves our problems without having to completely change the expression that
        // we will generate later.
        if (invokeParameters.Length != method.GetParameters().Length)
            function = Delegate.CreateDelegate(function.GetType(), function, invoke);

        var functionWithParameterCount = new _FunctionWithParameterCount(function, invokeParameters.Length);
        if (!_staticFunctions.TryAdd(name, functionWithParameterCount))
            throw new InvalidOperationException("There's a static function already registered with the given name.");
    }
    #endregion
    #region RegisterFunction - Many, many overloads
    public void RegisterFunction(Func<BigDecimal> function)
    {
        RegisterFunction(null, function);
    }
    public void RegisterFunction(Func<BigDecimal, BigDecimal> function)
    {
        RegisterFunction(null, function);
    }
    public void RegisterFunction(Func<BigDecimal, BigDecimal, BigDecimal> function)
    {
        RegisterFunction(null, function);
    }
    public void RegisterFunction(Func<BigDecimal, BigDecimal, BigDecimal, BigDecimal> function)
    {
        RegisterFunction(null, function);
    }
    public void RegisterFunction(Delegate function)
    {
        RegisterFunction(null, function);
    }

    public void RegisterFunction(string? name, Func<BigDecimal> function)
    {
        RegisterFunction(name, (Delegate) function);
    }
    public void RegisterFunction(string? name, Func<BigDecimal, BigDecimal> function)
    {
        RegisterFunction(name, (Delegate) function);
    }
    public void RegisterFunction(string? name, Func<BigDecimal, BigDecimal, BigDecimal> function)
    {
        RegisterFunction(name, (Delegate) function);
    }
    public void RegisterFunction(string? name, Func<BigDecimal, BigDecimal, BigDecimal, BigDecimal> function)
    {
        RegisterFunction(name, (Delegate) function);
    }
    public void RegisterFunction(string? name, Delegate function)
    {
        if (function == null)
            throw new ArgumentNullException("function");

        var method = function.Method;

        if (name == null)
            name = method.Name;

        int nameLength = name.Length;
        if (nameLength == 0)
            throw new ArgumentException("name can't be empty.", "name");

        char firstChar = name[0];
        if (firstChar < 'A' || firstChar > 'Z')
            throw new ArgumentException("name must start with an uppercase english letter.", "name");

        for (int i = 1; i < nameLength; i++)
            if (_IsInvalidNameCharacter(name[i]))
                throw new ArgumentException("name must only have english letters or numbers from 0 to 9.");

        if (method.ReturnType != typeof(BigDecimal))
            throw new ArgumentException($"function must have a return type of {nameof(BigDecimal)}.", "function");

        // In some situations (optimizations of static methods) the number of parameters of the method 
        // may be different than the number of parameters of the delegate. So, we validate the delegate itself.
        var invoke = function.GetType().GetMethod("Invoke");
        if (invoke == null)
            throw new InvalidProgramException("The required Invoke method could not be found.");

        var invokeParameters = invoke.GetParameters();
        foreach (var parameter in invokeParameters)
            if (parameter.ParameterType != typeof(BigDecimal))
                throw new ArgumentException($"The parameter {parameter.Name} of the given function isn't of type {nameof(BigDecimal)}.", "function");

        // If the method pointed by the delegate has a different number of parameters than the delegate itself,
        // we create a delegate to the current delegate's Invoke method. Yeah, we can create a delegate to a
        // delegate, and this solves our problems without having to completely change the expression that
        // we will generate later.
        if (invokeParameters.Length != method.GetParameters().Length)
            function = Delegate.CreateDelegate(function.GetType(), function, invoke);

        var functionWithParameterCount = new _FunctionWithParameterCount(function, invokeParameters.Length);
        _functions.Add(name, functionWithParameterCount);
    }
    #endregion

    private readonly Dictionary<string, MathVariable> _variables = new();
    private readonly Dictionary<string, _FunctionWithParameterCount> _functions = new();

    public MathVariable DeclareVariable(string name)
    {
        if (name == null)
            throw new ArgumentNullException("name");

        int nameLength = name.Length;
        if (nameLength == 0)
            throw new ArgumentException("name can't be empty.", "name");

        char firstChar = name[0];
        if (firstChar < 'a' || firstChar > 'z')
            throw new ArgumentException("name must start with a lowercase english letter.", "name");

        for (int i = 1; i < nameLength; i++)
            if (_IsInvalidNameCharacter(name[i]))
                throw new ArgumentException("name must only have english letters or numbers from 0 to 9.");

        var result = new MathVariable();
        _variables.Add(name, result);
        return result;
    }

    public Expression<Func<BigDecimal>> Parse(string expression)
    {
        if (expression == null)
            throw new ArgumentNullException("expression");

        var tokens = _ExtractTokens(expression);
        int tokenCount = tokens.Count;

        if (tokenCount == 0)
            throw new ArgumentException("The given expression is empty.");

        int tokenIndex = 0;
        var currentExpression = _ParseOneOrMoreIncludingSigns(tokens, ref tokenIndex, _TokenType.TokenListEnd);

        var lambda = Expression.Lambda<Func<BigDecimal>>(currentExpression);
        return lambda;
    }

    public Func<BigDecimal> Compile(string expression)
    {
        var linqExpression = Parse(expression);
        var result = linqExpression.Compile();
        return result;
    }

    private static readonly Expression _expressionConstant0 = Expression.Constant(0.0);
    private Expression _ParseOneOrMoreIncludingSigns(List<_Token> tokens, ref int tokenIndex, _TokenType closeToken)
    {
        var token = _GetCurrentToken(tokens, tokenIndex);
        switch (token._tokenType)
        {
            case _TokenType.Add:
                tokenIndex++;
                return _ParseOneOrMoreLowPriority(tokens, ref tokenIndex, closeToken);

            case _TokenType.Subtract:
            {
                tokenIndex++;

                var innerLeft = _GetValue(tokens, ref tokenIndex);
                var left = Expression.Subtract(_expressionConstant0, innerLeft);
                return _ParseOneOrMoreLowPriorityLoop(left, tokens, ref tokenIndex, closeToken);
            }
        }

        return _ParseOneOrMoreLowPriority(tokens, ref tokenIndex, closeToken);
    }

    private Exception _UnexpectedToken(_Token token)
    {
        if (token._tokenType == _TokenType.TokenListEnd)
            return new ArgumentException("More tokens were expected but the expression ended abruptly.");

        return new ArgumentException("The token '" + token._text + "' wasn't expected here.\nColumn: " + (token._characterIndex + 1));
    }

    private Expression _ParseOneOrMoreLowPriority(List<_Token> tokens, ref int tokenIndex, _TokenType closeToken)
    {
        var currentExpression = _ParseOneOrMoreHighPriority(tokens, ref tokenIndex);

        return _ParseOneOrMoreLowPriorityLoop(currentExpression, tokens, ref tokenIndex, closeToken);
    }

    private Expression _ParseOneOrMoreLowPriorityLoop(Expression currentExpression, List<_Token> tokens, ref int tokenIndex, _TokenType closeToken)
    {
        while (true)
        {
            var token = _GetCurrentToken(tokens, tokenIndex);
            if (token._tokenType == closeToken)
            {
                tokenIndex++;
                return currentExpression;
            }

            // At this moment each case needs to do a tokenIndex++, so I decided to put it
            // before the switch.
            tokenIndex++;
            switch (token._tokenType)
            {
                case _TokenType.Add: currentExpression = Expression.Add(currentExpression, _ParseOneOrMoreHighPriority(tokens, ref tokenIndex)); break;
                case _TokenType.Subtract: currentExpression = Expression.Subtract(currentExpression, _ParseOneOrMoreHighPriority(tokens, ref tokenIndex)); break;

                default:
                    throw _UnexpectedToken(token);
            }
        }
    }

    private Expression _ParseOneOrMoreHighPriority(List<_Token> tokens, ref int tokenIndex)
    {
        var currentExpression = _GetValue(tokens, ref tokenIndex);

        while (true)
        {
            var token = _GetCurrentToken(tokens, tokenIndex);

            // At this moment each case needs to do a tokenIndex++, so I decided to put it
            // before the switch. Unfortunately, I need to roll that back in the default case.
            tokenIndex++;

            switch (token._tokenType)
            {
                case _TokenType.Multiply: currentExpression = Expression.Multiply(currentExpression, _GetValue(tokens, ref tokenIndex)); break;
                case _TokenType.Divide: currentExpression = Expression.Divide(currentExpression, _GetValue(tokens, ref tokenIndex)); break;
                case _TokenType.Modulo: currentExpression = Expression.Modulo(currentExpression, _GetValue(tokens, ref tokenIndex)); break;

                default:
                    tokenIndex--;
                    return currentExpression;
            }
        }
    }

    private static _Token _GetNextToken(List<_Token> tokens, ref int tokenIndex)
    {
        tokenIndex++;

        return _GetCurrentToken(tokens, tokenIndex);
    }
    private static _Token _GetCurrentToken(List<_Token> tokens, int tokenIndex)
    {
        if (tokenIndex >= tokens.Count)
            throw new ArgumentException("The expression finished in a situation where more tokens are expected.");

        return tokens[tokenIndex];
    }

    private static readonly PropertyInfo _valueProperty = typeof(MathVariable).GetProperty("Value") ?? throw new InvalidProgramException("Could not find required property 'Value'.");
    private Expression _GetValue(List<_Token> tokens, ref int tokenIndex)
    {
        var token = _GetCurrentToken(tokens, tokenIndex);
        switch (token._tokenType)
        {
            case _TokenType.VariableName:
            {
                MathVariable? mathVariable;
                string variableName = token._text;
                _variables.TryGetValue(variableName, out mathVariable);

                // TODO should we extract the variables from the expression?
                // If we do this users can write any expression, we can parse it and then we 
                // can also return a collection of variables that can be presented on screen.
                // Well, maybe for a future version that will be an option.
                if (mathVariable == null)
                    throw new ArgumentException("A variable named " + variableName + " wasn't declared.");

                tokenIndex++;
                return Expression.MakeMemberAccess(Expression.Constant(mathVariable), _valueProperty);
            }

            case _TokenType.FunctionName:
            {
                _FunctionWithParameterCount functionWithParameterCount;
                string functionName = token._text;
                _functions.TryGetValue(functionName, out functionWithParameterCount);
                var function = functionWithParameterCount._function;
                if (function == null)
                {
                    _staticFunctions.TryGetValue(functionName, out functionWithParameterCount);

                    function = functionWithParameterCount._function;
                    if (function == null)
                        throw new ArgumentException("There's no function named " + functionName + ".");
                }

                token = _GetNextToken(tokens, ref tokenIndex);
                if (token._tokenType != _TokenType.OpenParenthesis)
                    throw new ArgumentException("( expected.");

                tokenIndex++;
                int parameterCount = functionWithParameterCount._parameterCount;
                var expressionArray = new Expression[parameterCount];
                for (int i = 0; i < parameterCount; i++)
                {
                    _TokenType closeToken = _TokenType.Comma;
                    if (i == parameterCount - 1)
                        closeToken = _TokenType.CloseParenthesis;

                    var subExpression = _ParseOneOrMoreIncludingSigns(tokens, ref tokenIndex, closeToken);
                    expressionArray[i] = subExpression;
                }

                if (function.Target != null)
                    return Expression.Call(Expression.Constant(function.Target), function.Method, expressionArray);

                return Expression.Call(function.Method, expressionArray);
            }

            case _TokenType.Value:
            {
                tokenIndex++;
                return Expression.Constant(token._value);
            }

            case _TokenType.OpenParenthesis:
            {
                tokenIndex++;
                return _ParseOneOrMoreIncludingSigns(tokens, ref tokenIndex, _TokenType.CloseParenthesis);
            }

            default:
                throw _UnexpectedToken(token);
        }
    }

    private enum _TokenType
    {
        Value,
        VariableName,
        FunctionName,
        OpenParenthesis,
        CloseParenthesis,
        Add,
        Subtract,
        Multiply,
        Divide,
        Modulo,
        Comma,
        TokenListEnd
    }
    private sealed class _Token
    {
        internal readonly _TokenType _tokenType;
        internal readonly int _characterIndex;
        internal readonly string _text;
        internal readonly BigDecimal _value;

        internal _Token(_TokenType tokenType, string text, int characterIndex) :
            this(tokenType, text, characterIndex, BigDecimal.Zero)
        {
        }
        internal _Token(_TokenType tokenType, string text, int characterIndex, BigDecimal value)
        {
            _tokenType = tokenType;
            _text = text;
            _characterIndex = characterIndex;
            _value = value;
        }
    }

    private static List<_Token> _ExtractTokens(string expression)
    {
        var result = new List<_Token>();

        int position = 0;
        int length = expression.Length;
        while (position < length)
        {
            char c = expression[position];

            if (c >= '0' && c <= '9')
            {
                position = _ParseNumber(position, expression, result);
                continue;
            }

            if (c >= 'a' && c <= 'z')
            {
                position = _ParseName(position, expression, _TokenType.VariableName, result);
                continue;
            }

            if (c >= 'A' && c <= 'Z')
            {
                position = _ParseName(position, expression, _TokenType.FunctionName, result);
                continue;
            }

            switch (c)
            {
                case '(': result.Add(new _Token(_TokenType.OpenParenthesis, "(", position)); break;
                case ')': result.Add(new _Token(_TokenType.CloseParenthesis, ")", position)); break;
                case '+': result.Add(new _Token(_TokenType.Add, "+", position)); break;
                case '-': result.Add(new _Token(_TokenType.Subtract, "-", position)); break;
                case '*': result.Add(new _Token(_TokenType.Multiply, "*", position)); break;
                case '/': result.Add(new _Token(_TokenType.Divide, "/", position)); break;
                case '%': result.Add(new _Token(_TokenType.Modulo, "%", position)); break;
                case ',': result.Add(new _Token(_TokenType.Comma, ",", position)); break;
                case ' ': break;
                default: throw new ArgumentException("Character at position " + position + " was not recognized.");
            }

            position++;
        }

        result.Add(new _Token(_TokenType.TokenListEnd, "", position));

        return result;
    }

    private static int _ParseNumber(int startPosition, string expression, List<_Token> result)
    {
        int position = startPosition + 1;
        int length = expression.Length;
        while (position < length)
        {
            char c = expression[position];

            if (c < '0' || c > '9')
            {
                if (c == '.')
                {
                    position++;
                    while (position < length)
                    {
                        c = expression[position];

                        if (c < '0' || c > '9')
                            break;

                        position++;
                    }
                }

                break;
            }

            position++;
        }

        var numberPart = expression.Substring(startPosition, position - startPosition);
        BigDecimal bigDecimalValue = BigDecimal.Parse(numberPart);
        result.Add(new _Token(_TokenType.Value, numberPart, startPosition, bigDecimalValue));
        return position;
    }

    private static int _ParseName(int startPosition, string expression, _TokenType tokenType, List<_Token> result)
    {
        int position = startPosition + 1;
        int length = expression.Length;
        while (position < length)
        {
            char c = expression[position];

            if (_IsInvalidNameCharacter(c))
                break;

            position++;
        }

        string name = expression.Substring(startPosition, position - startPosition);
        result.Add(new _Token(tokenType, name, startPosition));
        return position;
    }

    private static bool _IsInvalidNameCharacter(char c)
    {
        return (c < 'a' || c > 'z') && (c < 'A' || c > 'Z') && (c < '0' || c > '9');
    }
}