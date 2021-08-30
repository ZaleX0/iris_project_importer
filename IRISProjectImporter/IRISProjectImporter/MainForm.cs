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
                    SQLManager sqlManager = new SQLManager();
                    string connection = sqlManager.GetConnectionString(
                        hostTextBox.Text,
                        portTextBox.Text,
                        loginTextBox.Text,
                        passwordTextBox.Text,
                        "postgres");
                    BeginInvoke(new Action(() =>
                    {
                        dbNameComboBox.Items.Clear();
                        dbNameComboBox.Items.AddRange(sqlManager.GetDatabaseNames(connection));
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
            // Disabling button to prevent spamming
            startButton.Enabled = false;

            await Task.Run(() =>
            {
                try
                {
                    XmlFileReader xmlReader = new XmlFileReader();

                    // Getting PIC_*.xml file paths
                    PICFileManager picFileManager = new PICFileManager();
                    IndexFileManager indexFileManager = new IndexFileManager();
                    string[] picFilePaths = picFileManager.GetPICFilePaths(pathTextBox.Text);
                    string[] indexFilePaths = indexFileManager.GetIndexFilePaths(picFilePaths);

                    for (int i = 0; i < indexFilePaths.Length; i++)
                    {
                        #region SQLManager and connectionString
                        SQLManager sqlManager = new SQLManager();
                        string connectionString = sqlManager.GetConnectionString(
                            hostTextBox.Text,
                            portTextBox.Text,
                            loginTextBox.Text,
                            passwordTextBox.Text,
                            "iris_project_importer");
                        #endregion

                        string[] picsPerIndexPath = picFileManager.GetPICFilePaths(indexFilePaths[i]);

                        sqlManager.InsertIndexWithPICs(indexFilePaths[i], picsPerIndexPath, connectionString);
                    }

                    


                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    // Enabling button
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
