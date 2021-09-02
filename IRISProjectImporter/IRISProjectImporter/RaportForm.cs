using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Npgsql;

namespace IRISProjectImporter
{
    partial class RaportForm : Form
    {
        private string connectionString;
        private Logger _logger;

        public RaportForm(string connectionString, Logger _logger)
        {
            InitializeComponent();
            this.connectionString = connectionString;
            this._logger = _logger;
        }

        private void RaportForm_Load(object sender, EventArgs e)
        {
            try
            {
                new SQLManager().LoadDataGridViewAsync(dataGridView, connectionString);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async void refreshButton_Click(object sender, EventArgs e)
        {
            try
            {
                refreshButton.Enabled = false;
                await new SQLManager().LoadDataGridViewAsync(dataGridView, connectionString);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                refreshButton.Enabled = true;
            }
        }
    }
}
