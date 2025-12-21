using System.IO;
using UnityEngine;

namespace HitTrax.UnityUtilities
{
    public static class FileService
    {
        public static string ApplicationFolderPath(string folderName)
            => Path.Combine(Application.persistentDataPath, folderName);

        public static string ApplicationFilePath(string folderName, string fileName)
            => Path.Combine(ApplicationFolderPath(folderName), fileName);

        public static int ApplicationFilePathCount(string folderName)
        {
                 // Combine the persistent data path with your desired folder name
                string path = ApplicationFolderPath(folderName);

                // Check if the directory exists before attempting to count files
                if (Directory.Exists(path))
                {
                    // Get an array of file paths in the directory
                    string[] files = Directory.GetFiles(path);

                    // Return the count of files found
                    Debug.Log($"Found {files.Length} files in {path}");
                    return files.Length;
                }
                else
                {
                    Debug.LogWarning("Directory not found: " + path);
                    return 0;
                }
        }

        public static void SaveData(string data, string folderName, string fileName)
        {
            // Define the path where the file will be saved
            // Application.persistentDataPath is a good choice for persistent data across game sessions and platforms
            string path = ApplicationFolderPath(folderName);

            // Create the directory if it doesn't exist
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            // Combine the path and file name
            string filePath = Path.Combine(path, fileName);

            try
            {
                // Write the string data to the file
                File.WriteAllText(filePath, data);
                Debug.Log("Saving To: " + filePath);
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Error saving data: " + ex.Message);
            }
        }

    }
}
