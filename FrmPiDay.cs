using Pfz.Math;
using PiDay.Utils.Calculator;
using System.Diagnostics;

namespace PiDay;

public partial class FrmPiDay : Form
{
    private Thread thread;
    private volatile bool evaluating = false;
    private volatile bool interrupted = false;

    public FrmPiDay()
    {
        InitializeComponent();
    }

    private void EvalPI(int method, int decimalDigits)
    {
        try
        {
            StepCalculator calculator = method switch
            {
                // Super Lento
                0 => new VerySlowPICalculator(decimalDigits, 2),
                // Rápido
                1 => new MachinPICalculator(decimalDigits),
                // Super Rápido
                2 => new GaussLegendrePICalculator(decimalDigits),
                _ => throw new Exception($"Método não reconhecido: {method}"),
            };
            calculator.OnProgress += EvalPIProgress;
            calculator.OnComplete += EvalPIComplete;

            while (!interrupted && !calculator.Step()) { }
        }
        catch (ThreadInterruptedException)
        {
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            Debug.WriteLine(e.StackTrace);
        }
        finally
        {
            if (interrupted)
                EvalPIInterrupted();
        }
    }

    private void FrmPiDay_Load(object sender, EventArgs e)
    {
        cmbMethod.SelectedIndex = 2;
    }

    private void EvalPIProgress(float progress, BigDecimal currentEval, int computedDigits)
    {
        if (InvokeRequired)
        {
            BeginInvoke(EvalPIProgress, [progress, currentEval, computedDigits]);
            return;
        }

        pbProgress.Value = (int) (progress * 100);
    }

    private void EvalPIComplete(BigDecimal pi, int decimalDigits)
    {
        if (InvokeRequired)
        {
            BeginInvoke(EvalPIComplete, [pi, decimalDigits]);
            return;
        }

        string s = pi.ToString();
        int p = s.IndexOf('.');
        s = string.Concat(s.AsSpan(0, p), ".", s.AsSpan(p + 1, decimalDigits));
        txtPI.Text = s;

        evaluating = false;
        interrupted = false;
        pbProgress.Visible = false;
        btnCalculatePI.Text = "Calcule π";
    }

    private void EvalPIInterrupted()
    {
        if (InvokeRequired)
        {
            BeginInvoke(EvalPIInterrupted);
            return;
        }

        interrupted = false;
        evaluating = false;
        pbProgress.Visible = false;
        btnCalculatePI.Text = "Calcule π";
    }

    private void BtnCalculatePI_Click(object sender, EventArgs e)
    {
        if (evaluating)
        {
            interrupted = true;
        }
        else
        {
            btnCalculatePI.Text = "Cancelar";
            pbProgress.Visible = true;
            pbProgress.Value = 0;

            int decimalDigits = int.Parse(txtDecimalDigits.Text);
            int method = cmbMethod.SelectedIndex;

            evaluating = true;
            interrupted = false;
            thread = new Thread(() => EvalPI(method, decimalDigits));
            thread.Start();
        }
    }

    private void FrmPiDay_FormClosing(object sender, FormClosingEventArgs e)
    {
        interrupted = true;

        if (thread != null)
        {
            thread.Interrupt();
            thread = null;
        }
    }
}