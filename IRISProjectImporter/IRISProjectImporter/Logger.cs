using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

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
            //string log = $"[{DateTime.Now}] {message}\n";
            string log = $"{DateTime.Now:[HH:mm:ss]} {message}\n";
            
            Console.Write(log);

            if (richTextBox.InvokeRequired)
                richTextBox.BeginInvoke(new Action(() => richTextBox.AppendText(log)));
            else
                richTextBox.AppendText(log);
        }


        public void SaveLogToDir(string dirPath)
        {
            if (richTextBox.TextLength > 0)
            {
                if (!Directory.Exists(dirPath))
                    Directory.CreateDirectory(dirPath);

                string filename = $"{DateTime.Now:dd-MM-yyyy_HH-mm}.txt";
                richTextBox.SaveFile($"{dirPath}\\{filename}", RichTextBoxStreamType.PlainText);
            }
        }
    }
}
