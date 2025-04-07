using MaterialSkin;
using MaterialSkin.Controls;
using ScreenRecorderLib;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace wrec.UI.Tabs
{
    public class VideoTab : TabPage
    {
        // Contrôles UI
        public MaterialTextBox TxtWidth { get; private set; }
        public MaterialTextBox TxtHeight { get; private set; }
        public MaterialTextBox TxtVideoBitrate { get; private set; }
        public MaterialTextBox TxtFramerate { get; private set; }
        public MaterialCheckbox ChkFixedFramerate { get; private set; }
        public MaterialCheckbox ChkHardwareEncoding { get; private set; }
        public ComboBox CmbQuality { get; private set; }
        public ComboBox CmbEncoderProfile { get; private set; }
        public ComboBox CmbBitrateMode { get; private set; }

        public VideoTab()
        {
            InitializeUI();
            SetupEventHandlers();
        }

        private void InitializeUI()
        {
            this.Text = "Paramètres Vidéo";
            this.BackColor = Color.White;
            this.Padding = new Padding(10);

            // Configuration des marges et tailles
            const int margin = 20;
            const int labelWidth = 180;
            const int controlWidth = 220;
            int yPos = 30;

            // Résolution
            var lblResolution = new MaterialLabel
            {
                Text = "Résolution",
                Location = new Point(margin, yPos + 10),
                Size = new Size(labelWidth, 20),
                FontType = MaterialSkinManager.fontType.Body1
            };
            this.Controls.Add(lblResolution);

            // Largeur
            TxtWidth = new MaterialTextBox
            {
                Hint = "Largeur",
                Text = Screen.PrimaryScreen.Bounds.Width.ToString(),
                Location = new Point(margin + labelWidth, yPos),
                Size = new Size(109, 48),
                MaxLength = 4
            };
            this.Controls.Add(TxtWidth);

            // Hauteur
            TxtHeight = new MaterialTextBox
            {
                Hint = "Hauteur",
                Text = Screen.PrimaryScreen.Bounds.Height.ToString(),
                Location = new Point(margin + labelWidth + 110, yPos),
                Size = new Size(109, 48),
                MaxLength = 4
            };
            this.Controls.Add(TxtHeight);
            yPos += 55;

            // Bitrate vidéo
            var lblBitrate = new MaterialLabel
            {
                Text = "Bitrate vidéo",
                Location = new Point(margin, yPos + 10),
                Size = new Size(labelWidth, 20),
                FontType = MaterialSkinManager.fontType.Body1
            };
            this.Controls.Add(lblBitrate);

            TxtVideoBitrate = new MaterialTextBox
            {
                Hint = "kbps",
                Text = "8000",
                Location = new Point(margin + labelWidth, yPos),
                Size = new Size(controlWidth, 48),
                MaxLength = 6
            };
            this.Controls.Add(TxtVideoBitrate);
            yPos += 55;

            // Framerate
            var lblFramerate = new MaterialLabel
            {
                Text = "Framerate",
                Location = new Point(margin, yPos + 10),
                Size = new Size(labelWidth, 20),
                FontType = MaterialSkinManager.fontType.Body1
            };
            this.Controls.Add(lblFramerate);

            TxtFramerate = new MaterialTextBox
            {
                Hint = "FPS",
                Text = "60",
                Location = new Point(margin + labelWidth, yPos),
                Size = new Size(controlWidth, 48),
                MaxLength = 3
            };
            this.Controls.Add(TxtFramerate);

            yPos += 55;

            // Framerate fixe
            ChkFixedFramerate = new MaterialCheckbox
            {
                Text = "Framerate fixe",
                Location = new Point(margin + controlWidth + 220, yPos - 50),
                Size = new Size(150 , 36)
            };
            this.Controls.Add(ChkFixedFramerate);

            // Encodage matériel
            ChkHardwareEncoding = new MaterialCheckbox
            {
                Text = "Encodage matériel",
                Location = new Point(margin - 5, yPos),
                Size = new Size(controlWidth, 36)
            };
            this.Controls.Add(ChkHardwareEncoding);
            yPos += 50;

            // Qualité
            var lblQuality = new MaterialLabel
            {
                Text = "Qualité (CRF)",
                Location = new Point(margin, yPos + 10),
                Size = new Size(labelWidth, 20),
                FontType = MaterialSkinManager.fontType.Body1
            };
            this.Controls.Add(lblQuality);

            CmbQuality = new MaterialComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(margin + labelWidth, yPos),
                Size = new Size(controlWidth, 28)
            };
            CmbQuality.Items.AddRange(new object[]
            {
                "Le plus bas (25)",
                "Très basse (40)",
                "Basse (50)",
                "Moyenne (65)",
                "Haute (75)",
                "Très haute (85)"
            });
            CmbQuality.SelectedIndex = 2;
            this.Controls.Add(CmbQuality);
            yPos += 55;

            // Profil encodeur
            var lblProfile = new MaterialLabel
            {
                Text = "Profil encodeur",
                Location = new Point(margin, yPos + 10),
                Size = new Size(labelWidth, 20),
                FontType = MaterialSkinManager.fontType.Body1
            };
            this.Controls.Add(lblProfile);

            CmbEncoderProfile = new MaterialComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(margin + labelWidth, yPos),
                Size = new Size(controlWidth, 28)
            };
            CmbEncoderProfile.Items.AddRange(new object[] { "Baseline", "Main", "High" });
            CmbEncoderProfile.SelectedIndex = 1;
            this.Controls.Add(CmbEncoderProfile);
            yPos += 55;

            // Mode de bitrate
            var lblBitrateMode = new MaterialLabel
            {
                Text = "Mode de bitrate",
                Location = new Point(margin, yPos + 10),
                Size = new Size(labelWidth, 20),
                FontType = MaterialSkinManager.fontType.Body1
            };
            this.Controls.Add(lblBitrateMode);

            CmbBitrateMode = new MaterialComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(margin + labelWidth, yPos),
                Size = new Size(controlWidth, 28)
            };
            CmbBitrateMode.Items.AddRange(new object[] { "Constant (CBR)", "Qualité (CRF)", "Variable (VBR)" });
            CmbBitrateMode.SelectedIndex = 0;
            this.Controls.Add(CmbBitrateMode);

            // Ajout des tooltips
            InitializeToolTips();
        }

        private void InitializeToolTips()
        {
            var toolTip = new ToolTip
            {
                AutoPopDelay = 5000,
                InitialDelay = 500,
                ReshowDelay = 500,
                ShowAlways = true
            };

            toolTip.SetToolTip(TxtWidth, "Largeur de la vidéo en pixels (640-7680)");
            toolTip.SetToolTip(TxtHeight, "Hauteur de la vidéo en pixels (480-4320)");
            toolTip.SetToolTip(TxtVideoBitrate, "Qualité vidéo en kilobits par seconde (1000-100000)");
            toolTip.SetToolTip(TxtFramerate, "Images par seconde (1-240)");
        }

        private void SetupEventHandlers()
        {
            // Validation numérique pour tous les champs
            TxtWidth.KeyPress += NumericTextBox_KeyPress;
            TxtHeight.KeyPress += NumericTextBox_KeyPress;
            TxtVideoBitrate.KeyPress += NumericTextBox_KeyPress;
            TxtFramerate.KeyPress += NumericTextBox_KeyPress;

            // Validation spécifique pour la résolution
            TxtWidth.Leave += (sender, e) => ValidateResolution();
            TxtHeight.Leave += (sender, e) => ValidateResolution();
        }

        private void NumericTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Autorise uniquement les chiffres et la touche Backspace
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void ValidateResolution()
        {
            if (int.TryParse(TxtWidth.Text, out int width) &&
                int.TryParse(TxtHeight.Text, out int height))
            {
                // Maintien du ratio 16:9 si les deux valeurs sont valides
                const double targetRatio = 16.0 / 9.0;
                double currentRatio = (double)width / height;

                if (Math.Abs(currentRatio - targetRatio) > 0.01)
                {
                    var result = MessageBox.Show(
                        "Le ratio n'est pas standard (16:9). Voulez-vous ajuster automatiquement la hauteur?",
                        "Ratio inhabituel",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        TxtHeight.Text = Math.Round(width / targetRatio).ToString();
                    }
                }
            }
        }

        // Méthode de validation des paramètres vidéo
        public bool ValidateVideoSettings()
        {
            try
            {
                if (!int.TryParse(TxtWidth.Text, out int width) || width < 640 || width > 7680)
                {
                    ShowValidationError("Largeur invalide (640-7680)");
                    return false;
                }

                if (!int.TryParse(TxtHeight.Text, out int height) || height < 480 || height > 4320)
                {
                    ShowValidationError("Hauteur invalide (480-4320)");
                    return false;
                }

                if (!int.TryParse(TxtVideoBitrate.Text, out int bitrate) || bitrate < 1000 || bitrate > 100000)
                {
                    ShowValidationError("Bitrate invalide (1000-100000 kbps)");
                    return false;
                }

                if (!int.TryParse(TxtFramerate.Text, out int framerate) || framerate < 1 || framerate > 240)
                {
                    ShowValidationError("Framerate invalide (1-240 FPS)");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur de validation : {ex.Message}",
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void ShowValidationError(string message)
        {
            MessageBox.Show(message, "Paramètre invalide",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        // Méthode pour obtenir les paramètres vidéo
        public VideoSettings GetVideoSettings()
        {
            return new VideoSettings
            {
                Width = int.Parse(TxtWidth.Text),
                Height = int.Parse(TxtHeight.Text),
                Bitrate = int.Parse(TxtVideoBitrate.Text) * 1000,
                Framerate = int.Parse(TxtFramerate.Text),
                IsFixedFramerate = ChkFixedFramerate.Checked,
                IsHardwareEncodingEnabled = ChkHardwareEncoding.Checked,
                Quality = GetSelectedQuality(),
                EncoderProfile = GetSelectedEncoderProfile(),
                BitrateMode = GetSelectedBitrateMode()
            };
        }

        public int GetSelectedQuality()
        {
            int selectedIndex = CmbQuality.SelectedIndex;

            if (selectedIndex == 0) return 40;
            if (selectedIndex == 1) return 50;
            if (selectedIndex == 2) return 65;
            if (selectedIndex == 3) return 75;
            if (selectedIndex == 4) return 85;

            return 65;
        }

        public H264Profile GetSelectedEncoderProfile()
        {
            int selectedIndex = CmbEncoderProfile.SelectedIndex;
            
            if (selectedIndex == 0) return H264Profile.Baseline;
            if (selectedIndex == 1) return H264Profile.Main;
            if (selectedIndex == 2) return H264Profile.High;

            return H264Profile.Main;
        }

        public H264BitrateControlMode GetSelectedBitrateMode()
        {
            int selectedIndex = CmbBitrateMode.SelectedIndex;

            if (selectedIndex == 0) return H264BitrateControlMode.CBR;
            if (selectedIndex == 1) return H264BitrateControlMode.Quality;
            if (selectedIndex == 2) return H264BitrateControlMode.UnconstrainedVBR;

            return H264BitrateControlMode.CBR;
        }
    }

    public class VideoSettings
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int Bitrate { get; set; } // en bits par seconde
        public int Framerate { get; set; }
        public bool IsFixedFramerate { get; set; }
        public bool IsHardwareEncodingEnabled { get; set; }
        public int Quality { get; set; }
        public H264Profile EncoderProfile { get; set; } // "Baseline", "Main", "High"
        public H264BitrateControlMode BitrateMode { get; set; } // "CBR", "Quality", "VBR"
    }
}