using UnityEngine;

namespace ConfigurableStart
{
    [KSPAddon(KSPAddon.Startup.MainMenu, false)]
    public class EditorGUI : MonoBehaviour
    {
        public static EditorGUI Instance = null;

        public string startingDate  = "";
        public string unlockedTechs = "";
        public bool   unlockPartsInParentNodes;
        public string partUnlockFilters  = "";
        public string facilityUpgrades   = "";
        public string startingFunds      = "";
        public string startingScience    = "";
        public string startingRep        = "";
        public string kctLaunchpads      = "";
        public string kctUpgrades        = "";
        public string kctUnspentUpgrades = "";
        public bool   kctRemoveDefaultPads;
        public string tfStartingDU       = "";
        public string rfUnlockedConfigs  = "";
        public string completedContracts = "";
        public string acceptedContracts  = "";

        public        int  labelWidth    = (int)(200 * GameSettings.UI_SCALE);
        public        int  textAreaWidth = (int)(400 * GameSettings.UI_SCALE);
        public static bool showUI    = false;

        private static readonly GUIStyle TextFieldStyle = new GUIStyle(HighLogic.Skin.textField);

        public void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            TextFieldStyle.wordWrap = true;
        }
        
        public static void ShowEditorWindow(bool b)
        {
            showUI = b;
        }

        public void EditWindow(int windowID)
        {
            GUILayout.BeginVertical(HighLogic.Skin.box);
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Starting Date: ", HighLogic.Skin.label, GUILayout.Width(labelWidth));
                startingDate = GUILayout.TextArea(startingDate, TextFieldStyle);
                GUILayout.Label($"= {DateHandler.GetDatePreview(startingDate)}", HighLogic.Skin.label);
                GUILayout.EndHorizontal();

                TextAreaWithLabel("Unlocked Techs: ", ref unlockedTechs);
                
                TextAreaWithLabel("Part unlock filters: ", ref partUnlockFilters);
                
                TextAreaWithLabel("Facility Levels: ", ref facilityUpgrades);
                
                TextAreaWithLabel("Complete contracts: ", ref completedContracts);
                
                TextAreaWithLabel("Complete contracts: ", ref acceptedContracts);

                TextAreaWithLabel("Starting Funds: ", ref startingFunds);

                TextAreaWithLabel("Starting Science: ", ref startingScience);
                
                TextAreaWithLabel("Starting Reputation: ", ref startingRep);
                
                if (KCT.Found)
                {
                    TextAreaWithLabel("KCT Launchpads: ", ref kctLaunchpads);
                    
                    TextAreaWithLabel("KCT Site upgrades: ", ref kctUpgrades);
                    
                    TextAreaWithLabel("KCT Unspent upgrades: ", ref kctUnspentUpgrades);
                }

                if (RealFuels.Found)
                {
                    TextAreaWithLabel("Unlocked engine configs: ", ref rfUnlockedConfigs);
                }

                if (TestFlight.Found)
                {
                    TextAreaWithLabel("TestFlight DU: ", ref tfStartingDU);
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
                UpdateCurrentScenarioValues(ScenarioLoader.CurrentScenario);
            }
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Close", HighLogic.Skin.button))
            {
                ShowEditorWindow(false);
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUI.DragWindow();
            
            // local function
            void TextAreaWithLabel(string label, ref string value)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(label, HighLogic.Skin.label, GUILayout.Width(labelWidth));
                value = GUILayout.TextArea(value, textAreaWidth, TextFieldStyle);
                GUILayout.EndHorizontal();
            }
        }

        public void UpdateFromScenario(Scenario scn)
        {
            if (scn == null)
                return;
            
            startingDate = scn.StartingDate ?? "";
            unlockedTechs = scn.UnlockedTechs ?? "";
            unlockPartsInParentNodes = scn.UnlockPartsInParentNodes;
            partUnlockFilters = scn.PartUnlockFilters ?? "";
            kctLaunchpads = scn.KCTLaunchpads ?? "";
            kctUpgrades = scn.KCTUpgrades ?? "";
            kctUnspentUpgrades = scn.KCTUnspentUpgrades.ToString();
            kctRemoveDefaultPads = scn.KCTRemoveDefaultPads;
            facilityUpgrades = scn.FacilityUpgrades ?? "";
            rfUnlockedConfigs = scn.RFUnlockedConfigs ?? "";
            tfStartingDU = scn.TFStartingDU ?? "";
            completedContracts = scn.CompletedContracts ?? "";
            acceptedContracts = scn.AcceptedContracts ?? "";
            startingFunds = scn.StartingFunds.ToString();
            startingScience = scn.StartingScience.ToString();
            startingRep = scn.StartingRep.ToString();
        }

        public void UpdateCurrentScenarioValues(Scenario scn)
        {
            scn.StartingDate = startingDate;
            scn.UnlockedTechs = unlockedTechs;
            scn.UnlockPartsInParentNodes = unlockPartsInParentNodes;
            scn.PartUnlockFilters = partUnlockFilters;
            scn.FacilityUpgrades = facilityUpgrades;
            scn.KCTLaunchpads = kctLaunchpads;
            scn.KCTRemoveDefaultPads = kctRemoveDefaultPads;
            scn.TFStartingDU = tfStartingDU;
            scn.RFUnlockedConfigs = rfUnlockedConfigs;
            startingFunds.CSTryParse(out float funds);
            startingScience.CSTryParse(out float sci);
            startingRep.CSTryParse(out float rep);
            scn.StartingFunds = funds;
            scn.StartingScience = sci;
            scn.StartingRep = rep;
        }
    }
}
