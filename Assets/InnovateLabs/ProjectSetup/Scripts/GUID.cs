using System;
using UnityEngine;
using InnovateLabs.Utilities;

namespace InnovateLabs.Projects
{
    public class GUID : MonoBehaviour
    {
        [ReadValueAtInspector]
        [SerializeField]
        private string ID = Guid.NewGuid().ToString().ToUpper();


        public string GetGUID()
        {
            return ID;
        }


    }

}