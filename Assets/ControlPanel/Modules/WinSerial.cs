using System;
using System.IO;
using System.Runtime.InteropServices;

// System.IO.Ports works in the Mono editor and is missing from IL2CPP player builds.
// This talks to the COM port through kernel32, which the Windows player can call.
static class WinSerial
{
    const uint GenericRead = 0x80000000;
    const uint GenericWrite = 0x40000000;
    const uint OpenExisting = 3;
    const uint PurgeRxClear = 0x0008;
    const uint PurgeTxClear = 0x0004;
    const int ErrorTimeout = 1460;

    static readonly IntPtr InvalidHandle = new IntPtr(-1);

    public static IntPtr Open(string portName, int baudRate)
    {
        string path = portName.StartsWith(@"\\.\", StringComparison.Ordinal) ? portName : @"\\.\" + portName;
        IntPtr handle = CreateFile(path, GenericRead | GenericWrite, 0, IntPtr.Zero, OpenExisting, 0, IntPtr.Zero);
        if (handle == IntPtr.Zero || handle == InvalidHandle)
            throw new IOException("Could not open " + portName + ". Windows error " + Marshal.GetLastWin32Error() + ".");

        try
        {
            var state = new Dcb();
            state.Length = Marshal.SizeOf(state);
            if (!GetCommState(handle, ref state))
                throw new IOException("Could not read " + portName + " settings. Windows error " + Marshal.GetLastWin32Error() + ".");

            state.BaudRate = (uint)baudRate;
            state.ByteSize = 8;
            state.Parity = 0;
            state.StopBits = 0;
            state.Flags = 1 | 0x10 | 0x1000;
            if (!SetCommState(handle, ref state))
                throw new IOException("Could not set " + portName + " to " + baudRate + ". Windows error " + Marshal.GetLastWin32Error() + ".");

            var timeouts = new CommTimeouts
            {
                ReadIntervalTimeout = 50,
                ReadTotalTimeoutConstant = 500,
                WriteTotalTimeoutConstant = 500
            };
            SetCommTimeouts(handle, ref timeouts);
            SetupComm(handle, 4096, 4096);
            Discard(handle);
            EscapeCommFunction(handle, 5);
            EscapeCommFunction(handle, 3);
            return handle;
        }
        catch
        {
            Close(handle);
            throw;
        }
    }

    public static int Read(IntPtr handle, byte[] buffer)
    {
        if (!ReadFile(handle, buffer, buffer.Length, out int read, IntPtr.Zero))
        {
            int error = Marshal.GetLastWin32Error();
            if (error == 0 || error == ErrorTimeout)
                return 0;
            throw new IOException("Serial read failed. Windows error " + error + ".");
        }
        return read;
    }

    public static void Write(IntPtr handle, string message)
    {
        byte[] data = System.Text.Encoding.ASCII.GetBytes(message + "\n");
        if (!WriteFile(handle, data, data.Length, out _, IntPtr.Zero))
            throw new IOException("Serial write failed. Windows error " + Marshal.GetLastWin32Error() + ".");
    }

    public static void Discard(IntPtr handle)
    {
        if (handle != IntPtr.Zero && handle != InvalidHandle)
            PurgeComm(handle, PurgeRxClear | PurgeTxClear);
    }

    public static void Close(IntPtr handle)
    {
        if (handle != IntPtr.Zero && handle != InvalidHandle)
            CloseHandle(handle);
    }

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    static extern IntPtr CreateFile(string name, uint access, uint share, IntPtr security, uint disposition, uint flags, IntPtr template);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool ReadFile(IntPtr handle, byte[] buffer, int count, out int read, IntPtr overlapped);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool WriteFile(IntPtr handle, byte[] buffer, int count, out int written, IntPtr overlapped);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool CloseHandle(IntPtr handle);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool GetCommState(IntPtr handle, ref Dcb state);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool SetCommState(IntPtr handle, ref Dcb state);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool SetCommTimeouts(IntPtr handle, ref CommTimeouts timeouts);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool SetupComm(IntPtr handle, int inQueue, int outQueue);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool PurgeComm(IntPtr handle, uint flags);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool EscapeCommFunction(IntPtr handle, uint function);

    [StructLayout(LayoutKind.Sequential)]
    struct Dcb
    {
        public int Length;
        public uint BaudRate;
        public uint Flags;
        public ushort Reserved;
        public ushort XonLim;
        public ushort XoffLim;
        public byte ByteSize;
        public byte Parity;
        public byte StopBits;
        public byte XonChar;
        public byte XoffChar;
        public byte ErrorChar;
        public byte EofChar;
        public byte EvtChar;
        public ushort Reserved1;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct CommTimeouts
    {
        public uint ReadIntervalTimeout;
        public uint ReadTotalTimeoutMultiplier;
        public uint ReadTotalTimeoutConstant;
        public uint WriteTotalTimeoutMultiplier;
        public uint WriteTotalTimeoutConstant;
    }
}
