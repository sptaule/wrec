using ScreenRecorderLib;
using System.Drawing;

namespace wrec.Models
{
    public class RecorderOptions
    {
        public string OutputPath { get; set; }
        public OutputOptions OutputOptions { get; set; }
        public AudioOptions AudioOptions { get; set; }
        public VideoEncoderOptions VideoEncoderOptions { get; set; }
        public MouseOptions MouseOptions { get; set; }

        public ScreenRecorderLib.RecorderOptions ToScreenRecorderOptions()
        {
            return new ScreenRecorderLib.RecorderOptions
            {
                OutputOptions = OutputOptions,
                AudioOptions = AudioOptions,
                VideoEncoderOptions = VideoEncoderOptions,
                MouseOptions = MouseOptions
            };
        }
    }
}