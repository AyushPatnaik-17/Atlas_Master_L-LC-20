using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


namespace InnovateLabs.Utilities
{
    public static class PolygonGenerator 
    {
        //mesh properties
        static Mesh mesh;
        public static Vector3[] polygonPoints;
        public static int[] polygonTriangles;

        //polygon properties
        public static bool isFilled;
        public static int polygonSides;
        public static float polygonRadius;
        public static float centerRadius;

        private static readonly int cricleSide = 45;

        public static void InitMesh(Transform meshObject)
        {
            mesh = new Mesh();
            meshObject.GetComponent<MeshFilter>().mesh = mesh;
        }

        static void DrawFilled(int sides, float radius)
        {
            polygonPoints = GetCircumferencePoints(sides, radius).ToArray();
            polygonTriangles = DrawFilledTriangles(polygonPoints);
            mesh.Clear();
            mesh.vertices = polygonPoints;
            mesh.triangles = polygonTriangles;
        }

        static void DrawHollow(int sides, float outerRadius, float innerRadius)
        {
            List<Vector3> pointsList = new List<Vector3>();
            List<Vector3> outerPoints = GetCircumferencePoints(sides, outerRadius);
            pointsList.AddRange(outerPoints);
            List<Vector3> innerPoints = GetCircumferencePoints(sides, innerRadius);
            pointsList.AddRange(innerPoints);

            polygonPoints = pointsList.ToArray();

            polygonTriangles = DrawHollowTriangles(polygonPoints);
            mesh.Clear();
            mesh.vertices = polygonPoints;
            mesh.triangles = polygonTriangles;
        }

        static int[] DrawHollowTriangles(Vector3[] points)
        {
            int sides = points.Length / 2;
            int triangleAmount = sides * 2;

            List<int> newTriangles = new List<int>();
            for (int i = 0; i < sides; i++)
            {
                int outerIndex = i;
                int innerIndex = i + sides;

                //first triangle starting at outer edge i
                newTriangles.Add(outerIndex);
                newTriangles.Add(innerIndex);
                newTriangles.Add((i + 1) % sides);

                //second triangle starting at outer edge i
                newTriangles.Add(outerIndex);
                newTriangles.Add(sides + ((sides + i - 1) % sides));
                newTriangles.Add(outerIndex + sides);
            }
            return newTriangles.ToArray();
        }

        static List<Vector3> GetCircumferencePoints(int sides, float radius)
        {
            List<Vector3> points = new List<Vector3>();
            float circumferenceProgressPerStep = (float)1 / sides;
            float TAU = 2 * Mathf.PI;
            float radianProgressPerStep = circumferenceProgressPerStep * TAU;

            for (int i = 0; i < sides; i++)
            {
                float currentRadian = radianProgressPerStep * i;
                points.Add(new Vector3(Mathf.Cos(currentRadian) * radius, Mathf.Sin(currentRadian) * radius, 0));
            }
            return points;
        }

        static int[] DrawFilledTriangles(Vector3[] points)
        {
            int triangleAmount = points.Length - 2;
            List<int> newTriangles = new List<int>();
            for (int i = 0; i < triangleAmount; i++)
            {
                newTriangles.Add(0);
                newTriangles.Add(i + 2);
                newTriangles.Add(i + 1);
            }
            return newTriangles.ToArray();
        }

        public static void DrawRectangleMarker(float length, float width, float border)
        {
            float ilength = length - border;
            float iwidth = width - border;

            Vector3[] rectPoints =
            {
            //outer co-ordinates

            new Vector3(-length/2,-width/2,0),
            new Vector3(-length/2, width/2, 0),
            new Vector3(length/2, width/2, 0),
            new Vector3(length/2, -width/2, 0 ),

            //inner co-ordinates

            new Vector3(-ilength/2,-iwidth/2,0),
            new Vector3(-ilength/2, iwidth/2, 0),
            new Vector3(ilength/2, iwidth/2, 0),
            new Vector3(ilength/2, -iwidth/2, 0 ),
        };

            int[] trias =
            {
             0,1,5,
             5,4,0,
             1,2,5,
             2,6,5,
             2,3,6,
             3,7,6,
             3,0,7,
             0,4,7

        };

            mesh.Clear();
            mesh.vertices = rectPoints;
            mesh.triangles = trias;
            mesh.RecalculateNormals();
        }

        public static void DrawSquareMarker(float side, float border)
        {
            DrawRectangleMarker(side, side, border);
        }

        public static void DrawPolygonMarker(int sides,float inscribedRadius, float border)
        {
            DrawHollow(sides, inscribedRadius, inscribedRadius-border);
        }

        public static void DrawCircleMarker(float radius, float border)
        {
            DrawHollow(PolygonGenerator.cricleSide, radius, radius - border);
        }

        public static void SetMarkerText(ref Transform marker, string dimension)
        {
            var bounds = marker.GetComponent<MeshFilter>().sharedMesh.bounds;
            var rect = marker.GetComponentInChildren<RectTransform>();

            rect.sizeDelta = new Vector2(bounds.size.x * 0.5f, bounds.size.y * 0.5f);

            var markerText = marker.GetComponentInChildren<TextMeshPro>();
            var textSize = markerText.fontSize;
            markerText.text = $"<size={textSize*1.25}><b>Surface Marker</b></size>\n\nPoint to the location.\nPinch from right hand to set the position\n\n<i>Dimension : <b>{dimension}</b></i>";
        }

    }

}
