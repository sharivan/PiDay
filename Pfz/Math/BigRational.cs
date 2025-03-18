using System.Numerics;
using System.Text;

namespace Pfz.Math;

public sealed class BigRational :
    IEquatable<BigRational>,
    IComparable<BigRational>
{
    public BigRational() :
        this(0)
    {
    }
    public BigRational(BigInteger numerator) :
        this(numerator, BigInteger.One)
    {
    }

    /// <summary>
    /// Creates a new instance of BigRational providing the value (called numerator)
    /// and the divider (called denominator).
    /// </summary>
    /// <param name="numerator">The value before any division.</param>
    /// <param name="denominator">The value that will divide the numerator.</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public BigRational(BigInteger numerator, BigInteger denominator)
    {
        if (denominator < 1)
            throw new ArgumentOutOfRangeException(nameof(denominator));

        Numerator = numerator;
        Denominator = denominator;

        _CompressIfPossible();
    }

    public BigInteger Numerator { get; private set; }
    public BigInteger Denominator { get; private set; }

    public void Add(BigInteger value)
    {
        Numerator += value * Denominator;
        _CompressIfPossible();
    }

    public void Subtract(BigInteger value)
    {
        Numerator -= value * Denominator;
        _CompressIfPossible();
    }

    public void Multiply(BigInteger value)
    {
        Numerator *= value;
        _CompressIfPossible();
    }

    public void Divide(BigInteger value)
    {
        bool isNegative = value < 0;

        if (isNegative)
        {
            Numerator = -Numerator;
            value = -value;
        }

        Denominator *= value;
        _CompressIfPossible();
    }

    // TODO: Maybe add more primes to make things work great.
    private static readonly int[] _primeNumbers = new int[] { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31 };
    private void _CompressIfPossible()
    {
        // If divider is already one, there's nothing to compress.
        if (Denominator == 1)
            return;

        if ((Numerator % Denominator) == 0)
        {
            Numerator /= Denominator;
            Denominator = 1;
            return;
        }

        int primeLength = _primeNumbers.Length;
        int index = 0;
        while (index < primeLength)
        {
            var prime = _primeNumbers[index];

            var absValue = BigInteger.Abs(Numerator);
            if (prime > Denominator || prime > absValue)
                break;

            if ((Numerator % prime) == 0 && (Denominator % prime) == 0)
            {
                Numerator /= prime;
                Denominator /= prime;

                continue;
            }

            index++;
        }
    }

    public BigDecimal ToBigDecimal()
    {
        return ToBigDecimal(BigDecimal.DefaultFloatDigitCount);
    }
    public BigDecimal ToBigDecimal(BigInteger floatDigitCount)
    {
        var value = new BigDecimal(Numerator);
        var divider = new BigDecimal(Denominator);
        var result = BigDecimal.Divide(value, divider, floatDigitCount);
        return result;
    }

    public void Power(int exponent)
    {
        if (exponent == 0)
        {
            Numerator = 1;
            Denominator = 1;
            return;
        }

        if (exponent == 1)
            return;

        if (exponent > 1)
        {
            Numerator = BigInteger.Pow(Numerator, exponent);
            Denominator = BigInteger.Pow(Denominator, exponent);
            return;
        }

        // Let's make the exponenent positive.
        // Then apply it to denominator and numerator.
        exponent = -exponent;

        // Not sure why, but things get inverted here.
        var numerator = Numerator;
        var denominator = Denominator;

        Numerator = BigInteger.Pow(denominator, exponent);
        Denominator = BigInteger.Pow(numerator, exponent);
    }

    public int CompareTo(BigRational? other)
    {
        // We are always greater than null.
        if (other == null)
            return 1;

        // When dividers (the denominator) are the same,
        // we can compare things directly.
        var divider1 = Denominator;
        var divider2 = other.Denominator;
        if (divider1 == divider2)
            return Numerator.CompareTo(other.Numerator);

        // When the denominators are different, we just "cross" the
        // dividers and everything should be fine. We could alternatively
        // divide the values, but then we might suffer from some loss of precision.
        var value1 = Numerator * divider2;
        var value2 = other.Numerator * divider1;
        return value1.CompareTo(value2);
    }

    public override int GetHashCode()
    {
        // Usually the denominator does a division... but here, to generate
        // a hash-code, better use it as a multiplier.
        return (Numerator * Denominator).GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        var other = obj as BigRational;
        return Equals(other);
    }

    public bool Equals(BigRational? other)
    {
        if (other == null)
            return false;

        var value1 = Numerator * other.Denominator;
        var value2 = other.Numerator * Denominator;
        return value1 == value2;
    }

    public void ToString(StringBuilder stringBuilder)
    {
        stringBuilder.Append(nameof(BigRational));
        stringBuilder.Append('(');
        stringBuilder.Append(Numerator.ToString());
        stringBuilder.Append(" / ");
        stringBuilder.Append(Denominator.ToString());
        stringBuilder.Append(')');
    }

    public override string ToString()
    {
        var result = new StringBuilder();
        ToString(result);
        return result.ToString();
    }

    public void Add(BigRational other)
    {
        ArgumentNullException.ThrowIfNull(other);

        var thisDenominator = Denominator;
        var otherDenominator = other.Denominator;

        if (thisDenominator == otherDenominator)
            Numerator += other.Numerator;
        else
        {
            Denominator *= otherDenominator;

            Numerator *= otherDenominator;
            Numerator += other.Numerator * thisDenominator;
        }

        _CompressIfPossible();
    }

    public void Subtract(BigRational other)
    {
        ArgumentNullException.ThrowIfNull(other);

        var thisDenominator = Denominator;
        var otherDenominator = other.Denominator;

        if (thisDenominator == otherDenominator)
            Numerator -= other.Numerator;
        else
        {
            Denominator *= otherDenominator;

            Numerator *= otherDenominator;
            Numerator -= other.Numerator * thisDenominator;
        }

        _CompressIfPossible();
    }

    public void Multiply(BigRational other)
    {
        ArgumentNullException.ThrowIfNull(other);

        var thisDenominator = Denominator;
        var otherDenominator = other.Denominator;

        if (thisDenominator == otherDenominator)
            Numerator *= other.Numerator;
        else
        {
            Denominator *= otherDenominator;
            Numerator *= other.Numerator;
        }

        _CompressIfPossible();
    }

    public void Divide(BigRational other)
    {
        ArgumentNullException.ThrowIfNull(other);

        var thisDenominator = Denominator;
        var otherDenominator = other.Denominator;

        Denominator *= other.Numerator;
        Numerator *= otherDenominator;

        _CompressIfPossible();
    }
}