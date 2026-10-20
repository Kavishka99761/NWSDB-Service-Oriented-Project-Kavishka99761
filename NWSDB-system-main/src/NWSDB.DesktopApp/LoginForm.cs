using System.Drawing; //import the System.Drawing namespace for working with colors and fonts
using System.Windows.Forms; //import the System.Windows.Forms namespace for creating Windows Forms applications
using NWSDB.DesktopApp.Services; //import the NWSDB.DesktopApp.Services namespace for accessing the NwsdbServiceClient class

namespace NWSDB.DesktopApp; //define the namespace for the desktop application

public class LoginForm : Form //define the LoginForm class that inherits from Form
{
    private readonly NwsdbServiceClient _client; //declare a private readonly field for the NwsdbServiceClient instance
    private TextBox txtUsername = null!;//declare a TextBox control for the username input
    private TextBox txtPassword = null!;//declare a TextBox control for the password input
    private Button btnLogin = null!;//declare a Button control for the login action
    private Label lblError = null!;//declare a Label control for displaying error messages
    private Label lblApiStatus = null!;//declare a Label control for displaying API status

    public LoginForm(NwsdbServiceClient client) //define the constructor that takes a NwsdbServiceClient instance as a parameter
    {
        _client = client; //assign the client parameter to the private field
        InitializeComponents(); //call the method to initialize the form components
    }

    private void InitializeComponents() //define a private method to initialize the form components
    {
        Text = "NWSDB Admin Portal - Secure Access";
        Size = new Size(460, 480);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Color.FromArgb(245, 248, 252);
        Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);

        // Header Panel
        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 110,
            BackColor = Color.FromArgb(0, 34, 68)
        };

        var lblTitle = new Label
        {
            Text = "NWSDB SRI LANKA",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 16f, FontStyle.Bold),
            Location = new Point(20, 20),
            AutoSize = true
        };

        var lblSub = new Label
        {
            Text = "Operations Management & SOC Desktop Console",
            ForeColor = Color.FromArgb(144, 224, 239),
            Font = new Font("Segoe UI", 9f, FontStyle.Regular),
            Location = new Point(22, 58),
            AutoSize = true
        };

        header.Controls.Add(lblTitle);
        header.Controls.Add(lblSub);
        Controls.Add(header);

        // Main Login Container
        int top = 140;

        lblApiStatus = new Label
        {
            Text = "Checking service connectivity...",
            Location = new Point(40, top),
            Size = new Size(360, 20),
            ForeColor = Color.DimGray,
            Font = new Font("Segoe UI", 8.5f, FontStyle.Italic)
        };
        Controls.Add(lblApiStatus);
        top += 30;

        var lblUser = new Label { Text = "Username:", Location = new Point(40, top), AutoSize = true, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
        Controls.Add(lblUser);
        top += 25;

        txtUsername = new TextBox
        {
            Location = new Point(40, top),
            Size = new Size(360, 30),
            Text = "admin"
        };
        Controls.Add(txtUsername);
        top += 40;

        var lblPass = new Label { Text = "Password:", Location = new Point(40, top), AutoSize = true, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
        Controls.Add(lblPass);
        top += 25;

        txtPassword = new TextBox
        {
            Location = new Point(40, top),
            Size = new Size(360, 30),
            PasswordChar = '●',
            Text = "admin123"
        };
        Controls.Add(txtPassword);
        top += 45;

        lblError = new Label
        {
            Text = "",
            ForeColor = Color.Crimson,
            Location = new Point(40, top),
            Size = new Size(360, 25),
            Font = new Font("Segoe UI", 9f, FontStyle.Bold)
        };
        Controls.Add(lblError);
        top += 30;

        btnLogin = new Button
        {
            Text = "LOGIN TO CONSOLE",
            Location = new Point(40, top),
            Size = new Size(360, 42),
            BackColor = Color.FromArgb(0, 119, 182),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnLogin.FlatAppearance.BorderSize = 0;
        btnLogin.Click += async (s, e) => await HandleLoginAsync();
        Controls.Add(btnLogin);

        AcceptButton = btnLogin;

        Shown += async (s, e) =>
        {
            bool online = await _client.CheckApiConnectionAsync();
            lblApiStatus.Text = online 
                ? "● Web API Connected (http://localhost:5000)" 
                : "● Running in Standalone / Service Direct Mode";
            lblApiStatus.ForeColor = online ? Color.ForestGreen : Color.FromArgb(0, 102, 204);
        };
    }

    private async Task HandleLoginAsync()
    {
        lblError.Text = "";
        btnLogin.Enabled = false;
        btnLogin.Text = "Authenticating...";

        try
        {
            var res = await _client.LoginAsync(txtUsername.Text, txtPassword.Text);
            if (res.Success && res.Data != null)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                lblError.Text = res.Message;
            }
        }
        catch (Exception ex)
        {
            lblError.Text = "Login error: " + ex.Message;
        }
        finally
        {
            btnLogin.Enabled = true;
            btnLogin.Text = "LOGIN TO CONSOLE";
        }
    }
}
