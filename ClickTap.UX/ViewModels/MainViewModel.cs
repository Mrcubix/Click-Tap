using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using ClickTap.Lib.Contracts;
using ClickTap.Lib.Converters;
using ClickTap.Lib.Entities.Serializable;
using ClickTap.Lib.Tablet;
using ClickTap.UX.ViewModels.Bindings;
using CommunityToolkit.Mvvm.ComponentModel;
using OpenTabletDriver.External.Common.RPC;
using OpenTabletDriver.External.Common.Serializables;
using StreamJsonRpc;

namespace ClickTap.UX.ViewModels;

public partial class MainViewModel : NavigableViewModel
{
    #region Fields

    private CancellationTokenSource _reconnectionTokenSource = new();
    private RpcClient<IClickTapDaemon> _client;
    private SerializableSettings? _settings;

    private readonly ConnectionViewModel<IClickTapDaemon> _connectionScreenViewModel;

    #endregion

    #region Observable Fields

    [ObservableProperty]
    private bool _isReady = false;

    [ObservableProperty]
    private BindingsOverviewViewModel _bindingsOverviewViewModel;

    #endregion

    #region Initialization

    public MainViewModel()
    {
        BindingsOverviewViewModel = new();
        BindingsOverviewViewModel.SaveRequested += OnSaveRequested;
        BindingsOverviewViewModel.ApplyRequested += OnApplyRequested;

        _client = new("ClickTapDaemon");
        _client.Converters.Add(new SerializablePropertyConverter());

        _connectionScreenViewModel = new(_client);

        NextViewModel = _connectionScreenViewModel;

        InitializeClient();
    }

    private void InitializeClient()
    {
        //_client.Converters.AddRange(Converters);

        _client.Disconnected += OnClientDisconnected;
        _client.Attached += (sender, args) => Task.Run(() => OnClientAttached(sender, args));

        _ = Task.Run(ConnectRpcAsync);
    }

    #endregion

    #region Events

    public event EventHandler<SerializableSettings>? SettingsChanged;

    public event EventHandler? Ready;

    #endregion

    #region Methods

    //
    // RPC Client Methods
    //

    private async Task AttemptReconnectionIndefinitelyAsync()
    {
        if (_client.IsConnected)
            return;

        _reconnectionTokenSource = new();
        var token = _reconnectionTokenSource.Token;

        while (!_client.IsConnected && !token.IsCancellationRequested)
        {
            await Task.Delay(500, token);
            await ConnectRpcAsync();
        }
    }

    private async Task ConnectRpcAsync()
    {
        if (_client.IsConnected)
            return;

        try
        {
            await _client.ConnectAsync();
        }
        catch (Exception e)
        {
            HandleException(e);
        }
    }

    private async Task<List<SerializablePlugin>?> FetchPluginsAsync()
    {
        if (!_client.IsConnected)
            return null;

        try
        {
            return await _client.Instance.GetPlugins();
        }
        catch (Exception e)
        {
            HandleException(e);
        }

        return null;
    }

    private async Task<SerializableSettings?> FetchSettingsAsync()
    {
        if (!_client.IsConnected)
            return null;

        try
        {
            return await _client.Instance.GetSettings();
        }
        catch (Exception e)
        {
            HandleException(e);
        }

        return null;
    }

    private async Task SaveSettingsAsync()
    {
        if (!_client.IsConnected)
            return;

        try
        {
            await _client.Instance.SaveSettings();
        }
        catch (Exception e)
        {
            HandleException(e);
        }
    }

    private async Task ApplySettingsAsync(SerializableSettings settings)
    {
        if (!_client.IsConnected)
            return;

        try
        {
            await _client.Instance.ApplySettings(settings);
        }
        catch (Exception e)
        {
            HandleException(e);
        }
    }

    private async Task UpdateProfileAsync(SerializableProfile profile)
    {
        if (!_client.IsConnected)
            return;

        try
        {
            await _client.Instance.UpdateProfile(profile);
        }
        catch (Exception e)
        {
            HandleException(e);
        }
    }

    #endregion

    #region Event Handlers

    //
    // Plugin Client Event Handlers
    //

    private void OnClientDisconnected(object? sender, EventArgs e)
    {
        IsReady = false;

        if (_client.Instance != null)
            _client.Instance.TabletsChanged -= OnTabletsChanged;

        _ = Task.Run(AttemptReconnectionIndefinitelyAsync);
        NextViewModel = _connectionScreenViewModel;
    }

    private async Task OnClientAttached(object? sender, EventArgs e)
    {
        _client.Instance.TabletsChanged += OnTabletsChanged;

        var tempPlugins = await FetchPluginsAsync();

        if (tempPlugins != null)
            BindingsOverviewViewModel.Plugins = new ObservableCollection<SerializablePlugin>(tempPlugins);

        var tablets = await _client.Instance.GetTablets();

        if (tablets != null)
            OnTabletsChanged(this, tablets);

        IsReady = true;
        Ready?.Invoke(this, EventArgs.Empty);
        NextViewModel = BindingsOverviewViewModel;
    }

    public void OnTabletsChanged(object? sender, IEnumerable<SharedTabletReference> tablets)
    {
        _ = OnTabletsChangedCore(sender, tablets);
    }

    private async Task OnTabletsChangedCore(object? sender, IEnumerable<SharedTabletReference> tablets)
    {
        if (tablets == null)
            return;

        SerializableSettings? tempSettings = await FetchSettingsAsync();

        if (tempSettings != null)
        {
            _settings = tempSettings;

            // TODO : Display Screen about plugin version being outdated
            if (_settings.Version < 2)
                return;

            // Always set the settings first
            Dispatcher.UIThread.Post(() => OnSettingsChanged(_settings));
            Dispatcher.UIThread.Post(() => BindingsOverviewViewModel.SetTablets(tablets));
        }
    }

    private void OnSettingsChanged(SerializableSettings e)
    {
        bool isOverviewNextViewModel = NextViewModel is BindingsOverviewViewModel;

        BindingsOverviewViewModel.Settings = e;

        if (isOverviewNextViewModel)
            NextViewModel = BindingsOverviewViewModel;

        SettingsChanged?.Invoke(this, e);
    }

    private void OnSaveRequested(object? sender, EventArgs e)
    {
        OnApplyRequested(sender, e);
        _ = SaveSettingsAsync();
    }

    private void OnApplyRequested(object? sender, EventArgs e)
    {
        // We take the risk of applying an outdated profile otherwise
        BindingsOverviewViewModel.UpdateSelectedTabletProfile();
        _ = ApplySettingsAsync(BindingsOverviewViewModel.Settings);
    }

    #endregion

    #region Exception Handling

    private void HandleException(Exception e)
    {
        switch (e)
        {
            case RemoteRpcException re:
                Console.WriteLine($"An Error occured while attempting to connect to the RPC server: {re.Message}");
                Console.WriteLine("----------------------------------------");
                Console.WriteLine("This error could have occured due to an different version of Click & Tap being used with this Interface.");

                _connectionScreenViewModel.OnClientDisconnected(_client, EventArgs.Empty);
                break;
            case OperationCanceledException _:
                break;
            default:
                Console.WriteLine($"An unhanded exception occured: {e.Message}");

                // write the exception to a file
                File.WriteAllText("exception.txt", e.ToString());

                Console.WriteLine("The exception has been written to exception.txt");

                break;
        }
    }

    #endregion
}
