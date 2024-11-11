using System.Collections.Generic;
using System.Linq;
using ClickTap.Lib.Entities.Serializable;
using ClickTap.UX.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using OpenTabletDriver.External.Common.Serializables;

namespace ClickTap.UX.ViewModels.Bindings
{
    public partial class MouseSettingsViewModel : AuxiliarySettingsViewModel
    {
        #region Bindings

        [ObservableProperty]
        private StateBindingDisplayViewModel _mouseScrollUpBindingDisplay = new(new SerializableBinding(string.Empty, 0));

        [ObservableProperty]
        private StateBindingDisplayViewModel _mouseScrollDownBindingDisplay = new(new SerializableBinding(string.Empty, 0));

        #endregion

        #region Text Properties

        public override string Header => "Mouse Buttons";
        public override string Prefix => "Mouse Binding";

        #endregion

        #region Overrides

        public override void Build(TabletProfileOverview overview, IList<SerializablePlugin> plugins)
        {
            if (overview.Reference.MouseButtons == null)
            {
                IsEnabled = false;
                return;
            }

            var profile = overview.Profile;

            UnsubscribeFromEvents();
            Bindings.Clear();

            for (var i = 0; i < profile.MouseButtons.Length; i++)
            {
                SerializableBinding? settings = profile.MouseButtons[i];
                settings ??= new(string.Empty, 0); // Default to no binding

                profile.MouseButtons[i] ??= settings;

                var bindingDisplay = SetupNewBindingDisplay(settings);
                bindingDisplay.Content = GetFriendlyContentFromProperty(settings, plugins);
                bindingDisplay.Description = GetDescriptionForIndex(i);

                Bindings.Add(bindingDisplay);
            }

            var mouseScrollUpSettings = profile.MouseScrollUp ?? new(string.Empty, 0);
            var mouseScrollDownSettings = profile.MouseScrollDown ?? new(string.Empty, 0);

            profile.MouseScrollUp ??= mouseScrollUpSettings;
            profile.MouseScrollDown ??= mouseScrollDownSettings;

            // Scroll Wheel
            MouseScrollUpBindingDisplay = SetupNewBindingDisplay(mouseScrollUpSettings);
            MouseScrollUpBindingDisplay.Content = GetFriendlyContentFromProperty(mouseScrollUpSettings, plugins);
            MouseScrollUpBindingDisplay.Description = "Scroll Up";

            MouseScrollDownBindingDisplay = SetupNewBindingDisplay(mouseScrollDownSettings);
            MouseScrollDownBindingDisplay.Content = GetFriendlyContentFromProperty(mouseScrollDownSettings, plugins);
            MouseScrollDownBindingDisplay.Description = "Scroll Down";

            SubscribeToEvents();

            IsEnabled = true;
        }

        public override string GetDescriptionForIndex(int index)
        {
            return index switch
            {
                0 => "Primary Binding",
                1 => "Alternate Binding",
                2 => "Middle Binding",
                _ => base.GetDescriptionForIndex(index)
            };
        }

        public override void UpdateProfile(SerializableProfile profile)
        {
            profile.MouseButtons = Bindings.Select(binding => (SerializableBinding?)binding.PluginProperty).ToArray();
            profile.MouseScrollUp = MouseScrollUpBindingDisplay.PluginProperty as SerializableBinding;
            profile.MouseScrollDown = MouseScrollDownBindingDisplay.PluginProperty as SerializableBinding;
        }

        #region Event Related

        protected override void SubscribeToEvents()
        {
            // Bindings
            base.SubscribeToEvents();

            // Scroll Wheel
            MouseScrollUpBindingDisplay.BindingChanged += OnBindingsChanged;
            MouseScrollDownBindingDisplay.BindingChanged += OnBindingsChanged;
        }

        protected override void UnsubscribeFromEvents()
        {
            // Bindings
            base.UnsubscribeFromEvents();

            // Scroll Wheel
            MouseScrollUpBindingDisplay.BindingChanged -= OnBindingsChanged;
            MouseScrollDownBindingDisplay.BindingChanged -= OnBindingsChanged;
        }

        #endregion

        #endregion
    }
}