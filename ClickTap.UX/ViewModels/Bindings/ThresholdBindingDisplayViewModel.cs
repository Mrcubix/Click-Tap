using System;
using CommunityToolkit.Mvvm.ComponentModel;
using OpenTabletDriver.External.Common.Serializables;

namespace ClickTap.UX.ViewModels.Bindings;

public partial class ThresholdBindingDisplayViewModel : StateBindingDisplayViewModel, IDisposable
{
    #region Fields

    [ObservableProperty]
    private float _activationThreshold = 0f;

    [ObservableProperty]
    private string? _ThresholdDescription;

    #endregion

    #region Constructors

    public ThresholdBindingDisplayViewModel() : base() {}

    public ThresholdBindingDisplayViewModel(SerializablePluginSettingsStore settings, float activationThreshold) : base(settings)
    {
        ActivationThreshold = activationThreshold;
    }

    #endregion
}
