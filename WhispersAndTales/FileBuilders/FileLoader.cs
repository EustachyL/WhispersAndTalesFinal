using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Maui.Controls.PlatformConfiguration;
using WhispersAndTales.Dictionary;
using WhispersAndTales.Model;
using WhispersAndTales.Model.Interfaces;
using WhispersAndTales.Services;

namespace WhispersAndTales.FileBuilders
{
    public class FileLoader
    {
        private static readonly string _documentsPath = FileSystem.Current.AppDataDirectory;
        public static List<string> RequiredTags { get; set; } = [];
        public static async Task<bool> BuildFromFileAsync(string fileName)
        {
            // Ścieżka docelowa w folderze dokumentów 
            string saveDirectory = "/storage/emulated/0/Android/data/files";
#if ANDROID
            saveDirectory = "/storage/emulated/0/Documents";
#endif
            var parsedElement = "ResFile";
            string targetPath = Path.Combine(saveDirectory, fileName);
            try
            {
                // Ładowanie pliku XML do obiektu XDocument
                XDocument doc = XDocument.Load(targetPath);
                Log.Write("ładowanie pliku zasobów" + targetPath);
                XElement rootElement = doc.Root;
                foreach (var resFileElement in rootElement.Elements())
                {
                    parsedElement = resFileElement.Name.ToString();
                    foreach (ITaged ClassType in MainDictionary.Types)
                    {
                        var builder = ClassType.BuildFromElement(resFileElement);
                        if (ClassType.BuildFromElement(resFileElement) != null)
                            foreach (var SingularElement in resFileElement.Elements())
                            {
                                builder.Parse(SingularElement);
                                MainDictionary.Objectlist.Add((ITaged)builder.Build());
                            }
                    }

                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Write($"Error parsing XML on {parsedElement}: {ex.Message}");
                TextToSpeechService.SpeakAsync("Błąd podczas przetwarzania znacznika: " + parsedElement);
                return false;
            }
        }
        public static async Task<XDocument> LoadXmlDocumentAsync(string fileName)
        {
            var filePath = Path.Combine(_documentsPath, fileName);
            try
            {
                using (var stream = File.OpenRead(filePath))
                {
                    return XDocument.Load(stream);
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Błąd podczas odczytu pliku: {ex.Message}");
                return null;
            }
        }
        public static async Task SaveXmlDocumentAsync(XDocument document, string fileName)
        {
            var filePath = Path.Combine(_documentsPath, fileName);
            try
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await document.SaveAsync(stream, SaveOptions.None, CancellationToken.None);
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Błąd podczas zapisu pliku: {ex.Message}");
            }
        }

        public static async Task<bool> CheckAndRequestStoragePermission()
        {

            var status = PermissionStatus.Granted;

            return status == PermissionStatus.Granted;
        }
        public static async Task TryCopyFileToDocumentsAsync(string fileName = "adventure_scenario.xml")
        {
            try
            {
                // Ścieżka docelowa w folderze dokumentów 
                string saveDirectory = "/storage/emulated/0/Android/data/files";
#if ANDROID
                saveDirectory = "/storage/emulated/0/Documents";
#endif
                string targetPath = Path.Combine(saveDirectory, fileName);

                // Sprawdzenie, czy plik już istnieje
                if (File.Exists(targetPath))
                {
                    Log.Write("Plik już istnieje w folderze." + targetPath);
                    return;
                }

                // Wczytanie pliku z zasobów
                using Stream fileStream = await FileSystem.OpenAppPackageFileAsync(fileName);
                using FileStream outputStream = File.Create(targetPath);

                // Kopiowanie zawartości
                await fileStream.CopyToAsync(outputStream);
                Log.Write($"Plik {fileName} został skopiowany do {targetPath}");
            }
            catch (Exception ex)
            {
                Log.Write($"Błąd podczas kopiowania pliku: {ex.Message}");
            }
        }

    }

}
