using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using NWSDB.Data;
using NWSDB.DesktopApp;
using NWSDB.DesktopApp.Services;
using NWSDB.Services;
using NWSDB.Services.Interfaces;

namespace NWSDB.ScreenshotGenerator;

static class Program
{
    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [STAThread]
    static async Task Main()
    {
        ApplicationConfiguration.Initialize();

        string outputDir = @"d:\My Project\NWSDB system\screenshots";
        Directory.CreateDirectory(outputDir);
        Console.WriteLine($"Target screenshots folder: {outputDir}");

        var client = new NwsdbServiceClient();

        // 1. Capture Login Form
        try
        {
            using var loginForm = new LoginForm(client);
            loginForm.StartPosition = FormStartPosition.CenterScreen;
            loginForm.Show();
            SetForegroundWindow(loginForm.Handle);
            Application.DoEvents();
            await Task.Delay(400);
            Application.DoEvents();

            CaptureWindow(loginForm, Path.Combine(outputDir, "01_Desktop_Login.png"));
            Console.WriteLine("Captured 01_Desktop_Login.png");
            loginForm.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error capturing login form: {ex.Message}");
        }

        // 2. Capture MainForm Tabs
        try
        {
            using var mainForm = new MainForm(client);
            mainForm.StartPosition = FormStartPosition.CenterScreen;
            mainForm.Show();
            SetForegroundWindow(mainForm.Handle);
            Application.DoEvents();

            await mainForm.LoadAllDataAsync();
            await Task.Delay(600);
            Application.DoEvents();

            string[] tabNames = new[]
            {
                "02_Desktop_Tab1_Dashboard_Monitor.png",
                "03_Desktop_Tab2_Payments_ClosedXML.png",
                "04_Desktop_Tab3_Customer_Accounts.png",
                "05_Desktop_Tab4_Water_Usage.png",
                "06_Desktop_Tab5_Bills_Management.png",
                "07_Desktop_Tab6_Banking_Partners.png"
            };

            for (int i = 0; i < mainForm.MainTabControl.TabCount && i < tabNames.Length; i++)
            {
                mainForm.MainTabControl.SelectedIndex = i;
                Application.DoEvents();
                await Task.Delay(400);
                Application.DoEvents();

                CaptureWindow(mainForm, Path.Combine(outputDir, tabNames[i]));
                Console.WriteLine($"Captured {tabNames[i]}");
            }

            mainForm.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error capturing main form tabs: {ex.Message}");
        }

        // 3. Generate ClosedXML Excel Payment Report
        try
        {
            var services = new ServiceCollection();
            services.AddNwsdbServices("Data Source=nwsdb_system.db");
            using var sp = services.BuildServiceProvider();
            var reportService = sp.GetRequiredService<IReportService>();

            var excelBytes = await reportService.GeneratePaymentReportExcelAsync();
            string excelPath = Path.Combine(outputDir, "08_NWSDB_Payments_ClosedXML_Export.xlsx");
            await File.WriteAllBytesAsync(excelPath, excelBytes);
            Console.WriteLine($"Exported ClosedXML Excel report: {excelPath} ({excelBytes.Length} bytes)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Excel generation note: {ex.Message}");
        }

        Console.WriteLine("Desktop and Excel captures completed successfully!");
    }

    private static void CaptureWindow(Form form, string filePath)
    {
        form.BringToFront();
        Application.DoEvents();

        int width = form.Width;
        int height = form.Height;

        try
        {
            using var bmp = new Bitmap(width, height);
            using (var g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(form.Location.X, form.Location.Y, 0, 0, new Size(width, height));
            }
            bmp.Save(filePath, ImageFormat.Png);
        }
        catch
        {
            using var bmp = new Bitmap(width, height);
            form.DrawToBitmap(bmp, new Rectangle(0, 0, width, height));
            bmp.Save(filePath, ImageFormat.Png);
        }
    }
}
