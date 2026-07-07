using System.Diagnostics;
using System.Runtime.InteropServices;

namespace PacketMonitor.App.Services;

public record ProcessInfo(int Pid, string Name)
{
    public override string ToString() => $"{Name} (PID {Pid})";
}

/// <summary>
/// 透過 iphlpapi.dll 的連線表 API，維護「(protocol, local port) -> PID」對照，
/// 因為封包本身不帶 PID 資訊，只能用目前連線表反查。
/// </summary>
public class ProcessConnectionTracker
{
    private Dictionary<(bool isTcp, ushort port), int> _portToPid = new();
    private Dictionary<int, HashSet<ushort>> _pidToTcpPorts = new();
    private Dictionary<int, HashSet<ushort>> _pidToUdpPorts = new();
    private readonly object _lock = new();

    public static List<ProcessInfo> GetRunningProcesses()
    {
        return Process.GetProcesses()
            .Select(p =>
            {
                try
                {
                    return new ProcessInfo(p.Id, p.ProcessName);
                }
                catch
                {
                    return new ProcessInfo(p.Id, "(unknown)");
                }
            })
            .OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public HashSet<ushort> GetTcpPorts(int pid)
    {
        lock (_lock)
        {
            return _pidToTcpPorts.TryGetValue(pid, out var ports) ? new HashSet<ushort>(ports) : [];
        }
    }

    public HashSet<ushort> GetUdpPorts(int pid)
    {
        lock (_lock)
        {
            return _pidToUdpPorts.TryGetValue(pid, out var ports) ? new HashSet<ushort>(ports) : [];
        }
    }

    public int? ResolvePid(bool isTcp, ushort localPort)
    {
        lock (_lock)
        {
            return _portToPid.TryGetValue((isTcp, localPort), out var pid) ? pid : null;
        }
    }

    public void Refresh()
    {
        var portToPid = new Dictionary<(bool, ushort), int>();
        var pidToTcp = new Dictionary<int, HashSet<ushort>>();
        var pidToUdp = new Dictionary<int, HashSet<ushort>>();

        foreach (var (port, pid) in ReadTcpTable())
        {
            portToPid[(true, port)] = pid;
            if (!pidToTcp.TryGetValue(pid, out var set))
                pidToTcp[pid] = set = [];
            set.Add(port);
        }

        foreach (var (port, pid) in ReadUdpTable())
        {
            portToPid[(false, port)] = pid;
            if (!pidToUdp.TryGetValue(pid, out var set))
                pidToUdp[pid] = set = [];
            set.Add(port);
        }

        lock (_lock)
        {
            _portToPid = portToPid;
            _pidToTcpPorts = pidToTcp;
            _pidToUdpPorts = pidToUdp;
        }
    }

    private static IEnumerable<(ushort port, int pid)> ReadTcpTable()
    {
        var bufferSize = 0;
        _ = NativeMethods.GetExtendedTcpTable(IntPtr.Zero, ref bufferSize, false, NativeMethods.AF_INET,
            NativeMethods.TcpTableClass.TCP_TABLE_OWNER_PID_ALL, 0);

        var buffer = Marshal.AllocHGlobal(bufferSize);
        try
        {
            var result = NativeMethods.GetExtendedTcpTable(buffer, ref bufferSize, false, NativeMethods.AF_INET,
                NativeMethods.TcpTableClass.TCP_TABLE_OWNER_PID_ALL, 0);
            if (result != 0)
                yield break;

            var rowCount = Marshal.ReadInt32(buffer);
            var rowPtr = buffer + 4;
            var rowSize = Marshal.SizeOf<NativeMethods.MIB_TCPROW_OWNER_PID>();

            for (var i = 0; i < rowCount; i++)
            {
                var row = Marshal.PtrToStructure<NativeMethods.MIB_TCPROW_OWNER_PID>(rowPtr + i * rowSize);
                yield return (NativeMethods.ExtractPort(row.localPort), (int)row.owningPid);
            }
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    private static IEnumerable<(ushort port, int pid)> ReadUdpTable()
    {
        var bufferSize = 0;
        _ = NativeMethods.GetExtendedUdpTable(IntPtr.Zero, ref bufferSize, false, NativeMethods.AF_INET,
            NativeMethods.UdpTableClass.UDP_TABLE_OWNER_PID, 0);

        var buffer = Marshal.AllocHGlobal(bufferSize);
        try
        {
            var result = NativeMethods.GetExtendedUdpTable(buffer, ref bufferSize, false, NativeMethods.AF_INET,
                NativeMethods.UdpTableClass.UDP_TABLE_OWNER_PID, 0);
            if (result != 0)
                yield break;

            var rowCount = Marshal.ReadInt32(buffer);
            var rowPtr = buffer + 4;
            var rowSize = Marshal.SizeOf<NativeMethods.MIB_UDPROW_OWNER_PID>();

            for (var i = 0; i < rowCount; i++)
            {
                var row = Marshal.PtrToStructure<NativeMethods.MIB_UDPROW_OWNER_PID>(rowPtr + i * rowSize);
                yield return (NativeMethods.ExtractPort(row.localPort), (int)row.owningPid);
            }
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }
}
