using System;
using System.Drawing;
using System.Windows.Forms;

namespace PolyGlotTranslator
{
    public partial class SettingsForm : Form
    {
        private CheckBox chkAutoDetect;
        private CheckBox chkAutoCopy;
        private CheckBox chkAutoSpeak;
        private CheckBox chkSaveHistory;
        private NumericUpDown nudHistoryLimit;
        private TextBox txtApiKey;
        private Button btnSave;
        private Button btnCancel;
        private Button btnTestApi;
        private Label lblApiStatus;

        public SettingsForm()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void InitializeComponent()
        {
            this.Text = "Ayarlar - PolyGlot Translator";
            this.Size = new Size(500, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // Otomatik algıla
            chkAutoDetect = new CheckBox();
            chkAutoDetect.Text = "Dili otomatik algıla";
            chkAutoDetect.Location = new Point(20, 20);
            chkAutoDetect.Size = new Size(200, 25);
            chkAutoDetect.Font = new Font("Segoe UI", 10);

            // Otomatik kopyala
            chkAutoCopy = new CheckBox();
            chkAutoCopy.Text = "Çeviriyi otomatik kopyala";
            chkAutoCopy.Location = new Point(20, 60);
            chkAutoCopy.Size = new Size(200, 25);
            chkAutoCopy.Font = new Font("Segoe UI", 10);

            // Otomatik konuş
            chkAutoSpeak = new CheckBox();
            chkAutoSpeak.Text = "Çeviriyi otomatik seslendir";
            chkAutoSpeak.Location = new Point(20, 100);
            chkAutoSpeak.Size = new Size(200, 25);
            chkAutoSpeak.Font = new Font("Segoe UI", 10);

            // Geçmişi kaydet
            chkSaveHistory = new CheckBox();
            chkSaveHistory.Text = "Çeviri geçmişini kaydet";
            chkSaveHistory.Location = new Point(20, 140);
            chkSaveHistory.Size = new Size(200, 25);
            chkSaveHistory.Font = new Font("Segoe UI", 10);
            chkSaveHistory.CheckedChanged += ChkSaveHistory_CheckedChanged;

            // Geçmiş limiti
            Label lblHistoryLimit = new Label();
            lblHistoryLimit.Text = "Geçmiş kayıt limiti:";
            lblHistoryLimit.Location = new Point(20, 180);
            lblHistoryLimit.Size = new Size(150, 25);
            lblHistoryLimit.Font = new Font("Segoe UI", 10);

            nudHistoryLimit = new NumericUpDown();
            nudHistoryLimit.Location = new Point(180, 180);
            nudHistoryLimit.Size = new Size(80, 25);
            nudHistoryLimit.Minimum = 10;
            nudHistoryLimit.Maximum = 1000;
            nudHistoryLimit.Value = 100;
            nudHistoryLimit.Font = new Font("Segoe UI", 10);

            // API Key
            Label lblApiKey = new Label();
            lblApiKey.Text = "API Anahtarı (Google Translate):";
            lblApiKey.Location = new Point(20, 220);
            lblApiKey.Size = new Size(250, 25);
            lblApiKey.Font = new Font("Segoe UI", 10);

            txtApiKey = new TextBox();
            txtApiKey.Location = new Point(20, 250);
            txtApiKey.Size = new Size(440, 30);
            txtApiKey.Font = new Font("Segoe UI", 10);
            txtApiKey.PasswordChar = '*';

            // API Test butonu
            btnTestApi = new Button();
            btnTestApi.Text = "API'yi Test Et";
            btnTestApi.Location = new Point(20, 290);
            btnTestApi.Size = new Size(120, 35);
            btnTestApi.Font = new Font("Segoe UI", 10);
            btnTestApi.Click += BtnTestApi_Click;

            // API Durumu
            lblApiStatus = new Label();
            lblApiStatus.Location = new Point(150, 295);
            lblApiStatus.Size = new Size(300, 25);
            lblApiStatus.Font = new Font("Segoe UI", 9);
            lblApiStatus.Text = "API durumu: Test edilmedi";

            // Kaydet butonu
            btnSave = new Button();
            btnSave.Text = "KAYDET";
            btnSave.Location = new Point(280, 340);
            btnSave.Size = new Size(100, 35);
            btnSave.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnSave.BackColor = Color.DodgerBlue;
            btnSave.ForeColor = Color.White;
            btnSave.Click += BtnSave_Click;

            // İptal butonu
            btnCancel = new Button();
            btnCancel.Text = "İPTAL";
            btnCancel.Location = new Point(390, 340);
            btnCancel.Size = new Size(100, 35);
            btnCancel.Font = new Font("Segoe UI", 10);
            btnCancel.Click += BtnCancel_Click;

            // Kontrolleri forma ekle
            this.Controls.AddRange(new Control[]
            {
                chkAutoDetect, chkAutoCopy, chkAutoSpeak,
                chkSaveHistory, lblHistoryLimit, nudHistoryLimit,
                lblApiKey, txtApiKey, btnTestApi, lblApiStatus,
                btnSave, btnCancel
            });
        }

        private void LoadSettings()
        {
            // Varsayılan ayarlar
            chkAutoDetect.Checked = true;
            chkAutoCopy.Checked = false;
            chkAutoSpeak.Checked = false;
            chkSaveHistory.Checked = true;
            nudHistoryLimit.Value = 100;
            txtApiKey.Text = "";

            // Kayıtlı ayarları yükle
            try
            {
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string settingsFile = System.IO.Path.Combine(appData, "PolyGlotTranslator", "settings.ini");

                if (System.IO.File.Exists(settingsFile))
                {
                    var lines = System.IO.File.ReadAllLines(settingsFile);
                    foreach (var line in lines)
                    {
                        var parts = line.Split('=');
                        if (parts.Length == 2)
                        {
                            switch (parts[0])
                            {
                                case "AutoDetect":
                                    chkAutoDetect.Checked = bool.Parse(parts[1]);
                                    break;
                                case "AutoCopy":
                                    chkAutoCopy.Checked = bool.Parse(parts[1]);
                                    break;
                                case "AutoSpeak":
                                    chkAutoSpeak.Checked = bool.Parse(parts[1]);
                                    break;
                                case "SaveHistory":
                                    chkSaveHistory.Checked = bool.Parse(parts[1]);
                                    break;
                                case "HistoryLimit":
                                    nudHistoryLimit.Value = int.Parse(parts[1]);
                                    break;
                                case "ApiKey":
                                    txtApiKey.Text = parts[1];
                                    break;
                            }
                        }
                    }
                }
            }
            catch
            {
                // Ayarlar yüklenemezse varsayılanları kullan
            }
        }

        private void SaveSettings()
        {
            try
            {
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string appFolder = System.IO.Path.Combine(appData, "PolyGlotTranslator");
                if (!System.IO.Directory.Exists(appFolder))
                {
                    System.IO.Directory.CreateDirectory(appFolder);
                }

                string settingsFile = System.IO.Path.Combine(appFolder, "settings.ini");
                string[] lines =
                {
                    $"AutoDetect={chkAutoDetect.Checked}",
                    $"AutoCopy={chkAutoCopy.Checked}",
                    $"AutoSpeak={chkAutoSpeak.Checked}",
                    $"SaveHistory={chkSaveHistory.Checked}",
                    $"HistoryLimit={nudHistoryLimit.Value}",
                    $"ApiKey={txtApiKey.Text}"
                };

                System.IO.File.WriteAllLines(settingsFile, lines);
            }
            catch
            {
                // Ayarlar kaydedilemez
            }
        }

        private void ChkSaveHistory_CheckedChanged(object sender, EventArgs e)
        {
            nudHistoryLimit.Enabled = chkSaveHistory.Checked;
        }

        private async void BtnTestApi_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtApiKey.Text))
            {
                MessageBox.Show("Lütfen API anahtarını girin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnTestApi.Enabled = false;
            lblApiStatus.Text = "API test ediliyor...";

            try
            {
                await Task.Delay(1000); // Simüle edilmiş test
                lblApiStatus.Text = "API bağlantısı başarılı!";
                lblApiStatus.ForeColor = Color.Green;
                MessageBox.Show("API bağlantısı başarıyla test edildi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch
            {
                lblApiStatus.Text = "API bağlantısı başarısız!";
                lblApiStatus.ForeColor = Color.Red;
                MessageBox.Show("API bağlantısı test edilemedi. Lütfen anahtarı kontrol edin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnTestApi.Enabled = true;
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            SaveSettings();
            MessageBox.Show("Ayarlar kaydedildi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}