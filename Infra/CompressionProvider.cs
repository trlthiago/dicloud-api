using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

namespace dicloud_api
{
    public static class CompressionProvider
    {
        public const string Path = @"C:\Program Files\7-Zip\7z.exe";

        public static async Task<string> DecompressAsync(string path)
        {
            var escapedArgs = $"x {path}";
            await StartAsync(escapedArgs);
            return "";
        }

        public static async Task<string> CompressAsync(string path)
        {
            System.IO.FileInfo file = new System.IO.FileInfo(path);

            var zipName = file.Name.IndexOf('.') != -1
                            ? file.FullName.Replace(file.Extension, ".7z")
                            : file.FullName + ".7z";

            var escapedArgs = $"a {zipName} {path}";

            await StartAsync(escapedArgs);

            return zipName;
        }

        public static async Task StartAsync(string escapedArgs)
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = Path,
                    Arguments = escapedArgs,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                }
            };

            process.Start();
            string result = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync(); //61000
        }
    }
}
