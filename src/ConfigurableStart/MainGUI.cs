using System;
using UnityEngine;

namespace ConfigurableStart
{
    [KSPAddon(KSPAddon.Startup.MainMenu, false)]
    public class MainGUI : MonoBehaviour
    {
        public static MainGUI Instance;
        public bool Initialized { get; set; }

        //TODO: find a better starting position
        private static Rect selectionWindowRect = new Rect(267, 104, 400, 200);
        private static Rect editWindowRect      = new Rect(Screen.width - 850, 104, 800, 100);
        private static bool shouldResetUIHeight = false;
        private static bool showUI     = false;

        private static string[] loadedScenarioNames;
        private static int      selectedScenarioIndex = 0;

        public void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
                return;
            }

            Instance = this;
        }

        public void Start()
        {
            AttachToMainMenu();
        }

        public static void ShowSelectionWindow(bool b) => showUI = b;

        public void Setup(string[] names)
        {
            loadedScenarioNames = names;
            Initialized = true;
        }
        
        public void AttachToMainMenu()
        {
            if (FindObjectOfType<MainMenu>() is MainMenu menu)
            {
                menu.newGameBtn.onTap += delegate { ShowSelectionWindow(true); };
            }
            else
            {
                Utilities.LogErr("Couldn't find MainMenu. GUI disabled");
                EditorGUI.ShowEditorWindow(false);
                ShowSelectionWindow(false);
            }
        }
        
        public void OnGUI()
        {
            if (Initialized && showUI)
            {
                if (shouldResetUIHeight && Event.current.type == EventType.Layout)
                {
                    selectionWindowRect.height = 300;
                    shouldResetUIHeight = false;
                }

                selectionWindowRect = GUILayout.Window(GetInstanceID(), selectionWindowRect, SelectionWindow, "Scenario Selector", HighLogic.Skin.window);
            }
            if (Initialized && EditorGUI.showUI)
            {
                editWindowRect = GUILayout.Window(GetInstanceID() + 1, editWindowRect, EditorGUI.Instance.EditWindow, "Edit starting parameters", HighLogic.Skin.window);
            }
        }

        private static void SelectionWindow(int windowID)
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
                        ScenarioLoader.SetCurrentScenarioFromName(loadedScenarioNames[selectedScenarioIndex]);
                    }
                }
                GUILayout.BeginHorizontal();
                GUILayout.Label(ScenarioLoader.CurrentScenario?.Description ?? "");
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();

            GUILayout.Space(7);

            GUILayout.BeginVertical();
            //TODO
            //if (ScenarioLoader.CurrentScenario != null)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Edit", HighLogic.Skin.button))
                {
                    EditorGUI.showUI = !EditorGUI.showUI;
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Close", HighLogic.Skin.button))
            {
                showUI = false;
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUI.DragWindow();
        }
    }
}