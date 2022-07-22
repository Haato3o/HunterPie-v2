using HunterPie.Core.Client;
using HunterPie.Core.Http.Events;
using HunterPie.Core.Logger;
using HunterPie.Update.Remote;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HunterPie.Update
{
    internal class UpdateService
    {
        private readonly UpdateApi api = new();
        string latest;

        public async Task<Version> GetLatestVersion()
        {
            latest = await api.GetLatestVersion();

            if (latest is null)
                return null;

            Version parsed = new Version(latest);

            return parsed;
        }

        public async Task DownloadZip(EventHandler<PoogieDownloadEventArgs> callback)
        {
            await api.DownloadVersion(latest, callback);
        }

        public bool ExtractZip()
        {
            string filePath = ClientInfo.GetPathFor(@"temp/HunterPie.zip");
            string extractPath = ClientInfo.GetPathFor(@"temp/HunterPie");

            try
            {
                ZipFile.ExtractToDirectory(filePath, extractPath);
            }
            catch (Exception err)
            {
                Log.Error(err.ToString());
                Directory.Delete(ClientInfo.GetPathFor("temp"), true);
                return false;
            }

            return true;
        }

        private static string ComputeSHA256Checksum(string filename)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                using (FileStream stream = File.OpenRead(filename))
                {
                    Span<byte> buffer = stackalloc byte[256];
                    buffer = sha256.ComputeHash(stream);

                    StringBuilder builder = new(256 * 2);

                    foreach (byte b in buffer)
                        builder.Append($"{b:x2}");

                    return builder.ToString();
                }
            }
        }

        public async Task<Dictionary<string, string>> IndexAllFilesRecursively(string basePath, string relativePath = "", Dictionary<string, string> files = null)
        {
            if (files is null)
                files = new Dictionary<string, string>();

            foreach (string entry in Directory.GetFileSystemEntries(basePath))
            {
                FileAttributes attrib = File.GetAttributes(entry);
                string absolute = Path.Combine(basePath, entry);
                string relative = Path.Combine(relativePath, Path.GetRelativePath(basePath, entry));

                if ((attrib & FileAttributes.Directory) == FileAttributes.Directory)
                    await IndexAllFilesRecursively(absolute, relative, files);
                else
                    files.Add(relative, ComputeSHA256Checksum(absolute));

            }

            return files;
        }

        public bool ReplaceOldFiles(Dictionary<string, string> local, Dictionary<string, string> remote)
        {
            List<string> files = new();
            foreach (var (path, hash) in remote)
            {
                if (!local.ContainsKey(path))
                {
                    files.Add(path);
                    continue;
                }

                string localHash = local[path];

                if (localHash != hash)
                {
                    string oldFile = ClientInfo.GetPathFor(path) + ".old";
                    File.Move(ClientInfo.GetPathFor(path), oldFile);
                    files.Add(path);
                }
            }

            foreach (string file in files)
            {
                string updatedFile = Path.Combine(ClientInfo.ClientPath, @"temp/HunterPie", file);
                string localFile = ClientInfo.GetPathFor(file);

                if (!File.Exists(localFile))
                    Directory.CreateDirectory(
                        Path.GetDirectoryName(localFile)
                    );
                
                File.Move(updatedFile, localFile);
            }

            // Cleanup
            Directory.Delete(ClientInfo.GetPathFor("temp"), true);

            return true;
        }

        public bool CleanupOldFiles()
        {
            Stack<string> directories = new();

            directories.Push(ClientInfo.ClientPath);

            while (directories.Count > 0)
            {
                foreach (string entry in Directory.GetFileSystemEntries(directories.Pop()))
                {
                    FileAttributes attrib = File.GetAttributes(entry);

                    if ((attrib & FileAttributes.Directory) == FileAttributes.Directory)
                        directories.Push(entry);
                    else
                        if (entry.EndsWith(".old"))
                            try { File.Delete(entry); }
                            catch(Exception err) { Log.Error(err.ToString()); }

                }
            }

            return true;
        }
    }
}
