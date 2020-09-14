using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace CustomScenarioManager
{
    [KSPAddon(KSPAddon.Startup.MainMenu, false)]
    public class ScenarioLoader : MonoBehaviour
    {
        public static Dictionary<string, Scenario> LoadedScenarios { get; } = new Dictionary<string, Scenario>();
        public static bool initialized = false;
        public static bool mainMenuVisited = false;
        public static Scenario CurrentScenario
        {
            get
            {
                Scenario scn = null;
                LoadedScenarios?.TryGetValue(curScenarioName, out scn);
                return scn;
            }
        }

        private static string[] loadedScenarioNames;
        private static int selectedScenarioIndex = 0;
        private static string curScenarioName
        {
            get => loadedScenarioNames?[selectedScenarioIndex];
            set => selectedScenarioIndex = loadedScenarioNames.IndexOf(value);
        }

        private static Rect selectionWindowRect = new Rect(267, 104, 400, 200);
        private static Rect editWindowRect = new Rect(Screen.width - 800, 104, 500, 100);
        private static bool shouldResetUIHeight = false;
        private static bool showSelectionUI = false;
        private static bool showEditUI = false;


        public void Start()
        {
            Utilities.Log("Start called");

            if (!mainMenuVisited)
            {
                GameEvents.onGameNewStart.Add(SetScenario);
                mainMenuVisited = true;
            }

            LoadScenarios();
            AttachToMainMenu();

            Utilities.Log("Start finished");
        }

        public void AttachToMainMenu()
        {
            if (FindObjectOfType<MainMenu>() is MainMenu menu)
            {
                menu.newGameBtn.onTap += delegate { ShowSelectionUI(true); };

                if (LoadedScenarios.Count > 1)
                    initialized = true;
            }
            else
            {
                Utilities.Log("Couldn't find MainMenu");
            }
        }

        public void SetScenario()
        {
            ShowSelectionUI(false);
            ShowEditUI(false);

            SetActiveScenario(ScenarioManagerSettings.activeScenario);
            Destroy(this);
        }

        public void LoadScenarios()
        {
            Utilities.Log("Loading Scenarios");
            LoadedScenarios.Clear();
            LoadedScenarios["None"] = new Scenario();
            ConfigNode[] nodes = GameDatabase.Instance.GetConfigNodes("CUSTOMSCENARIO");

            foreach (var scenarioNode in nodes)
            {
                if (scenarioNode == null)
                    return;
                try
                {
                    var s = Scenario.Create(scenarioNode);
                    LoadedScenarios[s.ScenarioName] = s;
                }
                catch (Exception ex)
                {
                    Utilities.Log($"{ex}");
                }
            }
            int count = LoadedScenarios.Count() - 1;
            Utilities.Log($"Found {count} scenario{(count > 1 ? "s" : "")}");
            loadedScenarioNames = LoadedScenarios.Keys.ToArray();
        }

        public void SetActiveScenario(string scenarioName)
        {
            if (LoadedScenarios == null || scenarioName == null ||  scenarioName == "None")
                return;

            if (LoadedScenarios.ContainsKey(scenarioName))
            {
                LoadedScenarios[scenarioName].SetParameters();
                curScenarioName = scenarioName;
            }
            else
                Utilities.Log($"Couldn't find a scenario named \"{scenarioName}\"");
        }

        public void OnGUI()
        {
            if (initialized && showSelectionUI)
            {
                if (shouldResetUIHeight && Event.current.type == EventType.Layout)
                {
                    selectionWindowRect.height = 300;
                    shouldResetUIHeight = false;
                }

                selectionWindowRect = GUILayout.Window(GetInstanceID(), selectionWindowRect, SelectionWindow, "Scenario Selector", HighLogic.Skin.window);
            }
            if(initialized && showEditUI && CurrentScenario != null)
            {
                editWindowRect = GUILayout.Window(GetInstanceID() + 1, editWindowRect, ScenarioManagerSettings.EditWindow, "Edit starting parameters", HighLogic.Skin.window);
            }
        }

        private void SelectionWindow(int windowID)
        {
            GUILayout.BeginVertical(HighLogic.Skin.box);
            {
                GUILayout.Label("Choose Scenario preset:", HighLogic.Skin.label);
                {
                    int oldConfigIdx = selectedScenarioIndex;
                    selectedScenarioIndex = GUILayout.SelectionGrid(selectedScenarioIndex, loadedScenarioNames, 1, HighLogic.Skin.button);
                    if (oldConfigIdx != selectedScenarioIndex)
                    {
                        shouldResetUIHeight = true;
                        Utilities.Log("Selected Scenario changed, updating values");
                        ScenarioManagerSettings.UpdateFromScenario(CurrentScenario);
                    }
                }
                GUILayout.BeginHorizontal();
                GUILayout.Label(CurrentScenario.Description);
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();

            GUILayout.Space(7);

            GUILayout.BeginVertical();
            if(CurrentScenario != null)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Edit", HighLogic.Skin.button))
                {
                    showEditUI = !showEditUI;
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Close", HighLogic.Skin.button))
            {
                showSelectionUI = false;
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUI.DragWindow();
        }

        public static void UpdateCurrentScenario()
        {
            CurrentScenario.UpdateFromSettings();
        }

        public static void ShowSelectionUI(bool b)
        {
            showSelectionUI = b;
        }

        public static void ShowEditUI(bool b)
        {
            showEditUI = b;
        }
    }
}
