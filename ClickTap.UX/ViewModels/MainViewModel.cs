using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using ClickTap.Lib.Contracts;
using ClickTap.Lib.Entities.Serializable;
using ClickTap.Lib.Tablet;
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

    #endregion

    #region Observable Fields

    [ObservableProperty]
    private ObservableCollection<SerializablePlugin> _plugins = new();

    [ObservableProperty]
    private string _connectionStateText = "Not connected";

    [ObservableProperty]
    private bool _isReady = false;

    #endregion

    #region Initialization

    public MainViewModel()
    {
        _client = new("ClickTapDaemon");

        InitializeClient();
    }

    private void InitializeClient()
    {
        //_client.Converters.AddRange(Converters);

        _client.Connected += OnClientConnected;
        _client.Connecting += OnClientConnecting;
        _client.Disconnected += OnClientDisconnected;
        _client.Attached += (sender, args) => Task.Run(() => OnClientAttached(sender, args));

        _ = Task.Run(ConnectRpcAsync);
    }

    #endregion

    #region Events

    public event EventHandler<ObservableCollection<SerializablePlugin>>? PluginChanged;

    public event EventHandler<SerializableSettings>? SettingsChanged;

    public event EventHandler? Ready;

    public event EventHandler? Disconnected;

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

    private void OnClientConnected(object? sender, EventArgs e)
    {
        ConnectionStateText = "Connected";
    }

    private void OnClientConnecting(object? sender, EventArgs e)
    {
        ConnectionStateText = "Connecting...";
    }

    private void OnClientDisconnected(object? sender, EventArgs e)
    {
        ConnectionStateText = "Disconnected";
        IsReady = false;
        Disconnected?.Invoke(this, EventArgs.Empty);
        _client.Instance.TabletsChanged -= OnTabletsChanged;

        _ = Task.Run(() => AttemptReconnectionIndefinitelyAsync());
        NextViewModel = this;
    }

    private async Task OnClientAttached(object? sender, EventArgs e)
    {
        ConnectionStateText = "Connected, Fetching Plugins & Settings ...";

        _client.Instance.TabletsChanged += OnTabletsChanged;

        var tempPlugins = await FetchPluginsAsync();

        if (tempPlugins != null)
        {
            Plugins = new ObservableCollection<SerializablePlugin>(tempPlugins);
            PluginChanged?.Invoke(this, Plugins);
        }

        var tablets = await _client.Instance.GetTablets();

        if (tablets != null)
            OnTabletsChanged(this, tablets);

        IsReady = true;
        Ready?.Invoke(this, EventArgs.Empty);
        //NextViewModel = BindingsOverviewViewModel;
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
            Dispatcher.UIThread.Post(() => OnSettingsChanged(_settings));
            //Dispatcher.UIThread.Post(() => BindingsOverviewViewModel.SetTablets(tablets));
        }
    }

    private void OnSettingsChanged(SerializableSettings e)
    {
        //bool isOverviewNextViewModel = NextViewModel is BindingsOverviewViewModel;

        //BindingsOverviewViewModel.SetSettings(e);

        //if (isOverviewNextViewModel)
        //    NextViewModel = BindingsOverviewViewModel;

        SettingsChanged?.Invoke(this, e);
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
                Console.WriteLine("This error could have occured due to an different version of WheelAddon being used with this Interface.");

                ConnectionStateText = "Disconnected";
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
