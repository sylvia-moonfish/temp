using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace MacPatcher
{
    class Program
    {
        static void Main()
        {
            string versionUrl = "https://github.com/sylvia-moonfish/temp/raw/main/release/ffxivgame.ver";
            string dat0Url = "https://github.com/sylvia-moonfish/temp/raw/main/release/mac/000000.win32.dat0";
            string dat1Url = "https://github.com/sylvia-moonfish/temp/raw/main/release/mac/000000.win32.dat1";
            string indexUrl = "https://github.com/sylvia-moonfish/temp/raw/main/release/mac/000000.win32.index";

            // check if ffxiv exists.
            string basePath = string.Format("/Users/{0}/Library/Application Support/FINAL FANTASY XIV ONLINE/Bottles/published_Final_Fantasy/drive_c/Program Files (x86)/SquareEnix/FINAL FANTASY XIV - A Realm Reborn/game/", Environment.UserName);

            if (!Directory.Exists(basePath))
            {
                Console.WriteLine("ERR: Final Fantasy XIV is not found.");
                return;
            }

            // read version number.
            string version = File.ReadAllText(Path.GetFullPath(Path.Combine(basePath, "ffxivgame.ver"))).Trim();

            // grab version number from github release.
            HttpClient client = new HttpClient();
            HttpResponseMessage responseMessage = GetAsync(client, versionUrl);
            Task<string> stringTask = responseMessage.Content.ReadAsStringAsync();
            stringTask.Wait();
            string response = stringTask.Result.Trim();

            // if github release version is different from client version, abort patching.
            if (version != response)
            {
                Console.WriteLine("ERR: Client version is different from the server version.");
                Console.WriteLine(string.Format("ERR: {0}", version));
                return;
            }

            Console.WriteLine(string.Format("Version found: {0}. Patching...", version));

            string ffxivPath = Path.GetFullPath(Path.Combine(basePath, "sqpack", "ffxiv"));

            // download and overwrite data files.
            Console.WriteLine("Patching dat0...");
            downloadFile(client, dat0Url, Path.GetFullPath(Path.Combine(ffxivPath, "000000.win32.dat0")));

            Console.WriteLine("Patching dat1...");
            downloadFile(client, dat1Url, Path.GetFullPath(Path.Combine(ffxivPath, "000000.win32.dat1")));

            Console.WriteLine("Patching index...");
            downloadFile(client, indexUrl, Path.GetFullPath(Path.Combine(ffxivPath, "000000.win32.index")));

            Console.WriteLine("Patch complete!");
        }

        static HttpResponseMessage GetAsync(HttpClient client, string url)
        {
            Task<HttpResponseMessage> responseTask = client.GetAsync(url);
            responseTask.Wait();
            return responseTask.Result;
        }

        static void downloadFile(HttpClient client, string url, string downloadPath)
        {
            HttpResponseMessage responseMessage = GetAsync(client, url);
            Task<byte[]> bytesTask = responseMessage.Content.ReadAsByteArrayAsync();
            bytesTask.Wait();
            byte[] bytes = bytesTask.Result;
            File.WriteAllBytes(downloadPath, bytes);
        }
    }
}
