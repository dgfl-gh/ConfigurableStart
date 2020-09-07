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
        public static string kctLaunchpads;
        public static bool kctRemoveDefaultPads;
        public static string startingFunds;
        public static string startingScience;
        public static string startingRep;

        public static void EditWindow(int windowID)
        {
            GUILayout.BeginVertical(HighLogic.Skin.box);
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Starting Date: ", HighLogic.Skin.label, GUILayout.Width(150));
                startingDate = GUILayout.TextField(startingDate, HighLogic.Skin.textField);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Unlocked Techs: ", HighLogic.Skin.label, GUILayout.Width(150));
                unlockedTechs = GUILayout.TextField(unlockedTechs, HighLogic.Skin.textField);
                GUILayout.EndHorizontal();

                //GUILayout.BeginHorizontal();
                //GUILayout.Label("Completed Contracts: ", HighLogic.Skin.label, GUILayout.Width(150));
                //completedContracts = GUILayout.TextField(completedContracts, HighLogic.Skin.textField);
                //GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Facility Levels: ", HighLogic.Skin.label, GUILayout.Width(150));
                facilityUpgrades = GUILayout.TextField(facilityUpgrades, HighLogic.Skin.textField);
                GUILayout.EndHorizontal();

                if(KCT.Found)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("KCT Launchpads: ", HighLogic.Skin.label, GUILayout.Width(150));
                    kctLaunchpads = GUILayout.TextField(kctLaunchpads, HighLogic.Skin.textField);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    kctRemoveDefaultPads = GUILayout.Toggle(kctRemoveDefaultPads, "Remove default pad: " , HighLogic.Skin.toggle);
                    GUILayout.EndHorizontal();
                }

                GUILayout.BeginHorizontal();
                GUILayout.Label("Starting Funds: ", HighLogic.Skin.label, GUILayout.Width(150));
                startingFunds = GUILayout.TextField(startingFunds, HighLogic.Skin.textField);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Starting Science: ", HighLogic.Skin.label, GUILayout.Width(150));
                startingScience = GUILayout.TextField(startingScience, HighLogic.Skin.textField);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Starting Reputation: ", HighLogic.Skin.label, GUILayout.Width(150));
                startingRep = GUILayout.TextField(startingRep, HighLogic.Skin.textField);
                GUILayout.EndHorizontal();
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
            startingFunds = scn.StartingFunds.ToString();
            startingScience = scn.StartingScience.ToString();
            startingRep = scn.StartingRep.ToString();
        }
    }
}
