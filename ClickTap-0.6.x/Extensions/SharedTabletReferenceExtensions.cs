using ClickTap.Lib.Tablet;
using OpenTabletDriver.Plugin.Tablet;

namespace ClickTap.Extensions
{
    public static class SharedTabletReferenceExtension
    {
        public static TabletReference ToState(this SharedTabletReference tablet)
        {
            var properties = new TabletConfiguration()
            {
                Name = tablet.Name
            };

            properties.Specifications = new TabletSpecifications();

            if (tablet.PenDigitizer != null)
            {
                properties.Specifications.Digitizer = new DigitizerSpecifications()
                {
                    Width = tablet.PenDigitizer.Width,
                    Height = tablet.PenDigitizer.Height,
                    MaxX = tablet.PenDigitizer.MaxX,
                    MaxY = tablet.PenDigitizer.MaxY
                };
            }

            if (tablet.Pen != null)
            {
                properties.Specifications.Pen = new PenSpecifications()
                {
                    MaxPressure = tablet.Pen.MaxPressure,
                    Buttons = new ButtonSpecifications()
                    {
                        ButtonCount = tablet.Pen.Buttons.ButtonCount
                    }
                };
            }

            if (tablet.AuxButtons != null)
            {
                properties.Specifications.AuxiliaryButtons = new ButtonSpecifications()
                {
                    ButtonCount = tablet.AuxButtons.ButtonCount
                };
            }

            if (tablet.MouseButtons != null)
            {
                properties.Specifications.MouseButtons = new ButtonSpecifications()
                {
                    ButtonCount = tablet.MouseButtons.ButtonCount
                };
            }

            List<DeviceIdentifier> identifiers = new List<DeviceIdentifier>();

            if (tablet.DeviceIdentifier != null)
            {
                identifiers.Add(new DeviceIdentifier()
                {
                    VendorID = tablet.DeviceIdentifier.VendorID,
                    ProductID = tablet.DeviceIdentifier.ProductID,
                    InputReportLength = tablet.DeviceIdentifier.InputReportLength,
                    OutputReportLength = tablet.DeviceIdentifier.OutputReportLength,
                    ReportParser = tablet.DeviceIdentifier.ReportParser,
                    FeatureInitReport = tablet.DeviceIdentifier.FeatureInitReport,
                    OutputInitReport = tablet.DeviceIdentifier.OutputInitReport,
                    DeviceStrings = tablet.DeviceIdentifier.DeviceStrings,
                    InitializationStrings = tablet.DeviceIdentifier.InitializationStrings
                });
            }

            return new TabletReference(properties, identifiers);
        }
    }
}