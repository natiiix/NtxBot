using System;
using System.Windows.Forms;

namespace NtxBot
{
    public partial class FormUI : Form
    {
        public FormUI()
        {
            InitializeComponent();
        }

        public void AppendLog(string text) => richTextBoxLog.BeginInvoke(new Action(() => AppendLogUnsafe(text)));

        private void AppendLogUnsafe(string text)
        {
            // Add a line between log entries
            if (richTextBoxLog.Text != string.Empty)
            {
                richTextBoxLog.Text += Environment.NewLine;
            }

            DateTime dtNow = DateTime.Now;

            // Append the input text to the log
            richTextBoxLog.Text += string.Format("[{0}.{1:000}] {2}", dtNow.ToLongTimeString(), dtNow.Millisecond, text);

            // Scroll the rich text box to its end
            richTextBoxLog.SelectionStart = richTextBoxLog.Text.Length;
            richTextBoxLog.ScrollToCaret();
        }
    }
}