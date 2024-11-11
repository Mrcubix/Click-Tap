using System;
using System.ComponentModel;
using ClickTap.Lib.Entities.Serializable;
using ClickTap.Lib.Entities.Serializable.Bindings;
using CommunityToolkit.Mvvm.ComponentModel;

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

    public ThresholdBindingDisplayViewModel(string value, int identifier, float activationThreshold) : base(new SerializableThresholdPluginSettings(value, identifier, 0)) 
    {
        ActivationThreshold = activationThreshold;
    }

    public ThresholdBindingDisplayViewModel(SerializableThresholdBinding settings) : base(settings)
    {
        ActivationThreshold = settings.ActivationThreshold;
    }

    #endregion

    #region Event Handlers

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ActivationThreshold))
        {
            // TODO: Handle this
        }
    }

    #endregion
}
