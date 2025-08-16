using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace Fairmark.Helpers
{
    public class LogHelper : IDisposable
    {
        internal string LogFilePath = ApplicationData.Current.LocalFolder.Path + "\\Fairmark.log";
        private StreamWriter _writer;
        private FileStream _fileStream;
        private bool _isInitialized = false;

        public string logs
        {
            get
            {
                Close();
                string l = System.IO.File.ReadAllText(LogFilePath);
                _ = Task.Run(() => InitializeAsync());
                return l;
            }
        }
        public int logLineCount
        {
            get
            {
                try
                {
                    return System.IO.File.ReadAllLines(LogFilePath).Length;
                }
                catch (Exception)
                {
                    return 0;
                }
            }
        }

        public async Task InitializeAsync()
        {
            if (_isInitialized)
                return;
            try
            {
                StorageFile logfile = await (await ApplicationData.Current.LocalFolder.CreateFolderAsync("Default", CreationCollisionOption.OpenIfExists)).CreateFileAsync("Fairmark.log", CreationCollisionOption.OpenIfExists);
                _fileStream = new FileStream(logfile.Path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                _writer = new StreamWriter(_fileStream) { AutoFlush = true };
                _isInitialized = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to initialize log: {ex.Message}");
            }
        }

        public async Task WriteLogAsync(string message)
        {
            Debug.WriteLine(message);
            if (new Settings().AccessLogs)
            {
                if (!_isInitialized)
                    await InitializeAsync();
                try
                {
                    if (logLineCount > 300)
                    {
                        _writer.Dispose();
                        _fileStream.Dispose();
                        System.IO.File.WriteAllText(LogFilePath, string.Empty);
                        _fileStream = new FileStream(LogFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                        _writer = new StreamWriter(_fileStream) { AutoFlush = true };
                        _writer.WriteLine($"{DateTime.Now}: Log file cleared after 300 lines.");
                    }
                    await _writer.WriteLineAsync($"{DateTime.Now}: {message}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to write log: {ex.Message}");
                }
            }
        }



        public void Close()
        {
            try
            {
                _writer?.Dispose();
                _fileStream?.Dispose();
                _isInitialized = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to close log: {ex.Message}");
            }
        }

        public void Dispose()
        {
            Close();
        }
        public void WriteLog(string message)
        {
            _ = Task.Run(() => WriteLogAsync(message));
        }
    }
}
