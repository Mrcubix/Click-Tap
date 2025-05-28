using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using ClickTap.Lib.Entities.Serializable;
using ClickTap.Lib.Entities.Serializable.Bindings;
using ClickTap.UX.ViewModels;
using ClickTap.UX.ViewModels.Bindings;
using OpenTabletDriver.External.Avalonia.Dialogs;
using OpenTabletDriver.External.Avalonia.ViewModels;
using OpenTabletDriver.External.Avalonia.Views;
using OpenTabletDriver.External.Common.Enums;
using OpenTabletDriver.External.Common.Serializables;

namespace ClickTap.UX.Views;

public partial class MainWindow : AppMainWindow
{
    private static readonly BindingEditorDialogViewModel _bindingEditorDialogViewModel = new();
    private static readonly AdvancedBindingEditorDialogViewModel _advancedBindingEditorDialogViewModel = new();

    private bool _isEditorDialogOpen = false;

    public MainWindow()
    {
        InitializeComponent();
    }

    public override void ShowBindingEditorDialog(object? sender, BindingDisplayViewModel e)
    {
        _ = Dispatcher.UIThread.InvokeAsync(() => ShowBindingEditorDialogCore(e));
    }

    public override void ShowAdvancedBindingEditorDialog(object? sender, BindingDisplayViewModel e)
    {
        _ = Dispatcher.UIThread.InvokeAsync(() => ShowAdvancedBindingEditorDialogCore(e));
    }

    private async Task ShowBindingEditorDialogCore(BindingDisplayViewModel e)
    {
        if (DataContext is MainViewModel vm && !_isEditorDialogOpen)
        {
            _isEditorDialogOpen = true;

            // Now we set the view model's properties

            var plugins = vm.BindingsOverviewViewModel.Plugins;

            var bindingPlugins = plugins.Where(p => p.Type == PluginType.Binding).ToList();
            var selectedPlugin = bindingPlugins.FirstOrDefault(p => p.Identifier == e.Store?.Identifier);

            _bindingEditorDialogViewModel.Store = e.Store;

            // Now we setup the dialog
            var dialog = new BindingEditorDialog()
            {
                Plugins = plugins,
                DataContext = _bindingEditorDialogViewModel
            };

#if DEBUG
            dialog.AttachDevTools();
#endif

            // Now we show & handle the dialog
            await HandleBindingEditorDialog(dialog, e);
        }
    }

    private async Task ShowAdvancedBindingEditorDialogCore(BindingDisplayViewModel e)
    {
        if (DataContext is MainViewModel vm && !_isEditorDialogOpen)
        {
            _isEditorDialogOpen = true;

            // Now we set the view model's properties

            var plugins = vm.BindingsOverviewViewModel.Plugins;

            var bindingPlugins = plugins.Where(p => p.Type == PluginType.Binding).ToList();
            var selectedPlugin = bindingPlugins.FirstOrDefault(p => p.Identifier == e.Store?.Identifier);
            
            var settingsStoreEditor = new PluginSettingStoreEditorViewModel()
            {
                Properties = selectedPlugin?.Properties ?? [],
                Store = e.Store
            };

            // Now we set the view model's properties

            var advancedBindingEditorDialogViewModel = new AdvancedBindingEditorDialogViewModel([.. bindingPlugins], settingsStoreEditor)
            {
                SelectedBindingType = selectedPlugin,
            };

            // Now we setup the dialog
            var dialog = new AdvancedBindingEditorDialog()
            {
                DataContext = advancedBindingEditorDialogViewModel,
                Plugins = plugins
            };

#if DEBUG
            dialog.AttachDevTools();
#endif

            // Now we show & handle the dialog
            await HandleBindingEditorDialog(dialog, e);
        }
    }

    private async Task HandleBindingEditorDialog(Window dialog, BindingDisplayViewModel e)
    {
        var res = await dialog.ShowDialog<SerializablePluginSettingsStore>(this);

        _isEditorDialogOpen = false;

        // We handle the result
        e.Store = res;
        e.Content = res?.GetHumanReadableString();
    }
}