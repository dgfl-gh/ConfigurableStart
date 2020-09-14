using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace CustomScenarioManager
{
    public static class KCT
    {
        private static bool? _isInstalled = null;
        private static Type KCTGameStatesType = null;

        public static bool Found
        {
            get
            {
                if (!_isInstalled.HasValue)
                {
                    AssemblyLoader.loadedAssemblies.TypeOperation(t =>
                    {
                        if (t.FullName == "KerbalConstructionTime.KCTGameStates")
                        {
                            KCTGameStatesType = t;
                            Utilities.Log("KCT detected");
                        }
                    });

                    _isInstalled = KCTGameStatesType != null;
                }

                return _isInstalled.Value;
            }
        }

        public static void CreatePads(Dictionary<string, int> pads, bool removeOldPads)
        {
            if (!Found) return;

            if (removeOldPads && KCTGameStatesType.GetMethod("ClearLaunchpadList") is MethodInfo ClearPads)
            {
                ClearPads.Invoke(null, new object[] { });
            }

            if (KCTGameStatesType.GetMethod("CreateNewPad") is MethodInfo CreatePad)
            {
                foreach (var padKvp in pads)
                {
                    string padName = padKvp.Key;
                    int level = padKvp.Value;

                    CreatePad.Invoke(null, new object[] { padName, level });
                    Utilities.Log($"Created new KCT pad: {padName}, level {level}");
                    CustomScenarioData.kctLaunchpads.Append($"{padName}@{level},");
                }
            }
            else
            {
                Utilities.Log("Couldn't find CreateNewPad method. Have you updated KCT?");
            }
        }
    }
}
