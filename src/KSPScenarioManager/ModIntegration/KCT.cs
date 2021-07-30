using System;
using System.Collections.Generic;
using UniLinq;
using System.Reflection;

namespace CustomScenarioManager
{
    public static class KCT
    {
        private static bool? _isInstalled = null;
        private static Type KCTGameStatesType = null;
        private static Type KSCItemType = null;

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
                        }
                        else if (t.FullName == "KerbalConstructionTime.KSCItem")
                        {
                            KSCItemType = t;
                        }
                    });

                    if ((bool)(_isInstalled = KCTGameStatesType != null && KSCItemType != null))
                        Utilities.Log("KCT detected");
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
                Utilities.LogErr("Couldn't find CreateNewPad method. Have you updated KCT?");
            }
        }

        public static void SetUpgradePoints(string kctUpgrades)
        {
            Dictionary<string, string> sites = Utilities.DictionaryFromCommaSeparatedString<string>(kctUpgrades);

            if (!string.IsNullOrEmpty(sites?.FirstOrDefault().Value))
            {
                foreach (var site in sites)
                {
                    int[] points = Utilities.ArrayFromString<int>(site.Value, '-');

                    if (points is null || points.Length < 1)
                        continue;

                    int build = points[0];
                    int dev = points.Length > 1 ? points[1] : 0;
                    int research = points.Length > 2 ? points[2] : 0;

                    if (SetUpgradePointsForKSC(site.Key, build, dev, research))
                        Utilities.Log($"Set upgrades for KSC site {site.Key}");
                }
            }
            else
            {
                int[] points = Utilities.ArrayFromString<int>(kctUpgrades, '-');

                if (points != null && points.Length > 0)
                {
                    int build = points[0];
                    int dev = points.Length > 1 ? points[1] : 0;
                    int research = points.Length > 2 ? points[2] : 0;

                    if (SetUpgradePointsForKSC(null, build, dev, research))
                        Utilities.Log("Set upgrades for default KSC");
                }
            }
        }

        public static bool SetUpgradePointsForKSC(string KSC, int buildPnts, int devPnts, int researchPnts = 0)
        {
            if (!Found) return false;

            Type kscItemType = null;
            object kscItem = null;

            if (!string.IsNullOrEmpty(KSC))
            {
                // get List<KSCItem>
                // for some reason casting to a list fails, while casting to a IEnumerable works
                var KSCs = KCTGameStatesType.GetField("KSCs", BindingFlags.Public | BindingFlags.Static)?.GetValue(null) as IEnumerable<object>;

                foreach (var item in KSCs ?? Enumerable.Empty<object>())
                {
                    if (KSC == (string)item.GetType().GetField("KSCName", BindingFlags.Public | BindingFlags.Instance).GetValue(item))
                    {
                        kscItemType = item.GetType();
                        kscItem = item;
                        break;
                    }
                }
            }
            else
            {
                var kscItemFi = KCTGameStatesType.GetField("ActiveKSC", BindingFlags.Public | BindingFlags.Static);

                kscItemType = kscItemFi?.FieldType;
                kscItem = kscItemFi?.GetValue(null);
            }

            //in KCT:
            //public List<int> VABUpgrades = new List<int>() { 0 };
            //public List<int> RDUpgrades = new List<int>() { 0, 0 }; //unlock nodes / generate science

            if (kscItemType is null || kscItem is null)
                return false;

            var vabUpgradesFi = kscItemType.GetField("VABUpgrades", BindingFlags.Public | BindingFlags.Instance);
            var RDUpgradesFi = kscItemType.GetField("RDUpgrades", BindingFlags.Public | BindingFlags.Instance);

            bool b = false;

            if (vabUpgradesFi != null && buildPnts > 0)
            {
                vabUpgradesFi.SetValue(kscItem, new List<int> { buildPnts });
                b = true;
            }

            if (RDUpgradesFi != null && (researchPnts > 0 || devPnts > 0))
            {
                RDUpgradesFi.SetValue(kscItem, new List<int> { devPnts, researchPnts });
                b = true;
            }

            if (b)
            {
                SetUnspentPoints(buildPnts + researchPnts + devPnts, add: true);
                CustomScenarioData.kctUpgrades.Append($"{(string)kscItemType.GetField("KSCName").GetValue(kscItem)}@{buildPnts}-{devPnts}-{researchPnts},");
            }

            return b;
        }

        public static void SetUnspentPoints(int points, bool add = false)
        {
            if (!Found || points < 0) return;

            var fi = KCTGameStatesType.GetField("PurchasedUpgrades", BindingFlags.Public | BindingFlags.Static);
            var upgrades = fi.GetValue(null) as List<int>;

            if (add)
                upgrades[1] += points;
            else
                upgrades[1] = points;

            fi.SetValue(null, upgrades);
        }
    }
}
