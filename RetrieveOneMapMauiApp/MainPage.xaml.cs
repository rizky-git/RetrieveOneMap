using RetrieveOneMap;

namespace RetrieveOneMapMauiApp
{
    public partial class MainPage : ContentPage
    {
        string selectedAuthMode = "Token";

        public MainPage()
        {
            InitializeComponent();
            UpdateAuthVisibility();
        }

        private void OnAuthModeChanged(object sender, CheckedChangedEventArgs e)
        {
            if (sender is RadioButton rb && rb.IsChecked)
            {
                selectedAuthMode = rb.Value?.ToString();
                UpdateAuthVisibility();
            }
        }

        private void UpdateAuthVisibility()
        {
            bool useToken = selectedAuthMode == "Token";

            TokenEditor.IsEnabled = useToken;
            EmailEntry.IsEnabled = !useToken;
            PasswordEntry.IsEnabled = !useToken;

            if (useToken)
            {
                // Clear email and password when switching to token
                EmailEntry.Text = string.Empty;
                PasswordEntry.Text = string.Empty;
            }
            else
            {
                // Clear token when switching to credentials
                TokenEditor.Text = string.Empty;
            }
        }

        private void OnPostalCodeChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is Entry entry)
            {
                // Keep only digits and limit to 6
                entry.Text = new string(entry.Text
                    .Where(char.IsDigit)
                    .Take(6)
                    .ToArray());
            }
        }

        private bool _isPasswordVisible = false;

        private void OnTogglePasswordClicked(object sender, EventArgs e)
        {
            _isPasswordVisible = !_isPasswordVisible;
            PasswordEntry.IsPassword = !_isPasswordVisible;
            TogglePasswordButton.Source = _isPasswordVisible ? "eye_closed.png" : "eye.png";
        }

        private async void OnExtractClicked(object sender, EventArgs e)
        {
            string token = TokenEditor.Text?.Trim();
            string email = EmailEntry.Text?.Trim();
            string password = PasswordEntry.Text?.Trim();
            bool useToken = selectedAuthMode == "Token";

            if (useToken && string.IsNullOrWhiteSpace(token))
            {
                await DisplayAlert("Error", "Token is required.", "OK");
                return;
            }

            if (!useToken && (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password)))
            {
                await DisplayAlert("Error", "Email and password are required.", "OK");
                return;
            }

            if (!int.TryParse(StartPostalEntry.Text?.Trim(), out int start) ||
                !int.TryParse(EndPostalEntry.Text?.Trim(), out int end))
            {
                await DisplayAlert("Error", "Postal codes must be valid numbers.", "OK");
                return;
            }

            StatusLabel.Text = "Running extraction...";
            try
            {
                await OneMapHelper.RunExtractionAsync(
                    token,
                    email,
                    password,
                    start,
                    end,
                    reportStatus: msg => MainThread.BeginInvokeOnMainThread(() => StatusLabel.Text = msg),
                    reportError: err => MainThread.BeginInvokeOnMainThread(() => StatusLabel.Text = "❌ " + err)
                );

                await DisplayAlert("Success", "Extraction completed!", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                StatusLabel.Text += "\nDone.";
            }
        }
    }
}
