using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;

namespace IRISProjectImporter
{
    class Logger
    {
        RichTextBox richTextBox;

        public Logger(RichTextBox richTextBox)
        {
            this.richTextBox = richTextBox;
        }

        public void Log(string message)
        {
            //string log = $"{DateTime.Now:[HH:mm:ss]} {message}\n";
            string log = $"[{DateTime.Now}] {message}\n";

            if (richTextBox.InvokeRequired)
                richTextBox.BeginInvoke(new Action(() => richTextBox.AppendText(log)));
            else
                richTextBox.AppendText(log);
        }

        public void SaveToFile(string path)
        {
            // TODO: save log to file

        }
    }
}
