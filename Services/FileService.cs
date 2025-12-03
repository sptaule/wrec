using Newtonsoft.Json;
using wrec.Models;
using System;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace wrec.Services
{
    public class FileService
    {
        private const string ConfigFileName = "wrec_config.json";
        private const string RecordingsFolderName = "Enregistrements";
        private readonly string _appDataPath;

        public FileService()
        {
            // Chemin vers AppData/Roaming
            _appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "wrec");

            EnsureDirectoriesExist();
        }

        /// <summary>
        /// Crée les répertoires nécessaires s'ils n'existent pas
        /// </summary>
        private void EnsureDirectoriesExist()
        {
            try
            {
                if (!Directory.Exists(_appDataPath))
                {
                    Directory.CreateDirectory(_appDataPath);
                }

                var recordingsPath = GetRecordingsPath();
                if (!Directory.Exists(recordingsPath))
                {
                    Directory.CreateDirectory(recordingsPath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la création des répertoires : {ex.Message}",
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Charge la configuration depuis le fichier
        /// </summary>
        public AppConfig LoadConfig()
        {
            var configPath = Path.Combine(_appDataPath, ConfigFileName);

            if (!File.Exists(configPath))
                return null;

            try
            {
                string json = File.ReadAllText(configPath);
                return JsonConvert.DeserializeObject<AppConfig>(json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement de la configuration : {ex.Message}\nUne configuration par défaut sera utilisée.",
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }
        }

        /// <summary>
        /// Sauvegarde la configuration dans le fichier
        /// </summary>
        public void SaveConfig(AppConfig config)
        {
            var configPath = Path.Combine(_appDataPath, ConfigFileName);

            try
            {
                string json = JsonConvert.SerializeObject(config, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(configPath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la sauvegarde de la configuration : {ex.Message}",
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Génère un nom de fichier unique pour l'enregistrement
        /// </summary>
        public string GenerateRecordingFilename()
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            return Path.Combine(GetRecordingsPath(), $"Enregistrement_{timestamp}.mp4");
        }

        /// <summary>
        /// Obtient le chemin du dossier d'enregistrements par défaut
        /// </summary>
        public string GetRecordingsPath()
        {
            return Path.Combine(_appDataPath, RecordingsFolderName);
        }

        /// <summary>
        /// Ouvre le dossier des enregistrements dans l'explorateur de fichiers
        /// </summary>
        public void OpenRecordingsFolder()
        {
            try
            {
                string path = GetRecordingsPath();
                if (Directory.Exists(path))
                {
                    System.Diagnostics.Process.Start(path);
                }
                else
                {
                    MessageBox.Show("Le dossier d'enregistrements n'existe pas encore.",
                        "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Impossible d'ouvrir le dossier : {ex.Message}",
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Vérifie l'espace disque disponible
        /// </summary>
        public bool CheckDiskSpace(long requiredBytes, out string message)
        {
            message = string.Empty;
            try
            {
                var driveInfo = new DriveInfo(Path.GetPathRoot(_appDataPath));
                long availableSpace = driveInfo.AvailableFreeSpace;

                if (availableSpace < requiredBytes)
                {
                    double requiredGB = requiredBytes / (1024.0 * 1024 * 1024);
                    double availableGB = availableSpace / (1024.0 * 1024 * 1024);
                    message = $"Espace disque insuffisant.\nNécessaire : {requiredGB:0.00} GB\nDisponible : {availableGB:0.00} GB";
                    return false;
                }
                return true;
            }
            catch
            {
                // En cas d'erreur, on laisse passer mais on avertit
                message = "Impossible de vérifier l'espace disque disponible";
                return true;
            }
        }

        /// <summary>
        /// Supprime les anciens fichiers selon la politique de rétention
        /// </summary>
        public void CleanOldRecordings(int maxDaysToKeep)
        {
            if (maxDaysToKeep <= 0) return;

            try
            {
                string recordingsPath = GetRecordingsPath();
                var cutoffDate = DateTime.Now.AddDays(-maxDaysToKeep);

                foreach (var file in Directory.GetFiles(recordingsPath, "*.mp4"))
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.CreationTime < cutoffDate)
                    {
                        try
                        {
                            fileInfo.Delete();
                        }
                        catch
                        {
                            // Ignore les fichiers qui ne peuvent pas être supprimés
                        }
                    }
                }
            }
            catch
            {
                // Ignore les erreurs de nettoyage
            }
        }
    }
}
