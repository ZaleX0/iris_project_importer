using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
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

        bool _isRunning;
        Logger _logger;
        ProgressBarManager _pbm;

        public MainForm()
        {
            InitializeComponent();

            _isRunning = false;
            _logger = new Logger(logTextBox);
            _pbm = new ProgressBarManager(progressBar);

            if (Properties.Settings.Default.host != string.Empty)
            {
                hostTextBox.Text = Properties.Settings.Default.host;
                portTextBox.Text = Properties.Settings.Default.port;
                loginTextBox.Text = Properties.Settings.Default.login;
                passwordTextBox.Text = Properties.Settings.Default.password;
                pathTextBox.Text = Properties.Settings.Default.pathText;
                dbNameComboBox.Items.Add(Properties.Settings.Default.dbName);
                dbNameComboBox.SelectedIndex = 0;
            }
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
                        _logger.Log("Databases realoaded");
                    }));
                }
                catch (Exception ex)
                {
                    _logger.Log(ex.Message);
                    MessageBox.Show(this, ex.Message);
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
            if (folderBrowserDialog.SelectedPath != string.Empty)
            {
                string path = folderBrowserDialog.SelectedPath + "\\";
                pathTextBox.Text = path;
                _logger.Log($"Selected path: {path}");
            }
        }
        private async void startButton_Click(object sender, EventArgs e)
        {
            SaveSettings();

            _isRunning = true;

            // Disabling buttons to prevent spamming
            startButton.Enabled = false;
            reloadDbButton.Enabled = false;

            await Task.Run(() => {
                try
                {
                    #region SQLManager, dbName and connectionString
                    SQLManager sqlManager = new SQLManager(this, _logger);
                    string dbName = (string) Invoke(new Func<string>(() => dbNameComboBox.Text));
                    string connectionString = sqlManager.GetConnectionString(
                        hostTextBox.Text,
                        portTextBox.Text,
                        loginTextBox.Text,
                        passwordTextBox.Text,
                        dbName);
                    #endregion

                    if (dbName != string.Empty)
                    {
                        // check if schema exists, if not then create it
                        bool schema = sqlManager.CheckIfSchemaExists(connectionString);
                        if (!schema)
                        {
                            _logger.Log("Schema iris_project_info does not exists. Creating schema...");
                            sqlManager.CreateSchema(connectionString); // create tables triggers etc.
                            _logger.Log("Schema created");
                        }

                        // Getting PIC_*.xml file paths
                        _logger.Log("Searching for PIC_*.xml files...");
                        XmlFileReader xmlReader = new XmlFileReader();
                        PICFileManager picFileManager = new PICFileManager();

                        string path = pathTextBox.Text;
                        path = path[path.Length - 1] == '\\' ? path : path + '\\'; // if last char is not '\' then add it
                        string[] picFilePaths = picFileManager.GetPICFilePaths(path);

                        _pbm.SetupProgressBar(0, (picFilePaths.Length) * 10, 10);
                        _pbm.SetProgressBarValue(0);

                        // Importing data
                        Stopwatch s = Stopwatch.StartNew();
                        using (var connection = new NpgsqlConnection(connectionString))
                        {
                            connection.Open();
                            for (int i = 0; i < picFilePaths.Length; i++)
                            {
                                string picpath = xmlReader.GetElementContent(picFilePaths[i], "PicPath");

                                // Importing into db and returning the result (insert/skip)
                                switch (sqlManager.InsertPIC(picFilePaths[i], connection)) // here 
                                {
                                    case SQLManager.InsertPICResult.Insert:
                                        _logger.Log($"Inserting ({i + 1}/{picFilePaths.Length}): {picpath}");
                                        break;

                                    case SQLManager.InsertPICResult.Skip:
                                        _logger.Log($"Skip ({i + 1}/{picFilePaths.Length}): {picpath} Data already exists");
                                        break;

                                    default:
                                        break;
                                }
                                _pbm.StepProgressBar();
                            }
                            // it is needed to wait because sometimes connection is still executing last transaction
                            while (connection.FullState.HasFlag(ConnectionState.Executing)) continue;
                            // connection close
                        }
                        s.Stop();
                        _logger.Log($"Success! Elapsed ms: {s.ElapsedMilliseconds}");
                    }
                    else
                    {
                        _logger.Log("No Database selected");
                    }
                }
                catch (Exception ex)
                {
                    _pbm.SetProgressBarValue(0);
                    _logger.Log(ex.StackTrace);
                    _logger.Log(ex.Message);
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    // Enabling buttons
                    BeginInvoke(new Action(() => {
                        _isRunning = false;
                        startButton.Enabled = true;
                        reloadDbButton.Enabled = true;
                    }));
                }
            });
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (_isRunning)
                _logger.Log("WARNING: Importing interrupted by closing off the window.");

            _logger.SaveLogToDir("logs");
        }



        private void SaveSettings()
        {
            if (checkBox1.Checked)
            {
                Properties.Settings.Default.host = hostTextBox.Text;
                Properties.Settings.Default.port = portTextBox.Text;
                Properties.Settings.Default.login = loginTextBox.Text;
                Properties.Settings.Default.password = passwordTextBox.Text;
                Properties.Settings.Default.pathText = pathTextBox.Text;
                Properties.Settings.Default.dbName = dbNameComboBox.Text;
                Properties.Settings.Default.Save();
            }
            else
            {
                Properties.Settings.Default.host = string.Empty;
                Properties.Settings.Default.port = "5432";
                Properties.Settings.Default.login = string.Empty;
                Properties.Settings.Default.password = string.Empty;
                Properties.Settings.Default.pathText = string.Empty;
                Properties.Settings.Default.dbName = string.Empty;
                Properties.Settings.Default.Save();
            }
        }

    }
}
