using System.Collections.Generic;
using System.Linq;
using ClickTap.Lib.Entities.Serializable;
using ClickTap.Lib.Entities.Serializable.Bindings;
using ClickTap.UX.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using OpenTabletDriver.External.Common.Serializables;

namespace ClickTap.UX.ViewModels.Bindings
{
    public partial class PenSettingsViewModel : AuxiliarySettingsViewModel
    {
        #region Bindings

        [ObservableProperty]
        private ThresholdBindingDisplayViewModel _tipBindingDisplay = new(new SerializableThresholdBinding(string.Empty, 0, 0));

        [ObservableProperty]
        private ThresholdBindingDisplayViewModel _eraserBindingDisplay = new(new SerializableThresholdBinding(string.Empty, 0, 0));

        #endregion

        #region Text Properties

        public override string Header => "Pen Buttons";
        public override string Prefix => "Pen Binding";

        #endregion

        #region Overrides

        public override void Build(TabletProfileOverview overview, IList<SerializablePlugin> plugins)
        {
            if (overview.Reference.Pen == null)
            {
                IsEnabled = false;
                return;
            }

            var profile = overview.Profile;

            UnsubscribeFromEvents();   
            Bindings.Clear();

            for (var i = 0; i < profile.PenButtons.Length; i++)
            {
                SerializableBinding? settings = profile.PenButtons[i];
                settings ??= new(string.Empty, 0); // Default to no binding

                profile.PenButtons[i] ??= settings;

                var bindingDisplay = SetupNewBindingDisplay(settings);
                bindingDisplay.Description = GetDescriptionForIndex(i);

                Bindings.Add(bindingDisplay);
            }

            var tipSettings = profile.Tip ?? new SerializableThresholdBinding(string.Empty, 0, 0);
            var eraserSettings = profile.Eraser ?? new SerializableThresholdBinding(string.Empty, 0, 0);

            profile.Tip ??= tipSettings;
            profile.Eraser ??= eraserSettings;

            // Tip
            TipBindingDisplay = (ThresholdBindingDisplayViewModel)SetupNewBindingDisplay(tipSettings);
            TipBindingDisplay.Description = "Tip Binding";
            TipBindingDisplay.ThresholdDescription = "Tip Threshold";

            // Eraser
            EraserBindingDisplay = (ThresholdBindingDisplayViewModel)SetupNewBindingDisplay(eraserSettings);
            EraserBindingDisplay.Description = "Eraser Binding";
            EraserBindingDisplay.ThresholdDescription = "Eraser Threshold";

            SubscribeToEvents();

            IsEnabled = true;
        }

        public override void UpdateProfile(SerializableProfile profile)
        {
            profile.PenButtons = Bindings.Select(binding => (SerializableBinding?)binding.PluginProperty).ToArray();
            profile.Tip = TipBindingDisplay.PluginProperty as SerializableThresholdBinding;
            profile.Eraser = EraserBindingDisplay.PluginProperty as SerializableThresholdBinding;
        }

        #region Event Related

        protected override void SubscribeToEvents()
        {
            // Bindings
            base.SubscribeToEvents();

            // Tip & Eraser
            TipBindingDisplay.BindingChanged += OnBindingsChanged;
            EraserBindingDisplay.BindingChanged += OnBindingsChanged;
        }

        protected override void UnsubscribeFromEvents()
        {
            // Bindings
            base.UnsubscribeFromEvents();

            // Tip & Eraser
            TipBindingDisplay.BindingChanged -= OnBindingsChanged;
            EraserBindingDisplay.BindingChanged -= OnBindingsChanged;
        }

        #endregion

        #endregion
    }
}