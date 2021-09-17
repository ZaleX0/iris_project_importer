using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IRISProjectImporter
{
    class ProgressBarManager
    {
        readonly ProgressBar progressBar;

        public ProgressBarManager(ProgressBar progressBar)
        {
            this.progressBar = progressBar;
        }


        public void StepProgressBar()
        {
            if (progressBar.InvokeRequired)
                progressBar.BeginInvoke(new Action(() => progressBar.PerformStep()));
            else
                progressBar.PerformStep();
        }

        public void SetupProgressBar(int min, int max, int step)
        {
            if (progressBar.InvokeRequired)
            {
                progressBar.BeginInvoke(new Action(() =>
                {
                    progressBar.Minimum = min;
                    progressBar.Maximum = max;
                    progressBar.Step = step;
                }));
            }
            else
            {
                progressBar.Minimum = min;
                progressBar.Maximum = max;
                progressBar.Step = step;
            }
        }

        public void SetProgressBarValue(int val)
        {
            if (progressBar.InvokeRequired)
                progressBar.BeginInvoke(new Action(() => progressBar.Value = val));
            else
                progressBar.Value = val;
        }
    }
}
