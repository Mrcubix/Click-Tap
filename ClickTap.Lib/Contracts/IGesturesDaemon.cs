using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClickTap.Lib.Entities.Serializable;
using ClickTap.Lib.Tablet;
using OpenTabletDriver.External.Common.Contracts;

namespace ClickTap.Lib.Contracts
{
    public interface IClickTapDaemon : IPluginDaemon
    {
        event EventHandler<IEnumerable<SharedTabletReference>> TabletsChanged;

        /// <summary>
        ///   Returns whether a tablet is connected.
        /// </summary>
        /// <returns>True if a tablet is connected, false otherwise.</returns>
        public Task<bool> IsTabletConnected();

        /// <summary>
        ///   Returns the Contract version.
        /// <summary>
        public Task<int> GetContractVersion();

        /// <summary>
        ///   Returns the connected tablets.
        /// </summary>
        public Task<IEnumerable<SharedTabletReference>> GetTablets();

        /// <summary>
        ///   Request settings in serializable form.
        /// </summary>
        public Task<SerializableSettings> GetSettings();

        /// <summary>
        ///   Save settings.
        /// </summary>
        /// <returns>True if the settings were saved successfully, false otherwise.</returns>
        public Task<bool> SaveSettings();

        /// <summary>
        ///   Update All settings.
        /// </summary>
        public Task<bool> ApplySettings(SerializableSettings settings);

        /// <summary>
        ///   Update a specific profile.
        /// </summary>
        /// <param name="profile">The profile to update.</param>
        /// <returns>True if the profile was updated successfully, false otherwise.</returns>
        public Task<bool> UpdateProfile(SerializableProfile profile);
    }
}