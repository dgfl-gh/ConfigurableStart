using UnityEngine;

namespace CustomScenarioManager
{
    [KSPAddon(KSPAddon.Startup.MainMenu, false)]
    public class ScenarioEditorGUI : MonoBehaviour
    {
        public static ScenarioEditorGUI Instance = null;

        public string activeScenario;
        public string startingDate;
        public string unlockedTechs;
        public bool unlockPartsInParentNodes;
        public string partUnlockFilters;
        public string facilityUpgrades;
        public string startingFunds;
        public string startingScience;
        public string startingRep;
        public string kctLaunchpads;
        public bool kctRemoveDefaultPads;
        public string tfStartingDU;
        public string rfUnlockedConfigs;
        public string completedContracts;

        public int labelWidth = (int)(200 * GameSettings.UI_SCALE);
        public int textAreaWidth = (int)(400 * GameSettings.UI_SCALE);

        private readonly static GUIStyle textFieldStyle = new GUIStyle(HighLogic.Skin.textField);

        public void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            textFieldStyle.wordWrap = true;
        }

        public void EditWindow(int windowID)
        {
            GUILayout.BeginVertical(HighLogic.Skin.box);
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Starting Date: ", HighLogic.Skin.label, GUILayout.Width(labelWidth));
                startingDate = GUILayout.TextArea(startingDate, textFieldStyle);
                GUILayout.Label($"= {DateHandler.GetDatePreview(startingDate)}", HighLogic.Skin.label);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Unlocked Techs: ", HighLogic.Skin.label, GUILayout.Width(labelWidth));
                unlockedTechs = GUILayout.TextArea(unlockedTechs, textAreaWidth, textFieldStyle);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Part unlock filters: ", HighLogic.Skin.label, GUILayout.Width(labelWidth));
                partUnlockFilters = GUILayout.TextArea(partUnlockFilters, textAreaWidth, textFieldStyle);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Facility Levels: ", HighLogic.Skin.label, GUILayout.Width(labelWidth));
                facilityUpgrades = GUILayout.TextArea(facilityUpgrades, textAreaWidth, textFieldStyle);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Complete contracts: ", HighLogic.Skin.label, GUILayout.Width(labelWidth));
                completedContracts = GUILayout.TextArea(completedContracts, textAreaWidth, textFieldStyle);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Starting Funds: ", HighLogic.Skin.label, GUILayout.Width(labelWidth));
                startingFunds = GUILayout.TextArea(startingFunds, textAreaWidth, textFieldStyle);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Starting Science: ", HighLogic.Skin.label, GUILayout.Width(labelWidth));
                startingScience = GUILayout.TextArea(startingScience, textAreaWidth, textFieldStyle);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Starting Reputation: ", HighLogic.Skin.label, GUILayout.Width(labelWidth));
                startingRep = GUILayout.TextArea(startingRep, textAreaWidth, textFieldStyle);
                GUILayout.EndHorizontal();

                if (KCT.Found)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("KCT Launchpads: ", HighLogic.Skin.label, GUILayout.Width(labelWidth));
                    kctLaunchpads = GUILayout.TextArea(kctLaunchpads, textAreaWidth, textFieldStyle);
                    GUILayout.EndHorizontal();
                }

                if (RealFuels.Found)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Unlocked engine configs: ", HighLogic.Skin.label, GUILayout.Width(labelWidth));
                    rfUnlockedConfigs = GUILayout.TextArea(rfUnlockedConfigs, textAreaWidth, textFieldStyle);
                    GUILayout.EndHorizontal();
                }

                if (TestFlight.Found)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("TestFlight DU: ", HighLogic.Skin.label, GUILayout.Width(labelWidth));
                    tfStartingDU = GUILayout.TextArea(tfStartingDU, textAreaWidth, textFieldStyle);
                    GUILayout.EndHorizontal();
                }

                GUILayout.BeginHorizontal();
                unlockPartsInParentNodes = GUILayout.Toggle(unlockPartsInParentNodes, "Unlock parts in parent nodes ", HighLogic.Skin.toggle);
                GUILayout.EndHorizontal();

                if (KCT.Found)
                {
                    GUILayout.BeginHorizontal();
                    kctRemoveDefaultPads = GUILayout.Toggle(kctRemoveDefaultPads, "Remove default pad ", HighLogic.Skin.toggle);
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

        public void UpdateFromScenario(Scenario scn)
        {
            activeScenario = scn.ScenarioName;
            startingDate = scn.StartingDate;
            unlockedTechs = scn.UnlockedTechs;
            unlockPartsInParentNodes = scn.UnlockPartsInParentNodes;
            partUnlockFilters = scn.PartUnlockFilters;
            kctLaunchpads = scn.KCTLaunchpads;
            kctRemoveDefaultPads = scn.KCTRemoveDefaltPads;
            facilityUpgrades = scn.FacilityUpgrades;
            rfUnlockedConfigs = scn.RFUnlockedConfigs;
            tfStartingDU = scn.TFStartingDU;
            completedContracts = scn.CompletedContracts;
            startingFunds = scn.StartingFunds.ToString();
            startingScience = scn.StartingScience.ToString();
            startingRep = scn.StartingRep.ToString();
        }
    }
}
