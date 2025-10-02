using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ClickTap.Lib.Entities.Serializable;
using ClickTap.UX.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using OpenTabletDriver.External.Common.Serializables;

namespace ClickTap.UX.ViewModels.Bindings
{
    public partial class AuxiliarySettingsViewModel : NavigableViewModel
    {
        #region Fields

        [ObservableProperty]
        private bool _isEnabled = false;

        [ObservableProperty]
        private ObservableCollection<StateBindingDisplayViewModel> _bindings = new();

        #endregion

        #region Text Properties

        public virtual string Header => "Auxiliary";
        public virtual string Prefix => "Auxiliary Binding";

        #endregion

        #region Override

        public virtual void Build(TabletProfileOverview overview, IList<SerializablePlugin> plugins)
        {
            if (overview.Reference.AuxButtons == null || overview.Reference.AuxButtons.ButtonCount == 0)
            {
                IsEnabled = false;
                return;
            }

            var profile = overview.Profile;

            Bindings.Clear();

            for (var i = 0; i < profile.AuxButtons.Length; i++)
            {
                SerializablePluginSettingsStore? store = profile.AuxButtons[i];

                var bindingDisplay = new StateBindingDisplayViewModel(store!)
                {
                    Content = store?.GetHumanReadableString(),
                    Description = GetDescriptionForIndex(i)
                };

                Bindings.Add(bindingDisplay);
            }

            IsEnabled = true;
        }

        public virtual string GetDescriptionForIndex(int index) 
        {
            return $"{Prefix} {index + 1}";
        }

        public virtual void UpdateProfile(SerializableProfile profile)
        {
            profile.AuxButtons = Bindings.Select(binding => binding.Store).ToArray();
        }

        #endregion
    }
}