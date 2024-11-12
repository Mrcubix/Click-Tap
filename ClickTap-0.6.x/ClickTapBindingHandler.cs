using System.Diagnostics;
using ClickTap.Entities;
using ClickTap.Extensions;
using ClickTap.Lib.Bindings;
using ClickTap.Lib.Tablet;
using OpenTabletDriver;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.DependencyInjection;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OpenTabletDriver.Plugin.Tablet;
using TouchGestures;

namespace ClickTap;

[PluginName(PLUGIN_NAME)]
public class ClickTapBindingHandler : IPositionedPipelineElement<IDeviceReport>, IDisposable
{
    #region Constants

    private const string PLUGIN_NAME = "Click & Tap";

    #endregion

    #region Fields

    private List<Binding?> _bindings = new();
    private IOutputMode? _outputMode;
    private ClickTapDaemon? _daemon;
    private BulletproofBindableProfile _profile = null!;
    private BulletproofSharedTabletReference? _tablet;
    private bool _awaitingDaemon;
    private bool _initialized;
    private bool _awaitingRelease;

    #endregion

    #region Initialization

    public ClickTapBindingHandler()
    {
#if DEBUG
        WaitForDebugger();
        Log.Write(PLUGIN_NAME, "Debugger attached", LogLevel.Debug);
#endif

        ClickTapDaemon.DaemonLoaded += OnDaemonLoaded;
        _awaitingDaemon = true;
    }

    private static void WaitForDebugger()
    {
        Console.WriteLine("Waiting for debugger to attach...");

        while (!Debugger.IsAttached)
        {
            Thread.Sleep(100);
        }
    }

    public void Initialize()
    {
        FetchOutputMode();

        // Filters are loaded before tools for some reasons, so we have to wait for the daemon to be loaded
        _daemon = (ClickTapDaemon?)BulletproofClickTapDaemonBase.Instance;

        // OTD 0.6.4.0 doesn't dispose of plugins when detecting tablets, so unsubscribing early is necessary
        ClickTapDaemon.DaemonLoaded -= OnDaemonLoaded;
        _awaitingDaemon = false;

        if (Tablet != null)
            InitializeCore(Tablet);

        if (_daemon == null)
            Log.Write(PLUGIN_NAME, "Touch Gestures Daemon has not been enabled, please enable it in the 'Tools' tab", LogLevel.Error);
    }

    private void InitializeCore(TabletReference tablet)
    {
        _tablet = tablet.ToShared(new SharedTabletDigitizer());

        AddServices();

        if (_daemon != null)
        {
            _daemon.AddTablet(_tablet);
            _profile = (BulletproofBindableProfile)_daemon.GetSettingsForTablet(_tablet.Name);

            if (_profile != null)
            {
                _profile.ProfileChanged += OnProfileChanged;
                OnProfileChanged(this, EventArgs.Empty);

                _initialized = true;
            }

            Log.Write(PLUGIN_NAME, "Now handling touch gesture for: " + _tablet.Name);
        }
    }

    private void AddServices()
    {
        if (_tablet is BulletproofSharedTabletReference btablet)
        {
            object? pointer = _outputMode switch
            {
                AbsoluteOutputMode absoluteOutputMode => absoluteOutputMode.Pointer,
                RelativeOutputMode relativeOutputMode => relativeOutputMode.Pointer,
                _ => null
            };

            if (pointer is IMouseButtonHandler mouseButtonHandler)
                btablet.ServiceProvider.AddService(() => mouseButtonHandler);

            btablet.ServiceProvider.AddService(() => Tablet!);
        }
    }

    protected void FetchOutputMode()
    {
        if (_Driver is Driver driver && Tablet != null)
        {
            // fetch the device first
            var device = driver.InputDevices.Where(x => x.Properties.Name == Tablet.Properties.Name).FirstOrDefault();

            // then fetch the output mode
            _outputMode = device?.OutputMode;
        }
    }

    #endregion

    #region Properties

    [TabletReference]
    public TabletReference? Tablet { get; set; }

    [Resolved]
    public IDriver? _Driver { set; get; }

    public PipelinePosition Position => PipelinePosition.PreTransform;

    #endregion

    #region Events

    public event Action<IDeviceReport>? Emit;

    #endregion

    #region Methods

    public void Consume(IDeviceReport report)
    {
        HandleBinding(report);
        Emit?.Invoke(report);
    }

    public void HandleBinding(IDeviceReport report)
    {
        if (Tablet == null || !_initialized)
            return;

        // TODO: Actually implement the click & Tap functionality following :
        // https://101.wacom.com/userhelp/en/AdvancedOptions.htm
        // https://github.com/OpenTabletDriver/OpenTabletDriver/issues/3165

        if (report is IAuxReport auxReport)
            HandleAuxiliaryReport(Tablet, auxReport);
        if (report is ITabletReport tabletReport)
            HandleTabletReport(Tablet, Tablet.Properties.Specifications.Pen, tabletReport);
    }

    private void HandleTabletReport(TabletReference tablet, PenSpecifications pen, ITabletReport report)
    {
        // We handle pen buttons first, as the Tip and Eraser need to be handled separately from them
        HandleBindingCollection(tablet, report, _profile!.PenButtons, report.PenButtons);

        HandleTips(tablet, pen, report);
    }

    private void HandleTips(TabletReference tablet, PenSpecifications pen, ITabletReport report)
    {
        float pressurePercent = (float)report.Pressure / (float)pen.MaxPressure * 100f;
        ThresholdBinding? binding;

        if (pressurePercent == 0f && _awaitingRelease)
            _awaitingRelease = false;

        if (report is IEraserReport eraserReport && eraserReport.Eraser)
            binding = _profile?.Eraser;
        else
            binding = _profile?.Tip;

        if (binding != null && _bindings.Count > 0 &&
            pressurePercent > binding.ActivationThreshold)
        {
            _awaitingRelease = true;

            // Disable the tip or eraser
            binding.Invoke(false);

            // Enable the bindings instead
            foreach (var b in _bindings)
                b?.Invoke(true);
        }
        else if (!_awaitingRelease) // No bindings are active, invoke the binding
            binding?.Invoke(pressurePercent);
    }

    private void HandleAuxiliaryReport(TabletReference tablet, IAuxReport report)
    {
        HandleBindingCollection(tablet, report, _profile!.AuxButtons, report.AuxButtons);
    }

    private void HandleBindingCollection(TabletReference tablet, IDeviceReport report, IList<Binding?> bindings, IList<bool> newStates)
    {
        for (int i = 0; i < newStates.Count; i++)
            if (bindings[i] != null && newStates[i])
                _bindings.Add(bindings[i]);
            else
            {
                bindings[i]?.Invoke(false);
                _bindings.Remove(bindings[i]);
            }
    }

    #endregion

    #region Events Handlers

    public void OnEmit(IDeviceReport e)
        => Emit?.Invoke(e);

    public void OnDaemonLoaded(object? sender, EventArgs e)
        => Initialize();

    public void OnProfileChanged(object? sender, EventArgs e)
    {
        if (_profile == null)
            Log.Write(PLUGIN_NAME, "Settings are null", LogLevel.Error);
        else
            Log.Debug(PLUGIN_NAME, "Settings updated");
    }

    #endregion

    #region Interface Implementations

    public void Dispose()
    {
        // Unsubscribe from events
        if (_profile != null)
            _profile.ProfileChanged -= OnProfileChanged;

        if (_awaitingDaemon)
            ClickTapDaemon.DaemonLoaded -= OnDaemonLoaded;

        if (_tablet != null)
            _daemon?.RemoveTablet(_tablet);

        _awaitingDaemon = false;

        GC.SuppressFinalize(this);
    }

    #endregion
}
