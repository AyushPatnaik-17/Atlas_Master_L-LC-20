using UnityEngine;
using InnovateLabs.Utilities;

public class ProjectData_Hololens : ScriptableObject
{
    [ReadValueAtInspector] public string applicationName;
    [ReadValueAtInspector] public Texture2D appIcon;
    [ReadValueAtInspector] public string organisationName;
    [ReadValueAtInspector] public string packageName;
    [ReadValueAtInspector] public string packageDescription;
    [ReadValueAtInspector] public string deviceType;
    [ReadValueAtInspector] public string architecture;
    [ReadValueAtInspector] public string buildPath;
}
