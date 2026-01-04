using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Speech.Synthesis;

namespace PolyGlotTranslator
{
    public partial class Form1 : Form
    {
        // Kontroller
        private ComboBox cmbSourceLang;
        private ComboBox cmbTargetLang;
        private Button btnSwap;
        private TextBox txtSource;
        private TextBox txtResult;
        private Button btnTranslate;
        private Button btnClear;
        private Button btnCopy;
        private Button btnSpeakSource;
        private Button btnSpeakTarget;
        private Button btnHistory;
        private Button btnFavorites;
        private Button btnSettings;
        private Label lblCharCount;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel lblStatus;
        private ProgressBar progressBar;
        private TabControl tabControl;
        private TabPage tabTranslate;
        private TabPage tabHistory;
        private TabPage tabFavorites;
        private ListView lvHistory;
        private ListView lvFavorites;
        private Button btnClearHistory;
        private Button btnDeleteFavorite;
        private Button btnExportHistory;
        private Button btnExportFavorites;
        private Button btnImportHistory;

        // Veri yönetimi
        private TranslationManager translationManager;
        private List<TranslationHistory> translationHistory;
        private List<TranslationFavorite> translationFavorites;
        private SpeechSynthesizer speechSynthesizer;

        // Diller
        private Dictionary<string, string> languages = new Dictionary<string, string>
        {
            {"auto", "Otomatik Tespit"},
            {"tr", "Türkçe"},
            {"en", "Ýngilizce"},
            {"de", "Almanca"},
            {"fr", "Fransýzca"},
            {"es", "Ýspanyolca"},
            {"it", "Ýtalyanca"},
            {"ru", "Rusça"},
            {"ar", "Arapça"},
            {"zh-CN", "Çince (Basit)"},
            {"ja", "Japonca"},
            {"ko", "Korece"},
            {"pt", "Portekizce"},
            {"nl", "Hollandaca"},
            {"pl", "Lehçe"},
            {"sv", "Ýsveççe"},
            {"da", "Danca"},
            {"fi", "Fince"},
            {"no", "Norveççe"},
            {"el", "Yunanca"},
            {"he", "Ýbranice"},
            {"hi", "Hintçe"},
            {"bn", "Bengalce"},
            {"ur", "Urduca"},
            {"fa", "Farsça"},
            {"th", "Tayca"},
            {"vi", "Vietnamca"},
            {"id", "Endonezce"},
            {"ms", "Malayca"}
        };

        public Form1()
        {
            InitializeComponent();
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            // Manager'larý baþlat
            translationManager = new TranslationManager();
            translationHistory = new List<TranslationHistory>();
            translationFavorites = new List<TranslationFavorite>();
            speechSynthesizer = new SpeechSynthesizer();

            // Dilleri ComboBox'lara ekle
            LoadLanguages();

            // Durum çubuðunu ayarla
            lblStatus.Text = "Hazýr - Çevirmek için metni yazýn ve Çevir butonuna týklayýn";

            // Karakter sayacýný güncelle
            UpdateCharacterCount();

            // Geçmiþi ve favorileri yükle
            LoadHistory();
            LoadFavorites();
        }

        private void InitializeComponent()
        {
            // Ana form özellikleri
            this.Text = "PolyGlot Translator - Çoklu Dil Çeviri Uygulamasý";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(800, 600);

            // TabControl oluþtur
            tabControl = new TabControl();
            tabControl.Dock = DockStyle.Fill;
            tabControl.Location = new Point(0, 0);
            tabControl.Size = new Size(984, 641);

            // Tab Sayfalarý
            tabTranslate = new TabPage("Çeviri");
            tabHistory = new TabPage("Geçmiþ");
            tabFavorites = new TabPage("Favoriler");

            // TabControl'e sayfalarý ekle
            tabControl.TabPages.Add(tabTranslate);
            tabControl.TabPages.Add(tabHistory);
            tabControl.TabPages.Add(tabFavorites);

            // --- ÇEVÝRÝ SAYFASI ---
            // Kaynak dil ComboBox
            cmbSourceLang = new ComboBox();
            cmbSourceLang.Location = new Point(20, 20);
            cmbSourceLang.Size = new Size(200, 30);
            cmbSourceLang.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSourceLang.Font = new Font("Segoe UI", 10);

            // Hedef dil ComboBox
            cmbTargetLang = new ComboBox();
            cmbTargetLang.Location = new Point(250, 20);
            cmbTargetLang.Size = new Size(200, 30);
            cmbTargetLang.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTargetLang.Font = new Font("Segoe UI", 10);

            // Dil deðiþtirme butonu
            btnSwap = new Button();
            btnSwap.Location = new Point(460, 20);
            btnSwap.Size = new Size(50, 30);
            btnSwap.Text = "?";
            btnSwap.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            btnSwap.Click += BtnSwap_Click;

            // Kaynak metin kutusu
            txtSource = new TextBox();
            txtSource.Location = new Point(20, 70);
            txtSource.Size = new Size(450, 200);
            txtSource.Multiline = true;
            txtSource.ScrollBars = ScrollBars.Vertical;
            txtSource.Font = new Font("Segoe UI", 11);
            txtSource.TextChanged += TxtSource_TextChanged;

            // Sonuç metin kutusu
            txtResult = new TextBox();
            txtResult.Location = new Point(520, 70);
            txtResult.Size = new Size(450, 200);
            txtResult.Multiline = true;
            txtResult.ScrollBars = ScrollBars.Vertical;
            txtResult.Font = new Font("Segoe UI", 11);
            txtResult.ReadOnly = true;
            txtResult.BackColor = Color.WhiteSmoke;

            // Karakter sayacý
            lblCharCount = new Label();
            lblCharCount.Location = new Point(20, 280);
            lblCharCount.Size = new Size(200, 20);
            lblCharCount.Font = new Font("Segoe UI", 9);
            lblCharCount.Text = "Karakter: 0/5000";

            // Çevir butonu
            btnTranslate = new Button();
            btnTranslate.Location = new Point(20, 320);
            btnTranslate.Size = new Size(120, 40);
            btnTranslate.Text = "ÇEVÝR";
            btnTranslate.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnTranslate.BackColor = Color.DodgerBlue;
            btnTranslate.ForeColor = Color.White;
            btnTranslate.Click += BtnTranslate_Click;

            // Temizle butonu
            btnClear = new Button();
            btnClear.Location = new Point(150, 320);
            btnClear.Size = new Size(120, 40);
            btnClear.Text = "TEMÝZLE";
            btnClear.Font = new Font("Segoe UI", 10);
            btnClear.Click += BtnClear_Click;

            // Kopyala butonu
            btnCopy = new Button();
            btnCopy.Location = new Point(280, 320);
            btnCopy.Size = new Size(120, 40);
            btnCopy.Text = "SONUCU KOPYALA";
            btnCopy.Font = new Font("Segoe UI", 10);
            btnCopy.Click += BtnCopy_Click;

            // Kaynak dilde konuþ butonu
            btnSpeakSource = new Button();
            btnSpeakSource.Location = new Point(410, 280);
            btnSpeakSource.Size = new Size(60, 30);
            btnSpeakSource.Text = "??";
            btnSpeakSource.Font = new Font("Segoe UI", 12);
            btnSpeakSource.Click += BtnSpeakSource_Click;

            // Hedef dilde konuþ butonu
            btnSpeakTarget = new Button();
            btnSpeakTarget.Location = new Point(910, 280);
            btnSpeakTarget.Size = new Size(60, 30);
            btnSpeakTarget.Text = "??";
            btnSpeakTarget.Font = new Font("Segoe UI", 12);
            btnSpeakTarget.Click += BtnSpeakTarget_Click;

            // Geçmiþ butonu
            btnHistory = new Button();
            btnHistory.Location = new Point(520, 320);
            btnHistory.Size = new Size(120, 40);
            btnHistory.Text = "GEÇMÝÞ";
            btnHistory.Font = new Font("Segoe UI", 10);
            btnHistory.Click += BtnHistory_Click;

            // Favoriler butonu
            btnFavorites = new Button();
            btnFavorites.Location = new Point(650, 320);
            btnFavorites.Size = new Size(120, 40);
            btnFavorites.Text = "FAVORÝLER";
            btnFavorites.Font = new Font("Segoe UI", 10);
            btnFavorites.Click += BtnFavorites_Click;

            // Ayarlar butonu
            btnSettings = new Button();
            btnSettings.Location = new Point(780, 320);
            btnSettings.Size = new Size(120, 40);
            btnSettings.Text = "AYARLAR";
            btnSettings.Font = new Font("Segoe UI", 10);
            btnSettings.Click += BtnSettings_Click;

            // Progress bar
            progressBar = new ProgressBar();
            progressBar.Location = new Point(20, 380);
            progressBar.Size = new Size(450, 20);
            progressBar.Visible = false;

            // TabTranslate'e kontrolleri ekle
            tabTranslate.Controls.AddRange(new Control[]
            {
                cmbSourceLang, cmbTargetLang, btnSwap,
                txtSource, txtResult, lblCharCount,
                btnTranslate, btnClear, btnCopy,
                btnSpeakSource, btnSpeakTarget,
                btnHistory, btnFavorites, btnSettings,
                progressBar
            });

            // --- GEÇMÝÞ SAYFASI ---
            lvHistory = new ListView();
            lvHistory.Dock = DockStyle.Fill;
            lvHistory.View = View.Details;
            lvHistory.FullRowSelect = true;
            lvHistory.GridLines = true;
            lvHistory.Columns.Add("Tarih", 150);
            lvHistory.Columns.Add("Kaynak Dil", 100);
            lvHistory.Columns.Add("Hedef Dil", 100);
            lvHistory.Columns.Add("Kaynak Metin", 300);
            lvHistory.Columns.Add("Çeviri", 300);
            lvHistory.DoubleClick += LvHistory_DoubleClick;

            btnClearHistory = new Button();
            btnClearHistory.Text = "GEÇMÝÞÝ TEMÝZLE";
            btnClearHistory.Location = new Point(20, 20);
            btnClearHistory.Size = new Size(150, 35);
            btnClearHistory.Click += BtnClearHistory_Click;

            btnExportHistory = new Button();
            btnExportHistory.Text = "GEÇMÝÞÝ DIÞA AKTAR";
            btnExportHistory.Location = new Point(180, 20);
            btnExportHistory.Size = new Size(150, 35);
            btnExportHistory.Click += BtnExportHistory_Click;

            btnImportHistory = new Button();
            btnImportHistory.Text = "GEÇMÝÞÝ ÝÇE AKTAR";
            btnImportHistory.Location = new Point(340, 20);
            btnImportHistory.Size = new Size(150, 35);
            btnImportHistory.Click += BtnImportHistory_Click;

            Panel historyPanel = new Panel();
            historyPanel.Dock = DockStyle.Top;
            historyPanel.Height = 70;
            historyPanel.Controls.AddRange(new Control[] { btnClearHistory, btnExportHistory, btnImportHistory });

            Panel historyContainer = new Panel();
            historyContainer.Dock = DockStyle.Fill;
            historyContainer.Controls.Add(lvHistory);
            historyContainer.Controls.Add(historyPanel);
            historyContainer.Controls.SetChildIndex(historyPanel, 0);

            tabHistory.Controls.Add(historyContainer);

            // --- FAVORÝLER SAYFASI ---
            lvFavorites = new ListView();
            lvFavorites.Dock = DockStyle.Fill;
            lvFavorites.View = View.Details;
            lvFavorites.FullRowSelect = true;
            lvFavorites.GridLines = true;
            lvFavorites.Columns.Add("Tarih", 150);
            lvFavorites.Columns.Add("Kaynak Dil", 100);
            lvFavorites.Columns.Add("Hedef Dil", 100);
            lvFavorites.Columns.Add("Kaynak Metin", 300);
            lvFavorites.Columns.Add("Çeviri", 300);
            lvFavorites.DoubleClick += LvFavorites_DoubleClick;

            btnDeleteFavorite = new Button();
            btnDeleteFavorite.Text = "SEÇÝLENÝ SÝL";
            btnDeleteFavorite.Location = new Point(20, 20);
            btnDeleteFavorite.Size = new Size(150, 35);
            btnDeleteFavorite.Click += BtnDeleteFavorite_Click;

            btnExportFavorites = new Button();
            btnExportFavorites.Text = "FAVORÝLERÝ DIÞA AKTAR";
            btnExportFavorites.Location = new Point(180, 20);
            btnExportFavorites.Size = new Size(150, 35);
            btnExportFavorites.Click += BtnExportFavorites_Click;

            Panel favoritesPanel = new Panel();
            favoritesPanel.Dock = DockStyle.Top;
            favoritesPanel.Height = 70;
            favoritesPanel.Controls.AddRange(new Control[] { btnDeleteFavorite, btnExportFavorites });

            Panel favoritesContainer = new Panel();
            favoritesContainer.Dock = DockStyle.Fill;
            favoritesContainer.Controls.Add(lvFavorites);
            favoritesContainer.Controls.Add(favoritesPanel);
            favoritesContainer.Controls.SetChildIndex(favoritesPanel, 0);

            tabFavorites.Controls.Add(favoritesContainer);

            // --- STATUS STRIP ---
            statusStrip = new StatusStrip();
            lblStatus = new ToolStripStatusLabel();
            statusStrip.Items.Add(lblStatus);

            // Form'a kontrolleri ekle
            this.Controls.Add(tabControl);
            this.Controls.Add(statusStrip);
        }

        private void LoadLanguages()
        {
            cmbSourceLang.Items.Clear();
            cmbTargetLang.Items.Clear();

            foreach (var lang in languages)
            {
                cmbSourceLang.Items.Add($"{lang.Value} ({lang.Key})");
                cmbTargetLang.Items.Add($"{lang.Value} ({lang.Key})");
            }

            // Varsayýlan dilleri ayarla
            cmbSourceLang.SelectedIndex = cmbSourceLang.FindString("Türkçe (tr)");
            cmbTargetLang.SelectedIndex = cmbTargetLang.FindString("Ýngilizce (en)");
        }

        private void UpdateCharacterCount()
        {
            int count = txtSource.Text.Length;
            lblCharCount.Text = $"Karakter: {count}/5000";
            lblCharCount.ForeColor = count > 5000 ? Color.Red : Color.Black;
        }

        private void LoadHistory()
        {
            lvHistory.Items.Clear();
            foreach (var item in translationHistory)
            {
                ListViewItem lvi = new ListViewItem(item.Timestamp.ToString("yyyy-MM-dd HH:mm"));
                lvi.SubItems.Add(item.SourceLang);
                lvi.SubItems.Add(item.TargetLang);
                lvi.SubItems.Add(TruncateText(item.SourceText, 50));
                lvi.SubItems.Add(TruncateText(item.TranslatedText, 50));
                lvi.Tag = item;
                lvHistory.Items.Add(lvi);
            }
        }

        private void LoadFavorites()
        {
            lvFavorites.Items.Clear();
            foreach (var item in translationFavorites)
            {
                ListViewItem lvi = new ListViewItem(item.Timestamp.ToString("yyyy-MM-dd HH:mm"));
                lvi.SubItems.Add(item.SourceLang);
                lvi.SubItems.Add(item.TargetLang);
                lvi.SubItems.Add(TruncateText(item.SourceText, 50));
                lvi.SubItems.Add(TruncateText(item.TranslatedText, 50));
                lvi.Tag = item;
                lvFavorites.Items.Add(lvi);
            }
        }

        private string TruncateText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return text.Length <= maxLength ? text : text.Substring(0, maxLength) + "...";
        }

        private string GetLangCodeFromComboItem(string item)
        {
            if (string.IsNullOrEmpty(item)) return "auto";
            int start = item.LastIndexOf('(') + 1;
            int end = item.LastIndexOf(')');
            if (start > 0 && end > start)
            {
                return item.Substring(start, end - start);
            }
            return "auto";
        }

        // --- OLAY ÝÞLEYÝCÝLER ---
        private void TxtSource_TextChanged(object sender, EventArgs e)
        {
            UpdateCharacterCount();
        }

        private async void BtnTranslate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSource.Text))
            {
                MessageBox.Show("Lütfen çevrilecek metni girin.", "Uyarý", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (txtSource.Text.Length > 5000)
            {
                MessageBox.Show("Metin çok uzun! Maksimum 5000 karakter.", "Uyarý", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string sourceLang = GetLangCodeFromComboItem(cmbSourceLang.SelectedItem?.ToString());
            string targetLang = GetLangCodeFromComboItem(cmbTargetLang.SelectedItem?.ToString());

            if (sourceLang == targetLang)
            {
                MessageBox.Show("Kaynak ve hedef diller ayný olamaz!", "Uyarý", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                btnTranslate.Enabled = false;
                progressBar.Visible = true;
                progressBar.Style = ProgressBarStyle.Marquee;
                lblStatus.Text = "Çeviri yapýlýyor...";

                string translatedText = await translationManager.TranslateTextAsync(
                    txtSource.Text, sourceLang, targetLang);

                txtResult.Text = translatedText;

                // Geçmiþe ekle
                var historyItem = new TranslationHistory
                {
                    SourceText = txtSource.Text,
                    TranslatedText = translatedText,
                    SourceLang = languages.ContainsKey(sourceLang) ? languages[sourceLang] : sourceLang,
                    TargetLang = languages.ContainsKey(targetLang) ? languages[targetLang] : targetLang,
                    Timestamp = DateTime.Now
                };

                translationHistory.Insert(0, historyItem);
                if (translationHistory.Count > 100)
                {
                    translationHistory.RemoveAt(translationHistory.Count - 1);
                }

                LoadHistory();
                SaveHistoryToFile();

                lblStatus.Text = "Çeviri tamamlandý!";
                MessageBox.Show("Çeviri baþarýyla tamamlandý.", "Baþarýlý", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Çeviri hatasý: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Çeviri hatasý!";
            }
            finally
            {
                btnTranslate.Enabled = true;
                progressBar.Visible = false;
            }
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            txtSource.Clear();
            txtResult.Clear();
            UpdateCharacterCount();
        }

        private void BtnCopy_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtResult.Text))
            {
                Clipboard.SetText(txtResult.Text);
                lblStatus.Text = "Çeviri panoya kopyalandý!";
                MessageBox.Show("Çeviri panoya kopyalandý.", "Baþarýlý", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnSwap_Click(object sender, EventArgs e)
        {
            string temp = cmbSourceLang.SelectedItem?.ToString();
            cmbSourceLang.SelectedItem = cmbTargetLang.SelectedItem;
            cmbTargetLang.SelectedItem = temp;

            if (!string.IsNullOrEmpty(txtSource.Text) && !string.IsNullOrEmpty(txtResult.Text))
            {
                string tempText = txtSource.Text;
                txtSource.Text = txtResult.Text;
                txtResult.Text = tempText;
            }
        }

        private void BtnSpeakSource_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtSource.Text))
            {
                try
                {
                    speechSynthesizer.SpeakAsyncCancelAll();
                    speechSynthesizer.SpeakAsync(txtSource.Text);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Konuþma hatasý: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnSpeakTarget_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtResult.Text))
            {
                try
                {
                    speechSynthesizer.SpeakAsyncCancelAll();
                    speechSynthesizer.SpeakAsync(txtResult.Text);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Konuþma hatasý: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnHistory_Click(object sender, EventArgs e)
        {
            tabControl.SelectedTab = tabHistory;
        }

        private void BtnFavorites_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtSource.Text) && !string.IsNullOrEmpty(txtResult.Text))
            {
                var favorite = new TranslationFavorite
                {
                    SourceText = txtSource.Text,
                    TranslatedText = txtResult.Text,
                    SourceLang = languages.ContainsKey(GetLangCodeFromComboItem(cmbSourceLang.SelectedItem?.ToString())) ?
                                languages[GetLangCodeFromComboItem(cmbSourceLang.SelectedItem?.ToString())] :
                                GetLangCodeFromComboItem(cmbSourceLang.SelectedItem?.ToString()),
                    TargetLang = languages.ContainsKey(GetLangCodeFromComboItem(cmbTargetLang.SelectedItem?.ToString())) ?
                                languages[GetLangCodeFromComboItem(cmbTargetLang.SelectedItem?.ToString())] :
                                GetLangCodeFromComboItem(cmbTargetLang.SelectedItem?.ToString()),
                    Timestamp = DateTime.Now
                };

                translationFavorites.Insert(0, favorite);
                LoadFavorites();
                SaveFavoritesToFile();

                MessageBox.Show("Çeviri favorilere eklendi!", "Baþarýlý", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnSettings_Click(object sender, EventArgs e)
        {
            SettingsForm settingsForm = new SettingsForm();
            settingsForm.ShowDialog();
        }

        private void LvHistory_DoubleClick(object sender, EventArgs e)
        {
            if (lvHistory.SelectedItems.Count > 0)
            {
                var item = (TranslationHistory)lvHistory.SelectedItems[0].Tag;
                txtSource.Text = item.SourceText;
                txtResult.Text = item.TranslatedText;
                tabControl.SelectedTab = tabTranslate;
                UpdateCharacterCount();
            }
        }

        private void LvFavorites_DoubleClick(object sender, EventArgs e)
        {
            if (lvFavorites.SelectedItems.Count > 0)
            {
                var item = (TranslationFavorite)lvFavorites.SelectedItems[0].Tag;
                txtSource.Text = item.SourceText;
                txtResult.Text = item.TranslatedText;
                tabControl.SelectedTab = tabTranslate;
                UpdateCharacterCount();
            }
        }

        private void BtnClearHistory_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Tüm geçmiþi silmek istediðinizden emin misiniz?", "Onay",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                translationHistory.Clear();
                LoadHistory();
                SaveHistoryToFile();
            }
        }

        private void BtnDeleteFavorite_Click(object sender, EventArgs e)
        {
            if (lvFavorites.SelectedItems.Count > 0)
            {
                var item = (TranslationFavorite)lvFavorites.SelectedItems[0].Tag;
                translationFavorites.Remove(item);
                LoadFavorites();
                SaveFavoritesToFile();
            }
        }

        private void BtnExportHistory_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "CSV Dosyalarý (*.csv)|*.csv|Tüm Dosyalar (*.*)|*.*";
            saveDialog.FileName = "ceviri_gecmisi_" + DateTime.Now.ToString("yyyyMMdd") + ".csv";

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (StreamWriter writer = new StreamWriter(saveDialog.FileName, false, Encoding.UTF8))
                    {
                        writer.WriteLine("Tarih,Kaynak Dil,Hedef Dil,Kaynak Metin,Çeviri");
                        foreach (var item in translationHistory)
                        {
                            writer.WriteLine($"\"{item.Timestamp}\",\"{item.SourceLang}\",\"{item.TargetLang}\",\"{item.SourceText.Replace("\"", "\"\"")}\",\"{item.TranslatedText.Replace("\"", "\"\"")}\"");
                        }
                    }
                    MessageBox.Show("Geçmiþ baþarýyla dýþa aktarýldý.", "Baþarýlý", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Dýþa aktarma hatasý: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnExportFavorites_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "CSV Dosyalarý (*.csv)|*.csv|Tüm Dosyalar (*.*)|*.*";
            saveDialog.FileName = "favori_ceviriler_" + DateTime.Now.ToString("yyyyMMdd") + ".csv";

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (StreamWriter writer = new StreamWriter(saveDialog.FileName, false, Encoding.UTF8))
                    {
                        writer.WriteLine("Tarih,Kaynak Dil,Hedef Dil,Kaynak Metin,Çeviri");
                        foreach (var item in translationFavorites)
                        {
                            writer.WriteLine($"\"{item.Timestamp}\",\"{item.SourceLang}\",\"{item.TargetLang}\",\"{item.SourceText.Replace("\"", "\"\"")}\",\"{item.TranslatedText.Replace("\"", "\"\"")}\"");
                        }
                    }
                    MessageBox.Show("Favoriler baþarýyla dýþa aktarýldý.", "Baþarýlý", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Dýþa aktarma hatasý: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnImportHistory_Click(object sender, EventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "CSV Dosyalarý (*.csv)|*.csv|Tüm Dosyalar (*.*)|*.*";

            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (StreamReader reader = new StreamReader(openDialog.FileName, Encoding.UTF8))
                    {
                        string header = reader.ReadLine(); // Baþlýk satýrýný atla
                        while (!reader.EndOfStream)
                        {
                            string line = reader.ReadLine();
                            var parts = ParseCSVLine(line);

                            if (parts.Length >= 5)
                            {
                                translationHistory.Add(new TranslationHistory
                                {
                                    Timestamp = DateTime.Parse(parts[0]),
                                    SourceLang = parts[1],
                                    TargetLang = parts[2],
                                    SourceText = parts[3],
                                    TranslatedText = parts[4]
                                });
                            }
                        }
                    }
                    LoadHistory();
                    SaveHistoryToFile();
                    MessageBox.Show("Geçmiþ baþarýyla içe aktarýldý.", "Baþarýlý", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ýçe aktarma hatasý: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private string[] ParseCSVLine(string line)
        {
            List<string> result = new List<string>();
            bool inQuotes = false;
            string current = "";

            foreach (char c in line)
            {
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(current);
                    current = "";
                }
                else
                {
                    current += c;
                }
            }
            result.Add(current);
            return result.ToArray();
        }

        private void SaveHistoryToFile()
        {
            try
            {
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string appFolder = Path.Combine(appData, "PolyGlotTranslator");
                if (!Directory.Exists(appFolder))
                {
                    Directory.CreateDirectory(appFolder);
                }

                string historyFile = Path.Combine(appFolder, "history.dat");
                using (StreamWriter writer = new StreamWriter(historyFile, false, Encoding.UTF8))
                {
                    foreach (var item in translationHistory)
                    {
                        writer.WriteLine($"{item.Timestamp:o}|{item.SourceLang}|{item.TargetLang}|{item.SourceText}|{item.TranslatedText}");
                    }
                }
            }
            catch
            {
                // Dosya kaydetme hatasý görmezden gel
            }
        }

        private void SaveFavoritesToFile()
        {
            try
            {
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string appFolder = Path.Combine(appData, "PolyGlotTranslator");
                if (!Directory.Exists(appFolder))
                {
                    Directory.CreateDirectory(appFolder);
                }

                string favoritesFile = Path.Combine(appFolder, "favorites.dat");
                using (StreamWriter writer = new StreamWriter(favoritesFile, false, Encoding.UTF8))
                {
                    foreach (var item in translationFavorites)
                    {
                        writer.WriteLine($"{item.Timestamp:o}|{item.SourceLang}|{item.TargetLang}|{item.SourceText}|{item.TranslatedText}");
                    }
                }
            }
            catch
            {
                // Dosya kaydetme hatasý görmezden gel
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            speechSynthesizer.Dispose();
        }
    }

    public class TranslationHistory
    {
        public string SourceText { get; set; }
        public string TranslatedText { get; set; }
        public string SourceLang { get; set; }
        public string TargetLang { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class TranslationFavorite
    {
        public string SourceText { get; set; }
        public string TranslatedText { get; set; }
        public string SourceLang { get; set; }
        public string TargetLang { get; set; }
        public DateTime Timestamp { get; set; }
    }
}