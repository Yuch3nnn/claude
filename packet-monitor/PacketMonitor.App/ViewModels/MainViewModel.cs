using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using PacketMonitor.App.Models;
using PacketMonitor.App.Services;
using SharpPcap;

namespace PacketMonitor.App.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private const int MaxPackets = 5000;

    private readonly ProcessConnectionTracker _tracker = new();
    private readonly PacketCaptureService _captureService;

    private ProcessInfo? _selectedProcess;
    private ICaptureDevice? _selectedDevice;
    private PacketEntry? _selectedPacket;
    private bool _isCapturing;
    private string _statusText = "尚未開始擷取";

    public MainViewModel()
    {
        _captureService = new PacketCaptureService(_tracker);

        Devices = new ObservableCollection<ICaptureDevice>(PacketCaptureService.GetDevices());
        SelectedDevice = PacketCaptureService.GetDefaultDevice(Devices);
        RefreshProcesses();

        RefreshProcessesCommand = new RelayCommand(RefreshProcesses);
        StartCommand = new RelayCommand(StartCapture, () => !IsCapturing && SelectedProcess != null && SelectedDevice != null);
        StopCommand = new RelayCommand(StopCapture, () => IsCapturing);
    }

    public ObservableCollection<ProcessInfo> Processes { get; } = [];
    public ObservableCollection<ICaptureDevice> Devices { get; }
    public ObservableCollection<PacketEntry> Packets { get; } = [];

    public ProcessInfo? SelectedProcess
    {
        get => _selectedProcess;
        set => SetField(ref _selectedProcess, value);
    }

    public ICaptureDevice? SelectedDevice
    {
        get => _selectedDevice;
        set => SetField(ref _selectedDevice, value);
    }

    public PacketEntry? SelectedPacket
    {
        get => _selectedPacket;
        set => SetField(ref _selectedPacket, value);
    }

    public bool IsCapturing
    {
        get => _isCapturing;
        private set => SetField(ref _isCapturing, value);
    }

    public string StatusText
    {
        get => _statusText;
        private set => SetField(ref _statusText, value);
    }

    public ICommand RefreshProcessesCommand { get; }
    public ICommand StartCommand { get; }
    public ICommand StopCommand { get; }

    private void RefreshProcesses()
    {
        var previousPid = SelectedProcess?.Pid;
        Processes.Clear();
        foreach (var process in ProcessConnectionTracker.GetRunningProcesses())
            Processes.Add(process);

        SelectedProcess = previousPid.HasValue
            ? Processes.FirstOrDefault(p => p.Pid == previousPid.Value)
            : null;
    }

    private void StartCapture()
    {
        if (SelectedProcess == null || SelectedDevice == null)
            return;

        Packets.Clear();
        StatusText = $"擷取中：{SelectedProcess.Name} (PID {SelectedProcess.Pid})";
        IsCapturing = true;

        _captureService.Start(SelectedDevice, SelectedProcess.Pid, entry =>
            Application.Current.Dispatcher.BeginInvoke(() => AddPacket(entry)));
    }

    private void StopCapture()
    {
        _captureService.Stop();
        IsCapturing = false;
        StatusText = "已停止";
    }

    private void AddPacket(PacketEntry entry)
    {
        Packets.Add(entry);
        while (Packets.Count > MaxPackets)
            Packets.RemoveAt(0);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        CommandManager.InvalidateRequerySuggested();
    }
}
