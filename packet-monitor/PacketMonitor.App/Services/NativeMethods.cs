using System.Runtime.InteropServices;

namespace PacketMonitor.App.Services;

internal static class NativeMethods
{
    internal const int AF_INET = 2;
    internal const int ERROR_INSUFFICIENT_BUFFER = 122;

    internal enum TcpTableClass
    {
        TCP_TABLE_OWNER_PID_ALL = 5
    }

    internal enum UdpTableClass
    {
        UDP_TABLE_OWNER_PID = 1
    }

    [DllImport("iphlpapi.dll", SetLastError = true)]
    internal static extern uint GetExtendedTcpTable(
        IntPtr pTcpTable,
        ref int dwOutBufLen,
        bool sort,
        int ipVersion,
        TcpTableClass tblClass,
        int reserved);

    [DllImport("iphlpapi.dll", SetLastError = true)]
    internal static extern uint GetExtendedUdpTable(
        IntPtr pUdpTable,
        ref int dwOutBufLen,
        bool sort,
        int ipVersion,
        UdpTableClass tblClass,
        int reserved);

    [StructLayout(LayoutKind.Sequential)]
    internal struct MIB_TCPROW_OWNER_PID
    {
        public uint state;
        public uint localAddr;
        public uint localPort;
        public uint remoteAddr;
        public uint remotePort;
        public uint owningPid;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MIB_UDPROW_OWNER_PID
    {
        public uint localAddr;
        public uint localPort;
        public uint owningPid;
    }

    /// <summary>dwLocalPort 是網路位元組順序存在 DWORD 低 16 位元，需轉回 host order。</summary>
    internal static ushort ExtractPort(uint rawPort)
    {
        return (ushort)(((rawPort & 0xFF) << 8) | ((rawPort >> 8) & 0xFF));
    }
}
