using System;
using System.Collections.Generic;
using System.Reflection;

namespace ConfigurableStart
{
    public static class TestFlight
    {
        private static bool? isInstalled = null;
        private static Type flightManagerScenarioType = null;
        private static object instance = null;

        public static bool Found
        {
            get
            {
                if (!isInstalled.HasValue)
                {
                    AssemblyLoader.loadedAssemblies.TypeOperation(t =>
                    {
                        if (t.FullName == "TestFlightCore.TestFlightManagerScenario")
                        {
                            flightManagerScenarioType = t;
                            Utilities.Log("TestFlight detected");
                        }
                    });

                    isInstalled = flightManagerScenarioType != null;
                }

                return isInstalled.Value;
            }
        }

        public static object FlightManagerScenarioInstance
        {
            get
            {
                if (Found && instance == null)
                {
                    instance = flightManagerScenarioType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
                    if (instance == null)
                        Utilities.LogWrn("Couldn't hook into TF flight data manager instance");
                }
                return instance;
            }
        }

        public static void SetFlightDataForParts(Dictionary<string, float> data)
        {
            if (FlightManagerScenarioInstance == null) return;

            if (flightManagerScenarioType.GetMethod("SetFlightDataForPartName") is MethodInfo SetFlightDataForPartName)
            {
                foreach (KeyValuePair<string, float> kvp in data)
                {
                    try
                    {
                        //kvp.Key = part name
                        //kvp.Value = flight data
                        SetFlightDataForPartName.Invoke(FlightManagerScenarioInstance, new object[] { kvp.Key, kvp.Value });
                        Utilities.Log($"Flight Data for part {kvp.Key} set to {kvp.Value}");
                    }
                    catch (Exception ex)
                    {
                        Utilities.LogErr(ex);
                        Utilities.LogErr("Couldn't set TF flight data");
                        break;
                    }
                }
            }
        }
    }
}
