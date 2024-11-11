using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using ClickTap.Lib.Entities.Serializable;
using ClickTap.Lib.Entities.Serializable.Bindings;
using ClickTap.UX.ViewModels;
using ClickTap.UX.ViewModels.Bindings;
using OpenTabletDriver.External.Avalonia.Dialogs;
using OpenTabletDriver.External.Avalonia.ViewModels;
using OpenTabletDriver.External.Avalonia.Views;
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

            // Now we setup the dialog

            var dialog = new BindingEditorDialog()
            {
                Plugins = vm.BindingsOverviewViewModel.Plugins,
                DataContext = _bindingEditorDialogViewModel
            };

            // Now we show the dialog

            var res = await dialog.ShowDialog<SerializablePluginSettings>(this);

            HandleBindingEditorResult(res, e);
        }
    }

    private async Task ShowAdvancedBindingEditorDialogCore(BindingDisplayViewModel e)
    {
        if (DataContext is MainViewModel vm && !_isEditorDialogOpen)
        {
            _isEditorDialogOpen = true;

            var plugins = vm.BindingsOverviewViewModel.Plugins;

            // Fetch some data from the plugins

            var types = plugins.Select(p => p.PluginName ?? p.FullName ?? "Unknown").ToList();

            var currentPlugin = plugins.FirstOrDefault(p => p.Identifier == e.PluginProperty?.Identifier);
            var selectedType = currentPlugin?.PluginName ?? currentPlugin?.FullName ?? "Unknown";

            var validProperties = currentPlugin?.ValidProperties ?? new string[0];
            var selectedProperty = e.PluginProperty?.Value ?? "";

            // Now we set the view model's properties

            _advancedBindingEditorDialogViewModel.Types = new ObservableCollection<string>(types);
            _advancedBindingEditorDialogViewModel.SelectedType = selectedType;
            _advancedBindingEditorDialogViewModel.ValidProperties = new ObservableCollection<string>(validProperties);
            _advancedBindingEditorDialogViewModel.SelectedProperty = selectedProperty;

            // Now we setup the dialog

            var dialog = new AdvancedBindingEditorDialog()
            {
                DataContext = _advancedBindingEditorDialogViewModel,
                Plugins = plugins
            };

            // Now we show the dialog

            var res = await dialog.ShowDialog<SerializablePluginSettings>(this);

            HandleBindingEditorResult(res, e);
        }
    }

    private void HandleBindingEditorResult(SerializablePluginSettings result, BindingDisplayViewModel e)
    {
        if (DataContext is MainViewModel vm)
        {
            _isEditorDialogOpen = false;

            // We handle the result

            // The dialog was closed or the cancel button was pressed
            if (result == null)
                return;

            // The user selected "Clear"
            if (result.Identifier == -1 || result.Value == "None")
            {
                e.PluginProperty = null;
                e.Content = "";
            }
            else
            {
                // External doesn't support generic types so we need to generate the appropriate serializable
                e.PluginProperty = ConvertToCorrectSerializable(e, result);
                e.Content = AuxiliarySettingsViewModel.GetFriendlyContentFromProperty(result, vm.BindingsOverviewViewModel.Plugins);
            }
        }
    }

    private static SerializablePluginSettings ConvertToCorrectSerializable(BindingDisplayViewModel display, SerializablePluginSettings result)
    {
        return display switch
        {
            ThresholdBindingDisplayViewModel thresholdDisplay => new SerializableThresholdBinding(result.Value!, result.Identifier, thresholdDisplay.ActivationThreshold),
            StateBindingDisplayViewModel => new SerializableBinding(result.Value!, result.Identifier),
            _ => result
        };
    }
}