using System;
using OpenTabletDriver.External.Avalonia.ViewModels;
using OpenTabletDriver.External.Common.Serializables;

namespace ClickTap.UX.Events;

public class BindingsChangedArgs : EventArgs
{
    public SerializablePluginSettings? OldValue { get; }

    public SerializablePluginSettings? NewValue { get; }

    public BindingDisplayViewModel? Display { get; }

    public BindingsChangedArgs(SerializablePluginSettings? oldValue, SerializablePluginSettings? newValue, BindingDisplayViewModel? display)
    {
        OldValue = oldValue;
        NewValue = newValue;
        Display = display;
    }
}