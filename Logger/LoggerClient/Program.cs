
using System;
using System.IO.MemoryMappedFiles;
using System.Text;
using System.Threading;
using SharedLoggerLib;

class Program
{
    static void Main()
    {
        using var mmf = MemoryMappedFile.CreateOrOpen(SharedConstants.MemoryName,
            SharedConstants.HeaderSize + SharedConstants.SlotCount * SharedConstants.SlotSize);
        using var accessor = mmf.CreateViewAccessor();
        using var mutex = new Mutex(false, SharedConstants.MutexName);

        mutex.WaitOne();
        try
        {
            int writeIndex = accessor.ReadInt32(0);
            int slot = writeIndex % SharedConstants.SlotCount;
            int offset = SharedConstants.HeaderSize + slot * SharedConstants.SlotSize;

            byte status = accessor.ReadByte(offset + SharedConstants.StatusOffset);
            if (status == (byte)SlotStatus.Full)
            {
                Console.WriteLine("Slot full. Skipping.");
                return;
            }

            accessor.Write(0, writeIndex + 1); // update writeIndex

            // Write log message
            accessor.Write(offset + SharedConstants.StatusOffset, (byte)SlotStatus.Full);
            string msg = $"Log from client at {DateTime.UtcNow:O}";
            byte[] bytes = Encoding.UTF8.GetBytes(msg);
            accessor.WriteArray(offset + SharedConstants.DataOffset, bytes, 0, Math.Min(bytes.Length, SharedConstants.SlotSize - 1));
        }
        finally
        {
            mutex.ReleaseMutex();
        }

        Console.WriteLine("Log written to shared memory.");
    }

    private enum SlotStatus : byte
    {
        Empty = 0,
        Full = 1,
        Read = 2
    }
}
