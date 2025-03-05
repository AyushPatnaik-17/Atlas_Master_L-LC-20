using InnovateLabs.Utilities;
using System;
using UnityEngine;

namespace InnovateLabs
{
    public class ModelInfo : MonoBehaviour
    {
        public static string dateFormat = "dd-MM-yyyy HH-mm-ss";

        [SerializeField]
        [ReadValueAtInspector]
        private LengthUnit currentUnit;
        [ReadValueAtInspector]
        public string lastExported;
        [ReadValueAtInspector]
        public string lastImported;

        ModelInfo(LengthUnit unit)
        {
            currentUnit = unit;
        }


        public void SetCurrentUnit(LengthUnit unit)
        {
            currentUnit = unit;
        }
        public void SetCurrentUnit(string unit)
        {
            try
            {
                currentUnit = (LengthUnit)Enum.Parse(typeof(LengthUnit), unit);
            }
            catch (ArgumentException)
            {
                Debug.LogError($"Incorrect spelling. Try : {LengthUnit.mm}, {LengthUnit.cm}, {LengthUnit.m}");
            }
        }

        public LengthUnit GetCurrentUnit()
        {
            return currentUnit;
        }
        [ContextMenu("Test the case")]
        public void Test()
        {
            SetCurrentUnit("mm");
        }
    }


    public enum LengthUnit
    {
        [InspectorName("mm")] mm,
        [InspectorName("cm")] cm,
        [InspectorName("m")] m
    }

    public static class UnitConversion
    {
        public static float ConvertLength(float value, LengthUnit from, LengthUnit to)
        {
            var convertedValue = value;

            switch (from)
            {
                case LengthUnit.mm:
                    switch (to)
                    {
                        case LengthUnit.cm:
                            convertedValue = value / 10f; 
                            break;
                        case LengthUnit.m:
                            convertedValue = value / 1000f;
                            break;
                        default:
                            Debug.LogError("Incorrect conversion unit selected");
                            break;
                    }
                    break;
                case LengthUnit.cm:
                    switch (to)
                    {
                        case LengthUnit.mm:
                            convertedValue = value * 10f; 
                            break;
                        case LengthUnit.m:
                            convertedValue = value / 100f;
                            break;
                        default:
                            Debug.LogError("Incorrect conversion unit selected");
                            break;
                    }
                    break;
                case LengthUnit.m:
                    switch (to)
                    {
                        case LengthUnit.mm:
                            convertedValue = value * 1000f; 
                            break;
                        case LengthUnit.cm:
                            convertedValue = value * 100f;
                            break;
                        default:
                            Debug.LogError("Incorrect conversion unit selected");
                            break;
                    }
                    break;

                default:
                    break;
            }

            return convertedValue;
        }
    }
}