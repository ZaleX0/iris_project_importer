using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IRISProjectImporter
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            

            // TODO: DELETE THIS
            pathTextBox.Text = "C:\\Data_test\\IRISProjectImporter\\dane_test\\zdp_poznan";

        }

        private async void reloadDbButton_Click(object sender, EventArgs e)
        {
            await Task.Run(() =>
            {
                try
                {
                    string connection = SQLManager.GetConnectionString(
                        hostTextBox.Text,
                        portTextBox.Text,
                        loginTextBox.Text,
                        passwordTextBox.Text,
                        "postgres");
                    BeginInvoke(new Action(() =>
                    {
                        dbNameComboBox.Items.Clear();
                        dbNameComboBox.Items.AddRange(SQLManager.GetDatabaseNames(connection));
                    }));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });
        }

        private void folderBrowserDialogButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.ShowNewFolderButton = false;
            folderBrowserDialog.ShowDialog();
            if (folderBrowserDialog.SelectedPath != "")
            {
                pathTextBox.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private async void startButton_Click(object sender, EventArgs e)
        {
            startButton.Enabled = false;
            await Task.Run(() =>
            {
                try
                {
                    string[] picFilePaths = PICFileManager.GetPICFilePaths(pathTextBox.Text);
                    if (picFilePaths.Length > 0)
                    {
                        for (int i = 0; i < picFilePaths.Length; i++)
                        {
                            PICFileInfo pic = new PICFileInfo(picFilePaths[i]);
                            Console.WriteLine(pic.IndexXmlFilePath);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    BeginInvoke(new Action(() => startButton.Enabled = true));
                }
            });
        }
        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

    }
}
