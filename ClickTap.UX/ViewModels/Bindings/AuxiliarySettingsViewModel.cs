using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ClickTap.Lib.Entities.Serializable;
using ClickTap.UX.Events;
using ClickTap.UX.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using OpenTabletDriver.External.Common.Serializables;

namespace ClickTap.UX.ViewModels.Bindings
{
    public partial class AuxiliarySettingsViewModel : NavigableViewModel
    {
        #region Events

        public event EventHandler<BindingsChangedArgs>? BindingsChanged;

        #endregion

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

            // Avoid some double invokation shenanigans
            UnsubscribeFromEvents();
            Bindings.Clear();

            for (var i = 0; i < profile.AuxButtons.Length; i++)
            {
                SerializableBinding? settings = profile.AuxButtons[i];

                var bindingDisplay = new StateBindingDisplayViewModel(settings!)
                {
                    Description = GetDescriptionForIndex(i),
                    Content = GetFriendlyContentFromProperty(settings, plugins)
                };

                Bindings.Add(bindingDisplay);
            }

            SubscribeToEvents();

            IsEnabled = true;
        }

        public virtual string GetDescriptionForIndex(int index) 
        {
            return $"{Prefix} {index + 1}";
        }

        // TODO: Get rid of this
        public virtual void UpdateProfile(SerializableProfile profile)
        {
            profile.AuxButtons = Bindings.Select(binding => (SerializableBinding?)binding.PluginProperty).ToArray();
        }

        #region Event Related

        protected virtual void SubscribeToEvents()
        {
            foreach (var binding in Bindings)
                binding.BindingChanged += OnBindingsChanged;
        }

        protected virtual void UnsubscribeFromEvents()
        {
            foreach (var binding in Bindings)
                binding.BindingChanged -= OnBindingsChanged;
        }

        protected void OnBindingsChanged(object? sender, BindingsChangedArgs e)
        {
            BindingsChanged?.Invoke(this, e);
        }

        #endregion

        #endregion

        #region Static Methods

        public static string GetFriendlyContentFromProperty(SerializablePluginSettings? property, IList<SerializablePlugin> plugins)
        {
            if (property == null || property.Identifier == 0)
                return "";

            var pluginName = GetPluginNameFromIdentifier(property.Identifier, plugins);

            return $"{pluginName} : {property.Value}";
        }

        private static string? GetPluginNameFromIdentifier(int identifier, IList<SerializablePlugin>? Plugins)
        {
            if (Plugins == null)
                return null;

            return Plugins.FirstOrDefault(x => x.Identifier == identifier)?.PluginName ?? "Unknown";
        }

        #endregion
    }
}