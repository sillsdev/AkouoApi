using System.Reflection;

namespace AkouoApi.Utility
{
    public class ResourceHelpers
    {
        public static string LoadResource(string name)
        {
            //Load the file
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith(name));
            using Stream? stream = assembly.GetManifestResourceStream(resourceName);
            if (stream != null)
            {
                using StreamReader reader = new (stream);
                return reader.ReadToEnd();
            }
            return "";
        }
    }
}
