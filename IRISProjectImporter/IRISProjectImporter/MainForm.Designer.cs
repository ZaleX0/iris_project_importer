
namespace IRISProjectImporter
{
    partial class MainForm
    {
        /// <summary>
        /// Wymagana zmienna projektanta.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Wyczyść wszystkie używane zasoby.
        /// </summary>
        /// <param name="disposing">prawda, jeżeli zarządzane zasoby powinny zostać zlikwidowane; Fałsz w przeciwnym wypadku.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Kod generowany przez Projektanta formularzy systemu Windows

        /// <summary>
        /// Metoda wymagana do obsługi projektanta — nie należy modyfikować
        /// jej zawartości w edytorze kodu.
        /// </summary>
        private void InitializeComponent()
        {
            this.hostLabel = new System.Windows.Forms.Label();
            this.portLabel = new System.Windows.Forms.Label();
            this.loginLabel = new System.Windows.Forms.Label();
            this.passwordLabel = new System.Windows.Forms.Label();
            this.hostTextBox = new System.Windows.Forms.TextBox();
            this.portTextBox = new System.Windows.Forms.TextBox();
            this.loginTextBox = new System.Windows.Forms.TextBox();
            this.passwordTextBox = new System.Windows.Forms.TextBox();
            this.dbNameComboBox = new System.Windows.Forms.ComboBox();
            this.reloadDbButton = new System.Windows.Forms.Button();
            this.pathTextBox = new System.Windows.Forms.TextBox();
            this.folderBrowserDialogButton = new System.Windows.Forms.Button();
            this.databaseLabel = new System.Windows.Forms.Label();
            this.pathLabel = new System.Windows.Forms.Label();
            this.logTextBox = new System.Windows.Forms.RichTextBox();
            this.startButton = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // hostLabel
            // 
            this.hostLabel.AutoSize = true;
            this.hostLabel.Location = new System.Drawing.Point(12, 20);
            this.hostLabel.Name = "hostLabel";
            this.hostLabel.Size = new System.Drawing.Size(32, 13);
            this.hostLabel.TabIndex = 0;
            this.hostLabel.Text = "Host:";
            // 
            // portLabel
            // 
            this.portLabel.AutoSize = true;
            this.portLabel.Location = new System.Drawing.Point(12, 46);
            this.portLabel.Name = "portLabel";
            this.portLabel.Size = new System.Drawing.Size(29, 13);
            this.portLabel.TabIndex = 0;
            this.portLabel.Text = "Port:";
            // 
            // loginLabel
            // 
            this.loginLabel.AutoSize = true;
            this.loginLabel.Location = new System.Drawing.Point(12, 72);
            this.loginLabel.Name = "loginLabel";
            this.loginLabel.Size = new System.Drawing.Size(36, 13);
            this.loginLabel.TabIndex = 0;
            this.loginLabel.Text = "Login:";
            // 
            // passwordLabel
            // 
            this.passwordLabel.AutoSize = true;
            this.passwordLabel.Location = new System.Drawing.Point(12, 98);
            this.passwordLabel.Name = "passwordLabel";
            this.passwordLabel.Size = new System.Drawing.Size(56, 13);
            this.passwordLabel.TabIndex = 0;
            this.passwordLabel.Text = "Password:";
            // 
            // hostTextBox
            // 
            this.hostTextBox.Location = new System.Drawing.Point(72, 17);
            this.hostTextBox.Name = "hostTextBox";
            this.hostTextBox.Size = new System.Drawing.Size(280, 20);
            this.hostTextBox.TabIndex = 0;
            // 
            // portTextBox
            // 
            this.portTextBox.Location = new System.Drawing.Point(72, 43);
            this.portTextBox.Name = "portTextBox";
            this.portTextBox.Size = new System.Drawing.Size(280, 20);
            this.portTextBox.TabIndex = 1;
            this.portTextBox.Text = "5432";
            // 
            // loginTextBox
            // 
            this.loginTextBox.Location = new System.Drawing.Point(72, 69);
            this.loginTextBox.Name = "loginTextBox";
            this.loginTextBox.Size = new System.Drawing.Size(280, 20);
            this.loginTextBox.TabIndex = 2;
            // 
            // passwordTextBox
            // 
            this.passwordTextBox.Location = new System.Drawing.Point(72, 95);
            this.passwordTextBox.Name = "passwordTextBox";
            this.passwordTextBox.PasswordChar = '*';
            this.passwordTextBox.Size = new System.Drawing.Size(280, 20);
            this.passwordTextBox.TabIndex = 3;
            // 
            // dbNameComboBox
            // 
            this.dbNameComboBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.dbNameComboBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.dbNameComboBox.FormattingEnabled = true;
            this.dbNameComboBox.Location = new System.Drawing.Point(72, 135);
            this.dbNameComboBox.Name = "dbNameComboBox";
            this.dbNameComboBox.Size = new System.Drawing.Size(280, 21);
            this.dbNameComboBox.TabIndex = 4;
            // 
            // reloadDbButton
            // 
            this.reloadDbButton.Location = new System.Drawing.Point(358, 133);
            this.reloadDbButton.Name = "reloadDbButton";
            this.reloadDbButton.Size = new System.Drawing.Size(114, 23);
            this.reloadDbButton.TabIndex = 6;
            this.reloadDbButton.Text = "Reload";
            this.reloadDbButton.UseVisualStyleBackColor = true;
            this.reloadDbButton.Click += new System.EventHandler(this.reloadDbButton_Click);
            // 
            // pathTextBox
            // 
            this.pathTextBox.Location = new System.Drawing.Point(72, 162);
            this.pathTextBox.Name = "pathTextBox";
            this.pathTextBox.Size = new System.Drawing.Size(280, 20);
            this.pathTextBox.TabIndex = 5;
            // 
            // folderBrowserDialogButton
            // 
            this.folderBrowserDialogButton.Location = new System.Drawing.Point(358, 160);
            this.folderBrowserDialogButton.Name = "folderBrowserDialogButton";
            this.folderBrowserDialogButton.Size = new System.Drawing.Size(114, 23);
            this.folderBrowserDialogButton.TabIndex = 7;
            this.folderBrowserDialogButton.Text = "...";
            this.folderBrowserDialogButton.UseVisualStyleBackColor = true;
            this.folderBrowserDialogButton.Click += new System.EventHandler(this.folderBrowserDialogButton_Click);
            // 
            // databaseLabel
            // 
            this.databaseLabel.AutoSize = true;
            this.databaseLabel.Location = new System.Drawing.Point(12, 138);
            this.databaseLabel.Name = "databaseLabel";
            this.databaseLabel.Size = new System.Drawing.Size(56, 13);
            this.databaseLabel.TabIndex = 0;
            this.databaseLabel.Text = "Database:";
            // 
            // pathLabel
            // 
            this.pathLabel.AutoSize = true;
            this.pathLabel.Location = new System.Drawing.Point(12, 165);
            this.pathLabel.Name = "pathLabel";
            this.pathLabel.Size = new System.Drawing.Size(32, 13);
            this.pathLabel.TabIndex = 0;
            this.pathLabel.Text = "Path:";
            // 
            // logTextBox
            // 
            this.logTextBox.Location = new System.Drawing.Point(12, 231);
            this.logTextBox.Name = "logTextBox";
            this.logTextBox.ReadOnly = true;
            this.logTextBox.Size = new System.Drawing.Size(460, 218);
            this.logTextBox.TabIndex = 14;
            this.logTextBox.Text = "";
            // 
            // startButton
            // 
            this.startButton.Location = new System.Drawing.Point(358, 202);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(114, 23);
            this.startButton.TabIndex = 8;
            this.startButton.Text = "Start";
            this.startButton.UseVisualStyleBackColor = true;
            this.startButton.Click += new System.EventHandler(this.startButton_Click);
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(12, 202);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(340, 23);
            this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBar.TabIndex = 0;
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Checked = true;
            this.checkBox1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox1.Location = new System.Drawing.Point(376, 19);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(77, 17);
            this.checkBox1.TabIndex = 16;
            this.checkBox1.Text = "Remember";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 461);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.startButton);
            this.Controls.Add(this.logTextBox);
            this.Controls.Add(this.pathLabel);
            this.Controls.Add(this.databaseLabel);
            this.Controls.Add(this.folderBrowserDialogButton);
            this.Controls.Add(this.pathTextBox);
            this.Controls.Add(this.reloadDbButton);
            this.Controls.Add(this.dbNameComboBox);
            this.Controls.Add(this.passwordTextBox);
            this.Controls.Add(this.loginTextBox);
            this.Controls.Add(this.portTextBox);
            this.Controls.Add(this.hostTextBox);
            this.Controls.Add(this.passwordLabel);
            this.Controls.Add(this.loginLabel);
            this.Controls.Add(this.portLabel);
            this.Controls.Add(this.hostLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = "IRIS Project Importer v1.0";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label hostLabel;
        private System.Windows.Forms.Label portLabel;
        private System.Windows.Forms.Label loginLabel;
        private System.Windows.Forms.Label passwordLabel;
        private System.Windows.Forms.TextBox hostTextBox;
        private System.Windows.Forms.TextBox portTextBox;
        private System.Windows.Forms.TextBox loginTextBox;
        private System.Windows.Forms.TextBox passwordTextBox;
        private System.Windows.Forms.ComboBox dbNameComboBox;
        private System.Windows.Forms.Button reloadDbButton;
        private System.Windows.Forms.TextBox pathTextBox;
        private System.Windows.Forms.Button folderBrowserDialogButton;
        private System.Windows.Forms.Label databaseLabel;
        private System.Windows.Forms.Label pathLabel;
        private System.Windows.Forms.RichTextBox logTextBox;
        private System.Windows.Forms.Button startButton;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.CheckBox checkBox1;
    }
}

