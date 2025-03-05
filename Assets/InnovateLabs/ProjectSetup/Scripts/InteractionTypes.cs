using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InnovateLabs
{
    public enum InteractionTypes
    {
        [InspectorName("Touch")]
        Touch,
        [InspectorName("Translate")]
        Translate,
        [InspectorName("Rotate")]
        Rotate,
        [InspectorName("Part Collision")]
        PartCollision
    }
}
