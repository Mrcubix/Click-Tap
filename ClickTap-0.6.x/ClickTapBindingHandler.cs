﻿using System.Diagnostics;
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

        if (report is ITabletReport tabletReport)
            HandleTabletReport(Tablet, Tablet.Properties.Specifications.Pen, tabletReport);
        if (report is IAuxReport auxReport)
            HandleAuxiliaryReport(Tablet, auxReport);
        if (report is IMouseReport mouseReport)
            HandleMouseReport(Tablet, mouseReport);
    }

    private void HandleTabletReport(TabletReference tablet, PenSpecifications pen, ITabletReport report)
        {
            float pressurePercent = (float)report.Pressure / (float)pen.MaxPressure * 100f;
            if (report is IEraserReport eraserReport && eraserReport.Eraser)
                _profile?.Eraser?.Invoke(pressurePercent);
            else
                _profile?.Tip?.Invoke(pressurePercent);

            HandleBindingCollection(tablet, report, _profile!.PenButtons, report.PenButtons);
        }

        private void HandleAuxiliaryReport(TabletReference tablet, IAuxReport report)
        {
            HandleBindingCollection(tablet, report, _profile!.AuxButtons, report.AuxButtons);
        }

        private void HandleMouseReport(TabletReference tablet, IMouseReport report)
        {
            HandleBindingCollection(tablet, report, _profile!.MouseButtons, report.MouseButtons);

            _profile?.MouseScrollDown?.Invoke(report.Scroll.Y < 0);
            _profile?.MouseScrollUp?.Invoke(report.Scroll.Y > 0);
        }

        private static void HandleBindingCollection(TabletReference tablet, IDeviceReport report, IList<Binding?> bindings, IList<bool> newStates)
        {
            for (int i = 0; i < newStates.Count; i++)
                if (bindings[i] != null)
                    bindings[i]!.Invoke(newStates[i]);
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