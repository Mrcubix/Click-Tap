using System;
using System.ComponentModel;
using ClickTap.UX.Events;
using CommunityToolkit.Mvvm.ComponentModel;
using OpenTabletDriver.External.Avalonia.ViewModels;
using OpenTabletDriver.External.Common.Serializables;

namespace ClickTap.UX.ViewModels.Bindings;

public partial class StateBindingDisplayViewModel : BindingDisplayViewModel, IDisposable
{
    #region Fields

    [ObservableProperty]
    private bool _isReady = false;

    #endregion

    #region Constructors

    public StateBindingDisplayViewModel() : base()
    {
        Initialize();
    }

    public StateBindingDisplayViewModel(SerializablePluginSettingsStore store) : base(store)
    {
        Initialize();
    }

    private void Initialize()
    {
        PropertyChanged += OnPropertyChanged;
    }

    #endregion

    #region Events

    public event EventHandler<BindingsChangedArgs>? BindingChanged;

    #endregion

    #region Event Handlers

    protected void OnBindingChanged(object? sender, BindingsChangedArgs e) => BindingChanged?.Invoke(this, e);

    protected virtual void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PluginProperty))
        {
            var args = new BindingsChangedArgs(PluginProperty, PluginProperty, sender as StateBindingDisplayViewModel);
            BindingChanged?.Invoke(this, args);
        }
    }

    public new void Dispose()
    {
        base.Dispose();

        PropertyChanged -= OnPropertyChanged;

        BindingChanged = null;

        GC.SuppressFinalize(this);
    }

    #endregion
}
