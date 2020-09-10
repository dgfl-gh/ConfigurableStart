using KSP.Localization;
using System;
using UnityEngine;

namespace CustomScenarioManager
{
    public static class ScenarioManagerSettings
    {
        public static string activeScenario;
        public static string startingDate;
        public static string unlockedTechs;
        //public static string completedContracts;
        public static string facilityUpgrades;
        public static string startingFunds;
        public static string startingScience;
        public static string startingRep;
        public static string kctLaunchpads;
        public static bool kctRemoveDefaultPads;
        public static string tfStartingDU;
        public static string rfUnlockedConfigs;

        public const int textInputWidth = 200;

        public static void EditWindow(int windowID)
        {
            GUILayout.BeginVertical(HighLogic.Skin.box);
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Starting Date: ", HighLogic.Skin.label, GUILayout.Width(textInputWidth));
                startingDate = GUILayout.TextField(startingDate, HighLogic.Skin.textField);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Unlocked Techs: ", HighLogic.Skin.label, GUILayout.Width(textInputWidth));
                unlockedTechs = GUILayout.TextField(unlockedTechs, HighLogic.Skin.textField);
                GUILayout.EndHorizontal();

                //GUILayout.BeginHorizontal();
                //GUILayout.Label("Completed Contracts: ", HighLogic.Skin.label, GUILayout.Width(textInputWidth));
                //completedContracts = GUILayout.TextField(completedContracts, HighLogic.Skin.textField);
                //GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Facility Levels: ", HighLogic.Skin.label, GUILayout.Width(textInputWidth));
                facilityUpgrades = GUILayout.TextField(facilityUpgrades, HighLogic.Skin.textField);
                GUILayout.EndHorizontal();


                GUILayout.BeginHorizontal();
                GUILayout.Label("Starting Funds: ", HighLogic.Skin.label, GUILayout.Width(textInputWidth));
                startingFunds = GUILayout.TextField(startingFunds, HighLogic.Skin.textField);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Starting Science: ", HighLogic.Skin.label, GUILayout.Width(textInputWidth));
                startingScience = GUILayout.TextField(startingScience, HighLogic.Skin.textField);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Starting Reputation: ", HighLogic.Skin.label, GUILayout.Width(textInputWidth));
                startingRep = GUILayout.TextField(startingRep, HighLogic.Skin.textField);
                GUILayout.EndHorizontal();

                if(KCT.Found)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("KCT Launchpads: ", HighLogic.Skin.label, GUILayout.Width(textInputWidth));
                    kctLaunchpads = GUILayout.TextField(kctLaunchpads, HighLogic.Skin.textField);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    kctRemoveDefaultPads = GUILayout.Toggle(kctRemoveDefaultPads, "Remove default pad: " , HighLogic.Skin.toggle);
                    GUILayout.EndHorizontal();
                }

                if(RealFuels.Found)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Unlocked engine configs: ", HighLogic.Skin.label, GUILayout.Width(textInputWidth));
                    rfUnlockedConfigs = GUILayout.TextField(rfUnlockedConfigs, HighLogic.Skin.textField);
                    GUILayout.EndHorizontal();
                }

                if(TestFlight.Found)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("TestFlight DU: ", HighLogic.Skin.label, GUILayout.Width(textInputWidth));
                    tfStartingDU = GUILayout.TextField(tfStartingDU, HighLogic.Skin.textField);
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndVertical();

            GUILayout.Space(7);

            GUILayout.BeginVertical();
            if (GUILayout.Button("Apply", HighLogic.Skin.button))
            {
                ScenarioLoader.UpdateCurrentScenario();
            }
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Close", HighLogic.Skin.button))
            {
                ScenarioLoader.ShowEditUI(false);
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUI.DragWindow();
        }

        public static void UpdateFromScenario(Scenario scn)
        {
            activeScenario = scn.ScenarioName;
            startingDate = scn.StartingDate;
            unlockedTechs = scn.UnlockedTechs;
            //completedContracts = scn.CompletedContracts;
            kctLaunchpads = scn.KCTLaunchpads;
            kctRemoveDefaultPads = scn.KCTRemoveDefaltPads;
            facilityUpgrades = scn.FacilityUpgrades;
            rfUnlockedConfigs = scn.RFUnlockedConfigs;
            tfStartingDU = scn.TFStartingDU;
            startingFunds = scn.StartingFunds.ToString();
            startingScience = scn.StartingScience.ToString();
            startingRep = scn.StartingRep.ToString();
        }
    }
}
