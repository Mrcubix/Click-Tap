using System.Reflection;
using ClickTap.Bindings;
using ClickTap.Lib.Entities.Bindable;
using ClickTap.Lib.Entities.Serializable;
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

        public override void SetTablet(SharedTabletReference tablet)
        {
            if (tablet is not BulletproofSharedTabletReference btablet)
                return;

            if (btablet.ServiceProvider.GetService(typeof(TabletReference)) is TabletReference tabletRef)
            {
                foreach (var binding in this)
                {
                    if (binding is IBulletproofBinding bulletproofBinding)
                    {
                        bulletproofBinding.Tablet = tabletRef;
                        bulletproofBinding.Provider = btablet.ServiceProvider;
                    }
                }
            }
        }

        public override void Update(SerializableProfile profile, SharedTabletReference tablet, Dictionary<int, TypeInfo> identifierToPlugin)
        {
            FromSerializable(profile, identifierToPlugin, tablet);
            OnProfileChanged();
        }

        public override void FromSerializable(SerializableProfile profile, Dictionary<int, TypeInfo> identifierToPlugin, 
                                              SharedTabletReference? tablet = null)
        {
            if (tablet is not BulletproofSharedTabletReference btablet)
                return;

            var tabletReference = btablet.ServiceProvider.GetService(typeof(TabletReference)) as TabletReference;
            Name = profile.Name;

            Clear();

            if (profile.Tip != null)
                Tip = new BulletproofThresholdBinding(profile.TipActivationThreshold, profile.Tip, identifierToPlugin);

            profile.TipActivationThreshold = Tip?.ActivationThreshold ?? 1;

            if (profile.Eraser != null)
                Eraser = new BulletproofThresholdBinding(profile.EraserActivationThreshold, profile.Eraser, identifierToPlugin);

            profile.EraserActivationThreshold = Eraser?.ActivationThreshold ?? 1;

            PenButtons = profile.PenButtons.Select(penButton => penButton != null ? new BulletproofBinding(penButton, tabletReference) : null)
                                           .ToArray();

            AuxButtons = profile.AuxButtons.Select(auxButton => auxButton != null ? new BulletproofBinding(auxButton, tabletReference) : null)
                                           .ToArray();

            MouseButtons = profile.MouseButtons.Select(mouseButton => mouseButton != null ? new BulletproofBinding(mouseButton, tabletReference) : null)
                                               .ToArray();

            if (profile.MouseScrollUp != null)
                MouseScrollUp = new BulletproofBinding(profile.MouseScrollUp, tabletReference);

            if (profile.MouseScrollDown != null)
                MouseScrollDown = new BulletproofBinding(profile.MouseScrollDown, tabletReference);

            SetTablet(tablet);
            ConstructBindings(tablet);
        }

        public override SerializableProfile ToSerializable(Dictionary<int, TypeInfo> identifierToPlugin)
        {
            return new SerializableProfile()
            {
                Name = Name,
                Tip = Tip?.ToSerializable(identifierToPlugin),
                TipActivationThreshold = Tip?.ActivationThreshold ?? 1,
                Eraser = Eraser?.ToSerializable(identifierToPlugin),
                EraserActivationThreshold = Eraser?.ActivationThreshold ?? 1,
                PenButtons = PenButtons.Select(penButton => penButton?.ToSerializable(identifierToPlugin)).ToArray(),
                AuxButtons = AuxButtons.Select(auxButton => auxButton?.ToSerializable(identifierToPlugin)).ToArray(),
                MouseButtons = MouseButtons.Select(mouseButton => mouseButton?.ToSerializable(identifierToPlugin)).ToArray(),
                MouseScrollUp = MouseScrollUp?.ToSerializable(identifierToPlugin),
                MouseScrollDown = MouseScrollDown?.ToSerializable(identifierToPlugin)
            };
        }
    }
}