using Automatix.TikTokUploader.Services;

namespace Automatix.TikTokUploader.Helpers;

public class FolderHelper
{
    public static void CreateFolder(string path)
    {
        if (!Directory.Exists(path))
        {
            try
            {
                Directory.CreateDirectory(path);
                LogService.Log($"Folder {path} created successfully.");
            }
            catch (Exception ex)
            {
                LogService.Log($"Error during create folder {path}: " + ex.Message);
            }
        }
    }
}