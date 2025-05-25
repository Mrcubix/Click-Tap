using System;
using System.ComponentModel;
using ClickTap.Lib.Entities.Serializable;
using ClickTap.Lib.Entities.Serializable.Bindings;
using ClickTap.UX.Events;
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
        ActivationThreshold = settings?.ActivationThreshold ?? 0f;
    }

    #endregion

    #region Methods

    public override SerializableBinding? ToSerializableBinding()
    {
        if (PluginProperty == null || PluginProperty.Value == null || PluginProperty.Identifier == -1) return null;
        
        return new SerializableThresholdBinding(PluginProperty.Value, PluginProperty.Identifier, ActivationThreshold);
    }

    #endregion

    #region Event Handlers

    protected override void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // We should avoid sending an update to the daemon too frequently elsewhere
        if (e.PropertyName == nameof(ActivationThreshold))
        {
            var args = new BindingsChangedArgs(PluginProperty, PluginProperty, this);
            OnBindingChanged(this, args);
        }

        base.OnPropertyChanged(sender, e);
    }

    #endregion
}
