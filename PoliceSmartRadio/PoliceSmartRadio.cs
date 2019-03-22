using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using PoliceSmartRadio.Actions;
using PoliceSmartRadio.API;
using Rage;

namespace PoliceSmartRadio
{
    internal static class PoliceSmartRadio
    {
        public static string PlayerName = "NoNameSet";
        public static Random rnd = new Random();

        private static void MainLogic()
        {
            
            DisplayHandler.InitialiseTextures(true);
            Panic.IniSetup();
            RegisterActions();      
        }

        private static void RegisterActions()
        {          
            Functions.AddActionToButton(RequestPit.Main, RequestPit.available, "pit");
            Functions.AddActionToButton(PlateChecker.Main, "platecheck");
            Functions.AddActionToButton(Panic.Main, "panic");
            Functions.AddActionToButton(RunPedName.Main, RunPedName.IsAvailable, "pedcheck");
            Functions.AddActionToButton(EndCall.Main, LSPD_First_Response.Mod.API.Functions.IsCalloutRunning, "endcall");
            //API.Functions.AddActionToButton(Actions.K9.Main, "k9");
            Game.LogTrivial("All PoliceSmartRadio default buttons have been assigned actions.");
            if (!IsLspdfrPluginRunning("VocalDispatch", new Version("1.6.0.0"))) return;
            VocalDispatchHelper vc_platecheck = new VocalDispatchHelper();
            vc_platecheck.SetupVocalDispatchAPI("PoliceSmartRadio.PlateCheck", PlateChecker.vc_main);
            VocalDispatchHelper vc_requestpit = new VocalDispatchHelper();
            vc_requestpit.SetupVocalDispatchAPI("PoliceSmartRadio.RequestPIT", RequestPit.vc_main);
            VocalDispatchHelper vc_panic = new VocalDispatchHelper();
            vc_panic.SetupVocalDispatchAPI("PoliceSmartRadio.Panic", Panic.vc_main);
            VocalDispatchHelper vc_pedcheck = new VocalDispatchHelper();
            vc_pedcheck.SetupVocalDispatchAPI("PoliceSmartRadio.PedCheck", RunPedName.vc_main);
            VocalDispatchHelper vc_endcall = new VocalDispatchHelper();
            vc_endcall.SetupVocalDispatchAPI("PoliceSmartRadio.EndCall", EndCall.vc_main);
            Game.LogTrivial("PoliceSmartRadio Vocal Dispatch Integration complete.");

        }

        public static bool IsLspdfrPluginRunning(string plugin, Version minversion = null)
        {
            return LSPD_First_Response.Mod.API.Functions.GetAllUserPlugins().Select(assembly => assembly.GetName()).Where(an => string.Equals(an.Name, plugin, StringComparison.CurrentCultureIgnoreCase)).Any(an => minversion == null || an.Version.CompareTo(minversion) >= 0);
        }

        internal static void Initialise()
        {
            Game.LogTrivial("PoliceSmartRadio, developed by Albo1125, has been loaded successfully!");
            GameFiber.StartNew(delegate
            {                
                GameFiber.Wait(6000);
                Game.DisplayNotification("~b~PoliceSmartRadio~s~, developed by ~b~Albo1125 ~s~and repacked by ~b~Jogoyo~s~, has been loaded ~g~successfully.");

            });
            MainLogic();
        }        
    }
}
