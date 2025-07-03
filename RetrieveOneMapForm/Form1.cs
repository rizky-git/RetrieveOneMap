using RetrieveOneMap;

namespace RetrieveOneMapForm
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            string accessToken = rtbToken.Text.Trim();
            string email = tbEmail.Text.Trim();
            string password = tbPassword.Text.Trim();
            int startPostalCode = int.TryParse(tbStart.Text, out int start) ? start : 0;
            int endPostalCode = int.TryParse(tbTo.Text, out int end) ? end : 999999;

            try
            {
                if (email == "" && password == "" && accessToken == "")
                {
                    throw new ArgumentException("Invalid login credentials");
                }

                btnRun.Enabled = false;
                lbStatus.Text = "Starting extraction...";
                Cursor = Cursors.WaitCursor;

                await OneMapHelper.RunExtractionAsync(
                    accessToken,
                    email,
                    password,
                    startPostalCode,
                    endPostalCode,
                    reportStatus: msg => UpdateStatus(msg),
                    reportError: err => ShowErrorOnForm(err)
                );

                //MessageBox.Show("Extraction completed!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                CenteredMessageBox.CenteredMessageBox.Show(this, "Extraction completed!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                //MessageBox.Show($"Failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                CenteredMessageBox.CenteredMessageBox.Show(this, $"Failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnRun.Enabled = true;
                Cursor = Cursors.Default;
            }
        }

        private void UpdateStatus(string message)
        {
            if (lbStatus.InvokeRequired)
            {
                lbStatus.Invoke(() => lbStatus.Text = message);
            }
            else
            {
                lbStatus.Text = message;
            }
        }
        private void ShowErrorOnForm(string message)
        {
            if (lbStatus.InvokeRequired)
            {
                lbStatus.Invoke(() => lbStatus.Text = "❌ " + message);
            }
            else
            {
                lbStatus.Text = "❌ " + message;
            }
        }
    }
}
