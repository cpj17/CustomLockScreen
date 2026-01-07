using System;
using System.Configuration;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace CustomLockScreen
{
    public partial class LockScreenForm : Form
    {
        private const string ADMIN_UNLOCK_PASSWORD = "1234";
        private static string UNLOCK_PASSWORD = "1234";

        private Timer unlockTimer;
        private double fadeStep = 0.05;

        private Timer shakeTimer;
        private int shakeOffset = 0;
        private int shakeDirection = 1;
        private int shakeCount = 0;

        private Point passwordBaseLocation;
        private Point buttonBaseLocation;

        private Label lblName;
        private Label lblTime;
        private TextBox txtPassword;
        private Button btnUnlock;
        private Label lblError;
        private Timer clockTimer;

        private bool allowClose = false;
        private KeyboardHook keyboardHook;

        private int invalidAttempts = 0;
        private const int MAX_ATTEMPTS = 5;


        public LockScreenForm()
        {
            UNLOCK_PASSWORD = ConfigurationManager.AppSettings["hashedUnlockPassword"].ToString();  //Vht@2025

            InitializeLockScreen();
        }

        private void InitializeLockScreen()
        {
            // FORM
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            TopMost = true;
            BackColor = Color.Black;
            ShowInTaskbar = false;
            ControlBox = false;
            KeyPreview = true;

            // NAME
            lblName = new Label
            {
                Text = "CPJ",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 36, FontStyle.Bold),
                AutoSize = true
            };

            // TIME
            lblTime = new Label
            {
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 48),
                AutoSize = true
            };

            // PASSWORD TEXTBOX
            txtPassword = new TextBox
            {
                Width = 250,
                Font = new Font("Segoe UI", 18),
                PasswordChar = '●',
                TextAlign = HorizontalAlignment.Center
            };
            txtPassword.KeyDown += TxtPassword_KeyDown;

            // UNLOCK BUTTON
            btnUnlock = new Button
            {
                Text = "Unlock",
                Width = 100,
                Height = txtPassword.Height,
                Font = new Font("Segoe UI", 14),
                ForeColor = Color.White,
            };
            btnUnlock.Click += (s, e) => AttemptUnlock(txtPassword.Text);

            // ERROR
            lblError = new Label
            {
                ForeColor = Color.Red,
                AutoSize = true,
                Visible = false,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // TIMER
            clockTimer = new Timer { Interval = 1000 };
            clockTimer.Tick += (s, e) => UpdateTime();
            clockTimer.Start();

            Controls.AddRange(new Control[]
            {
                lblName, lblTime, txtPassword, btnUnlock, lblError
            });

            Load += (s, e) =>
            {
                keyboardHook = new KeyboardHook(); // SAFE HOOK
                UpdateTime();
                PositionControls();
                txtPassword.Focus();
            };

            KeyDown += LockScreenForm_KeyDown;

            unlockTimer = new Timer
            {
                Interval = 30 // smooth animation
            };
            unlockTimer.Tick += UnlockTimer_Tick;

            lblName.Font = new Font("Segoe UI Black", 36, FontStyle.Bold);
            lblTime.Font = new Font("Calibri", 48, FontStyle.Bold);
            txtPassword.Font = new Font("Verdana", 18, FontStyle.Regular);
            btnUnlock.Font = new Font("Tahoma", 14, FontStyle.Bold);
            lblError.Font = new Font("Segoe UI", 30, FontStyle.Italic);

            shakeTimer = new Timer { Interval = 15 };
            shakeTimer.Tick += ShakeTimer_Tick;

        }

        private void UpdateTime()
        {
            lblTime.Text = DateTime.Now.ToString("hh:mm:ss tt"); // 12-hour format with AM/PM
            PositionControls();
        }

        //private void PositionControls()
        //{
        //    int cx = Width / 2;
        //    int cy = Height / 2;

        //    lblName.Location = new Point(cx - lblName.Width / 2, cy - 230);
        //    lblTime.Location = new Point(cx - lblTime.Width / 2, cy - 150);

        //    // TEXTBOX + BUTTON SIDE BY SIDE
        //    int spacing = 10; // space between textbox and button
        //    int totalWidth = txtPassword.Width + spacing + btnUnlock.Width;
        //    int startX = cx - totalWidth / 2;

        //    txtPassword.Location = new Point(startX, cy - 40);
        //    btnUnlock.Location = new Point(startX + txtPassword.Width + spacing, cy - 40);

        //    lblError.Location = new Point(cx - lblError.Width / 2, cy + 40);
        //}

        //private void PositionControls()
        //{
        //    // --- CONTENT DIMENSIONS ---
        //    int spacing = 10; // space between textbox and button
        //    int row1Height = lblName.Height;
        //    int row2Height = lblTime.Height;
        //    int row3Height = txtPassword.Height;
        //    int row4Height = lblError.Visible ? lblError.Height : 0;

        //    int totalHeight = row1Height + 20 + row2Height + 40 + row3Height + 10 + row4Height;

        //    int startY = (Height - totalHeight) / 2;
        //    startY = 200;

        //    int cx = Width / 2;

        //    // --- Row 1: Name ---
        //    lblName.Location = new Point(cx - lblName.Width / 2, startY);

        //    // --- Row 2: Time ---
        //    lblTime.Location = new Point(cx - lblTime.Width / 2, startY + row1Height + 20);

        //    // --- Row 3: Password + Button ---
        //    int totalWidth = txtPassword.Width + spacing + btnUnlock.Width;
        //    int startX = cx - totalWidth / 2;

        //    txtPassword.Location = new Point(startX, startY + row1Height + 20 + row2Height + 40);
        //    btnUnlock.Location = new Point(startX + txtPassword.Width + spacing, txtPassword.Top);

        //    // --- Row 4: Error Label ---
        //    if (lblError.Visible)
        //        lblError.Location = new Point(cx - lblError.Width / 2, txtPassword.Top + txtPassword.Height + 10);
        //}

        private void PositionControls()
        {
            int cx = Width / 2;
            int cy = Height / 2;

            // Name
            lblName.Location = new Point(cx - lblName.Width / 2, cy - 220);

            // Time
            lblTime.Location = new Point(cx - lblTime.Width / 2, cy - 140);

            // Password + Button
            int spacing = 10;
            int totalWidth = txtPassword.Width + spacing + btnUnlock.Width;
            int startX = cx - totalWidth / 2;

            int inputY = cy - 20;

            txtPassword.Location = new Point(startX, inputY);
            btnUnlock.Location = new Point(startX + txtPassword.Width + spacing, inputY);

            // Save base positions for animation
            passwordBaseLocation = txtPassword.Location;
            buttonBaseLocation = btnUnlock.Location;

            // Error label (fixed position, no layout jump)
            lblError.Location = new Point(cx - lblError.Width / 2, inputY + txtPassword.Height + 12);
        }

        private void TxtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (invalidAttempts >= MAX_ATTEMPTS)
            {
                e.SuppressKeyPress = true;
                return;
            }

            if (e.KeyCode == Keys.Enter)
            {
                AttemptUnlock(txtPassword.Text);
                e.SuppressKeyPress = true;
            }
        }

        // SECRET SHORTCUT: CTRL + SHIFT + U
        private void LockScreenForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.Shift && e.KeyCode == Keys.U)
            {
                using (Form prompt = CreatePasswordPrompt())
                {
                    prompt.ShowDialog();
                }
            }
        }

        private void AttemptUnlock(string password)
        {
            if (Md5Helper.ComputeMd5(password) == UNLOCK_PASSWORD)
            {
                allowClose = true;
                keyboardHook?.Dispose();
                clockTimer.Stop();

                DisableInputs();
                StartUnlockAnimation();
            }
            else
            {
                invalidAttempts++;

                // LOCKED STATE
                if (invalidAttempts >= MAX_ATTEMPTS)
                {
                    txtPassword.Visible = false;
                    btnUnlock.Visible = false;

                    lblError.Text = "Please contact CPJ";
                    lblError.Visible = true;

                    // Stop animations
                    shakeTimer?.Stop();

                    PositionControls();
                    return;
                }

                shakeTimer.Stop();
                shakeCount = 0;
                shakeDirection = 1;
                shakeTimer.Start();

                txtPassword.Clear();
                txtPassword.Focus();
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!allowClose)
            {
                e.Cancel = true;
            }
            base.OnFormClosing(e);
        }

        private Form CreatePasswordPrompt()
        {
            Form f = new Form
            {
                Width = 400,
                Height = 180,
                Text = "Unlock",
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterScreen,
                TopMost = true
            };

            TextBox txt = new TextBox
            {
                PasswordChar = '●',
                Width = 260,
                Font = new Font("Segoe UI", 12),
                Left = 60,
                Top = 30
            };

            Button btn = new Button
            {
                Text = "Unlock",
                Width = 100,
                Height = 35,
                Left = 150,
                Top = 80
            };

            btn.Click += (s, e) =>
            {
                if (txt.Text == ADMIN_UNLOCK_PASSWORD)
                {
                    allowClose = true;
                    keyboardHook?.Dispose();

                    //Application.Exit();

                    DisableInputs();
                    StartUnlockAnimation();
                }
                else
                {
                    MessageBox.Show("Wrong password");
                    txt.Clear();
                }
            };

            f.Controls.Add(txt);
            f.Controls.Add(btn);
            return f;
        }

        private void StartUnlockAnimation()
        {
            unlockTimer.Start();
        }

        private void UnlockTimer_Tick(object sender, EventArgs e)
        {
            if (this.Opacity > 0)
            {
                this.Opacity -= fadeStep;
            }
            else
            {
                unlockTimer.Stop();
                Application.Exit();
            }
        }

        private void DisableInputs()
        {
            txtPassword.Enabled = false;
            btnUnlock.Enabled = false;
        }

        private void ShakeTimer_Tick(object sender, EventArgs e)
        {
            int shakeDistance = 6;

            txtPassword.Left = passwordBaseLocation.X + (shakeDistance * shakeDirection);
            btnUnlock.Left = buttonBaseLocation.X + (shakeDistance * shakeDirection);

            shakeDirection *= -1;
            shakeCount++;

            if (shakeCount >= 10)
            {
                shakeTimer.Stop();
                shakeCount = 0;
                shakeDirection = 1;

                // Reset positions
                txtPassword.Location = passwordBaseLocation;
                btnUnlock.Location = buttonBaseLocation;
            }
        }

    }

    public static class Md5Helper
    {
        public static string ComputeMd5(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert to hex string
                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashBytes)
                    sb.Append(b.ToString("x2"));

                return sb.ToString();
            }
        }
    }
}
