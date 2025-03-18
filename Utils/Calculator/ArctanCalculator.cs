using Pfz.Math;

using static PiDay.Utils.Consts;

namespace PiDay.Utils.Calculator;

public class ArctanCalculator(BigDecimal x, int decimalDigits, int margin = 5) : StepCalculator(decimalDigits, margin)
{
    private BigDecimal sum = BigDecimal.Zero;
    private BigDecimal i = BigDecimal.One;
    private BigDecimal signal = BigDecimal.One;
    private BigDecimal pot = x;
    private BigDecimal xSqr = x * x;

    public BigDecimal X { get; } = x;

    protected override BigDecimal StepEval()
    {
        var term = BigDecimal.Divide(pot, i, Digits);
        sum += signal * term;
        return sum;
    }

    protected override void AfterStep()
    {
        base.AfterStep();

        pot *= xSqr;
        signal = MINUS_ONE * signal;
        i += TWO;
    }
}