using System;
using ClickTap.Lib.Entities.Serializable;
using ClickTap.Lib.Tablet;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ClickTap.UX.Models;

public partial class TabletProfileOverview : ObservableObject, IDisposable
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private SharedTabletReference _reference = null!;

    [ObservableProperty]
    private SerializableProfile _profile = null!;

    // TODO: Probably have an object that stores the binding display for each settings

    public TabletProfileOverview()
    {
    }

    public TabletProfileOverview(string name)
    {
        Name = name;
    }

    public TabletProfileOverview(SerializableProfile profile)
    {
        Name = profile.Name;
    }

    public TabletProfileOverview(SharedTabletReference tablet)
    {
        Name = tablet.Name;
        Reference = tablet;
    }

    public TabletProfileOverview(SharedTabletReference tablet, SerializableProfile profile)
    {
        Name = profile.Name;
        Reference = tablet;
        Profile = profile;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public override string ToString()
    {
        return Name;
    }
}