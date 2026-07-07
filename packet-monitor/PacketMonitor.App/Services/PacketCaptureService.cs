using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using PacketDotNet;
using PacketMonitor.App.Models;
using SharpPcap;

namespace PacketMonitor.App.Services;

/// <summary>
/// 包裝 SharpPcap 擷取裝置：依目標 process 目前擁有的 port 組出 BPF filter 縮小擷取範圍，
/// 並在收到封包時再次以連線表比對 PID，避免 filter 因為 port 變動而短暫失準。
/// </summary>
public class PacketCaptureService : IDisposable
{
    private readonly ProcessConnectionTracker _tracker;
    private readonly HashSet<IPAddress> _localAddresses = GetLocalIPv4Addresses();

    private ICaptureDevice? _device;
    private System.Threading.Timer? _filterRefreshTimer;
    private int _targetPid;
    private Action<PacketEntry>? _onPacket;

    public PacketCaptureService(ProcessConnectionTracker tracker)
    {
        _tracker = tracker;
    }

    public static IReadOnlyList<ICaptureDevice> GetDevices() => CaptureDeviceList.Instance;

    /// <summary>找出目前實際對外連網（有預設閘道）的網卡，用來預先選取下拉選單。</summary>
    public static ICaptureDevice? GetDefaultDevice(IEnumerable<ICaptureDevice> devices)
    {
        var defaultNic = NetworkInterface.GetAllNetworkInterfaces()
            .Where(n => n.OperationalStatus == OperationalStatus.Up)
            .FirstOrDefault(n => n.GetIPProperties().GatewayAddresses
                .Any(g => g.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork));

        if (defaultNic == null)
            return null;

        var guid = defaultNic.Id.Trim('{', '}');
        return devices.FirstOrDefault(d => d.Name.Contains(guid, StringComparison.OrdinalIgnoreCase));
    }

    public void Start(ICaptureDevice device, int pid, Action<PacketEntry> onPacket)
    {
        Stop();

        _targetPid = pid;
        _onPacket = onPacket;
        _tracker.Refresh();

        device.Open(DeviceModes.Promiscuous, 1000);
        ApplyFilter(device, pid);
        device.OnPacketArrival += OnPacketArrival;
        device.StartCapture();
        _device = device;

        _filterRefreshTimer = new System.Threading.Timer(_ =>
        {
            _tracker.Refresh();
            ApplyFilter(device, pid);
        }, null, 1500, 1500);
    }

    public void Stop()
    {
        _filterRefreshTimer?.Dispose();
        _filterRefreshTimer = null;

        if (_device != null)
        {
            _device.OnPacketArrival -= OnPacketArrival;
            try
            {
                _device.StopCapture();
            }
            catch
            {
                // 裝置可能已經關閉，忽略
            }
            _device.Close();
            _device = null;
        }
    }

    private void ApplyFilter(ICaptureDevice device, int pid)
    {
        var tcpPorts = _tracker.GetTcpPorts(pid);
        var udpPorts = _tracker.GetUdpPorts(pid);

        if (tcpPorts.Count == 0 && udpPorts.Count == 0)
        {
            // 目前沒有已知連線，先設定一個恆假的 filter，避免抓到不相干的全量流量
            device.Filter = "greater 65535";
            return;
        }

        var clauses = tcpPorts.Select(p => $"tcp port {p}")
            .Concat(udpPorts.Select(p => $"udp port {p}"));
        device.Filter = string.Join(" or ", clauses);
    }

    private void OnPacketArrival(object sender, PacketCapture e)
    {
        try
        {
            var raw = e.GetPacket();
            var packet = Packet.ParsePacket(raw.LinkLayerType, raw.Data);
            var ipPacket = packet.Extract<IPPacket>();
            if (ipPacket == null)
                return;

            var tcpPacket = packet.Extract<TcpPacket>();
            var udpPacket = packet.Extract<UdpPacket>();

            bool isTcp;
            ushort srcPort, dstPort;
            byte[] payload;
            string protocolName;

            if (tcpPacket != null)
            {
                isTcp = true;
                srcPort = tcpPacket.SourcePort;
                dstPort = tcpPacket.DestinationPort;
                payload = tcpPacket.PayloadData ?? [];
                protocolName = "TCP";
            }
            else if (udpPacket != null)
            {
                isTcp = false;
                srcPort = udpPacket.SourcePort;
                dstPort = udpPacket.DestinationPort;
                payload = udpPacket.PayloadData ?? [];
                protocolName = "UDP";
            }
            else
            {
                return;
            }

            var srcIsLocal = _localAddresses.Contains(ipPacket.SourceAddress);
            var dstIsLocal = _localAddresses.Contains(ipPacket.DestinationAddress);

            var direction = srcIsLocal ? PacketDirection.Send
                : dstIsLocal ? PacketDirection.Receive
                : PacketDirection.Unknown;

            var localPort = srcIsLocal ? srcPort : dstPort;

            var resolvedPid = _tracker.ResolvePid(isTcp, localPort);
            if (resolvedPid != _targetPid)
                return;

            var processName = TryGetProcessName(resolvedPid.Value);

            var entry = new PacketEntry
            {
                Timestamp = raw.Timeval.Date,
                Direction = direction,
                Protocol = protocolName,
                LocalEndPoint = srcIsLocal
                    ? $"{ipPacket.SourceAddress}:{srcPort}"
                    : $"{ipPacket.DestinationAddress}:{dstPort}",
                RemoteEndPoint = srcIsLocal
                    ? $"{ipPacket.DestinationAddress}:{dstPort}"
                    : $"{ipPacket.SourceAddress}:{srcPort}",
                Length = raw.Data.Length,
                ProcessName = processName,
                Pid = resolvedPid.Value,
                Payload = payload
            };

            _onPacket?.Invoke(entry);
        }
        catch
        {
            // 單一封包解析失敗不應中斷整個擷取
        }
    }

    private static string TryGetProcessName(int pid)
    {
        try
        {
            return Process.GetProcessById(pid).ProcessName;
        }
        catch
        {
            return "(已結束)";
        }
    }

    private static HashSet<IPAddress> GetLocalIPv4Addresses()
    {
        var addresses = new HashSet<IPAddress>();
        foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
        {
            foreach (var addr in nic.GetIPProperties().UnicastAddresses)
            {
                if (addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    addresses.Add(addr.Address);
            }
        }
        return addresses;
    }

    public void Dispose() => Stop();
}
