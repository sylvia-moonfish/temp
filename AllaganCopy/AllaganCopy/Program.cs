using System.IO;
using System.Reflection;

namespace AllaganCopy
{
    class Program
    {
        static void Main()
        {
            // check paths.
            string globalPath = @"C:\Program Files (x86)\SquareEnix\FINAL FANTASY XIV - A Realm Reborn\game";
            if (!Directory.Exists(globalPath)) return;

            string koreanPath = @"C:\Program Files (x86)\FINAL FANTASY XIV - KOREA\game";
            if (!Directory.Exists(koreanPath)) return;

            // check and clean release path.
            string releasePath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "..", "..", "..", "..", "..", "release"));

            if (Directory.Exists(releasePath))
            {
                Directory.Delete(releasePath, true);
            }

            Directory.CreateDirectory(releasePath);

            // update game version.
            File.Copy(Path.Combine(globalPath, "ffxivgame.ver"), Path.Combine(releasePath, "ffxivgame.ver"));

            // check and clean release path for mac font files.
            string releaseMacPath = Path.Combine(releasePath, "mac");

            if (Directory.Exists(releaseMacPath))
            {
                Directory.Delete(releaseMacPath);
            }

            Directory.CreateDirectory(releaseMacPath);

            MacFont.ProduceMacFont(globalPath, koreanPath, releaseMacPath);
        }
    }
}
