
using System;

namespace SharedLoggerLib
{
    public static class SharedConstants
    {
        public const string MemoryName = "SHARED_LOG_MEMORY";
        public const string MutexName = "SHARED_LOG_MUTEX";
        public const int SlotCount = 1000;
        // Update slot size to accommodate 2000 characters
        public const int SlotSize = 16 + 8 + 1 + 32 + 32 + 2000 + 1; // 2090 bytes total
        public const int HeaderSize = 8;
        public const int StatusOffset = 0;
        public const int DataOffset = 1;
    }

    public enum LogLevel : byte
    {
        Trace, Debug, Info, Warning, Error, Fatal
    }

    public readonly struct LogEntry
    {
        public Guid Id { get; }
        public DateTime Timestamp { get; }
        public string Application { get; }
        public string Instance { get; }
        public LogLevel Level { get; }
        public string Message { get; }

        public LogEntry(Guid id, DateTime timestamp, string application, string instance, LogLevel level, string message)
        {
            Id = id;
            Timestamp = timestamp;
            Application = application;
            Instance = instance;
            Level = level;
            Message = message;
        }
    }
}
