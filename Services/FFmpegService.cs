using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace wrec.Services
{
    public enum CompressionQuality
    {
        Lowest,
        Low,
        Medium,
        High
    }

    public class FFmpegService
    {
        public event EventHandler<string> OnCompressionCompleted;
        public event EventHandler<string> OnCompressionFailed;
        public event EventHandler<string> OnCompressionStatusChanged;
        public event EventHandler<double> OnProgressChanged;

        private Process _ffmpegProcess;
        private bool _isCompressing = false;
        private string _ffmpegPath;
        private TimeSpan? _totalDuration;
        private DateTime _startTime;

        public FFmpegService()
        {
            // 1. Vérifier si ffmpeg est dans le PATH
            _ffmpegPath = FindFFmpegInPath();

            // 2. Si pas trouvé, chercher dans le dossier local
            if (string.IsNullOrEmpty(_ffmpegPath))
            {
                _ffmpegPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg", "ffmpeg.exe");
            }
        }

        public bool IsCompressing => _isCompressing;

        private string FindFFmpegInPath()
        {
            try
            {
                using (var process = new Process())
                {
                    process.StartInfo.FileName = "ffmpeg";
                    process.StartInfo.Arguments = "-version";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;

                    process.Start();
                    process.WaitForExit(1000);

                    if (process.ExitCode == 0)
                    {
                        return "ffmpeg";
                    }
                }
            }
            catch
            {
                // Ignorer les erreurs
            }

            return null;
        }

        private async Task<TimeSpan?> GetVideoDuration(string inputPath)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = _ffmpegPath,
                        Arguments = $"-i \"{inputPath}\" -hide_banner",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardError = true
                    }
                };

                var completionSource = new TaskCompletionSource<TimeSpan?>();
                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        var match = Regex.Match(e.Data, @"Duration:\s(\d{2}):(\d{2}):(\d{2})\.\d{2}");
                        if (match.Success)
                        {
                            int hours = int.Parse(match.Groups[1].Value);
                            int minutes = int.Parse(match.Groups[2].Value);
                            int seconds = int.Parse(match.Groups[3].Value);
                            completionSource.TrySetResult(new TimeSpan(hours, minutes, seconds));
                        }
                    }
                };

                process.Exited += (sender, e) =>
                {
                    if (!completionSource.Task.IsCompleted)
                        completionSource.TrySetResult(null);
                };

                process.Start();
                process.BeginErrorReadLine();
                return await completionSource.Task;
            }
            catch
            {
                return null;
            }
        }


        public async Task CompressVideoAsync(string inputPath, CompressionQuality quality)
        {
            if (_isCompressing) return;

            if (string.IsNullOrEmpty(_ffmpegPath))
            {
                OnCompressionFailed?.Invoke(this, "FFmpeg non trouvé.");
                return;
            }

            if (!File.Exists(inputPath))
            {
                OnCompressionFailed?.Invoke(this, "Fichier vidéo source introuvable.");
                return;
            }

            try
            {
                _isCompressing = true;
                _totalDuration = await GetVideoDuration(inputPath);
                _startTime = DateTime.Now;
                OnCompressionStatusChanged?.Invoke(this, "Compression en cours...");

                string outputPath = GetOutputPath(inputPath, quality);
                string arguments = GetFFmpegArguments(inputPath, outputPath, quality);

                await Task.Run(() => RunFFmpegProcess(arguments, outputPath));
            }
            catch (Exception ex)
            {
                _isCompressing = false;
                OnCompressionFailed?.Invoke(this, $"Erreur: {ex.Message}");
            }
        }

        private string GetOutputPath(string inputPath, CompressionQuality quality)
        {
            string directory = Path.GetDirectoryName(inputPath);
            string filename = Path.GetFileNameWithoutExtension(inputPath);
            string extension = Path.GetExtension(inputPath);
            string qualitySuffix = GetQualitySuffix(quality);

            return Path.Combine(directory, $"{filename}_{qualitySuffix}{extension}");
        }

        private string GetQualitySuffix(CompressionQuality quality)
        {
            switch (quality)
            {
                case CompressionQuality.Lowest:
                    return "compressed_lowest";
                case CompressionQuality.Low:
                    return "compressed_low";
                case CompressionQuality.Medium:
                    return "compressed_medium";
                default:
                    return "compressed_high";
            }
        }

        private string GetFFmpegArguments(string inputPath, string outputPath, CompressionQuality quality)
        {
            string videoCodec;
            string presetParam;
            string qualityParam;
            string audioBitrate;

            if (IsEncoderAvailable("av1_nvenc"))
            {
                videoCodec = "-c:v av1_nvenc";
                presetParam = GetNvencPreset(quality);
                qualityParam = GetNvencCQ(quality);
            }
            else if (IsEncoderAvailable("hevc_nvenc"))
            {
                videoCodec = "-c:v hevc_nvenc";
                presetParam = GetNvencPreset(quality);
                qualityParam = GetNvencCQ(quality);
            }
            else
            {
                videoCodec = "-c:v libx265";
                presetParam = GetSoftwarePreset(quality);
                qualityParam = GetSoftwareCRF(quality);
            }

            switch (quality)
            {
                case CompressionQuality.Lowest:
                    audioBitrate = "96k";
                    break;
                case CompressionQuality.Low:
                    audioBitrate = "128k";
                    break;
                default:
                    audioBitrate = "192k";
                    break;
            }

            string arguments = "-i \"" + inputPath + "\" " + videoCodec + " " + presetParam + " " + qualityParam +
                               " -c:a aac -b:a " + audioBitrate + " \"" + outputPath + "\" -y -hide_banner -loglevel error -stats";

            Console.WriteLine("FFmpeg arguments: " + arguments);
            return arguments;
        }

        private bool IsEncoderAvailable(string encoderName)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "ffmpeg",
                        Arguments = "-hide_banner -encoders",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                // Si c'est av1_nvenc, on vérifie aussi qu'on n'a pas "no capable devices found"
                if (encoderName == "av1_nvenc")
                {
                    var testProcess = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "ffmpeg",
                            Arguments = "-f lavfi -i testsrc=size=128x128:rate=1 -frames:v 1 -c:v av1_nvenc -y nul",
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        }
                    };

                    testProcess.Start();
                    string testOutput = testProcess.StandardOutput.ReadToEnd();
                    string testError = testProcess.StandardError.ReadToEnd();
                    testProcess.WaitForExit();

                    if (testError.Contains("No capable devices found") || testError.Contains("Error"))
                        return false;
                }

                return output.Contains(encoderName);
            }
            catch
            {
                return false;
            }
        }

        private string GetNvencPreset(CompressionQuality quality)
        {
            switch (quality)
            {
                case CompressionQuality.Lowest:
                    return "-preset p1";
                case CompressionQuality.Low:
                    return "-preset p3";
                case CompressionQuality.Medium:
                    return "-preset p5";
                default:
                    return "-preset p7";
            }
        }

        private string GetNvencCQ(CompressionQuality quality)
        {
            switch (quality)
            {
                case CompressionQuality.Lowest:
                    return "-cq 40";
                case CompressionQuality.Low:
                    return "-cq 30";
                case CompressionQuality.Medium:
                    return "-cq 25";
                default:
                    return "-cq 22";
            }
        }

        private string GetSoftwarePreset(CompressionQuality quality)
        {
            switch (quality)
            {
                case CompressionQuality.Lowest:
                case CompressionQuality.Low:
                    return "-preset faster";
                case CompressionQuality.Medium:
                    return "-preset fast";
                default:
                    return "-preset medium";
            }
        }

        private string GetSoftwareCRF(CompressionQuality quality)
        {
            switch (quality)
            {
                case CompressionQuality.Lowest:
                    return "-crf 38";
                case CompressionQuality.Low:
                    return "-crf 30";
                case CompressionQuality.Medium:
                    return "-crf 25";
                default:
                    return "-crf 22";
            }
        }


        private void RunFFmpegProcess(string arguments, string outputPath)
        {
            try
            {
                _ffmpegProcess = new Process();
                _ffmpegProcess.StartInfo.FileName = _ffmpegPath;
                _ffmpegProcess.StartInfo.Arguments = arguments;
                _ffmpegProcess.StartInfo.UseShellExecute = false;
                _ffmpegProcess.StartInfo.CreateNoWindow = true;
                _ffmpegProcess.StartInfo.RedirectStandardError = true;
                _ffmpegProcess.EnableRaisingEvents = true;

                _ffmpegProcess.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        // Mise à jour de la progression
                        var timeMatch = Regex.Match(e.Data, @"time=(\d{2}):(\d{2}):(\d{2})\.\d{2}");
                        if (timeMatch.Success && _totalDuration.HasValue)
                        {
                            int hours = int.Parse(timeMatch.Groups[1].Value);
                            int minutes = int.Parse(timeMatch.Groups[2].Value);
                            int seconds = int.Parse(timeMatch.Groups[3].Value);
                            var currentTime = new TimeSpan(hours, minutes, seconds);

                            double progress = (currentTime.TotalSeconds / _totalDuration.Value.TotalSeconds) * 100;
                            OnProgressChanged?.Invoke(this, Math.Min(100, progress));
                        }
                        else if (e.Data.Contains("speed="))
                        {
                            // Estimation basée sur le temps écoulé si la durée n'est pas disponible
                            if (!_totalDuration.HasValue)
                            {
                                var elapsed = DateTime.Now - _startTime;
                                // Estimation très approximative (50% après la moitié du temps)
                                OnProgressChanged?.Invoke(this, Math.Min(99, elapsed.TotalSeconds * 10));
                            }
                        }
                    }
                };

                // Variable locale pour capturer le résultat
                var completionSource = new TaskCompletionSource<bool>();

                _ffmpegProcess.Exited += (sender, e) =>
                {
                    try
                    {
                        bool success = false;
                        string message = "Erreur inconnue";

                        if (_ffmpegProcess != null)
                        {
                            success = _ffmpegProcess.ExitCode == 0 && File.Exists(outputPath);
                            message = success ? outputPath : $"Erreur FFmpeg (code: {_ffmpegProcess.ExitCode})";
                        }

                        completionSource.TrySetResult(success);

                        if (success)
                        {
                            OnCompressionCompleted?.Invoke(this, outputPath);
                            OnProgressChanged?.Invoke(this, 100);
                        }
                        else
                        {
                            OnCompressionFailed?.Invoke(this, message);
                        }
                    }
                    catch (Exception ex)
                    {
                        completionSource.TrySetException(ex);
                        OnCompressionFailed?.Invoke(this, $"Erreur lors du traitement: {ex.Message}");
                    }
                    finally
                    {
                        _isCompressing = false;
                        _ffmpegProcess?.Dispose();
                    }
                };

                _ffmpegProcess.Start();
                _ffmpegProcess.BeginErrorReadLine();

                completionSource.Task.ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        OnCompressionFailed?.Invoke(this, $"Erreur lors de la compression: {t.Exception?.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                _isCompressing = false;
                _ffmpegProcess?.Dispose();
                OnCompressionFailed?.Invoke(this, $"Erreur: {ex.Message}");
            }
        }

        public bool IsFFmpegAvailable()
        {
            try
            {
                if (!string.IsNullOrEmpty(_ffmpegPath))
                {
                    if (_ffmpegPath.Contains(Path.DirectorySeparatorChar))
                    {
                        return File.Exists(_ffmpegPath);
                    }
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public void CancelCompression()
        {
            if (_isCompressing && _ffmpegProcess != null && !_ffmpegProcess.HasExited)
            {
                try
                {
                    _ffmpegProcess.Kill();
                    _isCompressing = false;
                    OnCompressionStatusChanged?.Invoke(this, "Compression annulée");
                }
                catch { /* Ignorer les erreurs */ }
            }
        }
    }
}