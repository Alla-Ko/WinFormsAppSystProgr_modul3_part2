namespace WinFormsAppSystProgr_modul3_part2_task1_240520
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            StartSettings();
        }

        private List<CancellationTokenSource> cancellationTokenSources = new List<CancellationTokenSource>();
        List<CustomProgressBar> progressBarList = new List<CustomProgressBar>();
        List<Task> tasks = new List<Task>();
        private Random random = new Random();

        private void StartSettings()
        {
            buttonStart.Visible = true;
            buttonStop.Visible = false;
            panel2.Controls.Clear();
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            if (tasks.Any(task => !task.IsCompleted))
            {
                MessageBox.Show("Some tasks are still running. Please stop them first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int numberOfBars = (int)numericUpDown1.Value;
            if (numberOfBars < 1 || numberOfBars > 50)
            {
                MessageBox.Show("Choose from 1 to 50 progress bars, please", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            buttonStart.Visible = false;
            buttonStop.Visible = true;

            panel2.Controls.Clear();
            progressBarList.Clear();
            tasks.Clear();
            cancellationTokenSources.Clear();

            for (int i = 0; i < numberOfBars; i++)
            {
                var cts = new CancellationTokenSource();
                cancellationTokenSources.Add(cts);

                CustomProgressBar bar = new CustomProgressBar();
                bar.Dock = DockStyle.Top;
                bar.Padding = new Padding(0, 10, 0, 10);
                panel2.Controls.Add(bar);
                progressBarList.Add(bar);

                var token = cts.Token;
                Task newTask = Task.Run(() => Dance(bar, token), token);
                tasks.Add(newTask);
            }
        }

        private void Dance(CustomProgressBar bar, CancellationToken token)
        {
            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }

                try
                {
                    Invoke(new Action(() =>
                    {
                        if (!bar.IsDisposed)
                        {
                            bar.ProgressColor = Color.FromArgb(random.Next(256), random.Next(256), random.Next(256));
                            bar.Value = random.Next(101);
                        }
                    }));
                }
                catch (ObjectDisposedException)
                {
                    return;
                }

                Thread.Sleep(200);
            }
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            foreach (var cts in cancellationTokenSources)
            {
                cts.Cancel();
            }

            buttonStart.Visible = true;
            buttonStop.Visible = false;
        }

        private async void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach (var cts in cancellationTokenSources)
            {
                cts.Cancel();
            }

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (OperationCanceledException)
            {
                // Handle task cancellation
            }
            catch (ObjectDisposedException)
            {
                // Handle disposed object exception
            }
        }


    }
    public class CustomProgressBar : ProgressBar
    {
        private Color progressColor = Color.Blue;

        public Color ProgressColor
        {
            get { return progressColor; }
            set
            {
                progressColor = value;
                this.Invalidate(); // Refresh the progress bar when the color changes
            }
        }
        public CustomProgressBar()
        {
            this.SetStyle(ControlStyles.UserPaint, true);
            this.Height = 30;
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            Rectangle rect = this.ClientRectangle;
            Graphics g = e.Graphics;

            ProgressBarRenderer.DrawHorizontalBar(g, rect);
            rect.Inflate(-3, -3);
            if (this.Value > 0)
            {
                Rectangle clip = new Rectangle(rect.X, rect.Y, (int)Math.Round(((double)this.Value / this.Maximum) * rect.Width), rect.Height);
                using (SolidBrush brush = new SolidBrush(ProgressColor))
                {
                    g.FillRectangle(brush, clip);
                }
            }
        }
    }
}
