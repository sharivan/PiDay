// Licensed under CPOL: https://en.wikipedia.org/wiki/Code_Project_Open_License
// 
// Author: Paulo Francisco Zemek. August, 01, 2023.
// 
// The class in this file is still under development. Use it at your own risk.
// The purpose of this class is to provide an equivalent to BigInteger,
// but that works with decimal/floating point values.
// Internally, it effectively has just a BigInteger value, and a flag telling
// where the dot (the digit separator) goes. It also has a flag about positive
// or negative values.
// From the manual tests done so far it works great, but I still need to focus
// on more tests and try to cover all scenarios.

using System.Numerics;
using System.Text;

namespace Pfz.Math;

/// <summary>
/// This class is similar to a BigInteger (and in fact, it uses a BigInteger inside)
/// but it supports floating points/decimal values. You can compare it with the "decimal" type
/// if you want, but it is theoretically less limited in the range of values it supports, but
/// I (the author) was never able to prove that (at least not at the point where I wrote this
/// message). I hope it works for you. It implements the operators +, - * and /. Nothing else.
/// For the division, you can call Divide() to pass an extra parameter telling how many digits
/// after the dot (that is, how many decimal digits) are to be used.
/// Hope this class works well for your projects.
/// </summary>
public readonly struct BigDecimal:
	IEquatable<BigDecimal>,
	IComparable<BigDecimal>
{
	public static readonly BigDecimal Zero = new BigDecimal(0);
	public static readonly BigDecimal One = new BigDecimal(1);
	public static readonly BigDecimal Two = new BigDecimal(2);

	public static readonly BigInteger DefaultFloatDigitCount = new BigInteger(8);

	public static bool TryParse(string stringValue, out BigDecimal result)
	{
		if (string.IsNullOrEmpty(stringValue))
		{
			result = default;
			return false;
		}

		int i =0;
		int length = stringValue.Length;
		BigInteger bigInteger = 0;

		// First we check if the number is negative.
		bool isNegative = false;
		if (stringValue[i] == '-')
		{
			isNegative = true;
			i++;
		}

		// Then we skip all 0s at the left, if any.
		while (i < length)
		{
			char c = stringValue[i];

			if (c != '0')
				break;

			i++;
		}

		// Then we process all digits before the dot (the integer part).
		// Notice that this code allows .5 to be read the same way as 0.5.
		// That's a trait, not a bug of this code.
		while(i < length)
		{
			char c = stringValue[i];

			if (c == '.')
				break;

			if (c < '0' || c > '9')
			{
				result = default;
				return false;
			}

			bigInteger *= 10;
			bigInteger += c - '0';

			i++;
		}

		// Finally we got here. Either we found a dot, so we need to go to the next
		// index, or we reached the end. It doesn't matter... let's increase the int
		// and try to loop.
		i++;
		BigInteger countAfterDot = 0;
		while (i < length)
		{
			char c = stringValue[i];

			if (c < '0' || c > '9')
			{
				result = default;
				return false;
			}

			bigInteger *= 10;
			bigInteger += c - '0';

			countAfterDot++;
			i++;
		}

		if (isNegative)
			bigInteger = -bigInteger;

		result = new BigDecimal(bigInteger, countAfterDot);
		return true;
	}

	public static BigDecimal Parse(string stringValue)
	{
		if (!TryParse(stringValue, out var result))
			throw new ArgumentException();

		return result;
	}

	public static BigDecimal Abs(BigDecimal value)
	{
		if (value._isNegative)
			return new BigDecimal(value._rawValue, value._decimalSeparatorIndexFromEnd);

		return value;
	}

	// Is there a faster approach that works with BigIntegers?
	// It seems Log10 isn't faster at all.
	private static BigInteger _CountDigits(BigInteger value)
	{
		BigInteger count = 0;

		while (value > 0)
		{
			count++;
			value /= 10;
		}

		return count;
	}

	private readonly BigInteger _rawValue;
	private readonly BigInteger _decimalSeparatorIndexFromEnd;
	private readonly BigInteger _digitCount;
	private readonly bool _isNegative;

	// In theory we should have all conversions from smaller integers to a BigDecimal
	// But covering BigInteger and ulong and long, I believe we cover all constants
	// that naturally expand from byte to long or ulong and the like.
	public static implicit operator BigDecimal(BigInteger value)
	{
		return new BigDecimal(value);
	}

	public BigDecimal(BigInteger value):
		this(value, 0)
	{
	}
	public BigDecimal(BigInteger value, BigInteger decimalIndexFromEnd)
	{
		if (decimalIndexFromEnd < 0)
			throw new ArgumentOutOfRangeException(nameof(decimalIndexFromEnd));

		_isNegative = value < 0;
		if (_isNegative)
			value = -value;

		_digitCount = _CountDigits(value);
		_TrimRight(ref decimalIndexFromEnd, ref value, ref _digitCount);
		_rawValue = value;
		_decimalSeparatorIndexFromEnd = decimalIndexFromEnd;
	}

	private static void _TrimRight(ref BigInteger decimalIndexFromEnd, ref BigInteger value, ref BigInteger digitCount)
	{
		while (decimalIndexFromEnd > 0 && (value % 10) == 0)
		{
			decimalIndexFromEnd--;
			value /= 10;
			digitCount--;
		}
	}

	/// <summary>
	/// Gets the value that composes this BigDecimal without a floating point
	/// or a positive/negative sign.
	/// </summary>
	public BigInteger RawValue
	{
		get => _rawValue;
	}

	/// <summary>
	/// Gets the total digit count of the raw-value. Doesn't include the negative sign
	/// or the dot in such a count.
	/// </summary>
	public BigInteger DigitCount
	{
		get => _digitCount;
	}

	/// <summary>
	/// Gets where the decimal separator (the dot) is, counting from the end
	/// of the value. A value of 0 means the separator is at the end, and so
	/// it is not shown when doing a ToString(), as the number is considered
	/// integer.
	/// </summary>
	public BigInteger DecimalSeparatorIndexFromEnd
	{
		get => _decimalSeparatorIndexFromEnd;
	}

	public BigInteger ToBigInteger()
	{
		var value = _rawValue;
		for(int i=0; i< _decimalSeparatorIndexFromEnd; i++)
			value /= 10;

		if (_isNegative)
			value = -value;

		return value;
	}

	/// <summary>
	/// Gets a value indicating wether the contained value is negative or not.
	/// </summary>
	public bool IsNegative
	{
		get => _isNegative;
	}

	/// <summary>
	/// Adds the contents of this BigDecimal to an existing StringBuilder.
	/// This is useful if you need to add many different BigDecimals into a StringBuilder
	/// without having to convert each one of them to a string first.
	/// </summary>
	public void ToString(StringBuilder stringBuilder)
	{
		ArgumentNullException.ThrowIfNull(stringBuilder);

		if (_rawValue == 0)
		{
			stringBuilder.Append('0');
			return;
		}

		if (_isNegative)
			stringBuilder.Append('-');

		var value = _rawValue;
		var divisor = new BigInteger(1);
		for (BigInteger i = 1; i < _digitCount; i++)
			divisor *= 10;

		bool dotWritten = false;
		if (_decimalSeparatorIndexFromEnd >= _digitCount)
		{
			stringBuilder.Append("0.");
			dotWritten = true;

			var decimalIndexFromEnd = _decimalSeparatorIndexFromEnd-1;
			while (decimalIndexFromEnd >= _digitCount)
			{
				stringBuilder.Append('0');
				decimalIndexFromEnd--;
			}
		}

		var divisorIndex = _digitCount - _decimalSeparatorIndexFromEnd;

		for (BigInteger i = 0; i < _digitCount; i++)
		{
			if (!dotWritten && i == divisorIndex)
				stringBuilder.Append('.');

			var digitValue = (_rawValue / divisor) % 10;
			stringBuilder.Append((char)(digitValue + '0'));
			divisor /= 10;
		}
	}

	/// <summary>
	/// Converts this BigDecimal to a string. Assuming everything is fine, it will include
	/// absolutely all digits. It doesn't have a trim option on this method (this differs
	/// from BigInteger that trims values by default).
	/// </summary>
	public override string ToString()
	{
		if (_rawValue == 0)
			return "0";

		var stringBuilder = new StringBuilder();
		ToString(stringBuilder);
		return stringBuilder.ToString();
	}

	private static BigInteger _MakeItHaveThisAmountOfFloatDigits(BigDecimal value, BigInteger numberOfFloatDigits)
	{
		if (value._decimalSeparatorIndexFromEnd == numberOfFloatDigits)
			return value._rawValue;  // Already right size. Do nothing.

		var diff = numberOfFloatDigits - value._decimalSeparatorIndexFromEnd;
		var multiplier = new BigInteger(1);
		for (var i=0; i<diff; i++)
			multiplier *= 10;

		var intPart = value._rawValue * multiplier;
		return intPart;
	}

	public static BigDecimal operator + (BigDecimal a, BigDecimal b)
	{
		var digitFromEnd0 = a._decimalSeparatorIndexFromEnd;
		var digitFromEnd1 = b._decimalSeparatorIndexFromEnd;

		var maxIndex = BigInteger.Max(digitFromEnd0, digitFromEnd1);
		var intA = _MakeItHaveThisAmountOfFloatDigits(a, maxIndex);
		var intB = _MakeItHaveThisAmountOfFloatDigits(b, maxIndex);

		if (a._isNegative)
			intA = -intA;

		if (b._isNegative)
			intB = -intB;

		var sum = intA + intB;
		return new BigDecimal(sum, maxIndex);
	}

	public static BigDecimal operator - (BigDecimal a, BigDecimal b)
	{
		var digitFromEnd0 = a._decimalSeparatorIndexFromEnd;
		var digitFromEnd1 = b._decimalSeparatorIndexFromEnd;

		var maxIndex = BigInteger.Max(digitFromEnd0, digitFromEnd1);
		var intA = _MakeItHaveThisAmountOfFloatDigits(a, maxIndex);
		var intB = _MakeItHaveThisAmountOfFloatDigits(b, maxIndex);

		if (a._isNegative)
			intA = -intA;

		if (b._isNegative)
			intB = -intB;

		var subtraction = intA - intB;
		return new BigDecimal(subtraction, maxIndex);
	}

	public static BigDecimal operator * (BigDecimal a, BigDecimal b)
	{
		var digitFromEnd0 = a._decimalSeparatorIndexFromEnd;
		var digitFromEnd1 = b._decimalSeparatorIndexFromEnd;

		var intA = a._rawValue;
		var intB = b._rawValue;

		// It doesn't matter who is negative... as long as one is negative and the
		// other is not, we must make the result negative.
		bool isResultNegative = (a._isNegative != b._isNegative);
		var sum = a._rawValue * b._rawValue;
		if (isResultNegative)
			sum = -sum;

		var indexFromEnd = a._decimalSeparatorIndexFromEnd + b._decimalSeparatorIndexFromEnd;

		return new BigDecimal(sum, indexFromEnd);
	}

	public static BigDecimal operator / (BigDecimal dividend, BigDecimal divisor)
	{
		return BigDecimal.Divide(dividend, divisor, DefaultFloatDigitCount);
	}

	/// <summary>
	/// Divides the given dividend by the given divisor, and allows up to floatDigitCount
	/// of digits after the dot.
	/// </summary>
	public static BigDecimal Divide(BigDecimal dividend, BigDecimal divisor, BigInteger floatDigitCount)
	{
		if (floatDigitCount < 0)
			throw new ArgumentOutOfRangeException(nameof(floatDigitCount));

		if (divisor._rawValue == 0)
		{
			// This rule might look odd, but when simplifying expressions, x/x (x divided by x) is 1.
			// So, to keep the rule true, 0 divided by 0 is also 1.
			if (dividend._rawValue == 0)
				return One;

			throw new DivideByZeroException($"{nameof(divisor)} can only be zero if {nameof(dividend)} is zero.");
		}

		var maxDigitCount = BigInteger.Max(dividend._decimalSeparatorIndexFromEnd, divisor._decimalSeparatorIndexFromEnd);

		var finalFloatCount = maxDigitCount + floatDigitCount;
		var intDividend = _MakeItHaveThisAmountOfFloatDigits(dividend, finalFloatCount);
		var intDivisor = _MakeItHaveThisAmountOfFloatDigits(divisor, maxDigitCount);

		// It doesn't matter who is negative... as long as one is negative and the
		// other is not, we must make the result negative.
		bool isResultNegative = (dividend._isNegative != divisor._isNegative);
		var result = intDividend / intDivisor;
		if (isResultNegative)
			result = -result;

		var maxDividendDivisorDecimalPlace = BigInteger.Max(dividend._decimalSeparatorIndexFromEnd, divisor._decimalSeparatorIndexFromEnd);
		return new BigDecimal(result, finalFloatCount - maxDividendDivisorDecimalPlace);
	}

	public static BigDecimal operator % (BigDecimal dividend, BigDecimal divisor)
	{
		return Remainder(dividend, divisor, 0);
	}
	public static BigDecimal Remainder(BigDecimal dividend, BigDecimal divisor, BigInteger floatDigitCount)
	{
		var divisionResult = Divide(dividend, divisor, floatDigitCount);
		var result = dividend - divisionResult * divisor;
		return result;
	}

	private static readonly BigInteger _bigIntegerTwo = new (2);
	private static BigInteger _Power(BigInteger value, BigInteger power, BigInteger floatDigitCount)
	{
		if (power == BigInteger.Zero)
			return BigInteger.One;

		if (power == BigInteger.One)
			return value;

		if (power == _bigIntegerTwo)
			return value * value;

		var leftSide = power / _bigIntegerTwo;
		var rightSide = power - leftSide; // Notice that we might have 2 on left side and 3 on right side if power is 5.

		var resultLeftSide = _Power(value, leftSide, floatDigitCount);
		if (leftSide == rightSide)
			return resultLeftSide * resultLeftSide;

		var rightSideResult = _Power(value, rightSide, floatDigitCount);
		return resultLeftSide * rightSideResult;
	}

	public BigDecimal Power(BigInteger exponent)
	{
		return Power(exponent, DefaultFloatDigitCount);
	}
	public BigDecimal Power(BigInteger exponent, BigInteger floatDigitCount)
	{
		if (exponent == BigInteger.Zero)
			return One;

		if (exponent == BigInteger.One)
			return this;

		bool isExponentNegative = exponent < 0;
		if (isExponentNegative)
			exponent = -exponent;

		var decimalIndexFromEnd = _decimalSeparatorIndexFromEnd * exponent;
		var totalDigits = decimalIndexFromEnd + floatDigitCount;
		var calculatedValue = _Power(_rawValue, exponent, totalDigits);

		if (isExponentNegative)
		{
			calculatedValue = _MakeItHaveThisAmountOfFloatDigits(BigInteger.One, totalDigits) / calculatedValue;
			decimalIndexFromEnd = totalDigits; // TODO understand why this is needed. Maybe the prior line is wrong.
		}

			if (_isNegative && (exponent % 2) != 0)
			calculatedValue = -calculatedValue;

		var result = new BigDecimal(calculatedValue, decimalIndexFromEnd);
		return result;
	}

	public override int GetHashCode()
	{
		return (_rawValue.GetHashCode() + _isNegative.GetHashCode()) | (_decimalSeparatorIndexFromEnd.GetHashCode() << 17);
	}

	public override bool Equals(object? obj)
	{
		if (!(obj is BigDecimal))
			return false;

		return Equals((BigDecimal)obj);
	}
	public bool Equals(BigDecimal other)
	{
		return this == other;
	}

	public int CompareTo(BigDecimal other)
	{
		if (_isNegative != other._isNegative)
		{
			if (_isNegative)
				return -1;

			return 1;
		}

		var digitFromEnd0 = this._decimalSeparatorIndexFromEnd;
		var digitFromEnd1 = other._decimalSeparatorIndexFromEnd;

		var maxIndex = BigInteger.Max(digitFromEnd0, digitFromEnd1);
		var intA = _MakeItHaveThisAmountOfFloatDigits(this, maxIndex);
		var intB = _MakeItHaveThisAmountOfFloatDigits(other, maxIndex);

		if (_isNegative)
			return intB.CompareTo(intA);

		return intA.CompareTo(intB);
	}

	public static bool operator > (BigDecimal a, BigDecimal b)
	{
		return a.CompareTo(b) > 0;
	}
	public static bool operator >= (BigDecimal a, BigDecimal b)
	{
		return a.CompareTo(b) >= 0;
	}
	public static bool operator < (BigDecimal a, BigDecimal b)
	{
		return a.CompareTo(b) < 0;
	}
	public static bool operator <= (BigDecimal a, BigDecimal b)
	{
		return a.CompareTo(b) <= 0;
	}

	public static bool operator == (BigDecimal a, BigDecimal b)
	{
		return
			a._rawValue == b._rawValue &&
			a._decimalSeparatorIndexFromEnd == b._decimalSeparatorIndexFromEnd &&
			a._isNegative == b._isNegative;
	}
	public static bool operator != (BigDecimal a, BigDecimal b)
	{
		return !(a == b);
	}
}
