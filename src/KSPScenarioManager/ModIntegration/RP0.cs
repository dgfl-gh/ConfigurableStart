using System;
using System.Reflection;
using Expansions.Missions.Editor;
using UniLinq;
using UnityEngine;

namespace CustomScenarioManager
{
    public static class RP0
    {
        private static bool? _isInstalled = null;
        private static object _maintenanceHandler = null;
        private static Type MaintenanceHandlerType = null;
        private static Type CareerEventScopeType = null;
        private static Type CareerEventTypeEnumType = null;

        public static bool Found
        {
            get
            {
                if (!_isInstalled.HasValue)
                {
                    Assembly a = AssemblyLoader.loadedAssemblies.FirstOrDefault(la => string.Equals(la.dllName, "RP0", StringComparison.OrdinalIgnoreCase))?.assembly;
                    Type t = a?.GetType("RP0.CareerEventScope");

                    _isInstalled = false;
                    if (a == null)
                        return _isInstalled.Value;
                    
                    _isInstalled = true;
                    MaintenanceHandlerType = a.GetType("RP0.MaintenanceHandler");
                    CareerEventScopeType = a.GetType("RP0.CareerEventScope");
                    CareerEventTypeEnumType = a.GetType("RP0.CareerEventType");
                }

                return _isInstalled.Value;
            }
        }

        public static object MaintenanceHandler
        {
            get
            {
                if (Found && _maintenanceHandler == null)
                {
                    _maintenanceHandler = MaintenanceHandlerType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
                }
                return _maintenanceHandler;
            }
        }

        public static void ResetLastMaintenanceUpdate(double newUT)
        {
            if (MaintenanceHandlerType == null) return;

            try
            {
                MaintenanceHandlerType.GetField("lastUpdate").SetValue(_maintenanceHandler, newUT);
            }
            catch (Exception ex)
            {
                Utilities.LogErr($"Couldn't update last RP0 maintenance");
                Utilities.LogErr(ex);
            }
        }
        
        public static object InvokeCareerEventScope()
        {
            if (CareerEventScopeType == null || CareerEventTypeEnumType == null)
            {
                Utilities.LogWrn("Couldn't find career log hook. Have you updated RP0?");
                return null;
            }

            try
            {
                var ignoreEventFieldInfo = CareerEventTypeEnumType.GetField("Ignore");
                var ctor = CareerEventScopeType.GetConstructor(new Type[] {CareerEventTypeEnumType});

                return ctor?.Invoke(new object[] {ignoreEventFieldInfo.GetValue(CareerEventTypeEnumType)});
            }
            catch (Exception ex)
            {
                Utilities.LogErr("Couldn't invoke RP0 CareerLog scope");
                Utilities.LogErr(ex);

                return null;
            }
        }
        
        public static void DisposeCareerEventScope(object scope)
        {
            // simultaneously get method and check if scope == null
            scope?.GetType().GetMethod("Dispose", BindingFlags.Public | BindingFlags.Instance)?.Invoke(scope, null);
        }
    }
}
