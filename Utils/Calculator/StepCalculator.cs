using Pfz.Math;

using static PiDay.Utils.Consts;

namespace PiDay.Utils.Calculator;

public abstract class StepCalculator
{
    public delegate void ProgressDelegate(float progress, BigDecimal currentEval, int computedDigits);
    public delegate void CompletedDelegate(BigDecimal result, int decimalDigits);

    public event ProgressDelegate OnProgress;
    public event CompletedDelegate OnComplete;

    private int lastComputedDigits;
    private BigDecimal tenScale;
    private bool firstEval = true;

    public int DecimalDigits { get; }

    public int Margin { get; }

    public int Digits { get; }

    public float Progress
    {
        get;
        private set;
    } = 0;

    public bool IsComplete
    {
        get;
        private set;
    } = false;

    public BigDecimal Eval { get; private set; } = BigDecimal.Zero;

    public StepCalculator(int decimalDigits, int margin = 5)
    {
        DecimalDigits = decimalDigits;
        Margin = margin;
        Digits = decimalDigits + margin;

        lastComputedDigits = 0;
        tenScale = BigDecimal.Divide(BigDecimal.One, TEN, Digits);
    }

    protected virtual void BeforeStep()
    {
    }

    protected abstract BigDecimal StepEval();

    protected virtual void AfterStep()
    {
    }

    public bool Step()
    {
        if (IsComplete)
            return true;

        BeforeStep();
        var eval = StepEval();

        try
        {
            if (!firstEval)
            {
                var delta = BigDecimal.Abs(eval - Eval);
                if (delta == BigDecimal.Zero)
                {
                    Progress = 1;
                    IsComplete = true;
                    Eval = eval;
                    OnComplete?.Invoke(eval, DecimalDigits);
                    return true;
                }

                int computedDigits = lastComputedDigits;
                while (delta < tenScale)
                {
                    computedDigits++;
                    tenScale = BigDecimal.Divide(tenScale, TEN, Digits);
                }

                if (computedDigits > lastComputedDigits)
                {
                    Progress = (float) computedDigits / Digits;
                    OnProgress?.Invoke(Progress, eval, computedDigits);
                    lastComputedDigits = computedDigits;
                }
            }
            else
            {
                firstEval = false;
            }
        }
        finally
        {
            Eval = eval;
            AfterStep();
        }

        return false;
    }
}