using System;
using Avalonia.Controls;
using ClickTap.UX.ViewModels.Bindings;
using OpenTabletDriver.External.Avalonia.ViewModels;
using OpenTabletDriver.External.Avalonia.Views;

namespace ClickTap.UX.Controls.Containers;

public partial class ThresholdBindingDisplay : UserControl
{
    // --------------------------------- Constructor --------------------------------- //

    public ThresholdBindingDisplay()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is ThresholdBindingDisplayViewModel vm)
        {
            vm.ShowBindingEditorDialogRequested += ShowBindingEditorDialog;
            vm.ShowAdvancedBindingEditorDialogRequested += ShowAdvancedBindingEditorDialog;
        }
    }

    private void ShowBindingEditorDialog(object? sender, BindingDisplayViewModel e)
    {
        if (DataContext is ThresholdBindingDisplayViewModel vm)
        {
            if (TopLevel.GetTopLevel(this) is AppMainWindow window)
            {
                window.ShowBindingEditorDialog(sender, vm);
            }
        }
    }

    private void ShowAdvancedBindingEditorDialog(object? sender, BindingDisplayViewModel e)
    {
        if (DataContext is BindingDisplayViewModel vm)
        {
            if (TopLevel.GetTopLevel(this) is AppMainWindow window)
            {
                window.ShowAdvancedBindingEditorDialog(sender, vm);
            }
        }
    }
}
