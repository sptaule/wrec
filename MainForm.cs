using MaterialSkin;
using MaterialSkin.Controls;
using wrec.Models;
using wrec.Services;
using wrec.UI.Tabs;
using wrec.Utilities;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ScreenRecorderLib;
using wrec.UI.Controls;
using System.Diagnostics;
using wref.Services;
using System.Runtime.InteropServices;
using System.Reflection;

namespace wrec
{
    public partial class MainForm : MaterialForm
    {
        // Services initialisés directement
        private GlobalHotkey _globalHotkey;
        private readonly RecorderService _recorderService = new RecorderService();
        private readonly FileService _fileService = new FileService();
        private readonly FFmpegService _ffmpegService = new FFmpegService();

        // Configuration
        private AppConfig _config;
        private readonly Icon _normalIcon;
        private readonly Icon _recordingIcon;
        private NotifyIcon _trayIcon;
        private bool _recordingInProgress;
        private System.Windows.Forms.Timer _recordingTimer;
        private DateTime _recordingStartTime;

        // UI Components
        private MaterialButton _btnRecordStop;
        private MaterialLabel _lblStatus;
        private TabControl _tabControl;
        private CompressionPanel _compressionPanel;
        private MaterialLabel _lblRecordingTime;

        // Tabs
        private GeneralTab _generalTab;
        private VideoTab _videoTab;
        private AudioTab _audioTab;
        private MouseTab _mouseTab;

        public MainForm()
        {
            InitializeComponent();

            _recordingTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            _recordingTimer.Tick += RecordingTimer_Tick;

            // Load icons from embedded resources
            var assembly = Assembly.GetExecutingAssembly();
            _normalIcon = new Icon(assembly.GetManifestResourceStream("wrec.app-icn.ico"));
            _recordingIcon = new Icon(assembly.GetManifestResourceStream("wrec.app-icn-recording.ico"));
            this.Icon = _normalIcon;

            InitializeMaterialSkin();
            InitializeUIComponents();
            InitializeTrayIcon();
            LoadConfiguration();
            RegisterEvents();
            InitializeGlobalHotkeys();
        }

        private void InitializeTrayIcon()
        {
            _trayIcon = new NotifyIcon
            {
                Icon = _normalIcon,
                Text = "wrec - Enregistrement en cours",
                Visible = true
            };

            // Ajoutez un menu contextuel
            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Ouvrir la fenêtre", null, (s, e) => ShowFromTray());
            contextMenu.Items.Add("Arrêter l'enregistrement", null, (s, e) => StopRecordingFromTray());
            contextMenu.Items.Add("Quitter", null, (s, e) => ExitApplication());

            _trayIcon.ContextMenuStrip = contextMenu;
            _trayIcon.DoubleClick += (s, e) => ShowFromTray();
        }

        private void RecordingTimer_Tick(object sender, EventArgs e)
        {
            var elapsed = DateTime.Now - _recordingStartTime;
            _lblRecordingTime.Text = elapsed.ToString(@"hh\:mm\:ss");
        }

        private void ShowFromTray()
        {
            this.Invoke((MethodInvoker)delegate
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
                this.BringToFront();
                _trayIcon.Visible = false;
                this.ShowInTaskbar = true;
                if (_recorderService.IsRecording)
                {
                    _btnRecordStop.Enabled = true;
                }
            });
        }

        private void StopRecordingFromTray()
        {
            this.Invoke((MethodInvoker)delegate
            {
                if (_recorderService.IsRecording)
                {
                    StopRecording();
                    _btnRecordStop.Enabled = true;
                }
            });
        }

        private void ExitApplication()
        {
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
            Application.Exit();
        }

        #region Initialization Methods

        private void InitializeGlobalHotkeys()
        {
            _globalHotkey = new GlobalHotkey();
            _globalHotkey.HotkeyPressed += (sender, key) =>
            {
                this.Invoke((MethodInvoker)delegate
                {
                    if (key == Keys.F9 && !_recorderService.IsRecording)
                    {
                        StartRecording();
                    }
                    else if (key == Keys.F10 && _recorderService.IsRecording)
                    {
                        StopRecording();
                    }
                });
            };
            _globalHotkey.Register();
        }

        private void InitializeMaterialSkin()
        {
            MaterialSkinManager.Instance.AddFormToManage(this);
            MaterialSkinManager.Instance.Theme = MaterialSkinManager.Themes.LIGHT;
            MaterialSkinManager.Instance.ColorScheme = new ColorScheme(
                Primary.Indigo700,
                Primary.Indigo700,
                Primary.Indigo500,
                Accent.Indigo200,
                TextShade.WHITE);
        }

        private void InitializeUIComponents()
        {
            ConfigureMainWindow();
            InitializeTabs();
            InitializeMainControls();
            InitializeCompressionPanel();
        }

        private void ConfigureMainWindow()
        {
            this.Text = "wrec - a simple screen recorder";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(850, 750);
            this.MaximumSize = new Size(850, 750);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Padding = new Padding(10);
        }

        private void InitializeTabs()
        {
            _tabControl = new TabControl
            {
                Location = new Point(10, 70),
                Size = new Size(830, 435),
                Appearance = TabAppearance.Normal
            };

            _generalTab = new GeneralTab();
            _videoTab = new VideoTab();
            _audioTab = new AudioTab();
            _mouseTab = new MouseTab();

            _tabControl.Controls.AddRange(new TabPage[] {
                _generalTab,
                _videoTab,
                _audioTab,
                _mouseTab
            });

            _audioTab.LoadMicrophones();

            this.Controls.Add(_tabControl);
        }

        private void InitializeMainControls()
        {
            _btnRecordStop = new MaterialButton
            {
                Text = "DÉMARRER L'ENREGISTREMENT (Ctrl+F9)",
                Location = new Point(260, 520),
                Size = new Size(830, 48),
                Type = MaterialButton.MaterialButtonType.Contained,
                HighEmphasis = true,
                UseAccentColor = true
            };

            _lblStatus = new MaterialLabel
            {
                Text = "Prêt à enregistrer",
                Location = new Point(10, 570),
                Size = new Size(830, 30),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.Red
            };

            _lblRecordingTime = new MaterialLabel
            {
                Text = "00:00:00",
                Location = new Point(10, 610),
                Size = new Size(830, 30),
                TextAlign = ContentAlignment.MiddleCenter,
                Visible = false
            };

            this.Controls.Add(_btnRecordStop);
            this.Controls.Add(_lblStatus);
            this.Controls.Add(_lblRecordingTime);
        }

        private void LoadConfiguration()
        {
            _config = _fileService.LoadConfig() ?? new AppConfig();

            // Appliquer la configuration à l'interface
            _generalTab.TxtOutputPath.Text = _config.OutputFolder;
            _generalTab.ChkEnableCountdown.Checked = _config.CountdownEnabled;
            _generalTab.TxtCountdownDelay.Text = _config.CountdownDelay.ToString();
            _generalTab.ChkEnableAreaSelection.Checked = _config.UseAreaSelection;
            if (_config.UseAreaSelection && _config.AreaWidth > 0 && _config.AreaHeight > 0)
            {
                _generalTab.SelectedArea = new Rectangle(_config.AreaX, _config.AreaY, _config.AreaWidth, _config.AreaHeight);
                _generalTab.LblSelectedArea.Text = $"Zone: {_config.AreaX}, {_config.AreaY} - {_config.AreaWidth}x{_config.AreaHeight}";
                _generalTab.LblSelectedArea.ForeColor = Color.Green;
            }
            _videoTab.TxtVideoBitrate.Text = _config.VideoBitrate.ToString();
            _videoTab.TxtFramerate.Text = _config.FPS.ToString();
            _videoTab.ChkFixedFramerate.Checked = _config.FixedFramerate;
            _videoTab.ChkHardwareEncoding.Checked = _config.MaterialEncoding;
            _mouseTab.LeftClickColor = _config.LeftClickColor;
            _mouseTab.RightClickColor = _config.RightClickColor;
            _mouseTab.LeftColorSquare.BackColor = _config.LeftClickColor;
            _mouseTab.RightColorSquare.BackColor = _config.RightClickColor;
            _audioTab.ChkAudioEnabled.Checked = _config.IsSystemAudioEnabled;
            _audioTab.ChkMicrophoneEnabled.Checked = _config.IsMicrophoneEnabled;

            switch (_config.AudioBitrateKbps)
            {
                case 96:
                    _audioTab.CmbAudioBitrate.SelectedIndex = 0;
                    break;
                case 192:
                    _audioTab.CmbAudioBitrate.SelectedIndex = 2;
                    break;
                default:
                    _audioTab.CmbAudioBitrate.SelectedIndex = 1;
                    break;
            }

            _audioTab.TxtMicrophoneVolume.Text = _config.MicrophoneVolumePercent.ToString();
            _audioTab.TxtSystemVolume.Text = _config.SystemVolumePercent.ToString();

            switch (_config.Quality)
            {
                case 25: _videoTab.CmbQuality.SelectedIndex = 0; break;
                case 40: _videoTab.CmbQuality.SelectedIndex = 1; break;
                case 50: _videoTab.CmbQuality.SelectedIndex = 2; break;
                case 65: _videoTab.CmbQuality.SelectedIndex = 3; break;
                case 75: _videoTab.CmbQuality.SelectedIndex = 4; break;
                case 85: _videoTab.CmbQuality.SelectedIndex = 5; break;
                default: _videoTab.CmbQuality.SelectedIndex = 6; break;
            }

            switch (_config.EncoderProfile)
            {
                case H264Profile.Baseline: _videoTab.CmbEncoderProfile.SelectedIndex = 0; break;
                case H264Profile.Main: _videoTab.CmbEncoderProfile.SelectedIndex = 1; break;
                case H264Profile.High: _videoTab.CmbEncoderProfile.SelectedIndex = 2; break;
            }

            switch (_config.BitrateMode)
            {
                case H264BitrateControlMode.CBR: _videoTab.CmbBitrateMode.SelectedIndex = 0; break;
                case H264BitrateControlMode.Quality: _videoTab.CmbBitrateMode.SelectedIndex = 1; break;
                case H264BitrateControlMode.UnconstrainedVBR: _videoTab.CmbBitrateMode.SelectedIndex = 2; break;
            }
        }

        private void RegisterEvents()
        {
            _btnRecordStop.Click += BtnRecordStop_Click;
            _recorderService.OnRecordingComplete += Rec_OnRecordingComplete;
            _recorderService.OnRecordingFailed += Rec_OnRecordingFailed;
            _recorderService.OnStatusChanged += Rec_OnStatusChanged;

            _audioTab.ChkMicrophoneEnabled.CheckedChanged += ChkMicrophoneEnabled_CheckedChanged;
            _audioTab.ChkAudioEnabled.CheckedChanged += ChkAudioEnabled_CheckedChanged;
            _generalTab.BtnBrowseFolder.Click += BtnBrowseFolder_Click;
        }

        #endregion

        #region Event Handlers

        private void BtnRecordStop_Click(object sender, EventArgs e)
        {
            _btnRecordStop.Enabled = false;

            if (_recorderService.IsRecording)
            {
                StopRecording();
            }
            else
            {
                StartRecording();
            }
        }

        private void BtnBrowseFolder_Click(object sender, EventArgs e)
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                folderDialog.SelectedPath = Directory.Exists(_generalTab.TxtOutputPath.Text)
                    ? _generalTab.TxtOutputPath.Text
                    : _config.OutputFolder;

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    _generalTab.TxtOutputPath.Text = folderDialog.SelectedPath;
                    _config.OutputFolder = folderDialog.SelectedPath;
                    _fileService.SaveConfig(_config);
                }
            }
        }

        private void ChkMicrophoneEnabled_CheckedChanged(object sender, EventArgs e)
        {
            _audioTab.CmbMicrophones.Enabled = _audioTab.ChkMicrophoneEnabled.Checked;
            _audioTab.TxtMicrophoneVolume.Enabled = _audioTab.ChkMicrophoneEnabled.Checked;
        }

        private void ChkAudioEnabled_CheckedChanged(object sender, EventArgs e)
        {
            _audioTab.CmbAudioBitrate.Enabled = _audioTab.ChkAudioEnabled.Checked;
            _audioTab.TxtSystemVolume.Enabled = _audioTab.ChkAudioEnabled.Checked;
        }

        private void InitializeCompressionPanel()
        {
            _compressionPanel = new CompressionPanel
            {
                Location = new Point(10, 590),
                Visible = false
            };

            _compressionPanel.OnStatusChanged += (sender, status) => {
                this.Invoke((MethodInvoker)delegate {
                    // Ne pas afficher les messages de préparation dans le label principal
                    if (!status.StartsWith("Fichier original:"))
                    {
                        _lblStatus.Text = status;
                        _lblStatus.ForeColor = status.Contains("échec") ? Color.Red :
                                              status.Contains("terminé") ? Color.Green : Color.Black;
                    }
                });
            };

            _compressionPanel.OnCompressComplete += (sender, e) => {
                this.Invoke((MethodInvoker)delegate {
                    _btnRecordStop.Enabled = true;
                });
            };

            _compressionPanel.OnCompressCancel += (sender, e) => {
                this.Invoke((MethodInvoker)delegate {
                    _btnRecordStop.Enabled = true;
                });
            };

            // Gestion de la visibilité du label principal
            _compressionPanel.VisibleChanged += (sender, e) => {
                _btnRecordStop.Enabled = !_compressionPanel.Visible;
                _lblStatus.Visible = !_compressionPanel.Visible; // Masquer/afficher le label
                _lblRecordingTime.Visible = !_compressionPanel.Visible; // Masquer aussi le temps d'enregistrement si besoin
            };

            this.Controls.Add(_compressionPanel);
        }

        private void Rec_OnRecordingComplete(object sender, RecordingCompleteEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate {
                var elapsed = DateTime.Now - _recordingStartTime;
                var durationText = elapsed.ToString(@"hh\:mm\:ss");
                UpdateUIForRecording(false);
                long fileSize = new FileInfo(e.FilePath).Length / 1024 / 1024;
                _lblStatus.Text = $"Enregistrement terminé - Taille: {fileSize} MB";
                _lblStatus.ForeColor = Color.Black;

                var snackBar = new MaterialSnackBar(
                    Text: $"Enregistrement terminé - Durée: {durationText}",
                    Duration: 4500
                );

                snackBar.Show(this);

                if (_ffmpegService.IsFFmpegAvailable())
                {
                    _compressionPanel.ShowForVideo(e.FilePath);
                }
                else
                {
                    ShowFFmpegDownloadPrompt();
                }

                _btnRecordStop.Enabled = false;
            });
        }

        private void ShowFFmpegDownloadPrompt()
        {
            var result = MaterialMessageBox.Show(
                "FFmpeg n'est pas installé sur votre système. Voulez-vous le télécharger maintenant ?\n\n" +
                "FFmpeg est nécessaire pour la compression vidéo.",
                "FFmpeg requis",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    Process.Start("https://ffmpeg.org/download.html");
                }
                catch (Exception ex)
                {
                    MaterialMessageBox.Show($"Impossible d'ouvrir le lien de téléchargement: {ex.Message}",
                        "Erreur",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        private void CompressionPanel_OnCompressComplete(object sender, EventArgs e)
        {
            this.Invoke((MethodInvoker)delegate {
                _btnRecordStop.Enabled = true;

                // Réduire la fenêtre si le panel est caché
                if (!_compressionPanel.Visible)
                {
                    this.MinimumSize = new Size(850, 750);
                    this.MaximumSize = new Size(850, 750);
                }
            });
        }

        private void CompressionPanel_OnCompressCancel(object sender, EventArgs e)
        {
            this.Invoke((MethodInvoker)delegate {
                _btnRecordStop.Enabled = true;

                // Réduire la fenêtre si le panel est caché
                if (!_compressionPanel.Visible)
                {
                    this.MinimumSize = new Size(850, 750);
                    this.MaximumSize = new Size(850, 750);
                }
            });
        }

        private void Rec_OnRecordingFailed(object sender, RecordingFailedEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate {
                UpdateUIForRecording(false);
                _btnRecordStop.Enabled = true;
                _lblStatus.Text = "Échec : " + e.Error;
                MaterialMessageBox.Show("Erreur d'enregistrement : " + e.Error,
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            });
        }

        private void Rec_OnStatusChanged(object sender, RecordingStatusEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate {
                _lblStatus.Text = "Statut : " + e.Status;
            });
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Save configuration
            _config.OutputFolder = _generalTab.TxtOutputPath.Text;
            _config.LeftClickColor = _mouseTab.LeftClickColor;
            _config.RightClickColor = _mouseTab.RightClickColor;
            _config.CountdownEnabled = _generalTab.ChkEnableCountdown.Checked;
            _config.CountdownDelay = int.TryParse(_generalTab.TxtCountdownDelay.Text, out var delay) ? delay : 3;
            _config.UseAreaSelection = _generalTab.ChkEnableAreaSelection.Checked;
            _config.AreaX = _generalTab.SelectedArea.X;
            _config.AreaY = _generalTab.SelectedArea.Y;
            _config.AreaWidth = _generalTab.SelectedArea.Width;
            _config.AreaHeight = _generalTab.SelectedArea.Height;
            _config.VideoBitrate = int.TryParse(_videoTab.TxtVideoBitrate.Text, out var videoBotrate) ? videoBotrate : 5000;
            _config.FPS = int.TryParse(_videoTab.TxtFramerate.Text, out var fps) ? fps : 60;
            _config.FixedFramerate = _videoTab.ChkFixedFramerate.Checked;
            _config.MaterialEncoding = _videoTab.ChkHardwareEncoding.Checked;
            _config.Quality = _videoTab.GetSelectedQuality();
            _config.EncoderProfile = _videoTab.GetSelectedEncoderProfile();
            _config.BitrateMode = _videoTab.GetSelectedBitrateMode();

            switch (_audioTab.CmbAudioBitrate.SelectedIndex)
            {
                case 0:
                    _config.AudioBitrateKbps = 96;
                    break;
                case 2:
                    _config.AudioBitrateKbps = 192;
                    break;
                default:
                    _config.AudioBitrateKbps = 128;
                    break;
            }

            _config.IsSystemAudioEnabled = _audioTab.ChkAudioEnabled.Checked;
            _config.IsMicrophoneEnabled = _audioTab.ChkMicrophoneEnabled.Checked;
            _config.MicrophoneVolumePercent = int.TryParse(_audioTab.TxtMicrophoneVolume.Text, out var micVol) ? micVol : 50;
            _config.SystemVolumePercent = int.TryParse(_audioTab.TxtSystemVolume.Text, out var sysVol) ? sysVol : 50;

            _fileService.SaveConfig(_config);

            // Nettoyer les raccourcis globaux
            _recordingTimer?.Stop();
            _recordingTimer?.Dispose();
            _trayIcon?.Dispose();
            _normalIcon?.Dispose();
            _recordingIcon?.Dispose();
            _globalHotkey?.Dispose();
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
        }

        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);
        }

        #endregion

        #region Recording Methods

        private void StartRecording()
        {
            try
            {
                if (!ValidateInputs(out Models.RecorderOptions userOptions))
                    return;

                // Vérifier si le compte à rebours est activé
                if (_generalTab.ChkEnableCountdown?.Checked == true)
                {
                    int countdownSeconds;
                    if (int.TryParse(_generalTab.TxtCountdownDelay.Text, out countdownSeconds) && countdownSeconds > 0)
                    {
                        StartCountdown(countdownSeconds, () =>
                        {
                            // Cette action sera exécutée après le compte à rebours
                            ActuallyStartRecording(userOptions);
                        });
                        return; // On sort, l'enregistrement sera démarré par le callback
                    }
                }

                // Si pas de compte à rebours, démarrer immédiatement
                ActuallyStartRecording(userOptions);
            }
            catch (Exception ex)
            {
                MaterialMessageBox.Show($"Erreur lors du démarrage : {ex.Message}",
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void StartCountdown(int seconds, Action onComplete)
        {
            // Créer une form de countdown semi-transparente
            var countdownForm = new Form
            {
                Size = new Size(200, 200),
                FormBorderStyle = FormBorderStyle.None,
                StartPosition = FormStartPosition.CenterScreen,
                BackColor = Color.Black,
                Opacity = 0.7,
                TopMost = true,
                ShowInTaskbar = false
            };

            // Label pour afficher le nombre
            var countLabel = new Label
            {
                Text = seconds.ToString(),
                Font = new Font("Arial", 72, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                AutoSize = false
            };
            countdownForm.Controls.Add(countLabel);

            // Timer pour le compte à rebours
            var timer = new System.Windows.Forms.Timer
            {
                Interval = 1000
            };

            int remaining = seconds;
            timer.Tick += (s, e) =>
            {
                remaining--;
                countLabel.Text = remaining.ToString();

                if (remaining <= 0)
                {
                    timer.Stop();
                    countdownForm.Close();
                    onComplete?.Invoke();
                }
            };

            // Afficher le formulaire et démarrer le compte à rebours
            countdownForm.Show();
            timer.Start();
        }

        private void ActuallyStartRecording(Models.RecorderOptions userOptions)
        {
            // Minimize window when recording starts
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = true;

            // Envoyer directement nos options au service
            _recorderService.StartRecording(userOptions);
            UpdateUIForRecording(true);
        }

        private ScreenRecorderLib.RecorderOptions ConvertToScreenRecorderOptions(Models.RecorderOptions userOptions)
        {
            return new ScreenRecorderLib.RecorderOptions
            {
                OutputOptions = userOptions.OutputOptions,
                AudioOptions = userOptions.AudioOptions,
                VideoEncoderOptions = userOptions.VideoEncoderOptions,
                MouseOptions = userOptions.MouseOptions
            };
        }

        private void StopRecording()
        {
            _recorderService.StopRecording();
            UpdateUIForRecording(false);

            // Restaurer la fenêtre
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.BringToFront();
        }

        private bool ValidateInputs(out Models.RecorderOptions options)
        {
            options = null;

            try
            {
                // Validate area selection if enabled
                if (_generalTab.ChkEnableAreaSelection.Checked)
                {
                    if (_generalTab.SelectedArea.IsEmpty || _generalTab.SelectedArea.Width < 50 || _generalTab.SelectedArea.Height < 50)
                    {
                        MaterialMessageBox.Show("Veuillez sélectionner une zone valide pour l'enregistrement (minimum 50x50 pixels).",
                            "Zone non sélectionnée", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }
                }

                // Validate numeric fields
                Validator.ValidateNumericField(_videoTab.TxtWidth.Text, 640, 7680, "Largeur", out int width);
                Validator.ValidateNumericField(_videoTab.TxtHeight.Text, 480, 4320, "Hauteur", out int height);
                Validator.ValidateNumericField(_videoTab.TxtVideoBitrate.Text, 1000, 100000, "Bitrate vidéo", out int videoBitrate);
                Validator.ValidateNumericField(_videoTab.TxtFramerate.Text, 1, 240, "Framerate", out int framerate);

                if (_mouseTab.ChkShowMouseClicks.Checked)
                {
                    Validator.ValidateNumericField(_mouseTab.TxtClickRadius.Text, 5, 100, "Rayon du clic", out _);
                    Validator.ValidateNumericField(_mouseTab.TxtClickDuration.Text, 50, 2000, "Durée d'affichage", out _);
                }

                // Validate path
                Validator.ValidatePath(_generalTab.TxtOutputPath.Text);

                // Validate volumes
                Validator.ValidateNumericField(_audioTab.TxtMicrophoneVolume.Text, 0, 200, "Volume micro", out _);
                Validator.ValidateNumericField(_audioTab.TxtSystemVolume.Text, 0, 200, "Volume système", out _);

                // Audio bitrate
                AudioBitrate audioBitrate;
                switch (_audioTab.CmbAudioBitrate.SelectedIndex)
                {
                    case 0:
                        audioBitrate = AudioBitrate.bitrate_96kbps;
                        break;
                    case 2:
                        audioBitrate = AudioBitrate.bitrate_192kbps;
                        break;
                    default:
                        audioBitrate = AudioBitrate.bitrate_128kbps;
                        break;
                }

                // Qualité
                int quality;
                switch (_videoTab.CmbQuality.SelectedIndex)
                {
                    case 0: quality = 40; break;
                    case 1: quality = 50; break;
                    case 2: quality = 65; break;
                    case 3: quality = 75; break;
                    case 4: quality = 85; break;
                    default: quality = 65; break;
                }

                // Profil encodeur
                H264Profile encoderProfile;
                switch (_videoTab.CmbEncoderProfile.SelectedIndex)
                {
                    case 0: encoderProfile = H264Profile.Baseline; break;
                    case 1: encoderProfile = H264Profile.Main; break;
                    case 2: encoderProfile = H264Profile.High; break;
                    default: encoderProfile = H264Profile.Main; break;
                }

                // Mode de bitrate
                H264BitrateControlMode bitrateMode;
                switch (_videoTab.CmbBitrateMode.SelectedIndex)
                {
                    case 0: bitrateMode = H264BitrateControlMode.CBR; break;
                    case 1: bitrateMode = H264BitrateControlMode.Quality; break;
                    case 2: bitrateMode = H264BitrateControlMode.UnconstrainedVBR; break;
                    default: bitrateMode = H264BitrateControlMode.CBR; break;
                }

                // Build options
                options = new Models.RecorderOptions
                {
                    OutputPath = _generalTab.TxtOutputPath.Text,
                    UseAreaSelection = _generalTab.ChkEnableAreaSelection.Checked,
                    SelectedArea = _generalTab.SelectedArea,
                    OutputOptions = new OutputOptions
                    {
                        RecorderMode = RecorderMode.Video,
                        OutputFrameSize = new ScreenSize(width, height)
                    },
                    AudioOptions = new AudioOptions
                    {
                        Bitrate = audioBitrate,
                        Channels = AudioChannels.Stereo,
                        IsAudioEnabled = _audioTab.ChkAudioEnabled.Checked || _audioTab.ChkMicrophoneEnabled.Checked,
                        IsOutputDeviceEnabled = _audioTab.ChkAudioEnabled.Checked,
                        IsInputDeviceEnabled = _audioTab.ChkMicrophoneEnabled.Checked,
                        AudioInputDevice = _audioTab.ChkMicrophoneEnabled.Checked && _audioTab.CmbMicrophones.SelectedIndex >= 0
                            ? _audioTab.CmbMicrophones.SelectedItem.ToString()
                            : null,
                        InputVolume = float.Parse(_audioTab.TxtMicrophoneVolume.Text) / 100f,
                        OutputVolume = float.Parse(_audioTab.TxtSystemVolume.Text) / 100f
                    },
                    VideoEncoderOptions = new VideoEncoderOptions
                    {
                        Bitrate = videoBitrate * 1000,
                        Framerate = framerate,
                        IsFixedFramerate = _videoTab.ChkFixedFramerate.Checked,
                        Encoder = new H264VideoEncoder
                        {
                            BitrateMode = bitrateMode,
                            EncoderProfile = encoderProfile,
                        },
                        IsHardwareEncodingEnabled = _videoTab.ChkHardwareEncoding.Checked,
                        Quality = quality
                    },
                    MouseOptions = new MouseOptions
                    {
                        IsMousePointerEnabled = _mouseTab.ChkShowCursor.Checked,
                        IsMouseClicksDetected = _mouseTab.ChkShowMouseClicks.Checked,
                        MouseLeftClickDetectionColor = ColorToHex(_mouseTab.LeftClickColor),
                        MouseRightClickDetectionColor = ColorToHex(_mouseTab.RightClickColor),
                        MouseClickDetectionRadius = _mouseTab.ChkShowMouseClicks.Checked
                            ? int.Parse(_mouseTab.TxtClickRadius.Text)
                            : 30,
                        MouseClickDetectionDuration = _mouseTab.ChkShowMouseClicks.Checked
                            ? int.Parse(_mouseTab.TxtClickDuration.Text)
                            : 100,
                        MouseClickDetectionMode = _mouseTab.CmbClickDetectionMode.SelectedIndex == 0
                            ? MouseDetectionMode.Hook
                            : MouseDetectionMode.Polling
                    }
                };

                return true;
            }
            catch (Exception ex)
            {
                MaterialMessageBox.Show(ex.Message, "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
        }

        private string ColorToHex(Color color)
        {
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        private void UpdateUIForRecording(bool recording)
        {
            this.Invoke((MethodInvoker)delegate
            {
                _recordingInProgress = recording;

                if (recording)
                {
                    _recordingStartTime = DateTime.Now;
                    _recordingTimer.Start();
                    _lblRecordingTime.Visible = true;
                }
                else
                {
                    _recordingTimer.Stop();
                    _lblRecordingTime.Visible = false;
                }

                this.Icon = recording ? _recordingIcon : _normalIcon;
                _btnRecordStop.UseAccentColor = recording;
                _btnRecordStop.Text = recording ? "ARRÊTER L'ENREGISTREMENT (Ctrl+F10)" : "DÉMARRER L'ENREGISTREMENT (Ctrl+F9)";
                _btnRecordStop.Enabled = true; // Toujours activé (sauf cas particuliers)
                _lblStatus.Text = recording ? "Enregistrement en cours..." : "Prêt à enregistrer";
                _lblStatus.ForeColor = recording ? Color.Green : Color.Red;

                if (_trayIcon != null)
                {
                    _trayIcon.Icon = recording ? _recordingIcon : _normalIcon;
                    _trayIcon.Text = recording ? "Enregistrement en cours..." : "wrec - Enregistreur d'écran";
                    _trayIcon.Visible = recording;
                    this.ShowInTaskbar = !recording;
                }
            });
        }

        #endregion

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (_recordingInProgress && this.WindowState == FormWindowState.Minimized)
            {
                if (_trayIcon != null)
                {
                    this.Hide();
                    _trayIcon.Visible = true;
                    this.ShowInTaskbar = false;
                }
            }
        }

        #region ProcessCmdKey Override

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.F9 | Keys.Control)) // Nouveau raccourci
            {
                if (!_recorderService.IsRecording)
                {
                    StartRecording();
                }
                return true;
            }
            else if (keyData == (Keys.F10 | Keys.Control)) // Nouveau raccourci
            {
                if (_recorderService.IsRecording)
                {
                    StopRecording();
                }
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        #endregion
    }
}
