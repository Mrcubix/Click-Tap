using System;
using OpenTabletDriver.External.Common.RPC;

namespace ClickTap.UX.ViewModels
{
    public partial class ConnectionViewModel : NavigableViewModel
    {
        private string _connectionStateText = "Not connected";

        public string ConnectionStateText
        {
            get => _connectionStateText;
            private set => SetProperty(ref _connectionStateText, value);
        }

        protected virtual void OnClientConnected(object? sender, EventArgs e)
        {
            ConnectionStateText = "Connected";
        }

        protected virtual void OnClientConnecting(object? sender, EventArgs e)
        {
            ConnectionStateText = "Connecting...";
        }

        protected virtual void OnClientAttached(object? sender, EventArgs e)
        {
            ConnectionStateText = "Connected, Fetching Plugins & Settings ...";
        }

        public virtual void OnClientDisconnected(object? sender, EventArgs e)
        {
            ConnectionStateText = "Disconnected";
        }
    }

    public sealed partial class ConnectionViewModel<TClient> : ConnectionViewModel where TClient : class
    {
        private readonly RpcClient<TClient> _client;

        public ConnectionViewModel(RpcClient<TClient> client)
        {
            _client = client;

            _client.Connected += OnClientConnected;
            _client.Connecting += OnClientConnecting;
            _client.Attached += OnClientAttached;
            _client.Disconnected += OnClientDisconnected;
        }

        public void Dispose()
        {
            _client.Connected -= OnClientConnected;
            _client.Connecting -= OnClientConnecting;
            _client.Attached -= OnClientAttached;
            _client.Disconnected -= OnClientDisconnected;
        }
    }
}