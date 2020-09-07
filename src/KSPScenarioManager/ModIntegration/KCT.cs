using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

            if(KCTGameStatesType.GetMethod("CreateNewPad") is MethodInfo CreatePad)
            {
                foreach (var padKvp in pads)
                {
                    string padName = padKvp.Key;
                    int level = padKvp.Value;

                    CreatePad.Invoke(null, new object[] { padName, level, removeOldPads });
                    removeOldPads = false;
                }
            }
            else
            {
                Utilities.Log("Couldn't find CreateNewPad method. Have you updated KCT?");
            }
        }

        public static void AddUpgradePoints(List<string> upgrades)
        {
            int unspentPoints = 0, vabPoints = 0, sphPoints = 0, rdPoints = 0;

            if (upgrades.FirstOrDefault(s => int.TryParse(s, out unspentPoints)) is string p)
            {
                upgrades.Remove(p);
            }
            var dict = Utilities.DictionaryFromStringArray(upgrades.ToArray());
            foreach (var key in dict.Keys)
            {
                switch (key)
                {
                    case "VAB": vabPoints = dict[key]; break;
                    case "SPH": sphPoints = dict[key]; break;
                    case "RD": rdPoints = dict[key]; break;
                }
            }

            if (KCTGameStatesType.GetMethod("UpdateUpgradePoints") is MethodInfo UpdateUpgradePoints)
            {
                UpdateUpgradePoints.Invoke(null, new object[] { unspentPoints, vabPoints, sphPoints, rdPoints });
            }
            else
            {
                Utilities.Log("Couldn't find UpdateUpgradePoints method. Have you updated KCT?");
            }
        }
    }
}
