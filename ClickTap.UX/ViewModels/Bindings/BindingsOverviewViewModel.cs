using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Threading;
using ClickTap.Lib.Entities.Serializable;
using ClickTap.Lib.Tablet;
using ClickTap.UX.Events;
using ClickTap.UX.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenTabletDriver.External.Common.Serializables;

namespace ClickTap.UX.ViewModels.Bindings
{
    public partial class BindingsOverviewViewModel : NavigableViewModel, IDisposable
    {
        #region Fields

        private SerializableSettings _settings = new();

        [ObservableProperty]
        private bool _isReady = false;

        [ObservableProperty]
        private bool _isTabletsEmpty = true;

        [ObservableProperty]
        private bool _isEmpty = true;

        [ObservableProperty]
        private ObservableCollection<SerializablePlugin> _plugins = new();

        [ObservableProperty]
        private ObservableCollection<TabletProfileOverview> _tablets = new();

        [ObservableProperty]
        private TabletProfileOverview? _selectedTablet;

        private int _selectedTabletIndex = -1;

        #endregion

        #region Constructors

        // Design-time constructor
        public BindingsOverviewViewModel()
        {
            IsReady = false;
            NextViewModel = this;

            AuxiliarySettings.BindingsChanged += OnBindingsChanged;
            PenSettings.BindingsChanged += OnBindingsChanged;
            MouseSettings.BindingsChanged += OnBindingsChanged;
        }

        public BindingsOverviewViewModel(SerializableSettings settings) : this()
            => SetSettings(settings);

        #endregion

        #region Events

        private event EventHandler? TabletChanged;
        public event EventHandler<EventArgs>? SaveRequested;
        public event EventHandler<SerializableProfile>? ProfileChanged;

        #endregion

        #region Properties

        public int SelectedTabletIndex
        {
            get => _selectedTabletIndex;
            set
            {
                SetProperty(ref _selectedTabletIndex, value);

                if (value < 0 || value >= Tablets.Count)
                    return;

                SelectedTablet = Tablets[value];
                TabletChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public PenSettingsViewModel PenSettings { get; set; } = new();

        public MouseSettingsViewModel MouseSettings { get; set; } = new();

        public AuxiliarySettingsViewModel AuxiliarySettings { get; set; } = new();

        #endregion

        #region Methods

        /// <summary>
        ///   Set the settings of the view model.
        /// </summary>
        /// <param name="settings">The settings to set.</param>
        public void SetSettings(SerializableSettings settings)
        {
            // Dispose of previous bindings beforehand
            DisposeCurrentContext();

            _settings = settings;
        }

        /// <summary>
        ///   Set the tablets of the view model.
        /// </summary>
        /// <param name="tablets">The tablets to set.</param>
        public void SetTablets(IEnumerable<SharedTabletReference> tablets)
        {
            Tablets.Clear();

            foreach (var profile in _settings.Profiles)
            {
                var tablet = tablets.FirstOrDefault(x => x.Name == profile.Name);

                if (tablet == null)
                    continue;

                // Start building a new tablet overview && build the gesture binsing displays
                TabletProfileOverview overview = new(tablet, profile);

                // TODO: Create a binding display for each binding types

                Tablets.Add(overview);
            }

            int oldSelectedTabletIndex = SelectedTabletIndex;

            // The selected tablet may still exist
            if (SelectedTablet != null)
                SelectedTablet = Tablets.FirstOrDefault(x => x.Name == SelectedTablet.Name);

            SelectedTablet ??= Tablets.FirstOrDefault();

            if (SelectedTablet != null)
                SelectedTabletIndex = Tablets.IndexOf(SelectedTablet);

            IsTabletsEmpty = !Tablets.Any();

            TabletChanged += OnTabletChanged;

            IsReady = true;

            if (oldSelectedTabletIndex != SelectedTabletIndex)
                OnTabletChanged(this, EventArgs.Empty);
        }

        /// <summary>
        ///   Request the save of the current bindings.
        /// </summary>
        [RelayCommand(CanExecute = nameof(IsReady))]
        public void RequestSave() => SaveRequested?.Invoke(this, EventArgs.Empty);

        protected override void GoBack() => throw new InvalidOperationException();

        #endregion

        #region Event Handlers

        /// <summary>
        ///   Handle the change of the selected tablet.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTabletChanged(object? sender, EventArgs e)
        {
            if (SelectedTablet == null)
                return;

            Dispatcher.UIThread.InvokeAsync(() => PenSettings.Build(SelectedTablet, Plugins));
            Dispatcher.UIThread.InvokeAsync(() => MouseSettings.Build(SelectedTablet, Plugins));
            Dispatcher.UIThread.InvokeAsync(() => AuxiliarySettings.Build(SelectedTablet, Plugins));
        }

        #region Gesture related events

        private void OnBindingsChanged(object? sender, BindingsChangedArgs e)
        {
            if (sender is not AuxiliarySettingsViewModel auxiliarySettingsViewModel)
                throw new ArgumentException($"Sender must be a {nameof(AuxiliarySettingsViewModel)} or inheritor.", nameof(sender));

            if (e.Display is not StateBindingDisplayViewModel bindingDisplay)
                throw new ArgumentException($"Sender must be a {nameof(StateBindingDisplayViewModel)} or inheritor.", nameof(sender));

            if (SelectedTablet == null)
                return;

            // We need to update the content of the binding display
            bindingDisplay.Content = AuxiliarySettingsViewModel.GetFriendlyContentFromProperty(bindingDisplay.PluginProperty, Plugins);

            // TODO: disappointed, i shouldn't have to do that, look into rewriting the settings builder to modify the profile directly instead of having to update like this
            auxiliarySettingsViewModel.UpdateProfile(SelectedTablet.Profile);

            ProfileChanged?.Invoke(this, SelectedTablet.Profile);
        }

        //private void OnGestureCollectionChanged(object? sender, EventArgs e)
        //    => Dispatcher.UIThread.InvokeAsync(() => OnSearchTextChanged(SearchText));

        #endregion

        #endregion

        #region Static Methods

        private static bool GestureNameStartsWith(StateBindingDisplayViewModel gestureTileViewModel, string text)
        {
            return gestureTileViewModel.Description?.StartsWith(text, StringComparison.CurrentCultureIgnoreCase) ?? false;
        }

        private static bool GestureNameContains(StateBindingDisplayViewModel gestureTileViewModel, string text)
        {
            return gestureTileViewModel.Description?.Contains(text, StringComparison.CurrentCultureIgnoreCase) ?? false;
        }

        #endregion

        #region Disposal

        public void DisposeCurrentContext()
        {
            foreach (var tablet in Tablets)
                tablet.Dispose();

            TabletChanged -= OnTabletChanged;

            IsReady = false;

            Tablets.Clear();
            IsTabletsEmpty = true;
            IsEmpty = true;
        }

        public void Dispose()
        {
            DisposeCurrentContext();

            SaveRequested = null;
            ProfileChanged = null;

            GC.SuppressFinalize(this);
        }

        #endregion
    }
}