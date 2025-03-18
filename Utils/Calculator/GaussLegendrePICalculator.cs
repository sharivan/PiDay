using Pfz.Math;

using static PiDay.Utils.Consts;
using static PiDay.Utils.Functions;

namespace PiDay.Utils.Calculator
{
    public class GaussLegendrePICalculator : StepCalculator
    {
        private BigDecimal a0;
        private BigDecimal b0;
        private BigDecimal t0;
        private BigDecimal p0;
        private BigDecimal a;
        private BigDecimal b;
        private BigDecimal t;
        private BigDecimal p;

        public GaussLegendrePICalculator(int decimalDigits, int margin = 5) : base(decimalDigits, margin)
        {
            a0 = BigDecimal.One;
            b0 = BigDecimal.Divide(BigDecimal.One, SquareRoot(TWO, Digits), Digits);
            t0 = BigDecimal.Divide(BigDecimal.One, FOUR, Digits);
            p0 = BigDecimal.One;
        }

        protected override BigDecimal StepEval()
        {
            a = BigDecimal.Divide(a0 + b0, TWO, Digits);
            b = SquareRoot(a0 * b0, Digits);
            var deltaA = a0 - a;
            t = t0 - p0 * deltaA * deltaA;
            p = TWO * p0;

            var apb = a + b;
            var pi = BigDecimal.Divide(apb * apb, FOUR * t, Digits);
            return pi;
        }

        protected override void AfterStep()
        {
            base.AfterStep();

            a0 = a;
            b0 = b;
            t0 = t;
            p0 = p;
        }
    }
}