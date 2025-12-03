using MaterialSkin.Controls;
using wrec.Services;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace wrec.UI.Controls
{
    public class CompressionPanel : Panel
    {
        private MaterialLabel _lblTitle;
        private MaterialComboBox _cmbQuality;
        private MaterialProgressBar _progressBar;
        private MaterialButton _btnCompress;
        private MaterialButton _btnCancel;
        private MaterialLabel _lblStatus;
        private MaterialButton _btnClose;
        private MaterialButton _btnOpenFile;
        private MaterialButton _btnOpenFolder;

        private readonly FFmpegService _ffmpegService;
        private string _currentVideoPath;
        private string _compressedFilePath;

        public event EventHandler<string> OnStatusChanged;
        public event EventHandler OnCompressComplete;
        public event EventHandler OnCompressCancel;

        public CompressionPanel()
        {
            _ffmpegService = new FFmpegService();
            InitializeUI();
            RegisterEvents();
        }

        private void InitializeUI()
        {
            this.Size = new Size(700, 130);
            this.BackColor = Color.White;
            this.Visible = false;

            // Titre
            _lblTitle = new MaterialLabel
            {
                Text = "Compression vidéo",
                Location = new Point(10, 8),
                Size = new Size(180, 25),
                FontType = MaterialSkin.MaterialSkinManager.fontType.H6
            };
            this.Controls.Add(_lblTitle);

            // Status
            _lblStatus = new MaterialLabel
            {
                Text = "Prêt à compresser",
                Location = new Point(200, 10),
                Size = new Size(490, 20),
                TextAlign = ContentAlignment.MiddleRight,
                FontType = MaterialSkin.MaterialSkinManager.fontType.Body2
            };
            this.Controls.Add(_lblStatus);

            // Qualité
            _cmbQuality = new MaterialComboBox
            {
                Location = new Point(10, 40),
                Size = new Size(200, 40),
                Items = { "Qualité très basse", "Qualité basse", "Qualité moyenne", "Qualité haute" },
                SelectedIndex = 1
            };
            this.Controls.Add(_cmbQuality);

            // Bouton Ouvrir fichier (icône)
            _btnOpenFile = new MaterialButton
            {
                Text = "📄",
                Location = new Point(220, 45),
                Size = new Size(32, 32),
                Type = MaterialButton.MaterialButtonType.Outlined,
                Font = new Font("Segoe UI Emoji", 12F)
            };
            this.Controls.Add(_btnOpenFile);

            // Bouton Ouvrir dossier (icône)
            _btnOpenFolder = new MaterialButton
            {
                Text = "📁",
                Location = new Point(280, 45),
                Size = new Size(32, 32),
                Type = MaterialButton.MaterialButtonType.Outlined,
                Font = new Font("Segoe UI Emoji", 12F)
            };
            this.Controls.Add(_btnOpenFolder);

            // Bouton Compresser
            _btnCompress = new MaterialButton
            {
                Text = "COMPRESSER",
                Location = new Point(350, 45),
                Size = new Size(130, 32),
                Type = MaterialButton.MaterialButtonType.Contained,
                UseAccentColor = true
            };
            this.Controls.Add(_btnCompress);

            // Bouton Annuler
            _btnCancel = new MaterialButton
            {
                Text = "ANNULER",
                Location = new Point(490, 45),
                Size = new Size(110, 32),
                Type = MaterialButton.MaterialButtonType.Outlined,
                Visible = false
            };
            this.Controls.Add(_btnCancel);

            // Bouton fermeture panel
            _btnClose = new MaterialButton
            {
                Text = "FERMER",
                Location = new Point(595, 45),
                Size = new Size(120, 32),
                Type = MaterialButton.MaterialButtonType.Outlined
            };
            this.Controls.Add(_btnClose);

            // Barre de progression
            _progressBar = new MaterialProgressBar
            {
                Location = new Point(10, 95),
                Size = new Size(680, 8),
                Maximum = 100,
                Value = 0
            };
            this.Controls.Add(_progressBar);
        }

        private void RegisterEvents()
        {
            _btnCompress.Click += async (sender, e) =>
            {
                if (string.IsNullOrEmpty(_currentVideoPath)) return;

                CompressionQuality quality = CompressionQuality.Medium;
                switch (_cmbQuality.SelectedIndex)
                {
                    case 0:
                        quality = CompressionQuality.Lowest;
                        break;
                    case 1:
                        quality = CompressionQuality.Low;
                        break;
                    case 2:
                        quality = CompressionQuality.Medium;
                        break;
                    case 3:
                        quality = CompressionQuality.High;
                        break;
                }

                UpdateUIForCompression(true);
                await _ffmpegService.CompressVideoAsync(_currentVideoPath, quality);
            };

            _btnCancel.Click += (sender, e) =>
            {
                _ffmpegService.CancelCompression();
                UpdateUIForCompression(false);
                _lblStatus.Text = "Compression annulée";
                _progressBar.Value = 0;
                OnCompressCancel?.Invoke(this, EventArgs.Empty);
            };

            _btnOpenFile.Click += (sender, e) =>
            {
                if (string.IsNullOrEmpty(_currentVideoPath) || !File.Exists(_currentVideoPath))
                {
                    MaterialMessageBox.Show("Le fichier vidéo n'existe pas.",
                        "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                try
                {
                    Process.Start(_currentVideoPath);
                }
                catch (Exception ex)
                {
                    MaterialMessageBox.Show($"Erreur lors de l'ouverture du fichier: {ex.Message}",
                        "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            _btnOpenFolder.Click += (sender, e) =>
            {
                if (string.IsNullOrEmpty(_currentVideoPath) || !File.Exists(_currentVideoPath))
                {
                    MaterialMessageBox.Show("Le fichier vidéo n'existe pas.",
                        "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                try
                {
                    Process.Start("explorer.exe", $"/select,\"{_currentVideoPath}\"");
                }
                catch (Exception ex)
                {
                    MaterialMessageBox.Show($"Erreur lors de l'ouverture du dossier: {ex.Message}",
                        "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            _btnClose.Click += (sender, e) =>
            {
                if (_ffmpegService.IsCompressing)
                {
                    var result = MaterialMessageBox.Show("La compression est en cours. Voulez-vous vraiment annuler et fermer ?",
                        "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                    if (result == DialogResult.No) return;

                    _ffmpegService.CancelCompression();
                }

                this.Hide();
                ResetPanel();
                OnCompressCancel?.Invoke(this, EventArgs.Empty);
                _lblStatus.Text = "Prêt à compresser";
            };

            _ffmpegService.OnCompressionStatusChanged += (sender, status) =>
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() => _lblStatus.Text = status));
                }
                else
                {
                    _lblStatus.Text = status;
                }
            };

            _ffmpegService.OnProgressChanged += (sender, progress) =>
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() => _progressBar.Value = (int)progress));
                }
                else
                {
                    _progressBar.Value = (int)progress;
                }
            };

            _ffmpegService.OnCompressionCompleted += (sender, outputPath) =>
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() => CompressionCompleted(outputPath)));
                }
                else
                {
                    CompressionCompleted(outputPath);
                }
            };

            _ffmpegService.OnCompressionFailed += (sender, errorMessage) =>
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() => CompressionFailed(errorMessage)));
                }
                else
                {
                    CompressionFailed(errorMessage);
                }
            };
        }

        private void UpdateProgress(double progressValue)
        {
            _progressBar.Value = (int)progressValue;
            _lblStatus.Text = $"Compression en cours: {progressValue:0.0}%";
        }

        private void CompressionCompleted(string outputPath)
        {
            _compressedFilePath = outputPath;
            UpdateUIForCompression(false);
            _lblStatus.Text = "Compression terminée";
            OnStatusChanged?.Invoke(this, "Compression terminée");
            _progressBar.Value = 100;

            this.Invoke((MethodInvoker)delegate
            {
                // Calculate file sizes and percentage
                long originalSize = new FileInfo(_currentVideoPath).Length;
                long compressedSize = new FileInfo(outputPath).Length;
                double originalSizeMB = originalSize / 1024.0 / 1024.0;
                double compressedSizeMB = compressedSize / 1024.0 / 1024.0;
                double percentageChange = ((double)(compressedSize - originalSize) / originalSize) * 100;
                
                string sizeComparison;
                if (percentageChange < 0)
                {
                    sizeComparison = $"Taille originale: {originalSizeMB:F2} MB\n" +
                                   $"Taille compressée: {compressedSizeMB:F2} MB\n" +
                                   $"Gain: {Math.Abs(percentageChange):F1}% ({(originalSizeMB - compressedSizeMB):F2} MB économisés)";
                }
                else
                {
                    sizeComparison = $"Taille originale: {originalSizeMB:F2} MB\n" +
                                   $"Taille compressée: {compressedSizeMB:F2} MB\n" +
                                   $"Perte: +{percentageChange:F1}% ({(compressedSizeMB - originalSizeMB):F2} MB de plus)";
                }

                using (var dialog = new CustomDialog("Compression terminée",
                       $"La vidéo compressée a été enregistrée sous:\n{Path.GetFileName(outputPath)}\n\n{sizeComparison}"))
                {
                    dialog.AddButton("Ouvrir le fichier", () =>
                    {
                        try { Process.Start(outputPath); }
                        catch { /* Gérer l'erreur */ }
                    });

                    dialog.AddButton("Ouvrir le dossier", () =>
                    {
                        try { Process.Start("explorer.exe", $"/select,\"{outputPath}\""); }
                        catch { /* Gérer l'erreur */ }
                    });

                    dialog.AddButton("Supprimer l'original", () =>
                    {
                        try
                        {
                            File.Delete(_currentVideoPath);
                            MaterialMessageBox.Show("La vidéo originale a été supprimée.",
                                "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MaterialMessageBox.Show($"Erreur lors de la suppression: {ex.Message}",
                                "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    });

                    dialog.AddButton("Fermer", DialogResult.Cancel);

                    dialog.ShowDialog(this.FindForm());
                }

                // Don't hide the panel - allow user to compress again with different preset
                // Panel will only close when user clicks "Fermer" button
                _progressBar.Value = 0;
                OnCompressComplete?.Invoke(this, EventArgs.Empty);
            });
        }

        private void ResetPanel()
        {
            _currentVideoPath = null;
            _compressedFilePath = null;
            _progressBar.Value = 0;
            _cmbQuality.SelectedIndex = 1;
            _lblStatus.Text = "Prêt à compresser";
        }

        public class CustomDialog : Form
        {
            private FlowLayoutPanel _buttonsPanel;
            private Label _messageLabel;

            public CustomDialog(string title, string message)
            {
                this.Text = title;
                this.Size = new Size(450, 200);
                this.FormBorderStyle = FormBorderStyle.FixedDialog;
                this.StartPosition = FormStartPosition.CenterParent;
                this.MaximizeBox = false;
                this.MinimizeBox = false;

                _messageLabel = new Label
                {
                    Text = message,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Padding = new Padding(10)
                };

                _buttonsPanel = new FlowLayoutPanel
                {
                    Dock = DockStyle.Bottom,
                    FlowDirection = FlowDirection.RightToLeft,
                    Height = 50,
                    Padding = new Padding(5)
                };

                this.Controls.Add(_messageLabel);
                this.Controls.Add(_buttonsPanel);
            }

            public void AddButton(string text, Action action, bool isPrimary = false)
            {
                var button = new MaterialButton
                {
                    Text = text,
                    Size = new Size(120, 30),
                    Margin = new Padding(5),
                    DialogResult = DialogResult.None
                };

                if (isPrimary)
                {
                    button.BackColor = Color.FromArgb(63, 81, 181);
                    button.ForeColor = Color.White;
                }

                button.Click += (s, e) =>
                {
                    action?.Invoke();
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                };

                _buttonsPanel.Controls.Add(button);
            }

            public void AddButton(string text, DialogResult result, bool isPrimary = false)
            {
                var button = new MaterialButton
                {
                    Text = text,
                    Size = new Size(120, 30),
                    Margin = new Padding(5),
                    DialogResult = result
                };

                if (isPrimary)
                {
                    button.BackColor = Color.FromArgb(63, 81, 181);
                    button.ForeColor = Color.White;
                }

                _buttonsPanel.Controls.Add(button);
            }
        }

        private void CompressionFailed(string errorMessage)
        {
            UpdateUIForCompression(false);
            _lblStatus.Text = "Échec de la compression";
            OnStatusChanged?.Invoke(this, "Échec de la compression: " + errorMessage);
            _progressBar.Value = 0;

            MaterialMessageBox.Show(
                $"Erreur lors de la compression: {errorMessage}",
                "Erreur",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        private void UpdateUIForCompression(bool isCompressing)
        {
            _btnCompress.Enabled = !isCompressing;
            _cmbQuality.Enabled = !isCompressing;
            _btnCancel.Visible = isCompressing;

            string status = isCompressing ? "Compression en cours..." : "Prêt à compresser";
            _lblStatus.Text = status;
            OnStatusChanged?.Invoke(this, status);
        }

        public void ShowForVideo(string videoPath)
        {
            _currentVideoPath = videoPath;
            long fileSize = new FileInfo(videoPath).Length / 1024 / 1024;
            _lblStatus.Text = $"Fichier original: {fileSize} MB - Prêt à compresser";
            _progressBar.Value = 0;
            _btnCompress.Enabled = true;
            _cmbQuality.Enabled = true;
            _btnCancel.Visible = false;
            this.Visible = true;
        }

        public new void Hide()
        {
            this.Visible = false;
            ResetPanel();
            OnCompressCancel?.Invoke(this, EventArgs.Empty);
        }
    }
}
