using System.Numerics;
using System.Text;

namespace Pfz.Math;

public sealed class BigRational:
	IEquatable<BigRational>,
	IComparable<BigRational>
{
	private BigInteger _numerator; // value before any division
	private BigInteger _denominator; // divider

	public BigRational():
		this(0)
	{
	}
	public BigRational(BigInteger numerator):
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

		_numerator = numerator;
		_denominator = denominator;

		_CompressIfPossible();
	}

	public BigInteger Numerator
	{
		get => _numerator;
	}
	public BigInteger Denominator
	{
		get => _denominator;
	}

	public void Add(BigInteger value)
	{
		_numerator += value * _denominator;
		_CompressIfPossible();
	}

	public void Subtract(BigInteger value)
	{
		_numerator -= value * _denominator;
		_CompressIfPossible();
	}

	public void Multiply(BigInteger value)
	{
		_numerator *= value;
		_CompressIfPossible();
	}

	public void Divide(BigInteger value)
	{
		bool isNegative = value < 0;

		if (isNegative)
		{
			_numerator = -_numerator;
			value = -value;
		}

		_denominator *= value;
		_CompressIfPossible();
	}

	// TODO: Maybe add more primes to make things work great.
	private static readonly int[] _primeNumbers = new int[] { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31 };
	private void _CompressIfPossible()
	{
		// If divider is already one, there's nothing to compress.
		if (_denominator == 1)
			return;

		if ((_numerator % _denominator) == 0)
		{
			_numerator /= _denominator;
			_denominator = 1;
			return;
		}

		int primeLength = _primeNumbers.Length;
		int index = 0;
		while(index < primeLength)
		{
			var prime = _primeNumbers[index];

			var absValue = BigInteger.Abs(_numerator);
			if (prime > _denominator || prime > absValue)
				break;

			if ((_numerator % prime) == 0 && (_denominator % prime) == 0)
			{
				_numerator /= prime;
				_denominator /= prime;

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
		var value = new BigDecimal(_numerator);
		var divider = new BigDecimal(_denominator);
		var result = BigDecimal.Divide(value, divider, floatDigitCount);
		return result;
	}

	public void Power(int exponent)
	{
		if (exponent == 0)
		{
			_numerator = 1;
			_denominator = 1;
			return;
		}

		if (exponent == 1)
			return;

		if (exponent > 1)
		{
			_numerator = BigInteger.Pow(_numerator, exponent);
			_denominator = BigInteger.Pow(_denominator, exponent);
			return;
		}

		// Let's make the exponenent positive.
		// Then apply it to denominator and numerator.
		exponent = -exponent;

		// Not sure why, but things get inverted here.
		var numerator = _numerator;
		var denominator = _denominator;

		_numerator = BigInteger.Pow(denominator, exponent);
		_denominator = BigInteger.Pow(numerator, exponent);
	}

	public int CompareTo(BigRational? other)
	{
		// We are always greater than null.
		if (other == null)
			return 1;

		// When dividers (the denominator) are the same,
		// we can compare things directly.
		var divider1 = _denominator;
		var divider2 = other._denominator;
		if (divider1 == divider2)
			return _numerator.CompareTo(other._numerator);

		// When the denominators are different, we just "cross" the
		// dividers and everything should be fine. We could alternatively
		// divide the values, but then we might suffer from some loss of precision.
		var value1 = _numerator * divider2;
		var value2 = other._numerator * divider1;
		return value1.CompareTo(value2);
	}

	public override int GetHashCode()
	{
		// Usually the denominator does a division... but here, to generate
		// a hash-code, better use it as a multiplier.
		return (_numerator * _denominator).GetHashCode();
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

		var value1 = _numerator * other._denominator;
		var value2 = other._numerator * _denominator;
		return value1 == value2;
	}

	public void ToString(StringBuilder stringBuilder)
	{
		stringBuilder.Append(nameof(BigRational));
		stringBuilder.Append('(');
		stringBuilder.Append(_numerator.ToString());
		stringBuilder.Append(" / ");
		stringBuilder.Append(_denominator.ToString());
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

		var thisDenominator = _denominator;
		var otherDenominator = other._denominator;

		if (thisDenominator == otherDenominator)
			_numerator += other._numerator;
		else
		{
			_denominator *= otherDenominator;
			
			_numerator *= otherDenominator;
			_numerator += other._numerator * thisDenominator;
		}

		_CompressIfPossible();
	}

	public void Subtract(BigRational other)
	{
		ArgumentNullException.ThrowIfNull(other);

		var thisDenominator = _denominator;
		var otherDenominator = other._denominator;

		if (thisDenominator == otherDenominator)
			_numerator -= other._numerator;
		else
		{
			_denominator *= otherDenominator;

			_numerator *= otherDenominator;
			_numerator -= other._numerator * thisDenominator;
		}

		_CompressIfPossible();
	}

	public void Multiply(BigRational other)
	{
		ArgumentNullException.ThrowIfNull(other);

		var thisDenominator = _denominator;
		var otherDenominator = other._denominator;

		if (thisDenominator == otherDenominator)
			_numerator *= other._numerator;
		else
		{
			_denominator *= otherDenominator;
			_numerator *= other._numerator;
		}

		_CompressIfPossible();
	}

	public void Divide(BigRational other)
	{
		ArgumentNullException.ThrowIfNull(other);

		var thisDenominator = _denominator;
		var otherDenominator = other._denominator;

		_denominator *= other._numerator;
		_numerator *= otherDenominator;

		_CompressIfPossible();
	}
}
