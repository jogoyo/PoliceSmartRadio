using System;
using System.Linq;
using System.Reflection;
using Albo1125.Common;
using Albo1125.Common.CommonLibrary;
using LSPD_First_Response.Mod.API;
using Rage;

namespace PoliceSmartRadio
{
    internal class Main : Plugin
    {
        public Main()
        {
            UpdateChecker.VerifyXmlNodeExists(PluginName, FileId, DownloadUrl, Path);
            DependencyChecker.RegisterPluginForDependencyChecks(PluginName);
        }

        public override void Finally()
        {

        }

        public override void Initialize()
        {
            Game.Console.Print("PoliceSmartRadio " + Assembly.GetExecutingAssembly().GetName().Version + ", developed by Albo1125, loaded successfully!");
            Game.Console.Print("Special thanks to FinKone for the inspiration and OfficerSquare for the default UI.");
            Game.Console.Print("Please go on duty to start Police SmartRadio.");
            Functions.OnOnDutyStateChanged += Functions_OnOnDutyStateChanged;
        }

        private static readonly Version Albo1125CommonVer = new Version("6.6.3.0");
        private static readonly Version MadeForGtaVersion = new Version("1.0.1604.1");
        private const float MinimumRphVersion = 0.51f;

        private static readonly string[] AudioFilesToCheckFor = { "Plugins/LSPDFR/PoliceSmartRadio/Audio/ButtonScroll.wav", "Plugins/LSPDFR/PoliceSmartRadio/Audio/ButtonSelect.wav",
            "Plugins/LSPDFR/PoliceSmartRadio/Audio/PlateCheck/TargetPlate1.wav", "Plugins/LSPDFR/PoliceSmartRadio/Audio/PanicButton.wav" };

        private static readonly Version MadeForLspdfrVersion = new Version("0.4.39.22580");

        private static readonly string[] OtherFilesToCheckFor = { "Plugins/LSPDFR/Traffic Policer.dll", "Plugins/LSPDFR/PoliceSmartRadio/Config/GeneralConfig.ini",
            "Plugins/LSPDFR/PoliceSmartRadio/Config/ControllerConfig.ini", "Plugins/LSPDFR/PoliceSmartRadio/Config/KeyboardConfig.ini", "Plugins/LSPDFR/PoliceSmartRadio/Config/DisplayConfig.ini",
            "Plugins/LSPDFR/PoliceSmartRadio/Config/PanicButton.ini"};

        private static readonly Version TrafficPolicerVersion = new Version("6.14.0.0");
        private static readonly string[] ConflictingFiles = { "Plugins/LSPDFR/PoliceRadio.dll" };

        private const string FileId = "15354";
        private const string DownloadUrl = "http://www.lcpdfr.com/files/file/15354-police-smartradio-the-successor-to-police-radio/";
        private const string PluginName = "Police SmartRadio";
        private const string Path = "Plugins/LSPDFR/PoliceSmartRadio.dll";

        private static void Functions_OnOnDutyStateChanged(bool onDuty)
        {
            if (!onDuty) return;
            UpdateChecker.InitialiseUpdateCheckingProcess();
            if (!DependencyChecker.DependencyCheckMain(PluginName, Albo1125CommonVer, MinimumRphVersion,
                MadeForGtaVersion, MadeForLspdfrVersion, AudioFilesToCheckFor: AudioFilesToCheckFor,
                OtherRequiredFilesToCheckFor: OtherFilesToCheckFor)) return;
            if (!DependencyChecker.CheckIfThereAreNoConflictingFiles(PluginName, ConflictingFiles))
            {
                Game.LogTrivial("Old Police Radio still installed.");
                Game.DisplayNotification("~r~~h~Police SmartRadio detected the old PoliceRadio modification. You must delete it before using Police SmartRadio.");
                ExtensionMethods.DisplayPopupTextBoxWithConfirmation("Police SmartRadio Dependencies", "Police SmartRadio detected the old PoliceRadio modification. You must delete it before using Police SmartRadio. Unloading...", true);
                return;
            }
            if (!DependencyChecker.CheckIfFileExists("Plugins/LSPDFR/Traffic Policer.dll", TrafficPolicerVersion))
            {
                Game.LogTrivial("Traffic Policer is out of date for LSPDR+. Aborting. Required version: " + TrafficPolicerVersion);
                Game.DisplayNotification("~r~~h~Police SmartRadio detected Traffic Policer version lower than ~b~" + TrafficPolicerVersion);
                ExtensionMethods.DisplayPopupTextBoxWithConfirmation("Police SmartRadio Dependencies", "Police SmartRadio did not detect Traffic Policer or detected Traffic Policer version lower than " + TrafficPolicerVersion + ". Please install the appropriate version of Traffic Policer (link under Requirements on the download page). Unloading Police SmartRadio...", true);
                return;
            }
            GameFiber.StartNew(delegate
            {
                AppDomain.CurrentDomain.AssemblyResolve += ResolveAssemblyEventHandler;
                while (!IsLspdfrPluginRunning("Traffic Policer"))
                {
                    GameFiber.Yield();
                }
                PoliceSmartRadio.Initialise();

            });
        }

        private static bool IsLspdfrPluginRunning(string plugin, Version minversion = null)
        {
            return Functions.GetAllUserPlugins().Select(assembly => assembly.GetName()).Where(an => an.Name.ToLower() == plugin.ToLower()).Any(an => minversion == null || an.Version.CompareTo(minversion) >= 0);
        }

        private static Assembly ResolveAssemblyEventHandler(object sender, ResolveEventArgs args)
        {
            return Functions.GetAllUserPlugins().FirstOrDefault(assembly => args.Name.ToLower().Contains(assembly.GetName().Name.ToLower()));
        }
    }
}
