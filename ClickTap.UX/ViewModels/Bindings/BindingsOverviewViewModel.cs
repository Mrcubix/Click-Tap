using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Threading;
using ClickTap.Lib.Entities.Serializable;
using ClickTap.Lib.Tablet;
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
        private TabletProfileOverview? _lastSelectedTablet;

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
        }

        public BindingsOverviewViewModel(SerializableSettings settings) : this()
            => Settings = settings;

        #endregion

        #region Events

        private event EventHandler? TabletChanged;
        public event EventHandler<EventArgs>? SaveRequested;
        public event EventHandler<EventArgs>? ApplyRequested;

        #endregion

        #region Properties

        public SerializableSettings Settings 
        { 
            get => _settings;
            set
            {
                // Dispose of previous bindings beforehand
                DisposeCurrentContext();
                _settings = value;
            }
        }

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

                Tablets.Add(overview);
            }

            int oldSelectedTabletIndex = SelectedTabletIndex;

            _lastSelectedTablet = SelectedTablet;

            // The selected tablet may still exist
            if (SelectedTablet != null)
                SelectedTablet = Tablets.FirstOrDefault(x => x.Name == SelectedTablet.Name);

            SelectedTablet ??= Tablets.FirstOrDefault();

            if (SelectedTablet != null)
                SelectedTabletIndex = Tablets.IndexOf(SelectedTablet);

            IsTabletsEmpty = !Tablets.Any();

            TabletChanged += OnTabletChanged;

            IsReady = true;

            // This might not be a good idea, as a tablet could still exist, but may not be the same index
            if (oldSelectedTabletIndex != SelectedTabletIndex)
                OnTabletChanged(this, EventArgs.Empty);
        }

        public void UpdateSelectedTabletProfile()
        {
            if (SelectedTablet == null)
                return;

            PenSettings.UpdateProfile(SelectedTablet.Profile);
            MouseSettings.UpdateProfile(SelectedTablet.Profile);
            AuxiliarySettings.UpdateProfile(SelectedTablet.Profile);
        }

        /// <summary>
        ///   Request the save of the current bindings.
        /// </summary>
        [RelayCommand(CanExecute = nameof(IsReady))]
        public void RequestSave() => SaveRequested?.Invoke(this, EventArgs.Empty);

        /// <summary>
        ///   Request the settings to be applied.
        /// </summary>
        [RelayCommand(CanExecute = nameof(IsReady))]
        public void RequestApply() => ApplyRequested?.Invoke(this, EventArgs.Empty);

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
            
            if (_lastSelectedTablet != null)
                UpdateSelectedTabletProfile();

            Dispatcher.UIThread.InvokeAsync(() => PenSettings.Build(SelectedTablet, Plugins));
            Dispatcher.UIThread.InvokeAsync(() => MouseSettings.Build(SelectedTablet, Plugins));
            Dispatcher.UIThread.InvokeAsync(() => AuxiliarySettings.Build(SelectedTablet, Plugins));

            _lastSelectedTablet = SelectedTablet;
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
            ApplyRequested = null;

            GC.SuppressFinalize(this);
        }

        #endregion
    }
}