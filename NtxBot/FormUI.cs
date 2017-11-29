using System.Windows.Forms;

namespace NtxBot
{
    public partial class FormUI : Form
    {
        public FormUI()
        {
            InitializeComponent();
        }

        public void AppendLog(string text)
        {
            // Append the input text to the log
            richTextBoxLog.Text += text;

            // Scroll the rich text box to its end
            richTextBoxLog.SelectionStart = richTextBoxLog.Text.Length;
            richTextBoxLog.ScrollToCaret();
        }
    }
}