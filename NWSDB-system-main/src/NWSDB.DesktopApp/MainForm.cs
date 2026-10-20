using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using NWSDB.Core.DTOs;
using NWSDB.Core.Entities;
using NWSDB.Core.Enums;
using NWSDB.DesktopApp.Services;

namespace NWSDB.DesktopApp;

public class MainForm : Form
{
    private readonly NwsdbServiceClient _client;

    // Controls
    private Label lblTotalCustomers = null!;
    private Label lblTotalRevenue = null!;
    private Label lblPendingBills = null!;
    private Label lblTodayPayments = null!;
    private Label lblApiStatus = null!;

    private DataGridView dgvAudits = null!;
    private DataGridView dgvPayments = null!;
    private DataGridView dgvCustomers = null!;
    private DataGridView dgvUsages = null!;
    private DataGridView dgvBills = null!;
    private DataGridView dgvPartners = null!;

    // Payment Form Controls
    private TextBox txtPayAccount = null!;
    private TextBox txtPayAmount = null!;
    private ComboBox cmbPayMethod = null!;
    private TextBox txtPayNotes = null!;

    // Water Usage Form Controls
    private TextBox txtUsageAccount = null!;
    private TextBox txtUsageMonth = null!;
    private NumericUpDown numUsagePrev = null!;
    private NumericUpDown numUsageCurr = null!;
    private TextBox txtUsageOfficer = null!;

    public TabControl MainTabControl { get; private set; } = null!;

    public MainForm(NwsdbServiceClient client)
    {
        _client = client;
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        Text = "NWSDB Sri Lanka - Central Administrative & Billing Console (SOC)";
        Size = new Size(1100, 750);
        MinimumSize = new Size(950, 650);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 9f, FontStyle.Regular);
        BackColor = Color.FromArgb(240, 244, 248);

        // Top Header
        var headerPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 70,
            BackColor = Color.FromArgb(0, 34, 68),
            Padding = new Padding(20, 10, 20, 10)
        };

        var lblAppTitle = new Label
        {
            Text = "NATIONAL WATER SUPPLY AND DRAINAGE BOARD",
            Font = new Font("Segoe UI", 13f, FontStyle.Bold),
            ForeColor = Color.White,
            Location = new Point(20, 12),
            AutoSize = true
        };

        var lblAppSubtitle = new Label
        {
            Text = "Service-Oriented Management System | Administrator Console",
            Font = new Font("Segoe UI", 9f),
            ForeColor = Color.FromArgb(144, 224, 239),
            Location = new Point(22, 40),
            AutoSize = true
        };

        lblApiStatus = new Label
        {
            Text = "Connecting...",
            ForeColor = Color.Gold,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Location = new Point(780, 25),
            Size = new Size(280, 25),
            TextAlign = ContentAlignment.MiddleRight
        };

        headerPanel.Controls.Add(lblAppTitle);
        headerPanel.Controls.Add(lblAppSubtitle);
        headerPanel.Controls.Add(lblApiStatus);

        // Main Tab Control
        MainTabControl = new TabControl
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10f, FontStyle.Bold),
            Padding = new Point(14, 8)
        };

        MainTabControl.TabPages.Add(CreateDashboardTab());
        MainTabControl.TabPages.Add(CreatePaymentsTab());
        MainTabControl.TabPages.Add(CreateCustomersTab());
        MainTabControl.TabPages.Add(CreateUsageTab());
        MainTabControl.TabPages.Add(CreateBillsTab());
        MainTabControl.TabPages.Add(CreatePartnersTab());

        Controls.Add(MainTabControl);
        Controls.Add(headerPanel);
        headerPanel.SendToBack();

        Shown += async (s, e) => await LoadAllDataAsync();
    }

    private static void StyleGrid(DataGridView dgv)
    {
        dgv.Dock = DockStyle.Fill;
        dgv.ReadOnly = true;
        dgv.AllowUserToAddRows = false;
        dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgv.BackgroundColor = Color.White;
        dgv.BorderStyle = BorderStyle.None;
        dgv.EnableHeadersVisualStyles = false;
        dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 51, 102);
        dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
        dgv.ColumnHeadersHeight = 34;
        dgv.RowTemplate.Height = 28;
        dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 253);
    }

    private TabPage CreateDashboardTab()
    {
        var tab = new TabPage("📊 Dashboard & Monitor");
        tab.BackColor = Color.White;
        tab.Padding = new Padding(15);

        var pnlKpis = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 115,
            WrapContents = false,
            AutoScroll = true
        };

        pnlKpis.Controls.Add(CreateKpiCard("TOTAL CUSTOMERS", out lblTotalCustomers, Color.FromArgb(0, 119, 182)));
        pnlKpis.Controls.Add(CreateKpiCard("TOTAL REVENUE", out lblTotalRevenue, Color.FromArgb(46, 125, 50)));
        pnlKpis.Controls.Add(CreateKpiCard("PENDING BILLS", out lblPendingBills, Color.FromArgb(198, 40, 40)));
        pnlKpis.Controls.Add(CreateKpiCard("TODAY'S PAYMENTS", out lblTodayPayments, Color.FromArgb(230, 81, 0)));

        // Audit Monitor Section
        var pnlMonitor = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 10, 0, 0) };

        var pnlAuditHeader = new Panel { Dock = DockStyle.Top, Height = 40 };
        var lblAuditTitle = new Label
        {
            Text = "Live Transaction & System Audit Monitor",
            Font = new Font("Segoe UI", 11f, FontStyle.Bold),
            ForeColor = Color.FromArgb(0, 34, 68),
            Location = new Point(0, 6),
            AutoSize = true
        };

        var btnRefreshAudits = new Button
        {
            Text = "Refresh Feed",
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Location = new Point(880, 4),
            Size = new Size(130, 30),
            BackColor = Color.FromArgb(0, 119, 182),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnRefreshAudits.Click += async (s, e) => await LoadDashboardDataAsync();

        pnlAuditHeader.Controls.Add(lblAuditTitle);
        pnlAuditHeader.Controls.Add(btnRefreshAudits);

        dgvAudits = new DataGridView();
        StyleGrid(dgvAudits);

        pnlMonitor.Controls.Add(dgvAudits);
        pnlMonitor.Controls.Add(pnlAuditHeader);
        pnlAuditHeader.SendToBack();

        tab.Controls.Add(pnlMonitor);
        tab.Controls.Add(pnlKpis);
        pnlKpis.SendToBack();

        return tab;
    }

    private Control CreateKpiCard(string title, out Label valLabel, Color barColor)
    {
        var card = new Panel
        {
            Width = 240,
            Height = 95,
            Margin = new Padding(5, 5, 15, 5),
            BackColor = Color.FromArgb(248, 250, 252),
            BorderStyle = BorderStyle.FixedSingle
        };

        var bar = new Panel { Dock = DockStyle.Left, Width = 6, BackColor = barColor };
        card.Controls.Add(bar);

        var lblT = new Label
        {
            Text = title,
            Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
            ForeColor = Color.Gray,
            Location = new Point(16, 12),
            AutoSize = true
        };

        valLabel = new Label
        {
            Text = "--",
            Font = new Font("Segoe UI", 16f, FontStyle.Bold),
            ForeColor = Color.FromArgb(0, 34, 68),
            Location = new Point(16, 38),
            AutoSize = true
        };

        card.Controls.Add(lblT);
        card.Controls.Add(valLabel);
        return card;
    }

    private TabPage CreatePaymentsTab()
    {
        var tab = new TabPage("💳 Payments & ClosedXML");
        tab.BackColor = Color.White;
        tab.Padding = new Padding(15);

        // Counter Payment Panel
        var pnlCounter = new GroupBox
        {
            Text = "Advanced Counter Payment Processing",
            Dock = DockStyle.Top,
            Height = 110,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            ForeColor = Color.FromArgb(0, 34, 68),
            Padding = new Padding(10)
        };

        int left = 15;
        pnlCounter.Controls.Add(new Label { Text = "Account No:", Location = new Point(left, 25), AutoSize = true, Font = new Font("Segoe UI", 9f) });
        txtPayAccount = new TextBox { Location = new Point(left, 50), Size = new Size(130, 26), Font = new Font("Segoe UI", 9f), Text = "ACC10001" };
        pnlCounter.Controls.Add(txtPayAccount);

        left += 145;
        pnlCounter.Controls.Add(new Label { Text = "Amount (Rs.):", Location = new Point(left, 25), AutoSize = true, Font = new Font("Segoe UI", 9f) });
        txtPayAmount = new TextBox { Location = new Point(left, 50), Size = new Size(120, 26), Font = new Font("Segoe UI", 9f), Text = "1000.00" };
        pnlCounter.Controls.Add(txtPayAmount);

        left += 135;
        pnlCounter.Controls.Add(new Label { Text = "Method:", Location = new Point(left, 25), AutoSize = true, Font = new Font("Segoe UI", 9f) });
        cmbPayMethod = new ComboBox { Location = new Point(left, 50), Size = new Size(130, 26), Font = new Font("Segoe UI", 9f), DropDownStyle = ComboBoxStyle.DropDownList };
        cmbPayMethod.Items.AddRange(new object[] { "CashCounter", "CreditCard", "OnlineBanking", "PartnerApp" });
        cmbPayMethod.SelectedIndex = 0;
        pnlCounter.Controls.Add(cmbPayMethod);

        left += 145;
        pnlCounter.Controls.Add(new Label { Text = "Reference / Notes:", Location = new Point(left, 25), AutoSize = true, Font = new Font("Segoe UI", 9f) });
        txtPayNotes = new TextBox { Location = new Point(left, 50), Size = new Size(180, 26), Font = new Font("Segoe UI", 9f), Text = "Desk payment counter" };
        pnlCounter.Controls.Add(txtPayNotes);

        left += 195;
        var btnSubmitPay = new Button
        {
            Text = "RECORD PAYMENT",
            Location = new Point(left, 42),
            Size = new Size(170, 36),
            BackColor = Color.FromArgb(46, 125, 50),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat
        };
        btnSubmitPay.Click += async (s, e) => await HandleDesktopPaymentAsync();
        pnlCounter.Controls.Add(btnSubmitPay);

        // Export & Table Actions Bar
        var pnlActions = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(0, 8, 0, 8) };

        var btnExportExcel = new Button
        {
            Text = "Export to Excel (.xlsx) [ClosedXML]",
            Dock = DockStyle.Left,
            Width = 280,
            BackColor = Color.FromArgb(0, 119, 182),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat
        };
        btnExportExcel.Click += async (s, e) => await HandleExportPaymentsExcelAsync();

        var btnRefreshPayments = new Button
        {
            Text = "Refresh Payments",
            Dock = DockStyle.Right,
            Width = 140,
            BackColor = Color.FromArgb(220, 230, 242),
            FlatStyle = FlatStyle.Flat
        };
        btnRefreshPayments.Click += async (s, e) => await LoadPaymentsDataAsync();

        pnlActions.Controls.Add(btnExportExcel);
        pnlActions.Controls.Add(btnRefreshPayments);

        dgvPayments = new DataGridView();
        StyleGrid(dgvPayments);

        tab.Controls.Add(dgvPayments);
        tab.Controls.Add(pnlActions);
        tab.Controls.Add(pnlCounter);
        pnlCounter.SendToBack();
        pnlActions.SendToBack();

        return tab;
    }

    private TabPage CreateCustomersTab()
    {
        var tab = new TabPage("👥 Customer Accounts");
        tab.BackColor = Color.White;
        tab.Padding = new Padding(15);

        var pnlActions = new Panel { Dock = DockStyle.Top, Height = 45 };

        var btnAddCust = new Button
        {
            Text = "+ Add New Customer",
            Location = new Point(0, 5),
            Size = new Size(180, 32),
            BackColor = Color.FromArgb(0, 119, 182),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat
        };
        btnAddCust.Click += async (s, e) => await HandleAddCustomerAsync();

        var btnRefreshCust = new Button
        {
            Text = "Refresh List",
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Location = new Point(880, 5),
            Size = new Size(130, 32),
            BackColor = Color.FromArgb(220, 230, 242),
            FlatStyle = FlatStyle.Flat
        };
        btnRefreshCust.Click += async (s, e) => await LoadCustomersDataAsync();

        pnlActions.Controls.Add(btnAddCust);
        pnlActions.Controls.Add(btnRefreshCust);

        dgvCustomers = new DataGridView();
        StyleGrid(dgvCustomers);

        tab.Controls.Add(dgvCustomers);
        tab.Controls.Add(pnlActions);
        pnlActions.SendToBack();

        return tab;
    }

    private TabPage CreateUsageTab()
    {
        var tab = new TabPage("💧 Water Usage Details");
        tab.BackColor = Color.White;
        tab.Padding = new Padding(15);

        var pnlRecord = new GroupBox
        {
            Text = "Record / Update Monthly Meter Reading",
            Dock = DockStyle.Top,
            Height = 100,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            ForeColor = Color.FromArgb(0, 34, 68)
        };

        int left = 15;
        pnlRecord.Controls.Add(new Label { Text = "Account No:", Location = new Point(left, 22), AutoSize = true, Font = new Font("Segoe UI", 9f) });
        txtUsageAccount = new TextBox { Location = new Point(left, 45), Size = new Size(120, 24), Font = new Font("Segoe UI", 9f), Text = "ACC10001" };
        pnlRecord.Controls.Add(txtUsageAccount);

        left += 135;
        pnlRecord.Controls.Add(new Label { Text = "Month/Year:", Location = new Point(left, 22), AutoSize = true, Font = new Font("Segoe UI", 9f) });
        txtUsageMonth = new TextBox { Location = new Point(left, 45), Size = new Size(100, 24), Font = new Font("Segoe UI", 9f), Text = DateTime.UtcNow.ToString("yyyy-MM") };
        pnlRecord.Controls.Add(txtUsageMonth);

        left += 115;
        pnlRecord.Controls.Add(new Label { Text = "Prev Reading:", Location = new Point(left, 22), AutoSize = true, Font = new Font("Segoe UI", 9f) });
        numUsagePrev = new NumericUpDown { Location = new Point(left, 45), Size = new Size(100, 24), Font = new Font("Segoe UI", 9f), Maximum = 999999, Value = 254 };
        pnlRecord.Controls.Add(numUsagePrev);

        left += 115;
        pnlRecord.Controls.Add(new Label { Text = "Curr Reading:", Location = new Point(left, 22), AutoSize = true, Font = new Font("Segoe UI", 9f) });
        numUsageCurr = new NumericUpDown { Location = new Point(left, 45), Size = new Size(100, 24), Font = new Font("Segoe UI", 9f), Maximum = 999999, Value = 270 };
        pnlRecord.Controls.Add(numUsageCurr);

        left += 115;
        pnlRecord.Controls.Add(new Label { Text = "Meter Reader:", Location = new Point(left, 22), AutoSize = true, Font = new Font("Segoe UI", 9f) });
        txtUsageOfficer = new TextBox { Location = new Point(left, 45), Size = new Size(130, 24), Font = new Font("Segoe UI", 9f), Text = "Officer Perera" };
        pnlRecord.Controls.Add(txtUsageOfficer);

        left += 145;
        var btnSaveUsage = new Button
        {
            Text = "SAVE READING",
            Location = new Point(left, 40),
            Size = new Size(150, 32),
            BackColor = Color.FromArgb(0, 119, 182),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat
        };
        btnSaveUsage.Click += async (s, e) => await HandleSaveUsageAsync();
        pnlRecord.Controls.Add(btnSaveUsage);

        dgvUsages = new DataGridView();
        StyleGrid(dgvUsages);

        tab.Controls.Add(dgvUsages);
        tab.Controls.Add(pnlRecord);
        pnlRecord.SendToBack();

        return tab;
    }

    private TabPage CreateBillsTab()
    {
        var tab = new TabPage("🧾 Bills Management");
        tab.BackColor = Color.White;
        tab.Padding = new Padding(15);

        var pnlActions = new Panel { Dock = DockStyle.Top, Height = 48, Padding = new Padding(0, 5, 0, 5) };

        var btnBatchBill = new Button
        {
            Text = "Run Monthly Batch Billing (Tariff Engine)",
            Dock = DockStyle.Left,
            Width = 300,
            BackColor = Color.FromArgb(46, 125, 50),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat
        };
        btnBatchBill.Click += async (s, e) => await HandleBatchBillingAsync();

        var btnExportBills = new Button
        {
            Text = "Export Bills to Excel [ClosedXML]",
            Dock = DockStyle.Left,
            Width = 260,
            Margin = new Padding(10, 0, 0, 0),
            BackColor = Color.FromArgb(0, 119, 182),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat
        };
        btnExportBills.Click += async (s, e) => await HandleExportBillsExcelAsync();

        var btnRefreshBills = new Button
        {
            Text = "Refresh Bills",
            Dock = DockStyle.Right,
            Width = 140,
            BackColor = Color.FromArgb(220, 230, 242),
            FlatStyle = FlatStyle.Flat
        };
        btnRefreshBills.Click += async (s, e) => await LoadBillsDataAsync();

        pnlActions.Controls.Add(btnExportBills);
        pnlActions.Controls.Add(btnBatchBill);
        pnlActions.Controls.Add(btnRefreshBills);

        dgvBills = new DataGridView();
        StyleGrid(dgvBills);

        tab.Controls.Add(dgvBills);
        tab.Controls.Add(pnlActions);
        pnlActions.SendToBack();

        return tab;
    }

    private TabPage CreatePartnersTab()
    {
        var tab = new TabPage("🏦 Third-Party Partners");
        tab.BackColor = Color.White;
        tab.Padding = new Padding(15);

        var pnlActions = new Panel { Dock = DockStyle.Top, Height = 45 };

        var btnAddPartner = new Button
        {
            Text = "+ Register New Banking Partner",
            Location = new Point(0, 5),
            Size = new Size(240, 32),
            BackColor = Color.FromArgb(0, 119, 182),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat
        };
        btnAddPartner.Click += async (s, e) => await HandleAddPartnerAsync();

        var btnTogglePartner = new Button
        {
            Text = "Toggle Partner Status (Active/Inactive)",
            Location = new Point(255, 5),
            Size = new Size(270, 32),
            BackColor = Color.FromArgb(230, 81, 0),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat
        };
        btnTogglePartner.Click += async (s, e) => await HandleTogglePartnerStatusAsync();

        var btnRefreshPartners = new Button
        {
            Text = "Refresh Partners",
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Location = new Point(880, 5),
            Size = new Size(140, 32),
            BackColor = Color.FromArgb(220, 230, 242),
            FlatStyle = FlatStyle.Flat
        };
        btnRefreshPartners.Click += async (s, e) => await LoadPartnersDataAsync();

        pnlActions.Controls.Add(btnAddPartner);
        pnlActions.Controls.Add(btnTogglePartner);
        pnlActions.Controls.Add(btnRefreshPartners);

        dgvPartners = new DataGridView();
        StyleGrid(dgvPartners);

        tab.Controls.Add(dgvPartners);
        tab.Controls.Add(pnlActions);
        pnlActions.SendToBack();

        return tab;
    }

    // Data Loaders
    public async Task LoadAllDataAsync()
    {
        bool online = await _client.CheckApiConnectionAsync();
        lblApiStatus.Text = online ? "● Web API Connected" : "● Service Direct Mode";
        lblApiStatus.ForeColor = online ? Color.ForestGreen : Color.FromArgb(0, 102, 204);

        await LoadDashboardDataAsync();
        await LoadPaymentsDataAsync();
        await LoadCustomersDataAsync();
        await LoadUsageDataAsync();
        await LoadBillsDataAsync();
        await LoadPartnersDataAsync();
    }

    private async Task LoadDashboardDataAsync()
    {
        var summary = await _client.GetDashboardAsync();
        lblTotalCustomers.Text = summary.TotalCustomers.ToString();
        lblTotalRevenue.Text = "Rs. " + summary.TotalRevenueCollected.ToString("N2");
        lblPendingBills.Text = $"{summary.TotalPendingBills} (Rs. {summary.TotalPendingAmount:N0})";
        lblTodayPayments.Text = $"{summary.TotalTransactionsToday} (Rs. {summary.TotalAmountToday:N0})";

        var audits = await _client.GetRecentAuditsAsync(50);
        dgvAudits.DataSource = audits.Select(a => new
        {
            Date = a.Timestamp.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"),
            Action = a.Action,
            Actor = a.PerformedBy,
            Account = a.AccountNumber,
            Reference = a.ReferenceId,
            Details = a.Details
        }).ToList();
    }

    private async Task LoadPaymentsDataAsync()
    {
        var payments = await _client.GetAllPaymentsAsync();
        dgvPayments.DataSource = payments.Select(p => new
        {
            Reference = p.PaymentReference,
            Account = p.AccountNumber,
            Date = p.PaymentDate.ToLocalTime().ToString("yyyy-MM-dd HH:mm"),
            Method = p.Method,
            Masked = p.MaskedAccount,
            Amount = "Rs. " + p.Amount.ToString("N2"),
            Status = p.Status,
            Channel = p.PartnerName ?? "Direct Web"
        }).ToList();
    }

    private async Task LoadCustomersDataAsync()
    {
        var customers = await _client.GetAllCustomersAsync();
        dgvCustomers.DataSource = customers.Select(c => new
        {
            c.AccountNumber,
            c.FullName,
            c.NIC,
            c.Address,
            c.AreaBranch,
            c.Phone,
            c.MeterNumber,
            Outstanding = "Rs. " + c.CurrentOutstandingBalance.ToString("N2"),
            Active = c.IsActive ? "Yes" : "No"
        }).ToList();
    }

    private async Task LoadUsageDataAsync()
    {
        var usages = await _client.GetAllUsagesAsync();
        dgvUsages.DataSource = usages.Select(u => new
        {
            u.AccountNumber,
            Month = u.MonthYear,
            PrevReading = u.PreviousReading,
            CurrReading = u.CurrentReading,
            Units = u.UnitsConsumed,
            Date = u.ReadingDate.ToString("yyyy-MM-dd"),
            Officer = u.MeterReaderName,
            u.Remarks
        }).ToList();
    }

    private async Task LoadBillsDataAsync()
    {
        var bills = await _client.GetAllBillsAsync();
        dgvBills.DataSource = bills.Select(b => new
        {
            ID = b.Id,
            Account = b.AccountNumber,
            Customer = b.CustomerName,
            Month = b.MonthYear,
            Units = b.UnitsConsumed,
            WaterCharge = b.WaterCharge.ToString("N2"),
            ServiceCharge = b.FixedServiceCharge.ToString("N2"),
            Vat = b.VatAmount.ToString("N2"),
            Total = "Rs. " + b.TotalAmount.ToString("N2"),
            DueDate = b.DueDate.ToString("yyyy-MM-dd"),
            Status = b.Status.ToString()
        }).ToList();
    }

    private async Task LoadPartnersDataAsync()
    {
        var partners = await _client.GetAllPartnersAsync();
        dgvPartners.DataSource = partners.Select(p => new
        {
            p.Id,
            Code = p.PartnerCode,
            Name = p.PartnerName,
            Key = p.ApiKey,
            Email = p.ContactEmail,
            Active = p.IsActive ? "ACTIVE" : "INACTIVE",
            Transactions = p.TotalTransactionsProcessed,
            ProcessedAmount = "Rs. " + p.TotalAmountProcessed.ToString("N2")
        }).ToList();
    }

    // Event Handlers
    private async Task HandleDesktopPaymentAsync()
    {
        if (string.IsNullOrWhiteSpace(txtPayAccount.Text))
        {
            MessageBox.Show("Please enter an account number.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!decimal.TryParse(txtPayAmount.Text, out decimal amt) || amt <= 0)
        {
            MessageBox.Show("Please enter a valid amount greater than zero.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var method = Enum.Parse<PaymentMethod>(cmbPayMethod.SelectedItem?.ToString() ?? "CashCounter");

        var req = new MakePaymentRequest
        {
            AccountNumber = txtPayAccount.Text.Trim(),
            Amount = amt,
            Method = method,
            CardOrAccountNumber = "DESK-COUNTER-01",
            Notes = txtPayNotes.Text
        };

        var res = await _client.ProcessPaymentAsync(req);
        if (res.Success && res.Data != null)
        {
            MessageBox.Show($"Payment Successful!\nReceipt Reference: {res.Data.PaymentReference}\nAmount: Rs. {res.Data.AmountPaid:N2}\nRemaining Balance: Rs. {res.Data.RemainingBalance:N2}", "Payment Confirmed", MessageBoxButtons.OK, MessageBoxIcon.Information);
            await LoadAllDataAsync();
        }
        else
        {
            MessageBox.Show(res.Message, "Payment Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task HandleSaveUsageAsync()
    {
        if (numUsageCurr.Value < numUsagePrev.Value)
        {
            MessageBox.Show("Current reading cannot be less than previous reading.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var req = new RecordUsageRequest
        {
            AccountNumber = txtUsageAccount.Text.Trim(),
            MonthYear = txtUsageMonth.Text.Trim(),
            PreviousReading = numUsagePrev.Value,
            CurrentReading = numUsageCurr.Value,
            MeterReaderName = txtUsageOfficer.Text.Trim(),
            Remarks = "Entered from Desktop Console"
        };

        var res = await _client.RecordUsageAsync(req);
        if (res.Success)
        {
            MessageBox.Show($"Meter reading saved! Consumed: {req.CurrentReading - req.PreviousReading} units.", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
            await LoadUsageDataAsync();
            await LoadDashboardDataAsync();
        }
        else
        {
            MessageBox.Show(res.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task HandleBatchBillingAsync()
    {
        string month = DateTime.UtcNow.ToString("yyyy-MM");
        var res = await _client.BatchGenerateBillsAsync(month);
        if (res.Success)
        {
            MessageBox.Show($"Batch billing completed!\n{res.Message}", "Billing Engine", MessageBoxButtons.OK, MessageBoxIcon.Information);
            await LoadBillsDataAsync();
            await LoadDashboardDataAsync();
        }
        else
        {
            MessageBox.Show(res.Message, "Billing Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task HandleExportPaymentsExcelAsync()
    {
        try
        {
            var bytes = await _client.ExportPaymentsExcelAsync();
            using var sfd = new SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                FileName = $"NWSDB_Payment_Report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                await File.WriteAllBytesAsync(sfd.FileName, bytes);
                if (MessageBox.Show("Excel report generated successfully using ClosedXML!\nWould you like to open it now?", "ClosedXML Export", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    Process.Start(new ProcessStartInfo(sfd.FileName) { UseShellExecute = true });
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Export error: " + ex.Message, "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task HandleExportBillsExcelAsync()
    {
        try
        {
            var bytes = await _client.ExportBillsExcelAsync();
            using var sfd = new SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                FileName = $"NWSDB_Billing_Summary_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                await File.WriteAllBytesAsync(sfd.FileName, bytes);
                if (MessageBox.Show("Billing report generated successfully using ClosedXML!\nWould you like to open it now?", "ClosedXML Export", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    Process.Start(new ProcessStartInfo(sfd.FileName) { UseShellExecute = true });
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Export error: " + ex.Message, "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task HandleAddCustomerAsync()
    {
        string name = PromptInput("Enter Customer Full Name:", "Add Customer", "P. M. Perera");
        if (string.IsNullOrWhiteSpace(name)) return;

        string nic = PromptInput("Enter NIC Number:", "Add Customer", "199012345678");
        if (string.IsNullOrWhiteSpace(nic)) return;

        string address = PromptInput("Enter Premises Address:", "Add Customer", "No. 12, Galle Road, Colombo");
        string phone = PromptInput("Enter Contact Phone:", "Add Customer", "0771234567");

        var newCust = new CustomerDto
        {
            FullName = name,
            NIC = nic,
            Address = address,
            AreaBranch = "Colombo Central",
            Phone = phone,
            Email = $"{name.ToLower().Replace(" ", "")}@gmail.com",
            IsActive = true
        };

        var res = await _client.CreateCustomerAsync(newCust);
        if (res.Success)
        {
            MessageBox.Show($"Customer created!\nAccount Number: {res.Data?.AccountNumber}\nMeter Number: {res.Data?.MeterNumber}", "Customer Added", MessageBoxButtons.OK, MessageBoxIcon.Information);
            await LoadCustomersDataAsync();
            await LoadDashboardDataAsync();
        }
        else
        {
            MessageBox.Show(res.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task HandleAddPartnerAsync()
    {
        string code = PromptInput("Enter Partner Code (e.g. HNB, SEYLAN):", "New Partner", "HNB");
        if (string.IsNullOrWhiteSpace(code)) return;

        string name = PromptInput("Enter Partner Name:", "New Partner", "Hatton National Bank");
        string email = PromptInput("Enter Partner Contact Email:", "New Partner", "api@hnb.lk");

        var res = await _client.RegisterPartnerAsync(new RegisterPartnerRequest
        {
            PartnerCode = code,
            PartnerName = name,
            ContactEmail = email
        });

        if (res.Success && res.Data != null)
        {
            MessageBox.Show($"Partner Registered!\nCode: {res.Data.PartnerCode}\nGenerated API Key: {res.Data.ApiKey}", "Partner Registered", MessageBoxButtons.OK, MessageBoxIcon.Information);
            await LoadPartnersDataAsync();
        }
        else
        {
            MessageBox.Show(res.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task HandleTogglePartnerStatusAsync()
    {
        if (dgvPartners.SelectedRows.Count == 0)
        {
            MessageBox.Show("Please select a partner from the table.", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var row = dgvPartners.SelectedRows[0];
        int id = Convert.ToInt32(row.Cells["Id"].Value);

        var res = await _client.TogglePartnerStatusAsync(id);
        if (res.Success)
        {
            MessageBox.Show(res.Message, "Status Changed", MessageBoxButtons.OK, MessageBoxIcon.Information);
            await LoadPartnersDataAsync();
        }
    }

    private static string PromptInput(string text, string caption, string defaultVal = "")
    {
        using var prompt = new Form
        {
            Width = 400,
            Height = 180,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            Text = caption,
            StartPosition = FormStartPosition.CenterParent,
            MaximizeBox = false,
            MinimizeBox = false
        };

        var textLabel = new Label { Left = 20, Top = 15, Text = text, AutoSize = true };
        var textBox = new TextBox { Left = 20, Top = 40, Width = 340, Text = defaultVal };
        var confirmation = new Button { Text = "OK", Left = 260, Width = 100, Top = 80, DialogResult = DialogResult.OK, BackColor = Color.FromArgb(0, 119, 182), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };

        prompt.Controls.Add(textLabel);
        prompt.Controls.Add(textBox);
        prompt.Controls.Add(confirmation);
        prompt.AcceptButton = confirmation;

        return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
    }
}
