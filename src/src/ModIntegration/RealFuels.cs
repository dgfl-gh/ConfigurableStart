using System;
using System.Reflection;

namespace ConfigurableStart
{
    public static class RealFuels
    {
        private static bool? isInstalled = null;
        private static Type entryCostDatabaseType = null;

        public static bool Found
        {
            get
            {
                if (!isInstalled.HasValue)
                {
                    AssemblyLoader.loadedAssemblies.TypeOperation(t =>
                    {
                        if (t.FullName == "RealFuels.EntryCostDatabase")
                        {
                            entryCostDatabaseType = t;
                            Utilities.Log("RealFuels detected");
                        }
                    });

                    isInstalled = entryCostDatabaseType != null;
                }

                return isInstalled.Value;
            }
        }

        public static void UnlockEngineConfigs(string[] configNames)
        {
            if (!Found) return;

            if (entryCostDatabaseType.GetMethod("SetUnlocked", new Type[] { typeof(string) }) is MethodInfo SetUnlocked)
            {
                foreach (string config in configNames)
                {
                    SetUnlocked.Invoke(null, new object[] { config });
                    Utilities.Log($"Unlocked {config} engine config");
                }
            }
            else
            {
                Utilities.LogErr("Couldn't unlock RF engine configs");
            }
        }
    }
}
