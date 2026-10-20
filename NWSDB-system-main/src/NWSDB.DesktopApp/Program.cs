using NWSDB.DesktopApp.Services;

namespace NWSDB.DesktopApp;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        var client = new NwsdbServiceClient();

        using var loginForm = new LoginForm(client);
        if (loginForm.ShowDialog() == DialogResult.OK)
        {
            Application.Run(new MainForm(client));
        }
    }
}