using ClickTap.Lib.Tablet;
using OpenTabletDriver.Desktop.Reflection;

namespace ClickTap.Entities
{
    public class BulletproofSharedTabletReference : SharedTabletReference
    {
        public BulletproofSharedTabletReference() : base()
        {
            ServiceProvider = new ServiceManager();
        }

        public BulletproofSharedTabletReference(string name, SharedTabletDigitizer digitizer, 
                                                SharedTabletDigitizer touchDigitizer, SharedPenSpecifications? penSpecifications, 
                                                SharedButtonSpecifications? auxButtons, SharedButtonSpecifications? mouseButtons,
                                                IServiceManager serviceProvider)
            : base(name, digitizer, touchDigitizer, penSpecifications, auxButtons, mouseButtons)
        {
            ServiceProvider = serviceProvider;
        }

        public BulletproofSharedTabletReference(string name, SharedTabletDigitizer digitizer, 
                                                SharedTabletDigitizer touchDigitizer, SharedDeviceIdentifier deviceIdentifier,
                                                SharedPenSpecifications? penSpecifications, SharedButtonSpecifications? auxButtons, 
                                                SharedButtonSpecifications? mouseButtons, IServiceManager serviceProvider)
            : base(name, digitizer, touchDigitizer, penSpecifications, auxButtons, mouseButtons, deviceIdentifier)
        {
            ServiceProvider = serviceProvider;
        }

        public IServiceManager ServiceProvider { set; get; }
    }
}