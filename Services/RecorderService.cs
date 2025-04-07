using ScreenRecorderLib;
using wrec.Models;
using System;
using System.IO;

namespace wref.Services
{
    public class RecorderService
    {
        private Recorder _recorder;
        private bool _isRecording = false;

        public event EventHandler<RecordingCompleteEventArgs> OnRecordingComplete;
        public event EventHandler<RecordingFailedEventArgs> OnRecordingFailed;
        public event EventHandler<RecordingStatusEventArgs> OnStatusChanged;

        public void StartRecording(wrec.Models.RecorderOptions userOptions)
        {
            if (_isRecording) return;

            // 1. Générer le chemin complet du fichier
            string videoPath = Path.Combine(
                userOptions.OutputPath,
                $"Enregistrement_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.mp4");

            // 2. Créer les options spécifiques à la librairie
            var recorderOptions = new ScreenRecorderLib.RecorderOptions
            {
                OutputOptions = new OutputOptions
                {
                    RecorderMode = userOptions.OutputOptions.RecorderMode,
                    OutputFrameSize = userOptions.OutputOptions.OutputFrameSize
                },
                AudioOptions = userOptions.AudioOptions,
                VideoEncoderOptions = userOptions.VideoEncoderOptions,
                MouseOptions = userOptions.MouseOptions
            };

            // 3. Initialiser le recorder
            _recorder = Recorder.CreateRecorder(recorderOptions);
            _recorder.OnRecordingComplete += (s, e) => OnRecordingComplete?.Invoke(s, e);
            _recorder.OnRecordingFailed += (s, e) => OnRecordingFailed?.Invoke(s, e);
            _recorder.OnStatusChanged += (s, e) => OnStatusChanged?.Invoke(s, e);

            // 4. Démarrer l'enregistrement
            _recorder.Record(videoPath);
            _isRecording = true;
        }

        public void StopRecording()
        {
            if (!_isRecording) return;

            _recorder?.Stop();
            _isRecording = false;
        }

        public bool IsRecording => _isRecording;
    }
}