using System;
using System.Drawing;
using System.IO;
using ScreenRecorderLib;

namespace wrec.Models
{
    public class AppConfig
    {
        public Color LeftClickColor { get; set; } = Color.Yellow;
        public Color RightClickColor { get; set; } = Color.Orange;
        public string OutputFolder { get; set; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "Videos", "wrec");
        public bool CountdownEnabled { get; set; } = true;
        public int CountdownDelay { get; set; } = 3;
        public int VideoBitrate { get; set; } = 5000;
        public int FPS { get; set; } = 60;
        public bool FixedFramerate { get; set; } = false;
        public bool MaterialEncoding { get; set; } = true;
        public int Quality { get; set; } = 65;
        public H264Profile EncoderProfile { get; set; } = H264Profile.Main;
        public H264BitrateControlMode BitrateMode { get; set; } = H264BitrateControlMode.CBR;
        public bool IsSystemAudioEnabled { get; set; } = true;
        public bool IsMicrophoneEnabled { get; set; } = false;
        public int AudioBitrateKbps { get; set; } = 128;
        public int MicrophoneVolumePercent { get; set; } = 50;
        public int SystemVolumePercent { get; set; } = 50;

        // Area selection settings
        public bool UseAreaSelection { get; set; } = false;
        public int AreaX { get; set; } = 0;
        public int AreaY { get; set; } = 0;
        public int AreaWidth { get; set; } = 800;
        public int AreaHeight { get; set; } = 600;
    }
}
