using UnityEngine;
using InnovateLabs.Utilities;

namespace InnovateLabs.Projects
{
    public class MarkerData : ScriptableObject
    {
        [ReadValueAtInspector] public string currentMarkerType;

        // Marker Border

        [ReadValueAtInspector] public float border;
        [ReadValueAtInspector] public string border_unit;

        //Rectangle Marker

        [ReadValueAtInspector] public float length;
        [ReadValueAtInspector] public string length_unit;

        [ReadValueAtInspector] public float width;
        [ReadValueAtInspector] public string width_unit;

        //Square Marker

        [ReadValueAtInspector] public float sidelength;
        [ReadValueAtInspector] public string sidelength_unit;

        //Polygon Marker

        [ReadValueAtInspector] public int sides;
        [ReadValueAtInspector] public float inscribedRadius;
        [ReadValueAtInspector] public string inscribedRadius_unit;

        // Circle Marker

        [ReadValueAtInspector] public float radius;
        [ReadValueAtInspector] public string radius_unit;

        public void SetMarkerData(MarkerInfo markerInfo)
        {
            currentMarkerType = markerInfo.markerType.value;

            border = markerInfo.border.value;
            border_unit = markerInfo.border.GetDimension();

            length = markerInfo.rectangleMarker.length.value;
            length_unit = markerInfo.rectangleMarker.length.GetDimension();

            width = markerInfo.rectangleMarker.width.value;
            width_unit = markerInfo.rectangleMarker.width.GetDimension();

            sidelength = markerInfo.squareMarker.sideLength.value;
            sidelength_unit = markerInfo.squareMarker.sideLength.GetDimension();

            sides = markerInfo.polygonMarker.side;
            inscribedRadius = markerInfo.polygonMarker.inscribedRadius.value;
            inscribedRadius_unit = markerInfo.polygonMarker.inscribedRadius.GetDimension();

            radius_unit = markerInfo.circleMarker.radius.GetDimension();
            radius = markerInfo.circleMarker.radius.value;
        }

    }
}