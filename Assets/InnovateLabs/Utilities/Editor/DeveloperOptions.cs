using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace InnovateLabs.Projects
{
    public class DeveloperOptions : EditorWindow
    {
        private string _correctPassword = "DxDevs420";
        private TextField _enteredPassword;

        [MenuItem("File/ Utilities/ Developer Tools", priority = 9999)]
        public static void DeveloperTools()
        {
            DeveloperOptions window = GetWindow<DeveloperOptions>("Developer Tools");
            window.Show();
            //Debug.Log("Developer Options");
        }

        private void CreateGUI()
        {
            Debug.Log("Create GUI from Developer options");
            var root = rootVisualElement;

            VisualElement Spacer(float height)
            {
                var spacer = new VisualElement();
                spacer.style.height = height;
                return spacer;
            }

            Button openCleanUp = new Button(OpenCleanUpTool)
            {
                name = "Clean-Up Tool",
                text = "Clean-Up Tool",
                style = { fontSize = 16f },
            };

            openCleanUp.SetEnabled(false);

            _enteredPassword = new TextField("Enter Password :");
            _enteredPassword.isPasswordField = true;

            _enteredPassword.RegisterValueChangedCallback(evt =>
            {
                bool isPassCorrect = evt.newValue == _correctPassword;
                openCleanUp.SetEnabled(isPassCorrect);
            });

            Toggle showPassword = new Toggle("Show Password");

            showPassword.RegisterValueChangedCallback(evt =>
            {
                _enteredPassword.isPasswordField = !evt.newValue;
            });

            root.Add(Spacer(10f));
            root.Add(_enteredPassword);
            root.Add(showPassword);
            root.Add(openCleanUp);
        }



        private void OpenCleanUpTool()
        {
            GetWindow<DynamicCleanUpTool>("Project Clean-up", true, typeof(DeveloperOptions));
            GetWindow<DeveloperOptions>("Developer Tools", true, typeof(DeveloperOptions)).Close();

        }
    }

}
