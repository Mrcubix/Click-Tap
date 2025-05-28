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
        private ThresholdBindingDisplayViewModel _tipBindingDisplay = new(new SerializablePluginSettingsStore(string.Empty, string.Empty, 0), 1);

        [ObservableProperty]
        private ThresholdBindingDisplayViewModel _eraserBindingDisplay = new(new SerializablePluginSettingsStore(string.Empty, string.Empty, 0), 1);

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

            Bindings.Clear();

            for (var i = 0; i < profile.PenButtons.Length; i++)
            {
                SerializablePluginSettingsStore? store = profile.PenButtons[i];

                var bindingDisplay = new StateBindingDisplayViewModel(store!)
                {
                    Content = store?.GetHumanReadableString(),
                    Description = GetDescriptionForIndex(i)
                };

                Bindings.Add(bindingDisplay);
            }

            // Tip
            TipBindingDisplay = new ThresholdBindingDisplayViewModel(profile.Tip!, profile.TipActivationThreshold)
            {
                Content = profile.Tip?.GetHumanReadableString(),
                Description = "Tip Binding",
                ThresholdDescription = "Tip Threshold"
            };

            // Eraser
            EraserBindingDisplay = new ThresholdBindingDisplayViewModel(profile.Eraser!, profile.EraserActivationThreshold)
            {
                Content = profile.Eraser?.GetHumanReadableString(),
                Description = "Eraser Binding",
                ThresholdDescription = "Eraser Threshold"
            };

            IsEnabled = true;
        }

        public override void UpdateProfile(SerializableProfile profile)
        {
            profile.PenButtons = Bindings.Select(binding => binding.Store).ToArray();
            profile.Tip = TipBindingDisplay.Store;
            profile.TipActivationThreshold = TipBindingDisplay.ActivationThreshold;
            profile.Eraser = EraserBindingDisplay.Store;
            profile.EraserActivationThreshold = EraserBindingDisplay.ActivationThreshold;
        }

        #endregion
    }
}