
using System;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using SharedLoggerLib;

/// <summary>
/// Logger server program that reads log entries from shared memory and writes them to rotating binary log files.
/// </summary>
class Program
{
    /// <summary>
    /// The current log file name.
    /// </summary>
    static string currentFileName;

    /// <summary>
    /// The file stream for the current log file.
    /// </summary>
    static FileStream stream;

    /// <summary>
    /// The binary writer for the current log file.
    /// </summary>
    static BinaryWriter writer;

    /// <summary>
    /// Flag indicating whether the logger server is running.
    /// </summary>
    static bool running = true;

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool SetProcessShutdownParameters(uint dwLevel, uint dwFlags);

    /// <summary>
    /// Entry point for the logger server. Continuously reads log entries from shared memory and writes them to disk.
    /// </summary>
    static void Main()
    {
        var _mutex = new Mutex(true, "SingleInstance", out var createdNew);
        if (!createdNew)
            return;

        try
        {
            // Set this high so your app exits late in shutdown order
            SetProcessShutdownParameters(0x4FF, 0); // level 0x000 to 0x4FF

            // Handle process exit to ensure cleanup
            AppDomain.CurrentDomain.ProcessExit += OnExit;
            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true; // prevent immediate exit
                running = false;
            };

            using var mmf = MemoryMappedFile.CreateOrOpen(SharedConstants.MemoryName,
                SharedConstants.HeaderSize + SharedConstants.SlotCount * SharedConstants.SlotSize);
            using var accessor = mmf.CreateViewAccessor();
            using var mutex = new Mutex(false, SharedConstants.MutexName);

            InitLogFile();

            Console.WriteLine("Logger server started. Reading logs...");

            while (running)
            {
                mutex.WaitOne();
                try
                {
                    int readIndex = accessor.ReadInt32(4);
                    int writeIndex = accessor.ReadInt32(0);
                    if (readIndex == writeIndex)
                    {
                        Thread.Sleep(100);
                        continue;
                    }

                    int slot = readIndex % SharedConstants.SlotCount;
                    int offset = SharedConstants.HeaderSize + slot * SharedConstants.SlotSize;

                    byte status = accessor.ReadByte(offset + SharedConstants.StatusOffset);
                    if (status == (byte)1) // Full
                    {
                        byte[] buffer = new byte[SharedConstants.SlotSize - 1];
                        accessor.ReadArray(offset + SharedConstants.DataOffset, buffer, 0, buffer.Length);

                        var id = new Guid(new ReadOnlySpan<byte>(buffer, 0, 16));
                        var timestamp = new DateTime(BitConverter.ToInt64(buffer, 16), DateTimeKind.Local);
                        var application = Encoding.UTF8.GetString(buffer, 24, 32).TrimEnd();
                        var instance = Encoding.UTF8.GetString(buffer, 56, 32).TrimEnd();
                        var message = Encoding.UTF8.GetString(buffer, 88, 160).TrimEnd();
                        var level = (LogLevel)buffer[248];

                        RotateLogFileIfNeeded();

                        WriteLogEntry(id, timestamp, level, application, instance, message);

                        accessor.Write(offset + SharedConstants.StatusOffset, (byte)2); // mark as read
                        accessor.Write(4, readIndex + 1); // update readIndex
                    }
                }
                finally
                {
                    mutex.ReleaseMutex();
                }

                Thread.Sleep(50);
            }
        }
        catch (Exception ex)
        {
            EventLog.WriteEntry("ATMApp", $"Error occurred in logger server: {ex.Message}", EventLogEntryType.Error);
            throw;
        }
    }

    /// <summary>
    /// Initializes the log file and associated writer.
    /// </summary>
    static void InitLogFile()
    {
        currentFileName = $@"C:\ProgramData\NextGen\Logs\ATMAppLog.bin";
        if(!File.Exists(currentFileName))
            File.Create(currentFileName);
        stream = new FileStream(currentFileName, FileMode.Append, FileAccess.Write, FileShare.Read);
        writer = new BinaryWriter(stream);
    }

    /// <summary>
    /// Rotates the log file if it exceeds 100 MB in size.
    /// </summary>
    static void RotateLogFileIfNeeded()
    {
        if (stream.Length >= 100 * 1024 * 1024)
        {
            writer.Dispose();
            stream.Dispose();
            File.Move(currentFileName, @$"Backups\{currentFileName}_{DateTime.Now.ToString("MMddyyyy-HHmmss")}.bak", true);
            InitLogFile();
        }
    }

    /// <summary>
    /// Writes a single log entry to the current log file.
    /// </summary>
    /// <param name="id">The unique identifier for the log entry.</param>
    /// <param name="timestamp">The timestamp of the log entry.</param>
    /// <param name="level">The log level.</param>
    /// <param name="application">The application name.</param>
    /// <param name="instance">The instance name.</param>
    /// <param name="message">The log message.</param>
    static void WriteLogEntry(Guid id, DateTime timestamp, LogLevel level, string application, string instance, string message)
    {
        writer.Write(id.ToByteArray());
        writer.Write(timestamp.ToUniversalTime().Ticks);
        writer.Write((byte)level);
        writer.Write(Encoding.UTF8.GetBytes(application.PadRight(32).Substring(0, 32)));
        writer.Write(Encoding.UTF8.GetBytes(instance.PadRight(32).Substring(0, 32)));
        writer.Write(Encoding.UTF8.GetBytes(message.PadRight(160).Substring(0, 160)));
        writer.Flush();
    }

    /// <summary>
    /// Handles process exit events to ensure a graceful shutdown and resource cleanup.
    /// </summary>
    /// <param name="sender">The event sender (not used).</param>
    /// <param name="e">The event arguments.</param>
    static void OnExit(object sender, EventArgs e)
    {
        running = false;
        Cleanup();
    }

    /// <summary>
    /// Cleans up resources by flushing and disposing the writer and stream.
    /// Handles any exceptions that may occur during cleanup.
    /// </summary>
    static void Cleanup()
    {
        try
        {
            writer?.Flush();
            writer?.Dispose();
            stream?.Dispose();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error during shutdown: " + ex.Message);
        }
    }
}
