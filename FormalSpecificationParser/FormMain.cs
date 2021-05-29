using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.IO;
using System.Text.RegularExpressions;

namespace FormalSpecificationParser
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(txtInput.Text))
            {
                string input = txtInput.Text.Trim();
                string output = null;
                Function function = new Function(input);
                List<Argument> args = function.getArgs();

                output = ParseFormalSpec(function, args);
                txtOutput.Text = output;
            }
            else
                MessageBox.Show("You must enter a specification to analyze!");
        }

        private string ParseFormalSpec(Function func, List<Argument> args)
        {
            string output = "#include <iostream>\nusing namespace std;\n\n";
            output += ParseString.createInputFunction(args);
            output += ParseString.ParsePre(func.getPre(), func.getName(), args);
            output += ParseString.ParsePost(func.getPost(), func.getName(), args);

            output += "int main() {\n";
            foreach (Argument arg in args)
            {
                output += "\t" + arg.ToString();
                switch (arg.getDatatype())
                {
                    case "bool":
                        output += " = true;\n";
                        break;
                    case "int":
                    case "float":
                        output += " = 0;\n";
                        break;
                    case "string":
                        output += ";\n";
                        break;
                }
            }

            output += "\n\tdo {\n\t\tInput(" + ParseString.ArgListWithoutDatatype(args) + ";\n";
            output += "\t\tif (!Check_" + func.getName() + "(" + ParseString.ArgListWithoutDatatype(args) + ")\n";
            output += "\t\t\tcout << \"One or more parameters have an invalid input. Please retry.\\n\";\n";
            output += "\t} while (!Check_" + func.getName() + "(" + ParseString.ArgListWithoutDatatype(args) + ");\n\n";

            output += "\t" + args[args.Count() - 1].getName() + " = " + func.getName() + "("
                + ParseString.ArgListWithoutDatatype(args) + ";\n";
            output += "\tcout << \"Result: \" << " + args[args.Count() - 1].getName() + ";\n";
            output += "\treturn 0;\n}";

            return output;
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            txtInput.Clear();
            txtOutput.Clear();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Text files (*.txt)|*.txt";
            ofd.RestoreDirectory = true;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string filePath = ofd.FileName;

                var fileStream = ofd.OpenFile();

                using (StreamReader reader = new StreamReader(fileStream))
                {
                    txtInput.Text = reader.ReadToEnd();
                    reader.Close();
                }
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.DefaultExt = "cpp";
            sfd.Filter = "txt files (*.txt)|*.txt|C++ File|*.cpp";
            sfd.FilterIndex = 2;
            sfd.FileName = txtOutputName.Text;

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(sfd.FileName, txtOutput.Text);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }

        private void txtInput_TextChanged(object sender, EventArgs e)
        {
            string typenames = @"\b(N|Z|R|B|char\\*)\b";
            MatchCollection typenameMatches = Regex.Matches(txtInput.Text, typenames);

            string sections = @"\b(pre|post)\b";
            MatchCollection sectionMatches = Regex.Matches(txtInput.Text, sections);

            string operators = @"(&&|\|\|)";
            MatchCollection operatorMatches = Regex.Matches(txtInput.Text, operators);

            int originalIndex = txtInput.SelectionStart;
            int originalLength = txtInput.SelectionLength;
            Color originalColor = Color.Black;
            Font originalFont = txtInput.Font;

            lblOutputName.Focus();

            txtInput.SelectionStart = 0;
            txtInput.SelectionLength = txtInput.Text.Length;
            txtInput.SelectionColor = originalColor;

            foreach (Match m in typenameMatches)
            {
                txtInput.SelectionStart = m.Index;
                txtInput.SelectionLength = m.Length;
                txtInput.SelectionColor = Color.Red;
            }

            foreach (Match m in sectionMatches)
            {
                txtInput.SelectionStart = m.Index;
                txtInput.SelectionLength = m.Length;
                txtInput.SelectionColor = Color.Blue;
            }

            foreach (Match m in operatorMatches)
            {
                txtInput.SelectionStart = m.Index;
                txtInput.SelectionLength = m.Length;
                txtInput.SelectionColor = Color.Brown;
                txtInput.SelectionFont = new Font(txtInput.Font, FontStyle.Bold);
            }

            txtInput.SelectionStart = originalIndex;
            txtInput.SelectionLength = originalLength;
            txtInput.SelectionColor = originalColor;
            txtInput.SelectionFont = originalFont;

            txtInput.Focus();
        }

        private void txtInput_onKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.V)
            {
                txtInput.Text += (string)Clipboard.GetData("Text");
                e.Handled = true;
            }
        }

        private void txtOutput_onTextChanged(object sender, EventArgs e)
        {
            string keywords = @"\b(namespace|using|void|int|bool|float|string|true|false)\b";
            MatchCollection keywordMatches = Regex.Matches(txtOutput.Text, keywords);

            string include = @"\b(iostream)\b";
            MatchCollection includeMatches = Regex.Matches(txtOutput.Text, include);

            string controls = @"\b(if|else|while|do|return)\b";
            MatchCollection controlMatches = Regex.Matches(txtOutput.Text, controls);

            string strings = "\".+?\"";
            MatchCollection stringMatches = Regex.Matches(txtOutput.Text, strings);

            int originalIndex = txtOutput.SelectionStart;
            int originalLength = txtOutput.SelectionLength;
            Color originalColor = Color.Black;

            lblOutputName.Focus();

            txtOutput.SelectionStart = 0;
            txtOutput.SelectionLength = txtOutput.Text.Length;
            txtOutput.SelectionColor = originalColor;

            foreach (Match m in keywordMatches)
            {
                txtOutput.SelectionStart = m.Index;
                txtOutput.SelectionLength = m.Length;
                txtOutput.SelectionColor = Color.Blue;
                txtOutput.SelectionFont = new Font(txtOutput.Font, FontStyle.Bold);
            }

            foreach (Match m in includeMatches)
            {
                txtOutput.SelectionStart = m.Index;
                txtOutput.SelectionLength = m.Length;
                txtOutput.SelectionColor = Color.Brown;
            }

            foreach (Match m in controlMatches)
            {
                txtOutput.SelectionStart = m.Index;
                txtOutput.SelectionLength = m.Length;
                txtOutput.SelectionColor = Color.Purple;
            }

            foreach (Match m in stringMatches)
            {
                txtOutput.SelectionStart = m.Index;
                txtOutput.SelectionLength = m.Length;
                txtOutput.SelectionColor = Color.Brown;
            }


            txtOutput.SelectionStart = originalIndex;
            txtOutput.SelectionLength = originalLength;
            txtOutput.SelectionColor = originalColor;

            txtInput.Focus();
        }
    }
}
