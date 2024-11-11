using System.Reflection;
using ClickTap.Bindings;
using ClickTap.Lib.Entities.Bindable;
using ClickTap.Lib.Entities.Serializable;
using ClickTap.Lib.Entities.Serializable.Bindings;
using ClickTap.Lib.Tablet;
using OpenTabletDriver.Plugin.Tablet;

namespace ClickTap.Entities
{
    public class BulletproofBindableProfile : BindableProfile<BulletproofBinding, BulletproofThresholdBinding>
    {
        public override void ConstructBindings(SharedTabletReference? tablet = null)
        {
            Tip?.Construct();
            Eraser?.Construct();

            foreach (var penButton in PenButtons)
                penButton?.Construct();

            foreach (var auxButton in AuxButtons)
                auxButton?.Construct();

            foreach (var mouseButton in MouseButtons)
                mouseButton?.Construct();

            MouseScrollUp?.Construct();
            MouseScrollDown?.Construct();
        }

        public override void Update(SerializableProfile profile, SharedTabletReference tablet, Dictionary<int, TypeInfo> identifierToPlugin)
        {
            FromSerializable(profile, identifierToPlugin, tablet, this);
            OnProfileChanged();
        }

        public override void FromSerializable(SerializableProfile profile, Dictionary<int, TypeInfo> identifierToPlugin, 
                                              SharedTabletReference? tablet = null)
        {
            if (tablet is not BulletproofSharedTabletReference btablet)
                return;

            Clear();

            var tabletRef = btablet.ServiceProvider.GetService(typeof(TabletReference)) as TabletReference;

            Name = profile.Name;

            if (profile.Tip != null)
                Tip = new BulletproofThresholdBinding(profile.Tip.ActivationThreshold, profile.Tip, identifierToPlugin, 
                                                      tabletRef, btablet.ServiceProvider);

            if (profile.Eraser != null)
                Eraser = new BulletproofThresholdBinding(profile.Eraser.ActivationThreshold, profile.Eraser, identifierToPlugin,
                                                         tabletRef, btablet.ServiceProvider);

            PenButtons = profile.PenButtons.Select(penButton => penButton != null ? new BulletproofBinding(penButton, identifierToPlugin) : null)
                                           .ToArray();

            AuxButtons = profile.AuxButtons.Select(auxButton => auxButton != null ? new BulletproofBinding(auxButton, identifierToPlugin) : null)
                                           .ToArray();

            MouseButtons = profile.MouseButtons.Select(mouseButton => mouseButton != null ? new BulletproofBinding(mouseButton, identifierToPlugin) : null)
                                               .ToArray();

            if (profile.MouseScrollUp != null)
                MouseScrollUp = new BulletproofBinding(profile.MouseScrollUp, identifierToPlugin);

            if (profile.MouseScrollDown != null)
                MouseScrollDown = new BulletproofBinding(profile.MouseScrollDown, identifierToPlugin);

            ConstructBindings(tablet);
        }

        public override SerializableProfile ToSerializable(Dictionary<int, TypeInfo> identifierToPlugin)
        {
            return new SerializableProfile()
            {
                Name = Name,
                Tip = Tip?.ToSerializable(identifierToPlugin) as SerializableThresholdBinding,
                Eraser = Eraser?.ToSerializable(identifierToPlugin) as SerializableThresholdBinding,
                PenButtons = PenButtons.Select(penButton => penButton?.ToSerializable(identifierToPlugin)).ToArray(),
                AuxButtons = AuxButtons.Select(auxButton => auxButton?.ToSerializable(identifierToPlugin)).ToArray(),
                MouseButtons = MouseButtons.Select(mouseButton => mouseButton?.ToSerializable(identifierToPlugin)).ToArray(),
                MouseScrollUp = MouseScrollUp?.ToSerializable(identifierToPlugin),
                MouseScrollDown = MouseScrollDown?.ToSerializable(identifierToPlugin)
            };
        }
    }
}