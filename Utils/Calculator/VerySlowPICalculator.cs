using Pfz.Math;

using static PiDay.Utils.Consts;

namespace PiDay.Utils.Calculator;

public class VerySlowPICalculator : StepCalculator
{
    private ArctanCalculator calculator;

    public VerySlowPICalculator(int decimalDigits, int margin = 5) : base(decimalDigits, margin) => calculator = new ArctanCalculator(BigDecimal.One, decimalDigits, margin);

    protected override BigDecimal StepEval()
    {
        calculator.Step();
        var eval = FOUR * calculator.Eval;
        return eval;
    }
}