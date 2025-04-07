using MaterialSkin.Controls;
using ScreenRecorderLib;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace wrec.UI.Tabs
{
    public class AudioTab : TabPage
    {
        public MaterialCheckbox ChkAudioEnabled { get; private set; }
        public MaterialCheckbox ChkMicrophoneEnabled { get; private set; }
        public MaterialComboBox CmbAudioBitrate { get; private set; }
        public MaterialComboBox CmbMicrophones { get; private set; }
        public MaterialTextBox TxtMicrophoneVolume { get; private set; }
        public MaterialTextBox TxtSystemVolume { get; private set; }

        public AudioTab()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Text = "Paramètres Audio";

            int margin = 20;
            int controlWidth = 220;
            int labelWidth = 180;
            int yPos = 30;

            // Audio système
            ChkAudioEnabled = new MaterialCheckbox
            {
                Text = "Enregistrer l'audio système",
                Location = new Point(margin - 10, yPos),
                Size = new Size(400, 30),
                Checked = true
            };
            this.Controls.Add(ChkAudioEnabled);
            yPos += 45;

            // Microphone
            ChkMicrophoneEnabled = new MaterialCheckbox
            {
                Text = "Enregistrer l'audio micro",
                Location = new Point(margin - 10, yPos - 5),
                Size = new Size(400, 30),
                Checked = false
            };
            this.Controls.Add(ChkMicrophoneEnabled);
            yPos += 55;

            // Bitrate audio
            var lblBitrate = new MaterialLabel
            {
                Text = "Bitrate audio",
                Location = new Point(margin, yPos + 10),
                Size = new Size(labelWidth, 20)
            };
            this.Controls.Add(lblBitrate);

            CmbAudioBitrate = new MaterialComboBox
            {
                Location = new Point(margin + labelWidth, yPos - 4),
                Size = new Size(controlWidth, 48),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            CmbAudioBitrate.Items.AddRange(new object[] { "96 kbps", "128 kbps", "192 kbps" });
            CmbAudioBitrate.SelectedIndex = 1;
            this.Controls.Add(CmbAudioBitrate);
            yPos += 55;

            // Microphone
            var lblMicrophone = new MaterialLabel
            {
                Text = "Microphone",
                Location = new Point(margin, yPos + 10),
                Size = new Size(labelWidth, 20)
            };
            this.Controls.Add(lblMicrophone);

            CmbMicrophones = new MaterialComboBox
            {
                Location = new Point(margin + labelWidth, yPos - 4),
                Size = new Size(controlWidth, 48),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Enabled = false
            };
            this.Controls.Add(CmbMicrophones);
            yPos += 55;

            // Volume micro
            var lblMicroVolume = new MaterialLabel
            {
                Text = "Volume micro (%)",
                Location = new Point(margin, yPos + 10),
                Size = new Size(labelWidth, 20)
            };
            this.Controls.Add(lblMicroVolume);

            TxtMicrophoneVolume = new MaterialTextBox
            {
                Text = "50",
                Location = new Point(margin + labelWidth, yPos - 4),
                Size = new Size(controlWidth, 48)
            };
            this.Controls.Add(TxtMicrophoneVolume);
            yPos += 55;

            // Volume système
            var lblSystemVolume = new MaterialLabel
            {
                Text = "Volume système (%)",
                Location = new Point(margin, yPos + 10),
                Size = new Size(labelWidth, 20)
            };
            this.Controls.Add(lblSystemVolume);

            TxtSystemVolume = new MaterialTextBox
            {
                Text = "50",
                Location = new Point(margin + labelWidth, yPos - 4),
                Size = new Size(controlWidth, 48)
            };
            this.Controls.Add(TxtSystemVolume);
        }

        public void LoadMicrophones()
        {
            CmbMicrophones.Items.Clear();
            try
            {
                var inputDevices = Recorder.GetSystemAudioDevices(AudioDeviceSource.InputDevices);
                foreach (var device in inputDevices)
                {
                    CmbMicrophones.Items.Add(device.FriendlyName);
                }

                if (CmbMicrophones.Items.Count > 0)
                {
                    CmbMicrophones.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MaterialMessageBox.Show($"Erreur lors de la récupération des microphones : {ex.Message}",
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}