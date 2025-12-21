using System.IO;

namespace HitTrax.CoreUtilities
{
    // TODO: Move this to Unity Utilities since it references Unity Engine 
    public static class JsonUtilities
    {
        public static Safe<T> DeserializeJsonFromPath<T>(this string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    string content = File.ReadAllText(path);
                    return UnityEngine.JsonUtility.FromJson<T>(content);
                }
            }
            catch
            {
                return new Safe<T>();
            }

            return new Safe<T>();
        }
    }
}
