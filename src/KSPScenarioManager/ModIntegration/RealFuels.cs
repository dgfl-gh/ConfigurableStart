using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CustomScenarioManager
{
    public static class RealFuels
    {
        private static bool? _isInstalled = null;
        private static Type EntryCostDatabaseType = null;

        public static bool Found
        {
            get
            {
                if (!_isInstalled.HasValue)
                {
                    AssemblyLoader.loadedAssemblies.TypeOperation(t =>
                    {
                        if (t.FullName == "RealFuels.EntryCostDatabase")
                        {
                            EntryCostDatabaseType = t;
                            Utilities.Log("RealFuels detected");
                        }
                    });

                    _isInstalled = EntryCostDatabaseType != null;
                }

                return _isInstalled.Value;
            }
        }

        public static void UnlockEngineConfigs(string[] configNames)
        {
            if (!Found) return;

            if (EntryCostDatabaseType.GetMethod("SetUnlocked", new Type[] { typeof(string) }) is MethodInfo SetUnlocked)
            {
                foreach (string config in configNames)
                {
                    SetUnlocked.Invoke(null, new object[] { config });
                    Utilities.Log($"Unlocked {config} engine config");
                }
            }
            else
            {
                Utilities.Log("Couldn't unlock RF engine configs");
            }
        }
    }
}
