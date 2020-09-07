using System;
using System.Reflection;
using UnityEngine;

namespace CustomScenarioManager
{
    public static class RP0
    {
        private static bool? _isInstalled = null;
        private static Type MaintenanceHandlerType = null;
        private static object instance = null;

        public static bool Found
        {
            get
            {
                if (!_isInstalled.HasValue)
                {
                    AssemblyLoader.loadedAssemblies.TypeOperation(t =>
                    {
                        if (t.FullName == "RP0.MaintenanceHandler")
                        {
                            MaintenanceHandlerType = t;
                            Utilities.Log("RP0 detected");
                        }
                    });

                    _isInstalled = MaintenanceHandlerType != null;
                }

                return _isInstalled.Value;
            }
        }

        public static object Instance
        {
            get
            {
                if (Found && instance == null)
                {
                    instance = MaintenanceHandlerType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static).GetValue(null);
                }
                return instance;
            }
            set => instance = value;
        }

        public static void ResetLastMaintenanceUpdate(double newUT)
        {
            if (Instance == null) return;

            try
            {
                MaintenanceHandlerType.GetField("lastUpdate").SetValue(instance, newUT);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
        }
    }
}
