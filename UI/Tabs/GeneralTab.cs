using MaterialSkin;
using MaterialSkin.Controls;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace wrec.UI.Tabs
{
    public class GeneralTab : TabPage
    {
        // Contrôles UI
        public MaterialTextBox TxtOutputPath { get; private set; }
        public MaterialButton BtnBrowseFolder { get; private set; }
        public MaterialTextBox TxtCountdownDelay { get; private set; }
        public MaterialCheckbox ChkEnableCountdown { get; private set; }

        // Chemin par défaut
        private readonly string _defaultOutputPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "Videos",
            "wrec"
        );

        public GeneralTab()
        {
            InitializeUI();
            SetupEventHandlers();
        }

        private void InitializeUI()
        {
            this.Text = "Paramètres Généraux";
            this.BackColor = Color.White;
            this.Padding = new Padding(10);

            // Configuration des marges et tailles
            const int margin = 20;
            const int labelWidth = 180;
            const int controlWidth = 220;
            int yPos = 30;

            // Label - Dossier de sortie
            var lblOutput = new MaterialLabel
            {
                Text = "Dossier de sortie",
                Location = new Point(margin, yPos + 15),
                Size = new Size(labelWidth, 20),
                FontType = MaterialSkinManager.fontType.Body1
            };
            this.Controls.Add(lblOutput);

            // TextBox - Chemin de sortie
            TxtOutputPath = new MaterialTextBox
            {
                Hint = "Chemin du dossier d'enregistrement",
                Text = _defaultOutputPath,
                Location = new Point(margin + labelWidth, yPos),
                Size = new Size(controlWidth + 180, 48),
                MaxLength = 260 // Longueur max des chemins sous Windows
            };
            this.Controls.Add(TxtOutputPath);

            // Bouton - Parcourir
            BtnBrowseFolder = new MaterialButton
            {
                Text = "Parcourir...",
                Location = new Point(margin + labelWidth + controlWidth + 190, yPos + 6),
                Size = new Size(90, 36),
                Type = MaterialButton.MaterialButtonType.Outlined,
                Cursor = Cursors.Hand
            };
            this.Controls.Add(BtnBrowseFolder);

            // Ajouter un tooltip
            var toolTip = new ToolTip
            {
                AutoPopDelay = 5000,
                InitialDelay = 500,
                ReshowDelay = 500,
                ShowAlways = true
            };
            toolTip.SetToolTip(TxtOutputPath, "Dossier où seront enregistrées les vidéos");
            toolTip.SetToolTip(BtnBrowseFolder, "Sélectionner un dossier de destination");

            yPos += 80;

            // Checkbox - Activer le délai
            ChkEnableCountdown = new MaterialCheckbox
            {
                Text = "Activer le délai avant enregistrement",
                Checked = true,
                Location = new Point(margin, yPos + 2),
                Size = new Size(290, 30)
            };
            this.Controls.Add(ChkEnableCountdown);

            // TextBox - Délai en secondes
            TxtCountdownDelay = new MaterialTextBox
            {
                Hint = "Secondes",
                Text = "3",
                Location = new Point(margin + labelWidth + 155, yPos - 4),
                Size = new Size(120, 48),
                MaxLength = 2
            };
            this.Controls.Add(TxtCountdownDelay);

            // Ajouter un tooltip
            toolTip.SetToolTip(TxtCountdownDelay, "Nombre de secondes avant de démarrer l'enregistrement");
            toolTip.SetToolTip(ChkEnableCountdown, "Activer/désactiver le délai avant l'enregistrement");
        }

        private void SetupEventHandlers()
        {
            BtnBrowseFolder.Click += (sender, e) =>
            {
                using (var folderDialog = new FolderBrowserDialog())
                {
                    folderDialog.Description = "Sélectionnez le dossier de destination";
                    folderDialog.SelectedPath = Directory.Exists(TxtOutputPath.Text)
                        ? TxtOutputPath.Text
                        : _defaultOutputPath;

                    if (folderDialog.ShowDialog() == DialogResult.OK)
                    {
                        TxtOutputPath.Text = folderDialog.SelectedPath;
                    }
                }
            };

            // Validation du chemin lorsqu'on quitte le TextBox
            TxtOutputPath.Leave += (sender, e) =>
            {
                if (!Directory.Exists(TxtOutputPath.Text))
                {
                    try
                    {
                        Directory.CreateDirectory(TxtOutputPath.Text);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Impossible de créer le dossier : {ex.Message}",
                            "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        TxtOutputPath.Text = _defaultOutputPath;
                    }
                }
            };

            ChkEnableCountdown.CheckedChanged += (sender, e) =>
            {
                TxtCountdownDelay.Enabled = ChkEnableCountdown.Checked;
            };

            // Validation numérique pour le délai
            TxtCountdownDelay.KeyPress += (sender, e) =>
            {
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                {
                    e.Handled = true;
                }
            };
        }

        // Méthode pour valider le chemin
        public bool ValidateOutputPath()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(TxtOutputPath.Text))
                {
                    MessageBox.Show("Le chemin de sortie ne peut pas être vide",
                        "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                // Vérifie si le chemin est valide
                var fullPath = Path.GetFullPath(TxtOutputPath.Text);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chemin invalide : {ex.Message}",
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
        }
    }
}