using Pfz.Math;

using static PiDay.Utils.Consts;

namespace PiDay.Utils.Calculator;

public class MachinPICalculator : StepCalculator
{
    private ArctanCalculator oneFifithArctanCalculator;
    private ArctanCalculator one239thArctanCalculator;

    public MachinPICalculator(int decimalDigits, int margin = 5) : base(decimalDigits, margin)
    {
        oneFifithArctanCalculator = new ArctanCalculator(BigDecimal.Divide(BigDecimal.One, FIVE, Digits), decimalDigits, margin);
        one239thArctanCalculator = new ArctanCalculator(BigDecimal.Divide(BigDecimal.One, C239, Digits), decimalDigits, margin);
    }

    protected override BigDecimal StepEval()
    {
        oneFifithArctanCalculator.Step();
        one239thArctanCalculator.Step();

        var eval = FOUR * (FOUR * oneFifithArctanCalculator.Eval - one239thArctanCalculator.Eval);
        return eval;
    }
}