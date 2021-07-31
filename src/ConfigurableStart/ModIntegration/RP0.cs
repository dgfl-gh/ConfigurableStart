using System;
using System.Reflection;
using UniLinq;

namespace ConfigurableStart
{
    public static class RP0
    {
        private static bool? isInstalled = null;
        private static object maintenanceHandler = null;
        private static Type maintenanceHandlerType = null;
        private static Type careerEventScopeType = null;
        private static Type careerEventTypeEnumType = null;

        public static bool Found
        {
            get
            {
                if (!isInstalled.HasValue)
                {
                    Assembly a = AssemblyLoader.loadedAssemblies.FirstOrDefault(la => string.Equals(la.dllName, "RP0", StringComparison.OrdinalIgnoreCase))?.assembly;

                    isInstalled = false;
                    if (a == null)
                        return isInstalled.Value;
                    
                    isInstalled = true;
                    maintenanceHandlerType = a.GetType("RP0.MaintenanceHandler");
                    careerEventScopeType = a.GetType("RP0.CareerEventScope");
                    careerEventTypeEnumType = a.GetType("RP0.CareerEventType");
                }

                return isInstalled.Value;
            }
        }

        public static object MaintenanceHandler
        {
            get
            {
                if (Found && maintenanceHandler == null)
                {
                    maintenanceHandler = maintenanceHandlerType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
                }
                return maintenanceHandler;
            }
        }

        public static void ResetLastMaintenanceUpdate(double newUT)
        {
            if (maintenanceHandlerType == null)
                return;

            try
            {
                maintenanceHandlerType.GetField("lastUpdate").SetValue(maintenanceHandler, newUT);
            }
            catch (Exception ex)
            {
                Utilities.LogErr($"Couldn't update last RP0 maintenance");
                Utilities.LogErr(ex);
            }
        }
        
        public static object InvokeCareerEventScope()
        {
            if (careerEventScopeType == null || careerEventTypeEnumType == null)
            {
                Utilities.LogWrn("Couldn't find career log hook. Have you updated RP0?");
                return null;
            }

            try
            {
                var ignoreEventFieldInfo = careerEventTypeEnumType.GetField("Ignore");
                var ctor = careerEventScopeType.GetConstructor(new Type[] {careerEventTypeEnumType});

                return ctor?.Invoke(new object[] {ignoreEventFieldInfo.GetValue(careerEventTypeEnumType)});
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
