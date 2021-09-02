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
    partial class MainForm : Form
    {

        Logger _logger;
        ProgressBarManager _pbm;

        public MainForm()
        {
            InitializeComponent();

            _logger = new Logger(logTextBox);
            _pbm = new ProgressBarManager(progressBar);

            // TODO: zapamietac dane logowania
            hostTextBox.Text = "localhost";
            portTextBox.Text = "5432";
            loginTextBox.Text = "postgres";
            passwordTextBox.Text = "zaq12wsx";
            pathTextBox.Text = "C:\\Data_test\\IRISProjectImporter\\dane_test\\zdp_poznan";
            dbNameComboBox.Items.Add("iris_project_importer");
            dbNameComboBox.SelectedIndex = 0;
        }

        private async void reloadDbButton_Click(object sender, EventArgs e)
        {
            _logger.Log("Reloading databases...");

            reloadDbButton.Enabled = false;
            dbNameComboBox.Items.Clear();
            await Task.Run(() => {
                try
                {
                    #region SQLManager and connectionString
                    SQLManager sqlManager = new SQLManager();
                    string connectionString = sqlManager.GetConnectionString(
                        hostTextBox.Text,
                        portTextBox.Text,
                        loginTextBox.Text,
                        passwordTextBox.Text,
                        "postgres");
                    #endregion

                    string[] dbNames = sqlManager.GetDatabaseNames(connectionString);
                    BeginInvoke(new Action(() => {
                        dbNameComboBox.Items.AddRange(dbNames);
                        dbNameComboBox.SelectedIndex = 0;
                        _logger.Log("Databases realoaded.");
                    }));
                }
                catch (Exception ex)
                {
                    //_logger.Log(ex.StackTrace);
                    _logger.Log(ex.Message);
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    BeginInvoke(new Action(() => reloadDbButton.Enabled = true));
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
                string path = folderBrowserDialog.SelectedPath;
                pathTextBox.Text = path;
                _logger.Log($"Selected path: {folderBrowserDialog.SelectedPath}");
            }
        }

        private async void startButton_Click(object sender, EventArgs e)
        {
            // Disabling buttons to prevent spamming
            startButton.Enabled = false;
            reloadDbButton.Enabled = false;

            await Task.Run(() => {
                try
                {
                    #region SQLManager, dbName and connectionString
                    SQLManager sqlManager = new SQLManager(_logger, _pbm);
                    string dbName = (string) Invoke(new Func<string>(() => dbNameComboBox.Text));
                    string connectionString = sqlManager.GetConnectionString(
                        hostTextBox.Text,
                        portTextBox.Text,
                        loginTextBox.Text,
                        passwordTextBox.Text,
                        dbName);
                    #endregion

                    if (!dbName.Equals(""))
                    {
                        XmlFileReader xmlReader = new XmlFileReader();

                        // Getting PIC_*.xml file paths
                        PICFileManager picFileManager = new PICFileManager();
                        IndexFileManager indexFileManager = new IndexFileManager();
                        string[] picFilePaths = picFileManager.GetPICFilePaths(pathTextBox.Text);
                        string[] indexFilePaths = indexFileManager.GetIndexFilePaths(picFilePaths);

                        #region Logger and ProgressBarManager Code
                        _pbm.SetProgressBarValue(0);
                        _pbm.SetProgressBar(0, (indexFilePaths.Length + picFilePaths.Length) * 10, 10);
                        #endregion

                        Console.WriteLine(picFilePaths[0]);
                        Console.WriteLine(indexFilePaths[0]);

                        for (int i = 0; i < indexFilePaths.Length; i++)
                        {
                            string[] picsPerIndexPath = picFileManager.GetPICFilePaths(indexFilePaths[i]);
                            //sqlManager.InsertIndexWithPICs(indexFilePaths[i], picsPerIndexPath, connectionString);
                            sqlManager.test(indexFilePaths[i], connectionString);
                            _pbm.StepProgressBar();
                        }
                    }
                    else
                    {
                        _logger.Log("No Database selected.");
                    }
                }
                catch (Exception ex)
                {
                    //_logger.Log(ex.StackTrace);
                    _logger.Log(ex.Message);
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    // Enabling buttons
                    BeginInvoke(new Action(() => {
                        startButton.Enabled = true;
                        reloadDbButton.Enabled = true;
                    }));
                }
            });
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            // TODO: log info if inserting got cancelled
            _logger.SaveLogToDir("IRISProjectImporter_Logs");
        }

        #region
        private void raportFormButton_Click(object sender, EventArgs e)
        {
            string dbName = dbNameComboBox.Text;
            string connectionString = new SQLManager().GetConnectionString(
                hostTextBox.Text,
                portTextBox.Text,
                loginTextBox.Text,
                passwordTextBox.Text,
                dbName);
            new RaportForm(connectionString, _logger).Show();
        }
        #endregion
    }
}
