using RetrieveOneMap;

namespace RetrieveOneMapForm
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            rbToken.Checked = true;
            rbCredential.Checked = false;

            rtbToken.Enabled = true;
            tbEmail.Enabled = false;
            tbPassword.Enabled = false;
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            string accessToken = rtbToken.Text.Trim();
            string email = tbEmail.Text.Trim();
            string password = tbPassword.Text.Trim();

            try
            {
                if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(password) && string.IsNullOrWhiteSpace(accessToken))
                {
                    throw new ArgumentException("Invalid login credentials");
                }

                if ((!string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(password)) && string.IsNullOrWhiteSpace(accessToken))
                {
                    throw new ArgumentException("Invalid login credentials");
                }

                if (!int.TryParse(tbStart.Text.Trim(), out int startPostalCode))
                {
                    throw new ArgumentException("Start Postal Code must be a valid number");
                }

                if (!int.TryParse(tbTo.Text.Trim(), out int endPostalCode))
                {
                    throw new ArgumentException("End Postal Code must be a valid number");
                }

                // Disable controls
                SetControlsEnabled(false);
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

                CenteredMessageBox.CenteredMessageBox.Show(this, "Extraction completed!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                CenteredMessageBox.CenteredMessageBox.Show(this, $"Failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Re-enable controls
                SetControlsEnabled(true);
                Cursor = Cursors.Default;
            }
        }

        private void SetControlsEnabled(bool enabled)
        {
            rtbToken.Enabled = enabled;
            tbEmail.Enabled = enabled;
            tbPassword.Enabled = enabled;
            tbStart.Enabled = enabled;
            tbTo.Enabled = enabled;
            btnRun.Enabled = enabled;
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

        private void rbToken_CheckedChanged(object sender, EventArgs e)
        {
            rtbToken.Enabled = true;
            tbEmail.Enabled = false;
            tbPassword.Enabled = false;
        }

        private void rbCredential_CheckedChanged(object sender, EventArgs e)
        {
            rtbToken.Enabled = false;
            tbEmail.Enabled = true;
            tbPassword.Enabled = true;
        }
    }
}
