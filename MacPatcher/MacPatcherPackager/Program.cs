using System.IO;
using System.Reflection;

namespace MacPatcherPackager
{
    class Program
    {
        static void Main()
        {
            string solutionPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "..", "..", "..", ".."));
            string binPath = Path.GetFullPath(Path.Combine(solutionPath, "MacPatcher", "bin"));
            string releasePath = Path.GetFullPath(Path.Combine(binPath, "Release"));
            string publishPath = Path.GetFullPath(Path.Combine(releasePath, "net5.0", "osx-x64", "publish"));

            // check if publish path is correct.
            if (!Directory.Exists(publishPath)) return;

            // clean and create temp directory under publish.
            string tempPath = Path.GetFullPath(Path.Combine(publishPath, "temp"));

            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }

            Directory.CreateDirectory(tempPath);

            // create Contents directory.
            string contentsPath = Path.GetFullPath(Path.Combine(tempPath, "Contents"));
            Directory.CreateDirectory(contentsPath);

            // create MacOS directory.
            string macOSPath = Path.GetFullPath(Path.Combine(contentsPath, "MacOS"));
            Directory.CreateDirectory(macOSPath);

            // move files to MacOS.
            string[] files = Directory.GetFiles(publishPath);
            foreach (string file in files)
            {
                File.Move(file, Path.GetFullPath(Path.Combine(macOSPath, Path.GetFileName(file))), true);
            }

            // create and write plist
            using (StreamWriter sw = new StreamWriter(Path.GetFullPath(Path.Combine(contentsPath, "Info.plist")), false))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("<!DOCTYPE plist PUBLIC \"-//Apple//DTD PLIST 1.0//EN\" \"http://www.apple.com/DTDs/PropertyList-1.0.dtd\">");
                sw.WriteLine("<plist version=\"1.0\">");
                sw.WriteLine("<dict>");
                sw.WriteLine("\t<key>CFBundleExecutable</key>");
                sw.WriteLine("\t<string>MacPatcher</string>");
                sw.WriteLine("\t<key>CFBundleName</key>");
                sw.WriteLine("\t<string>MacPatcher</string>");
                sw.WriteLine("</dict>");
                sw.WriteLine("</plist>");
            }

            string appPath = Path.GetFullPath(Path.Combine(solutionPath, "MacPatcher.app"));

            if (Directory.Exists(appPath))
            {
                Directory.Delete(appPath, true);
            }

            // move to solution path and rename as app.
            Directory.Move(tempPath, appPath);

            // clean bin.
            Directory.Delete(binPath, true);
        }
    }
}
