
using System;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;
using SharedLoggerLib;

namespace SharedLoggerClientLib
{
    public class SharedLogClient
    {
        private readonly MemoryMappedFile _mmf;
        private readonly MemoryMappedViewAccessor _accessor;
        private readonly Mutex _mutex;
        private readonly string _application;

        [SupportedOSPlatform("windows")]
        public SharedLogClient(string application)
        {
            _application = application;

            // Check if LoggerServer process is running
            var processes = Process.GetProcessesByName("LoggerServer");
            if (processes.Length == 0)
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "LoggerServer.exe",
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Failed to start LoggerServer process.", ex);
                }
            }

            _mmf = MemoryMappedFile.CreateOrOpen(SharedConstants.MemoryName,
                SharedConstants.HeaderSize + SharedConstants.SlotCount * SharedConstants.SlotSize);
            _accessor = _mmf.CreateViewAccessor();
            _mutex = new Mutex(false, SharedConstants.MutexName);
        }

        [SupportedOSPlatform("windows")]
        public void Log(string instance, LogLevel level, string message)
        {
            _mutex.WaitOne();
            try
            {
                int writeIndex = _accessor.ReadInt32(0);
                int slot = writeIndex % SharedConstants.SlotCount;
                int offset = SharedConstants.HeaderSize + slot * SharedConstants.SlotSize;

                byte status = _accessor.ReadByte(offset + SharedConstants.StatusOffset);
                if (status == (byte)SlotStatus.Full)
                    return; // drop log if slot still full

                _accessor.Write(0, writeIndex + 1); // increment write index
                _accessor.Write(offset + SharedConstants.StatusOffset, (byte)SlotStatus.Full);

                Span<byte> buffer = stackalloc byte[SharedConstants.SlotSize - 1];
                var id = Guid.NewGuid();
                var timestamp = DateTime.UtcNow.Ticks;
                Encoding.UTF8.GetBytes(_application.PadRight(32).Substring(0, 32), buffer.Slice(24, 32));
                Encoding.UTF8.GetBytes(instance.PadRight(32).Substring(0, 32), buffer.Slice(56, 32));
                Encoding.UTF8.GetBytes(message.PadRight(160).Substring(0, 160), buffer.Slice(88, 160));

                id.TryWriteBytes(buffer.Slice(0, 16));
                BitConverter.TryWriteBytes(buffer.Slice(16, 8), timestamp);
                buffer[248] = (byte)level;

                _accessor.WriteArray(offset + SharedConstants.DataOffset, buffer.ToArray(), 0, buffer.Length);
            }
            finally
            {
                _mutex.ReleaseMutex();
            }
        }

        private enum SlotStatus : byte
        {
            Empty = 0,
            Full = 1,
            Read = 2
        }
    }
}
