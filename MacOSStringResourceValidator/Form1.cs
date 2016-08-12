using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MacOSStringResourceValidator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            this.backgroundWorker1.DoWork += backgroundWorker1_DoWork;
            this.backgroundWorker1.RunWorkerCompleted += backgroundWorker1_RunWorkerCompleted;
        }

        void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.progressBar1.Visible = false;

            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (e.Cancelled)
            {
                MessageBox.Show("Validation be cancelled.", "Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
            else if (null != e.Result && !String.IsNullOrEmpty(e.Result.ToString()))
            {
                this.resultTextBox.Text = e.Result.ToString();
            }
            else
            {
                this.resultTextBox.Text = String.Empty;
                MessageBox.Show("Validation passed!", "Passed", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            //e.Result = ValidateStringsByRubuStrings(e);
            e.Result = ValidateStringsByNative(e);
        }

        private static string ValidateStringsByNative(DoWorkEventArgs e)
        {
            StringsFile file = new StringsFile(e.Argument.ToString());
            file.Validate();
            return file.ValidationMessage;
        }

        private static string ValidateStringsByRubuStrings(DoWorkEventArgs e)
        {
            var basePath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            var cmd = "cmd.exe";
            var args = "/c ruby " + basePath + "\\Scripts\\Rubustrings-master\\rubustrings " + e.Argument;
            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo(cmd, args);
            psi.WorkingDirectory = basePath;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;

            System.Diagnostics.Process process = System.Diagnostics.Process.Start(psi);
            System.IO.StreamReader outputReader = process.StandardOutput;
            var errorReader = process.StandardError;

            process.WaitForExit();

            StringBuilder builder = new StringBuilder();
            if (process.HasExited)
            {
                builder.Append(outputReader.ReadToEnd());
                builder.Append(errorReader.ReadToEnd());
            }
            outputReader.Close();
            errorReader.Close();

            return builder.ToString();
        }

        private void browseBtn_Click(object sender, EventArgs e)
        {
            this.openFileDialog1.FileName = this.filePathTextBox.Text;
            var result = this.openFileDialog1.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                this.filePathTextBox.Text = this.openFileDialog1.FileName;
                validationBtn_Click(this, null);
            }

        }

        private void validationBtn_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(this.filePathTextBox.Text))
            {
                browseBtn_Click(sender, e);
            }
            this.progressBar1.Visible = true;
            this.backgroundWorker1.RunWorkerAsync(this.filePathTextBox.Text);
            
        }

        private void aboutBtn_Click(object sender, EventArgs e)
        {
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            var result = MessageBox.Show(version + Environment.NewLine 
                    + "Validator for .strings files(iOS string resource file), running on Windows, .NET 4.5" + Environment.NewLine 
                    + "Visite it on github now?", 
                "About", MessageBoxButtons.YesNo, 
                MessageBoxIcon.Information);
            if (result == System.Windows.Forms.DialogResult.Yes)
            {
                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = "https://github.com/fishtrees/MacOSStringResourceValidator";
                info.Verb = "open";

                Process.Start(info);
            }
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Link;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string filePath = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            this.filePathTextBox.Text = filePath;
            validationBtn_Click(this, null);
        }

    }
}
