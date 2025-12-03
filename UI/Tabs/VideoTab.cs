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
        // Contrôles UI principaux
        public ComboBox CmbQualityPreset { get; private set; }
        public ComboBox CmbResolution { get; private set; }
        public MaterialCheckbox ChkHardwareEncoding { get; private set; }

        // Contrôles avancés
        public MaterialTextBox TxtWidth { get; private set; }
        public MaterialTextBox TxtHeight { get; private set; }
        public MaterialTextBox TxtVideoBitrate { get; private set; }
        public MaterialTextBox TxtFramerate { get; private set; }
        public MaterialCheckbox ChkFixedFramerate { get; private set; }
        public ComboBox CmbQuality { get; private set; }
        public ComboBox CmbEncoderProfile { get; private set; }
        public ComboBox CmbBitrateMode { get; private set; }

        // Panneau avancé
        private Panel advancedPanel;

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
            this.AutoScroll = true;

            // Configuration des marges et tailles
            const int margin = 20;
            const int labelWidth = 180;
            const int controlWidth = 300;
            int yPos = 30;

            // ===== SECTION SIMPLE =====
            var lblSimpleTitle = new MaterialLabel
            {
                Text = "Paramètres Rapides",
                Location = new Point(margin, yPos),
                Size = new Size(400, 30),
                FontType = MaterialSkinManager.fontType.H6
            };
            this.Controls.Add(lblSimpleTitle);
            yPos += 40;

            // Preset de qualité
            var lblQualityPreset = new MaterialLabel
            {
                Text = "Qualité vidéo",
                Location = new Point(margin, yPos + 10),
                Size = new Size(labelWidth, 20),
                FontType = MaterialSkinManager.fontType.Body1
            };
            this.Controls.Add(lblQualityPreset);

            CmbQualityPreset = new MaterialComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(margin + labelWidth, yPos),
                Size = new Size(controlWidth, 28)
            };
            CmbQualityPreset.Items.AddRange(new object[]
            {
                "Basse (Petite taille de fichier)",
                "Moyenne (Équilibrée)",
                "Haute (Qualité supérieure)",
                "Ultra (Meilleure qualité)"
            });
            CmbQualityPreset.SelectedIndex = LoadQualityPreset(); // Charger le preset sauvegardé
            this.Controls.Add(CmbQualityPreset);
            yPos += 55;

            // Résolution
            var lblResolution = new MaterialLabel
            {
                Text = "Résolution",
                Location = new Point(margin, yPos + 10),
                Size = new Size(labelWidth, 20),
                FontType = MaterialSkinManager.fontType.Body1
            };
            this.Controls.Add(lblResolution);

            CmbResolution = new MaterialComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(margin + labelWidth, yPos),
                Size = new Size(controlWidth, 28)
            };
            CmbResolution.Items.AddRange(new object[]
            {
                "720p (1280x720)",
                "1080p (1920x1080)",
                "1440p (2560x1440)",
                "4K (3840x2160)",
                "Plein écran (Résolution actuelle)",
                "Personnalisée..."
            });
            CmbResolution.SelectedIndex = 4; // Plein écran par défaut
            this.Controls.Add(CmbResolution);
            yPos += 55;

            // Encodage matériel
            ChkHardwareEncoding = new MaterialCheckbox
            {
                Text = "Activer l'accélération matérielle (recommandé)",
                Location = new Point(margin, yPos),
                Size = new Size(400, 36),
                Checked = true
            };
            this.Controls.Add(ChkHardwareEncoding);
            yPos += 50;

            // ===== SECTION AVANCÉE =====
            advancedPanel = new Panel
            {
                Location = new Point(0, yPos),
                Size = new Size(this.Width, 450),
                BackColor = Color.FromArgb(250, 250, 250),
                BorderStyle = BorderStyle.None,
                Visible = true,
                AutoScroll = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            this.Controls.Add(advancedPanel);

            int advYPos = 20;

            var lblAdvancedTitle = new MaterialLabel
            {
                Text = "Options Avancées",
                Location = new Point(margin, advYPos),
                Size = new Size(400, 30),
                FontType = MaterialSkinManager.fontType.H6
            };
            advancedPanel.Controls.Add(lblAdvancedTitle);
            advYPos += 40;

            // Résolution personnalisée
            var lblCustomRes = new MaterialLabel
            {
                Text = "Résolution personnalisée",
                Location = new Point(margin, advYPos + 10),
                Size = new Size(labelWidth, 20),
                FontType = MaterialSkinManager.fontType.Body1
            };
            advancedPanel.Controls.Add(lblCustomRes);

            TxtWidth = new MaterialTextBox
            {
                Hint = "Largeur",
                Text = Screen.PrimaryScreen.Bounds.Width.ToString(),
                Location = new Point(margin + labelWidth, advYPos),
                Size = new Size(109, 48),
                MaxLength = 4
            };
            advancedPanel.Controls.Add(TxtWidth);

            TxtHeight = new MaterialTextBox
            {
                Hint = "Hauteur",
                Text = Screen.PrimaryScreen.Bounds.Height.ToString(),
                Location = new Point(margin + labelWidth + 120, advYPos),
                Size = new Size(109, 48),
                MaxLength = 4
            };
            advancedPanel.Controls.Add(TxtHeight);
            advYPos += 60;

            // Framerate
            var lblFramerate = new MaterialLabel
            {
                Text = "Framerate (FPS)",
                Location = new Point(margin, advYPos + 10),
                Size = new Size(labelWidth, 20),
                FontType = MaterialSkinManager.fontType.Body1
            };
            advancedPanel.Controls.Add(lblFramerate);

            TxtFramerate = new MaterialTextBox
            {
                Hint = "FPS",
                Text = "60",
                Location = new Point(margin + labelWidth, advYPos),
                Size = new Size(controlWidth, 48),
                MaxLength = 3
            };
            advancedPanel.Controls.Add(TxtFramerate);
            advYPos += 60;

            // Framerate fixe
            ChkFixedFramerate = new MaterialCheckbox
            {
                Text = "Framerate fixe (CFR)",
                Location = new Point(margin, advYPos),
                Size = new Size(300, 36)
            };
            advancedPanel.Controls.Add(ChkFixedFramerate);
            advYPos += 45;

            // Bitrate vidéo
            var lblBitrate = new MaterialLabel
            {
                Text = "Bitrate vidéo (kbps)",
                Location = new Point(margin, advYPos + 10),
                Size = new Size(labelWidth, 20),
                FontType = MaterialSkinManager.fontType.Body1
            };
            advancedPanel.Controls.Add(lblBitrate);

            TxtVideoBitrate = new MaterialTextBox
            {
                Hint = "kbps",
                Text = "8000",
                Location = new Point(margin + labelWidth, advYPos),
                Size = new Size(controlWidth, 48),
                MaxLength = 6
            };
            advancedPanel.Controls.Add(TxtVideoBitrate);
            advYPos += 60;

            // Qualité CRF
            var lblQuality = new MaterialLabel
            {
                Text = "Qualité (CRF)",
                Location = new Point(margin, advYPos + 10),
                Size = new Size(labelWidth, 20),
                FontType = MaterialSkinManager.fontType.Body1
            };
            advancedPanel.Controls.Add(lblQuality);

            CmbQuality = new MaterialComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(margin + labelWidth, advYPos),
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
            advancedPanel.Controls.Add(CmbQuality);
            advYPos += 55;

            // Profil encodeur
            var lblProfile = new MaterialLabel
            {
                Text = "Profil encodeur H.264",
                Location = new Point(margin, advYPos + 10),
                Size = new Size(labelWidth, 20),
                FontType = MaterialSkinManager.fontType.Body1
            };
            advancedPanel.Controls.Add(lblProfile);

            CmbEncoderProfile = new MaterialComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(margin + labelWidth, advYPos),
                Size = new Size(controlWidth, 28)
            };
            CmbEncoderProfile.Items.AddRange(new object[] { "Baseline", "Main", "High" });
            CmbEncoderProfile.SelectedIndex = 1;
            advancedPanel.Controls.Add(CmbEncoderProfile);
            advYPos += 55;

            // Mode de bitrate
            var lblBitrateMode = new MaterialLabel
            {
                Text = "Mode de bitrate",
                Location = new Point(margin, advYPos + 10),
                Size = new Size(labelWidth, 20),
                FontType = MaterialSkinManager.fontType.Body1
            };
            advancedPanel.Controls.Add(lblBitrateMode);

            CmbBitrateMode = new MaterialComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(margin + labelWidth, advYPos),
                Size = new Size(controlWidth, 28)
            };
            CmbBitrateMode.Items.AddRange(new object[] { "Constant (CBR)", "Qualité (CRF)", "Variable (VBR)" });
            CmbBitrateMode.SelectedIndex = 0;
            advancedPanel.Controls.Add(CmbBitrateMode);

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

            toolTip.SetToolTip(CmbQualityPreset, "Choisissez un preset de qualité adapté à vos besoins");
            toolTip.SetToolTip(CmbResolution, "Sélectionnez la résolution de votre enregistrement");
            toolTip.SetToolTip(ChkHardwareEncoding, "Utilise votre carte graphique pour un encodage plus rapide");
            toolTip.SetToolTip(TxtWidth, "Largeur de la vidéo en pixels (640-7680)");
            toolTip.SetToolTip(TxtHeight, "Hauteur de la vidéo en pixels (480-4320)");
            toolTip.SetToolTip(TxtVideoBitrate, "Qualité vidéo en kilobits par seconde (1000-100000)");
            toolTip.SetToolTip(TxtFramerate, "Images par seconde (1-240)");
        }

        private void SetupEventHandlers()
        {
            // Gestion du changement de preset de qualité
            CmbQualityPreset.SelectedIndexChanged += (sender, e) =>
            {
                ApplyQualityPreset();
                SaveQualityPreset();
            };

            // Gestion du changement de résolution
            CmbResolution.SelectedIndexChanged += (sender, e) => ApplyResolutionPreset();

            // Validation numérique pour tous les champs
            TxtWidth.KeyPress += NumericTextBox_KeyPress;
            TxtHeight.KeyPress += NumericTextBox_KeyPress;
            TxtVideoBitrate.KeyPress += NumericTextBox_KeyPress;
            TxtFramerate.KeyPress += NumericTextBox_KeyPress;

            // Validation spécifique pour la résolution
            TxtWidth.Leave += (sender, e) => ValidateResolution();
            TxtHeight.Leave += (sender, e) => ValidateResolution();
        }



        private void ApplyQualityPreset()
        {
            switch (CmbQualityPreset.SelectedIndex)
            {
                case 0: // Basse
                    TxtVideoBitrate.Text = "3000";
                    TxtFramerate.Text = "30";
                    CmbQuality.SelectedIndex = 1; // Très basse
                    CmbBitrateMode.SelectedIndex = 0; // CBR
                    CmbEncoderProfile.SelectedIndex = 0; // Baseline
                    break;
                case 1: // Moyenne
                    TxtVideoBitrate.Text = "8000";
                    TxtFramerate.Text = "60";
                    CmbQuality.SelectedIndex = 2; // Basse
                    CmbBitrateMode.SelectedIndex = 0; // CBR
                    CmbEncoderProfile.SelectedIndex = 1; // Main
                    break;
                case 2: // Haute
                    TxtVideoBitrate.Text = "15000";
                    TxtFramerate.Text = "60";
                    CmbQuality.SelectedIndex = 4; // Haute
                    CmbBitrateMode.SelectedIndex = 2; // VBR
                    CmbEncoderProfile.SelectedIndex = 2; // High
                    break;
                case 3: // Ultra
                    TxtVideoBitrate.Text = "25000";
                    TxtFramerate.Text = "60";
                    CmbQuality.SelectedIndex = 5; // Très haute
                    CmbBitrateMode.SelectedIndex = 2; // VBR
                    CmbEncoderProfile.SelectedIndex = 2; // High
                    break;
            }
        }

        private void ApplyResolutionPreset()
        {
            switch (CmbResolution.SelectedIndex)
            {
                case 0: // 720p
                    TxtWidth.Text = "1280";
                    TxtHeight.Text = "720";
                    break;
                case 1: // 1080p
                    TxtWidth.Text = "1920";
                    TxtHeight.Text = "1080";
                    break;
                case 2: // 1440p
                    TxtWidth.Text = "2560";
                    TxtHeight.Text = "1440";
                    break;
                case 3: // 4K
                    TxtWidth.Text = "3840";
                    TxtHeight.Text = "2160";
                    break;
                case 4: // Plein écran
                    TxtWidth.Text = Screen.PrimaryScreen.Bounds.Width.ToString();
                    TxtHeight.Text = Screen.PrimaryScreen.Bounds.Height.ToString();
                    break;
                case 5: // Personnalisée
                    // Ne rien faire, l'utilisateur peut modifier manuellement
                    break;
            }
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

            if (selectedIndex == 0) return 25;
            if (selectedIndex == 1) return 40;
            if (selectedIndex == 2) return 50;
            if (selectedIndex == 3) return 65;
            if (selectedIndex == 4) return 75;
            if (selectedIndex == 5) return 85;

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

        // Méthodes pour sauvegarder et charger le preset de qualité
        private void SaveQualityPreset()
        {
            try
            {
                Properties.Settings.Default.VideoQualityPreset = CmbQualityPreset.SelectedIndex;
                Properties.Settings.Default.Save();
            }
            catch (Exception)
            {
                // Ignorer les erreurs de sauvegarde
            }
        }

        private int LoadQualityPreset()
        {
            try
            {
                int savedPreset = Properties.Settings.Default.VideoQualityPreset;
                // Vérifier que le preset est valide (0-3)
                if (savedPreset >= 0 && savedPreset <= 3)
                {
                    return savedPreset;
                }
            }
            catch (Exception)
            {
                // Ignorer les erreurs de chargement
            }
            return 1; // Moyenne par défaut
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
