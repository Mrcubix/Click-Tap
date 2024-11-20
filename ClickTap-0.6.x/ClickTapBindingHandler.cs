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

    private IOutputMode? _outputMode;
    private ClickTapDaemon? _daemon;
    private BulletproofBindableProfile _profile = null!;
    private BulletproofSharedTabletReference? _tablet;
    private bool _awaitingDaemon;
    private bool _initialized;

    private ThresholdBinding? _currentTipBinding;
    private bool _thresholdReached;
    private float _currentPressurePercent;

    private IAuxReport? _currentAuxReport;
    private bool _awaitingRelease;

    #endregion

    #region Initialization

    public ClickTapBindingHandler()
    {
#if DEBUG
        WaitForDebugger();
        Log.Write(PLUGIN_NAME, "Debugger attached", LogLevel.Debug);
#endif

        // Tools are loaded last, so we need to wait for the daemon to be loaded
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

            // Some Bindings require a pointer to be passed to mouse events
            if (pointer is IMouseButtonHandler mouseButtonHandler)
                btablet.ServiceProvider.AddService(() => mouseButtonHandler);

            // Some Bindings may require the tablet when constructed for some reasons
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
        if (_initialized)
            HandleBinding(report);

        Emit?.Invoke(report);
    }

    public void HandleBinding(IDeviceReport report)
    {
        if (Tablet == null || !_initialized)
            return;

        // Reset the current tip binding
        _currentTipBinding = null;

        if (report is ITabletReport tabletReport)
            HandleTabletReport(Tablet.Properties.Specifications.Pen, tabletReport);
        if (report is IAuxReport auxReport)
            HandleAuxiliaryReport(auxReport);

        // Handle the tip or eraser when used LAST
        _currentTipBinding?.Invoke(report, _thresholdReached && _awaitingRelease == false);
    }

    private void HandleTabletReport(PenSpecifications pen, ITabletReport report)
    {
        _currentPressurePercent = (float)report.Pressure / (float)pen.MaxPressure * 100f;

        if (report is IEraserReport eraserReport && eraserReport.Eraser)
            _currentTipBinding = _profile.Eraser;
        else
            _currentTipBinding = _profile.Tip;

        bool _oldThresholdState = _thresholdReached;
        _thresholdReached = _currentPressurePercent > _currentTipBinding?.ActivationThreshold;

        // PENDING: Could prob make it work using just _thresholdReached instead of full release
        _awaitingRelease &= _currentPressurePercent != 0f;

        // We need to trigger an update on aux buttons on state change
        if (_oldThresholdState != _thresholdReached && _currentAuxReport != null)
            HandleAuxiliaryReport(_currentAuxReport);

        HandleBindingCollection(report, _profile.PenButtons, report.PenButtons);
    }

    private void HandleAuxiliaryReport(IAuxReport report)
    {
        HandleBindingCollection(report, _profile.AuxButtons, report.AuxButtons);
        _currentAuxReport = report;
    }

    private void HandleBindingCollection(IDeviceReport report, IList<Binding?> bindings, IList<bool> newStates)
    {
        for (int i = 0; i < newStates.Count; i++)
        {
            bool newState = newStates[i] && _thresholdReached;
            bindings[i]?.Invoke(report, newState);

            // We need to track if a binding was invoked with true
            _awaitingRelease |= newState;
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
