using System;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.CrestronThread;
using Crestron.SimplSharpPro.Diagnostics;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.UI;

namespace Vc4Test1
{
    public enum SystemJoins
    {
        PowerOn = 10,
        PowerOff = 11,
        PowerToggle = 12,
        PowerTransition = 13
    }

    public enum SystemFb
    {
        PowerOnFb = 10,
        PowerOffFb = 11,
        PowerToggleFb = 12
    }

    public class ControlSystem : CrestronControlSystem
    {
        private XpanelForSmartGraphics tp1;
        private bool bSystemPowerOn;

        public ControlSystem()
            : base()
        {
            try
            {
                Thread.MaxNumberOfUserThreads = 20;

                // System defaults to OFF
                bSystemPowerOn = false;
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in ControlSystem constructor: {0}", e.Message);
            }
        }

        public override void InitializeSystem()
        {
            try
            {
                tp1 = new XpanelForSmartGraphics(0x03, this);
                
                tp1.OnlineStatusChange += tp_OnlineChange;
                tp1.UserSpecifiedObject = new Action<bool>(online => { if (online) UpdateFeedback(); });
                
                tp1.SigChange += tp_SigChange;
                tp1.BooleanOutput[(uint)SystemJoins.PowerToggle].UserObject = new Action<bool>(press => { if (press) ToggleSystemPower(); });
                tp1.BooleanOutput[(uint)SystemJoins.PowerTransition].UserObject = new Action<bool>(done => { if (done) UpdatePowerStatusText(); });
                
                tp1.Register();
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in InitializeSystem: {0}", e.Message);
            }
        }

        public void tp_OnlineChange(GenericBase dev, OnlineOfflineEventArgs args)
        {
            var obj = dev.UserSpecifiedObject;

            if (obj is Action<bool>)
            {
                var func = (Action<bool>)obj;
                func(args.DeviceOnLine);
            }
        }

        public void tp_SigChange(BasicTriList dev, SigEventArgs args)
        {
            var obj = args.Sig.UserObject;

            if (obj is Action<bool>)
            {
                var func = (Action<bool>)obj;
                func(args.Sig.BoolValue);
            }
        }

        void UpdateFeedback()
        {
            tp1.BooleanInput[(uint)SystemFb.PowerOnFb].BoolValue = bSystemPowerOn;
            tp1.BooleanInput[(uint)SystemFb.PowerOffFb].BoolValue = !bSystemPowerOn;
            tp1.BooleanInput[(uint)SystemFb.PowerToggleFb].BoolValue = bSystemPowerOn;
        }

        void UpdatePowerStatusText()
        {
            if (bSystemPowerOn)
                tp1.StringInput[10].StringValue = "ON";
            else
                tp1.StringInput[10].StringValue = "OFF";
        }

        void ToggleSystemPower()
        {
            bSystemPowerOn = !bSystemPowerOn;

            UpdateFeedback();
        }
    }
}