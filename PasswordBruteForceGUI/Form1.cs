using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PasswordBruteForceGUI
{
    public partial class Form1 : Form
    {
        private const string Characters = "abc123";
        private const int MaxLength = 6;

        private TextBox txtPassword;
        private TextBox txtHash;
        private TextBox txtFoundPassword;
        private TextBox txtLog;

        private Button btnGenerate;
        private Button btnStart;
        private Button btnStop;

        private ProgressBar progressBar;
        private Label lblElapsed;
        private Label lblProgress;
        private Label lblThreads;

        private readonly PasswordHasher hasher = new PasswordHasher();
        private PasswordGenerator passwordGenerator;
        private PerformanceLogger logger;

        private string currentPassword;
        private string currentHash;

        private CancellationTokenSource cancellationTokenSource;
        private Stopwatch stopwatch;
        private System.Windows.Forms.Timer timer;

        public Form1()
        {
            InitializeComponent();

            passwordGenerator = new PasswordGenerator(Characters);
            logger = new PerformanceLogger();
            stopwatch = new Stopwatch();

            CreateGui();
            CreateTimer();
        }

        private void CreateGui()
        {
            this.Text = "Password Brute Force GUI";
            this.Width = 900;
            this.Height = 650;
            this.StartPosition = FormStartPosition.CenterScreen;

            Label title = new Label();
            title.Text = "SHA256 Password Brute Force Demo";
            title.Left = 250;
            title.Top = 20;
            title.Width = 450;
            title.Font = new System.Drawing.Font("Arial", 16, System.Drawing.FontStyle.Bold);
            this.Controls.Add(title);

            btnGenerate = new Button();
            btnGenerate.Text = "Create Password";
            btnGenerate.Left = 30;
            btnGenerate.Top = 70;
            btnGenerate.Width = 140;
            btnGenerate.Click += BtnGenerate_Click;
            this.Controls.Add(btnGenerate);

            Label lblPassword = new Label();
            lblPassword.Text = "Generated password:";
            lblPassword.Left = 30;
            lblPassword.Top = 115;
            lblPassword.Width = 160;
            this.Controls.Add(lblPassword);

            txtPassword = new TextBox();
            txtPassword.Left = 200;
            txtPassword.Top = 112;
            txtPassword.Width = 200;
            txtPassword.ReadOnly = true;
            this.Controls.Add(txtPassword);

            Label lblHash = new Label();
            lblHash.Text = "SHA256 hash:";
            lblHash.Left = 30;
            lblHash.Top = 155;
            lblHash.Width = 160;
            this.Controls.Add(lblHash);

            txtHash = new TextBox();
            txtHash.Left = 200;
            txtHash.Top = 152;
            txtHash.Width = 630;
            txtHash.ReadOnly = true;
            this.Controls.Add(txtHash);

            Label lblInfo = new Label();
            lblInfo.Text = "Character set: abc123 | Random password length: [4-6) | Search length: 1 to 6";
            lblInfo.Left = 30;
            lblInfo.Top = 195;
            lblInfo.Width = 700;
            this.Controls.Add(lblInfo);

            lblThreads = new Label();
            lblThreads.Text = "Maximum threads: CPU cores - 1 = " + Math.Max(1, Environment.ProcessorCount - 1);
            lblThreads.Left = 30;
            lblThreads.Top = 220;
            lblThreads.Width = 400;
            this.Controls.Add(lblThreads);

            btnStart = new Button();
            btnStart.Text = "Start Attack";
            btnStart.Left = 30;
            btnStart.Top = 260;
            btnStart.Width = 120;
            btnStart.Click += BtnStart_Click;
            this.Controls.Add(btnStart);

            btnStop = new Button();
            btnStop.Text = "Stop Attack";
            btnStop.Left = 170;
            btnStop.Top = 260;
            btnStop.Width = 120;
            btnStop.Enabled = false;
            btnStop.Click += BtnStop_Click;
            this.Controls.Add(btnStop);

            progressBar = new ProgressBar();
            progressBar.Left = 30;
            progressBar.Top = 310;
            progressBar.Width = 800;
            progressBar.Height = 25;
            progressBar.Minimum = 0;
            progressBar.Maximum = 100;
            this.Controls.Add(progressBar);

            lblProgress = new Label();
            lblProgress.Text = "Progress: 0% | Attempts: 0";
            lblProgress.Left = 30;
            lblProgress.Top = 345;
            lblProgress.Width = 350;
            this.Controls.Add(lblProgress);

            lblElapsed = new Label();
            lblElapsed.Text = "Elapsed time: 00:00:00.000";
            lblElapsed.Left = 430;
            lblElapsed.Top = 345;
            lblElapsed.Width = 350;
            this.Controls.Add(lblElapsed);

            Label lblFound = new Label();
            lblFound.Text = "Found password:";
            lblFound.Left = 30;
            lblFound.Top = 385;
            lblFound.Width = 160;
            this.Controls.Add(lblFound);

            txtFoundPassword = new TextBox();
            txtFoundPassword.Left = 200;
            txtFoundPassword.Top = 382;
            txtFoundPassword.Width = 200;
            txtFoundPassword.ReadOnly = true;
            this.Controls.Add(txtFoundPassword);

            Label lblLog = new Label();
            lblLog.Text = "Performance log:";
            lblLog.Left = 30;
            lblLog.Top = 425;
            lblLog.Width = 200;
            this.Controls.Add(lblLog);

            txtLog = new TextBox();
            txtLog.Left = 30;
            txtLog.Top = 450;
            txtLog.Width = 800;
            txtLog.Height = 120;
            txtLog.Multiline = true;
            txtLog.ScrollBars = ScrollBars.Vertical;
            txtLog.ReadOnly = true;
            this.Controls.Add(txtLog);
        }

        private void CreateTimer()
        {
            timer = new System.Windows.Forms.Timer();
            timer.Interval = 100;
            timer.Tick += Timer_Tick;
        }

        private void BtnGenerate_Click(object sender, EventArgs e)
        {
            currentPassword = passwordGenerator.GeneratePassword();
            currentHash = hasher.ComputeSha256Hash(currentPassword);

            txtPassword.Text = currentPassword;
            txtHash.Text = currentHash;
            txtFoundPassword.Clear();
            progressBar.Value = 0;
            lblProgress.Text = "Progress: 0% | Attempts: 0";

            logger.Clear();
            AddLog("Password created.");
            AddLog("Static salt: " + PasswordHasher.StaticSalt);
            AddLog("Password was hashed using SHA256.");
        }

        private async void BtnStart_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(currentHash))
            {
                BtnGenerate_Click(sender, e);
            }

            btnGenerate.Enabled = false;
            btnStart.Enabled = false;
            btnStop.Enabled = true;

            txtFoundPassword.Clear();
            progressBar.Value = 0;
            lblProgress.Text = "Progress: 0% | Attempts: 0";

            cancellationTokenSource = new CancellationTokenSource();

            CandidateGenerator candidateGenerator = new CandidateGenerator(Characters);
            BruteForceValidator validator = new BruteForceValidator(currentHash, hasher);
            BruteForceWorker worker = new BruteForceWorker(candidateGenerator, validator);

            stopwatch.Restart();
            timer.Start();

            try
            {
                AddLog("Brute force attack started.");
                AddLog("Algorithm starts from length 1 and searches up to length 6.");
                AddLog("Target password length is not given to the brute force algorithm.");

                AddLog("Running single-thread brute force...");
                AttackResult singleResult = await worker.RunSingleThreadAsync(
                    MaxLength,
                    cancellationTokenSource.Token,
                    UpdateProgress);

                ShowResult(singleResult);

                if (cancellationTokenSource.IsCancellationRequested)
                {
                    AddLog("Attack stopped by user.");
                    return;
                }

                progressBar.Value = 0;
                lblProgress.Text = "Progress: 0% | Attempts: 0";

                AddLog("Running multi-thread brute force...");
                AttackResult multiResult = await worker.RunMultiThreadAsync(
                    MaxLength,
                    cancellationTokenSource.Token,
                    UpdateProgress);

                ShowResult(multiResult);

                if (!string.IsNullOrEmpty(multiResult.FoundPassword))
                {
                    txtFoundPassword.Text = multiResult.FoundPassword;
                }

                if (!string.IsNullOrEmpty(singleResult.FoundPassword) &&
                    !string.IsNullOrEmpty(multiResult.FoundPassword))
                {
                    double singleMs = singleResult.ElapsedTime.TotalMilliseconds;
                    double multiMs = multiResult.ElapsedTime.TotalMilliseconds;

                    AddLog("Performance comparison:");
                    AddLog("Single-thread time: " + singleMs.ToString("0.000") + " ms");
                    AddLog("Multi-thread time: " + multiMs.ToString("0.000") + " ms");
                }
            }
            catch (Exception ex)
            {
                AddLog("Error: " + ex.Message);
            }
            finally
            {
                stopwatch.Stop();
                timer.Stop();

                btnGenerate.Enabled = true;
                btnStart.Enabled = true;
                btnStop.Enabled = false;
            }
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                AddLog("Stop button clicked. Running threads will stop.");
                btnStop.Enabled = false;
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            lblElapsed.Text = "Elapsed time: " + stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff");
        }

        private void UpdateProgress(int percent, long attempts)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action<int, long>(UpdateProgress), percent, attempts);
                return;
            }

            percent = Math.Max(0, Math.Min(100, percent));
            progressBar.Value = percent;
            lblProgress.Text = "Progress: " + percent + "% | Attempts: " + attempts;
        }

        private void ShowResult(AttackResult result)
        {
            if (result.Stopped)
            {
                AddLog(result.Mode + " stopped.");
                return;
            }

            AddLog(result.Mode + " finished.");
            AddLog("Threads used: " + result.ThreadsUsed);
            AddLog("Found password: " + result.FoundPassword);
            AddLog("Attempts: " + result.Attempts);
            AddLog("Elapsed time: " + result.ElapsedTime.TotalMilliseconds.ToString("0.000") + " ms");
        }

        private void AddLog(string message)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.BeginInvoke(new Action<string>(AddLog), message);
                return;
            }

            logger.Add(message);
            txtLog.Text = logger.ToString();
            txtLog.SelectionStart = txtLog.Text.Length;
            txtLog.ScrollToCaret();
        }
    }
}