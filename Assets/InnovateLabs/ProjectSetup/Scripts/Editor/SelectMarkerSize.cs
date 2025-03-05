using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;
using UnityEditor.Build;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using InnovateLabs.Utilities;
using System;

namespace InnovateLabs.Projects
{
    public class SelectMarkerSize : EditorWindow
    {
        MarkerData markerData;

        DropdownField markerType;

        List<VisualElement> markers = new List<VisualElement>();

        LengthInputElement border;

        RectangleMarker rectangleMarker;
        SquareMarker squareMarker;
        PolygonMarker polygonMarker;
        CircleMarker circleMarker;

        Button saveMarker;

        VisualElement editMarker;

        [MenuItem("Innovate Labs/Select Marker Size %#W", false, 22)]
        public static void ProjectSetupMarkerSize()
        {
            SelectMarkerSize wnd = GetWindow<SelectMarkerSize>();
            wnd.titleContent = new GUIContent("Select Marker");
            wnd.Show();
        }
        

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;

            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/InnovateLabs/ProjectSetup/Scripts/Editor/SelectMarkerSize.uxml");
            VisualElement UIFromXML = visualTree.Instantiate();
            root.Add(UIFromXML);

            Init(root);

        }

        private void Init(VisualElement root)
        {
            markerType = root.Q<DropdownField>("MarkerType");

            var rectangle = root.Q<VisualElement>("RectangleMarker");
            markers.Add(rectangle);
            rectangle.style.display = DisplayStyle.None;

            var square = root.Q<VisualElement>("SquareMarker");
            markers.Add(square);
            square.style.display = DisplayStyle.None;

            var polygon = root.Q<VisualElement>("PolygonMarker");
            markers.Add(polygon);
            polygon.style.display = DisplayStyle.None;

            var circle = root.Q<VisualElement>("CircleMarker");
            markers.Add(circle);
            circle.style.display = DisplayStyle.None;


            markerType.RegisterCallback<ChangeEvent<string>>(e =>
            {
                OnMarkerTypeChange(markerType.index);
                UpdateMarker();
            });

            saveMarker = root.Q<Button>("SaveMarker");

            editMarker = root.Q<VisualElement>("SavedActions");

            border = new LengthInputElement(root.Q<VisualElement>("Border"));

            rectangleMarker = new RectangleMarker(root.Q<VisualElement>("RectangleMarker"));
            squareMarker = new SquareMarker(root.Q<VisualElement>("SquareMarker"));
            polygonMarker = new PolygonMarker(root.Q<VisualElement>("PolygonMarker"));
            circleMarker = new CircleMarker(root.Q<VisualElement>("CircleMarker"));

            saveMarker.clicked += OnSaveClicked;
            editMarker.Q<Button>("EditMarker").clicked += OnEditClicked;
            editMarker.Q<Button>("ImportModel").clicked += OnImportClicked;

            var meshHolder = GameObject.FindObjectOfType<Marker>();
            
            PolygonGenerator.InitMesh(meshHolder.transform);

            //Write code to add paraphernelic marker details

            SubscribeValueChange();

            if (File.Exists("Assets/InnovateLabs/ProjectSetup/Data/MarkerData.asset"))
            {
                markerData = (MarkerData)AssetDatabase.LoadAssetAtPath("Assets/InnovateLabs/ProjectSetup/Data/MarkerData.asset", typeof(MarkerData));

                LoadData();

                OnSaveClicked();
            }
            else
            {
                markerData = ScriptableObject.CreateInstance<MarkerData>();

                var markerInfo = new MarkerInfo(markerType, border, rectangleMarker, squareMarker, polygonMarker, circleMarker);

                markerData.SetMarkerData(markerInfo);

                string projectDataPath = "Assets/InnovateLabs/ProjectSetup/Data/MarkerData.asset";

                AssetDatabase.CreateAsset(markerData, projectDataPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                rectangle.style.display = DisplayStyle.Flex;
            }
        }

        private void SubscribeValueChange()
        {
            border.OnValueChanged += () => { UpdateMarker(); };
            rectangleMarker.length.OnValueChanged += () => { UpdateMarker(); };
            rectangleMarker.width.OnValueChanged += () => { UpdateMarker(); };
            squareMarker.sideLength.OnValueChanged += () => { UpdateMarker(); };
            polygonMarker.inscribedRadius.OnValueChanged += () => { UpdateMarker(); };
            polygonMarker.OnSideChanged += () => { UpdateMarker(); };
            circleMarker.radius.OnValueChanged += () => { UpdateMarker(); };
        }

        private void OnMarkerTypeChange(int value)
        {
            for(int i=0; i < markers.Count; i++)
            {
                if(i==value)
                {
                    markers[i].style.display = DisplayStyle.Flex;
                }
                else
                {

                    markers[i].style.display = DisplayStyle.None;
                }
            }

            saveMarker.style.display = DisplayStyle.Flex;
            editMarker.style.display = DisplayStyle.None;
        }

        private void UpdateMarker()
        {
            var marker = FindObjectOfType<Marker>().transform;
            switch (markerType.index)
            {
                
                case 0: // Draw Rectangle
                    PolygonGenerator.DrawRectangleMarker(rectangleMarker.length.ValueInMeter(), rectangleMarker.width.ValueInMeter(), border.ValueInMeter());
                    
                    PolygonGenerator.SetMarkerText(ref marker, $"{rectangleMarker.length.value}{rectangleMarker.length.GetDimension()} x {rectangleMarker.width.value}{rectangleMarker.width.GetDimension()}");
                    break;
                case 1: // Draw Square
                    PolygonGenerator.DrawSquareMarker(squareMarker.sideLength.ValueInMeter(), border.ValueInMeter());
                    PolygonGenerator.SetMarkerText(ref marker, $"{squareMarker.sideLength.value}{squareMarker.sideLength.GetDimension()}");
                    break;
                case 2: // Draw Polygon
                    PolygonGenerator.DrawPolygonMarker(polygonMarker.side, polygonMarker.inscribedRadius.ValueInMeter(), border.ValueInMeter());
                    PolygonGenerator.SetMarkerText(ref marker, $"{polygonMarker.side} sides in {polygonMarker.inscribedRadius.value}{polygonMarker.inscribedRadius.GetDimension()}");
                    break;
                case 3: // Draw Circle
                    PolygonGenerator.DrawCircleMarker(circleMarker.radius.ValueInMeter(), border.ValueInMeter());
                    PolygonGenerator.SetMarkerText(ref marker, $"{circleMarker.radius.value}{circleMarker.radius.GetDimension()}");
                    break;
                default:
                    Debug.LogError("Error! Wrong Marker Type selected");
                    break;
            }

            markerData.SetMarkerData(new MarkerInfo(markerType, border, rectangleMarker, squareMarker, polygonMarker, circleMarker));

            EditorUtility.SetDirty(markerData);
        }


        private void LoadData()
        {

            markerType.SetValueWithoutNotify(markerType.choices[markerType.choices.IndexOf(markerData.currentMarkerType)]);

            border.SetDimension(markerData.border_unit);
            border.SetValue(markerData.border);

            rectangleMarker.length.SetDimension(markerData.length_unit);
            rectangleMarker.length.SetValue(markerData.length);

            rectangleMarker.width.SetDimension(markerData.width_unit);
            rectangleMarker.width.SetValue(markerData.width);

            squareMarker.sideLength.SetDimension(markerData.sidelength_unit);
            squareMarker.sideLength.SetValue(markerData.sidelength);

            polygonMarker.SetSides(markerData.sides);
            polygonMarker.inscribedRadius.SetDimension(markerData.inscribedRadius_unit);
            polygonMarker.inscribedRadius.SetValue(markerData.inscribedRadius);

            circleMarker.radius.SetDimension(markerData.radius_unit);
            circleMarker.radius.SetValue(markerData.radius);
            
            OnMarkerTypeChange(markerType.index);
            UpdateMarker();

        }

        private void OnSaveClicked()
        {
            ToggleEdit(false);

            saveMarker.style.display = DisplayStyle.None;
            editMarker.style.display = DisplayStyle.Flex;
        }
        private void OnEditClicked()
        {
            ToggleEdit(true);

            saveMarker.style.display = DisplayStyle.Flex;
            editMarker.style.display = DisplayStyle.None;
        }

        private void OnImportClicked()
        {
            GetWindow<ImportModel>("Import 3D Model",true, typeof(SelectMarkerSize));
        }

        private void ToggleEdit(bool isToggle)
        {
            markerType.SetEnabled(isToggle);
            border.SetEnabled(isToggle);
            foreach(VisualElement v in markers)
            {
                v.SetEnabled(isToggle);
            }
        }
    }

    
    [Serializable]
    public class MarkerInfo
    {
        public DropdownField markerType;
        public LengthInputElement border;
        public RectangleMarker rectangleMarker;
        public SquareMarker squareMarker;
        public PolygonMarker polygonMarker;
        public CircleMarker circleMarker;

        public MarkerInfo(DropdownField markerType,LengthInputElement border ,RectangleMarker rectangleMarker, SquareMarker squareMarker, PolygonMarker polygonMarker, CircleMarker circleMarker)
        {
            this.markerType = markerType;
            this.border = border;
            this.rectangleMarker = rectangleMarker;
            this.squareMarker = squareMarker;
            this.polygonMarker = polygonMarker;
            this.circleMarker = circleMarker;
        }
    }

    [Serializable]
    public class LengthInputElement
    {
        VisualElement container;
        FloatField size;
        DropdownField dimension;

        public float value;

        string previousUnit;
        string currentUnit;

        public Action OnValueChanged;

        public LengthInputElement(VisualElement container)
        {
            this.container = container;
            size = this.container.Q<FloatField>();
            dimension = this.container.Q<DropdownField>();


            value = size.value;
            currentUnit = dimension.value != null ? dimension.value : "mm";

            size.RegisterCallback<KeyDownEvent>(OnKeyDown);

            size.RegisterCallback<ChangeEvent<float>>(e=>
            {
                value = size.value;
                OnValueChanged?.Invoke();
            });

            dimension.RegisterCallback<ChangeEvent<string>>(e=>
            {
                previousUnit = currentUnit;
                currentUnit = dimension.value;

                OnDimensionChanged();
                if (OnValueChanged != null) OnValueChanged.Invoke();
            });

        }

        void OnKeyDown(KeyDownEvent e)
        {
            if ((e.keyCode >= KeyCode.Alpha0 && e.keyCode <= KeyCode.Alpha9) ||
                (e.keyCode >= KeyCode.Keypad0 && e.keyCode <= KeyCode.Keypad9) ||
                e.keyCode == KeyCode.Period)
            {
                string newVal = size.value.ToString() + e.character;
                if(float.TryParse(newVal, out float result))
                {
                    value = result;
                    OnValueChanged?.Invoke();
                }
            }
        }

        private void OnDimensionChanged()
        {
            Debug.Log($"previous dimension : {previousUnit} & current dimension {currentUnit}");
            if (previousUnit == "mm" && currentUnit == "m")
            {
                size.value /= 1000f;
                value = size.value;
            }
            else if (previousUnit == "mm" && currentUnit == "cm")
            {
                size.value /= 10f;
                value = size.value;
            }
            else if (previousUnit == "cm" && currentUnit == "m")
            {
                size.value /= 100f;
                value = size.value;
            }
            else if (previousUnit == "cm" && currentUnit == "mm")
            {
                size.value *= 10f;
                value = size.value;
            }
            else if (previousUnit == "m" && currentUnit == "mm")
            {
                size.value *= 1000f;
                value = size.value;
            }
            else if (previousUnit == "m" && currentUnit == "cm")
            {
                size.value *= 100f;
                value = size.value;
            }
            else
            {
                value = size.value;
            }
        }
        public float ValueInMeter()
        {
            float value_m = value;

            if(currentUnit == "mm")
            {
                value_m = value / 1000f;
            }
            if(currentUnit == "cm")
            {
                value_m = value / 100f;
            }

            return value_m;
        }
        public float ValueInMM()
        {
            float value_mm = value;
            
            if(currentUnit == "cm")
            {
                value_mm = value * 100f;
            }
            if(currentUnit == "m")
            {
                value_mm = value * 1000f;
            }

            return value_mm;
        }

        public string GetDimension()
        {
            return dimension.value;
        }

        public void SetDimension(string unit)
        {
            dimension.SetValueWithoutNotify(dimension.choices[dimension.choices.IndexOf(unit)]);
            currentUnit = dimension.value;
        }

        public void SetValue(float value)
        {
            size.SetValueWithoutNotify(value);
            this.value = value;
        }

        public void SetEnabled(bool value)
        {
            container.SetEnabled(value);
        }

    }

    [Serializable]
    public class RectangleMarker
    {
        VisualElement rectangleMarker;
        public LengthInputElement length;
        public LengthInputElement width;

        public RectangleMarker(VisualElement rectangleMarker)
        {
            this.rectangleMarker = rectangleMarker;

            length = new LengthInputElement(rectangleMarker.Q<VisualElement>("Length"));
            width = new LengthInputElement(rectangleMarker.Q<VisualElement>("Width"));
        }
    }
    [Serializable]
    public class SquareMarker
    {
        VisualElement squareMarker;
        public LengthInputElement sideLength;

        public SquareMarker(VisualElement squareMarker)
        {
            this.squareMarker = squareMarker;

            sideLength = new LengthInputElement(squareMarker.Q<VisualElement>("SideLength"));
        }
    }
    [Serializable]
    public class PolygonMarker
    {
        VisualElement polygonMarker;
        SliderInt sides;
        public LengthInputElement inscribedRadius;

        public int side = 0;
        public Action OnSideChanged;
        public PolygonMarker(VisualElement polygonMarker)
        {
            this.polygonMarker = polygonMarker;
            sides = polygonMarker.Q<SliderInt>();
            inscribedRadius = new LengthInputElement(polygonMarker.Q<VisualElement>("PolygonRadius"));
            side = sides.value;
            sides.RegisterCallback<ChangeEvent<int>>(e => 
            {
                side = sides.value;
                if (OnSideChanged != null) OnSideChanged.Invoke();
            });
        }

        public void SetSides(int value)
        {
            sides.SetValueWithoutNotify(value);
            this.side = value;
        }
    }
    [Serializable]
    public class CircleMarker
    {
        VisualElement circleMarker;
        public LengthInputElement radius;
        
        public CircleMarker(VisualElement circleMarker)
        {
            this.circleMarker = circleMarker;
            radius = new LengthInputElement(circleMarker.Q<VisualElement>("CircleRadius"));
        }
    }

    public enum Units
    {
        mm,
        cm,
        m
    }
}
