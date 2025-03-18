namespace PiDay;

partial class FrmPiDay
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        label1 = new Label();
        btnCalculatePI = new Button();
        label2 = new Label();
        txtDecimalDigits = new TextBox();
        txtPI = new RichTextBox();
        label3 = new Label();
        cmbMethod = new ComboBox();
        pbProgress = new ProgressBar();
        SuspendLayout();
        // 
        // label1
        // 
        label1.AutoSize = true;
        label1.Location = new Point(7, 9);
        label1.Name = "label1";
        label1.Size = new Size(29, 15);
        label1.TabIndex = 0;
        label1.Text = "π = ";
        // 
        // btnCalculatePI
        // 
        btnCalculatePI.Anchor =  AnchorStyles.Bottom | AnchorStyles.Left;
        btnCalculatePI.Location = new Point(12, 198);
        btnCalculatePI.Name = "btnCalculatePI";
        btnCalculatePI.Size = new Size(76, 21);
        btnCalculatePI.TabIndex = 2;
        btnCalculatePI.Text = "Calcule π";
        btnCalculatePI.UseVisualStyleBackColor = true;
        btnCalculatePI.Click += BtnCalculatePI_Click;
        // 
        // label2
        // 
        label2.Anchor =  AnchorStyles.Bottom | AnchorStyles.Left;
        label2.AutoSize = true;
        label2.Location = new Point(12, 139);
        label2.Name = "label2";
        label2.Size = new Size(90, 15);
        label2.TabIndex = 3;
        label2.Text = "Casas decimais:";
        // 
        // txtDecimalDigits
        // 
        txtDecimalDigits.Anchor =  AnchorStyles.Bottom | AnchorStyles.Left;
        txtDecimalDigits.Location = new Point(108, 136);
        txtDecimalDigits.Name = "txtDecimalDigits";
        txtDecimalDigits.Size = new Size(100, 23);
        txtDecimalDigits.TabIndex = 4;
        txtDecimalDigits.Text = "50";
        // 
        // txtPI
        // 
        txtPI.Anchor =  AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        txtPI.Location = new Point(35, 6);
        txtPI.Name = "txtPI";
        txtPI.Size = new Size(676, 114);
        txtPI.TabIndex = 5;
        txtPI.Text = "";
        // 
        // label3
        // 
        label3.Anchor =  AnchorStyles.Bottom | AnchorStyles.Left;
        label3.AutoSize = true;
        label3.Location = new Point(12, 169);
        label3.Name = "label3";
        label3.Size = new Size(52, 15);
        label3.TabIndex = 6;
        label3.Text = "Método:";
        // 
        // cmbMethod
        // 
        cmbMethod.Anchor =  AnchorStyles.Bottom | AnchorStyles.Left;
        cmbMethod.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbMethod.FormattingEnabled = true;
        cmbMethod.Items.AddRange(new object[] { "Lento pra Cassete", "Rápido (Fórmula de Machin)", "Super Rápido (Algoritmo de Gauss-Legendre)" });
        cmbMethod.Location = new Point(70, 166);
        cmbMethod.Name = "cmbMethod";
        cmbMethod.Size = new Size(266, 23);
        cmbMethod.TabIndex = 7;
        // 
        // pbProgress
        // 
        pbProgress.Anchor =  AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        pbProgress.Location = new Point(94, 196);
        pbProgress.Name = "pbProgress";
        pbProgress.Size = new Size(617, 23);
        pbProgress.TabIndex = 8;
        pbProgress.Visible = false;
        // 
        // FrmPiDay
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(723, 228);
        Controls.Add(pbProgress);
        Controls.Add(cmbMethod);
        Controls.Add(label3);
        Controls.Add(txtPI);
        Controls.Add(txtDecimalDigits);
        Controls.Add(label2);
        Controls.Add(btnCalculatePI);
        Controls.Add(label1);
        Name = "FrmPiDay";
        Text = "Calculadora de π";
        FormClosing += FrmPiDay_FormClosing;
        Load += FrmPiDay_Load;
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private Label label1;
    private Button btnCalculatePI;
    private Label label2;
    private TextBox txtDecimalDigits;
    private RichTextBox txtPI;
    private Label label3;
    private ComboBox cmbMethod;
    private ProgressBar pbProgress;
}