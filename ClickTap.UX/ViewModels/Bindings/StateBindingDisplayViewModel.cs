using System;
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

    public StateBindingDisplayViewModel() : base() {}

    public StateBindingDisplayViewModel(SerializablePluginSettingsStore store) : base(store) {}

    #endregion
}
