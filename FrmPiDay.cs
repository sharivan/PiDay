using Pfz.Math;
using System.Numerics;

namespace PiDay;

public partial class FrmPiDay : Form
{
    private delegate void EvalPiCompletedDelegate(BigDecimal pi, int decimalDigits);

    private static readonly BigDecimal MINUS_ONE = new BigDecimal(-1);
    private static readonly BigDecimal TWO = new BigDecimal(2);
    private static readonly BigDecimal FOUR = new BigDecimal(4);
    private static readonly BigDecimal FIVE = new BigDecimal(5);
    private static readonly BigDecimal C239 = new BigDecimal(239);

    public FrmPiDay()
    {
        InitializeComponent();
    }

    private static BigDecimal ArcTan(BigDecimal x, int digits)
    {    
        var scale = digits + 2;
        var tenScale = new BigDecimal(1, scale);
        var soma = BigDecimal.Zero;
        var i = BigDecimal.One;
        var signal = BigDecimal.One;
        var potencia = x;
        var x2 = x * x;

        do
        {
            var termo = BigDecimal.Divide(potencia, i, digits);
            potencia *= x2;

            if (termo < tenScale)
                break;

            soma += signal * termo;
            signal = MINUS_ONE * signal;
            i += TWO;
        }
        while (true);

        return soma;
    }

    private void EvalPISlow(int decimalDigits, EvalPiCompletedDelegate onComplete)
    {
        var digits = decimalDigits + 2;
        var pi = FOUR * ArcTan(BigDecimal.One, digits);

        onComplete(pi, decimalDigits);
    }

    private void EvalPIMachin(int decimalDigits, EvalPiCompletedDelegate onComplete)
    {
        var digits = decimalDigits + 2;
        var one_fifith = BigDecimal.Divide(BigDecimal.One, FIVE, digits);
        var one_239th = BigDecimal.Divide(BigDecimal.One, C239, digits);
        var pi = FOUR * (FOUR * ArcTan(one_fifith, digits) - ArcTan(one_239th, digits));

        onComplete(pi, decimalDigits);
    }

    private static BigDecimal SquareRoot(BigDecimal x, int digits)
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

    private void EvalPIGaussLegendre(int decimalDigits, EvalPiCompletedDelegate onComplete)
    {
        var digits = decimalDigits + 2;
        var a0 = BigDecimal.One;
        var b0 = BigDecimal.Divide(BigDecimal.One, SquareRoot(TWO, digits), digits);
        var t0 = BigDecimal.Divide(BigDecimal.One, FOUR, digits);
        var p0 = BigDecimal.One;        
        var lastPI = BigDecimal.Zero;

        do
        {
            var a = BigDecimal.Divide(a0 + b0, TWO, digits);
            var b = SquareRoot(a0 * b0, digits);
            var deltaA = a0 - a;
            var t = t0 - p0 * deltaA * deltaA;
            var p = TWO * p0;

            var apb = a + b;
            var pi = BigDecimal.Divide(apb * apb, FOUR * t, digits);

            if (pi.CompareTo(lastPI) == 0)
                break;

            a0 = a;
            b0 = b;
            t0 = t;
            p0 = p;
            lastPI = pi;
        }
        while (true);

        onComplete(lastPI, decimalDigits);
    }

    private void EvalPI(int method, int decimalDigits, EvalPiCompletedDelegate onComplete)
    {
        switch (method)
        {
            case 0: // Super Lento
                EvalPISlow(decimalDigits, onComplete);
                break;

            case 1: // Rápido
                EvalPIMachin(decimalDigits, onComplete);
                break;

            case 2: // Super Rápido
                EvalPIGaussLegendre(decimalDigits, onComplete);
                break;
        }
    }

    private void FrmPiDay_Load(object sender, EventArgs e)
    {
        cmbMethod.SelectedIndex = 0;
    }

    private void EvalPICompleted(BigDecimal pi, int decimalDigits)
    {
        if (InvokeRequired)
        {
            BeginInvoke(EvalPICompleted, [pi, decimalDigits]);
            return;
        }

        var s = pi.ToString();
        var p = s.IndexOf('.');
        s = s.Substring(0, p) + '.' + s.Substring(p + 1, decimalDigits);
        txtPI.Text = s;
    }

    private void btnCalculatePI_Click(object sender, EventArgs e)
    {
        var decimalDigits = int.Parse(txtDecimalDigits.Text);
        var method = cmbMethod.SelectedIndex;

        var t = new Thread(() => EvalPI(method, decimalDigits, EvalPICompleted));
        t.Start();       
    }
}