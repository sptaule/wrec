using MaterialSkin.Controls;
using System.Drawing;
using System.Windows.Forms;

namespace wrec.UI.Tabs
{
    public class MouseTab : TabPage
    {
        public MaterialCheckbox ChkShowCursor { get; private set; }
        public MaterialCheckbox ChkShowMouseClicks { get; private set; }
        public Panel LeftColorSquare { get; private set; }
        public Panel RightColorSquare { get; private set; }
        public MaterialTextBox TxtClickRadius { get; private set; }
        public MaterialTextBox TxtClickDuration { get; private set; }
        public MaterialComboBox CmbClickDetectionMode { get; private set; }

        private Color _leftClickColor = Color.Yellow;
        private Color _rightClickColor = Color.Orange;

        public Color LeftClickColor
        {
            get => _leftClickColor;
            set => _leftClickColor = value;
        }

        public Color RightClickColor
        {
            get => _rightClickColor;
            set => _rightClickColor = value;
        }

        public MouseTab()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Text = "Paramètres Souris";

            int margin = 20;
            int controlWidth = 220;
            int labelWidth = 180;
            int yPos = 30;

            // Curseur
            ChkShowCursor = new MaterialCheckbox
            {
                Text = "Afficher le curseur de souris",
                Location = new Point(margin - 10, yPos),
                Size = new Size(400, 30),
                Checked = true
            };
            this.Controls.Add(ChkShowCursor);
            yPos += 40;

            // Clics souris
            ChkShowMouseClicks = new MaterialCheckbox
            {
                Text = "Afficher les clics de souris",
                Location = new Point(margin - 10, yPos),
                Size = new Size(400, 30),
                Checked = false
            };
            this.Controls.Add(ChkShowMouseClicks);
            yPos += 60;

            // Couleur clic gauche
            var lblLeftClick = new MaterialLabel
            {
                Text = "Couleur clic gauche",
                Location = new Point(margin, yPos + 5),
                Size = new Size(labelWidth, 20)
            };
            this.Controls.Add(lblLeftClick);

            LeftColorSquare = new Panel
            {
                Location = new Point(margin + labelWidth, yPos - 4),
                Size = new Size(36, 36),
                BackColor = LeftClickColor,
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(LeftColorSquare);

            var btnLeftClick = new MaterialButton
            {
                Text = "Choisir",
                Location = new Point(margin + labelWidth + 40, yPos - 4),
                Size = new Size(controlWidth - 10, 36),
                Type = MaterialButton.MaterialButtonType.Outlined
            };
            btnLeftClick.Click += (s, e) =>
            {
                if (ShowColorDialog(ref _leftClickColor))
                {
                    LeftColorSquare.BackColor = _leftClickColor;
                }
            };
            this.Controls.Add(btnLeftClick);
            yPos += 40;

            // Couleur clic droit
            var lblRightClick = new MaterialLabel
            {
                Text = "Couleur clic droit",
                Location = new Point(margin, yPos + 5),
                Size = new Size(labelWidth, 20)
            };
            this.Controls.Add(lblRightClick);

            RightColorSquare = new Panel
            {
                Location = new Point(margin + labelWidth, yPos - 4),
                Size = new Size(36, 36),
                BackColor = RightClickColor,
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(RightColorSquare);

            var btnRightClick = new MaterialButton
            {
                Text = "Choisir",
                Location = new Point(margin + labelWidth + 40, yPos - 4),
                Size = new Size(controlWidth - 10, 36),
                Type = MaterialButton.MaterialButtonType.Outlined
            };
            btnRightClick.Click += (s, e) =>
            {
                if (ShowColorDialog(ref _rightClickColor))
                {
                    RightColorSquare.BackColor = _rightClickColor;
                }
            };
            this.Controls.Add(btnRightClick);
            yPos += 55;

            // Rayon du cercle
            var lblRadius = new MaterialLabel
            {
                Text = "Rayon du cercle",
                Location = new Point(margin, yPos + 10),
                Size = new Size(labelWidth, 20)
            };
            this.Controls.Add(lblRadius);

            TxtClickRadius = new MaterialTextBox
            {
                Hint = "pixels",
                Text = "30",
                Location = new Point(margin + labelWidth, yPos - 4),
                Size = new Size(controlWidth, 48)
            };
            this.Controls.Add(TxtClickRadius);
            yPos += 55;

            // Durée d'affichage
            var lblDuration = new MaterialLabel
            {
                Text = "Durée d'affichage",
                Location = new Point(margin, yPos + 10),
                Size = new Size(labelWidth, 20)
            };
            this.Controls.Add(lblDuration);

            TxtClickDuration = new MaterialTextBox
            {
                Hint = "millisecondes",
                Text = "100",
                Location = new Point(margin + labelWidth, yPos - 4),
                Size = new Size(controlWidth, 48)
            };
            this.Controls.Add(TxtClickDuration);
            yPos += 55;

            // Mode de détection
            var lblDetectionMode = new MaterialLabel
            {
                Text = "Mode de détection",
                Location = new Point(margin, yPos + 10),
                Size = new Size(labelWidth, 20)
            };
            this.Controls.Add(lblDetectionMode);

            CmbClickDetectionMode = new MaterialComboBox
            {
                Location = new Point(margin + labelWidth, yPos - 4),
                Size = new Size(controlWidth, 48),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            CmbClickDetectionMode.Items.AddRange(new object[] { "Hook (précis)", "Polling (performant)" });
            CmbClickDetectionMode.SelectedIndex = 0;
            this.Controls.Add(CmbClickDetectionMode);
        }

        private bool ShowColorDialog(ref Color color)
        {
            using (var colorDialog = new ColorDialog())
            {
                colorDialog.Color = color;
                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    color = colorDialog.Color;
                    return true;
                }
            }
            return false;
        }
    }
}