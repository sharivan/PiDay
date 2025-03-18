using Pfz.Math;

using static PiDay.Utils.Consts;

namespace PiDay.Utils;

public sealed class Functions
{
    public static BigDecimal SquareRoot(BigDecimal x, int digits)
    {
        var result = x;
        var lastResult = result;

        do
        {
            result = BigDecimal.Divide(result + BigDecimal.Divide(x, result, digits), TWO, digits);
            if (result.CompareTo(lastResult) == 0)
                break;

            lastResult = result;
        }
        while (true);

        return result;
    }
}