using Microsoft.MixedReality.Toolkit.UX;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandCollisionChecker : MonoBehaviour
{

    public Material collisionMaterial;
    public GameObject detectionFeedback;
    public PressableButton toggleHandInteraction;
    public static List<CollidingPart> collidingParts = new List<CollidingPart>();
    public List<CollidingPart> collisionHistory = new List<CollidingPart>();
    //public AssemblyManager AssemblyManager;
    public bool isTriggerAvailable = false;
    public void TriggerEnter(Collider other)
    {
        if (!isTriggerAvailable) return;

        if (collidingParts.Exists(x => x.part == other.gameObject)) return;

        if (collidingParts.Count != 0) detectionFeedback.SetActive(true);

        CollidingPart collision = null;

        collision = collisionHistory.Find(x => x.part == other.gameObject);
        if (collision == null)
        {
            collision = new CollidingPart(other.gameObject);
            collisionHistory.Add(collision);

        }
        //AssemblyManager.WarningText.text = "Interference Detected";
        collision.SetMaterial(collisionMaterial);
        collision.collisionCount++;
        collidingParts.Add(collision);

    }
    public void TriggerStay(Collider other)
    {

        var collision = collidingParts.Find(x => x.part == other.gameObject);

        if (collision == null) return;
        if (!isTriggerAvailable)
        {
            collision.SetDefaultMaterial();
            collidingParts.Remove(collision);
            detectionFeedback.SetActive(false);
            return;
        }
        //AssemblyManager.WarningText.text = "Interference Detected";
        detectionFeedback.SetActive(true);

        collision.SetMaterial(collisionMaterial);

    }
    public void TriggerExit(Collider other)
    {
        if (!isTriggerAvailable) return;

        var collision = collidingParts.Find(x => x.part == other.gameObject);

        if (collision == null) return;

        collision.SetDefaultMaterial();
        collidingParts.Remove(collision);

        if (collidingParts.Count == 0) detectionFeedback.SetActive(false);
    }

    public void ToggleHandInteractionBehaviour()
    {
        isTriggerAvailable = toggleHandInteraction.IsToggled.Active;
    }

    public void Init()
    {
        toggleHandInteraction.OnClicked.AddListener(ToggleHandInteractionBehaviour);
    }

    private void Start()
    {
        Init();
    }
}
