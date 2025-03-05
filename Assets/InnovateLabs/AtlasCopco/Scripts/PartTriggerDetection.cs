using System.Collections.Generic;
using UnityEngine;

public class PartTriggerDetection : MonoBehaviour
{
    public Material collisionMaterial;
    public GameObject detectionFeedback;
    //public PressableButton toggleCollision;
    //public static List<CollidingPart> collidingParts = new List<CollidingPart>();
    public List<CollidingPart> collisionHistory = new List<CollidingPart>();
    //public bool isTriggerAvailable = false;

    public AssemblyManager AssemblyManager;
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Part"))
        {
            //AssemblyManager.InterferenceWarning.SetActive(true);
            TriggerEnter(other);
        }
    }
    public void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Part"))
        {
            //AssemblyManager.InterferenceWarning.SetActive(true);
            TriggerStay(other);
        }
    }
    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Part"))
        {
            //AssemblyManager.InterferenceWarning.SetActive(false);
            TriggerExit(other);
        }
    }

    public void TriggerEnter(Collider other)
    {
        if (!AssemblyManager.IsTriggerAvailable) return;

        if (HandCollisionChecker.collidingParts.Exists(x => x.part == other.gameObject)) return;

        if (HandCollisionChecker.collidingParts.Count != 0) detectionFeedback.SetActive(true);

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
        HandCollisionChecker.collidingParts.Add(collision);

    }
    public void TriggerStay(Collider other)
    {

        var collision = HandCollisionChecker.collidingParts.Find(x => x.part == other.gameObject);

        if (collision == null) return;
        if (!AssemblyManager.IsTriggerAvailable)
        {
            collision.SetDefaultMaterial();
            HandCollisionChecker.collidingParts.Remove(collision);
            detectionFeedback.SetActive(false);
            return;
        }
        //AssemblyManager.WarningText.text = "Interference Detected";
        detectionFeedback.SetActive(true);

        collision.SetMaterial(collisionMaterial);

    }
    public void TriggerExit(Collider other)
    {
        if (!AssemblyManager.IsTriggerAvailable) return;

        var collision = HandCollisionChecker.collidingParts.Find(x => x.part == other.gameObject);

        if (collision == null) return;

        collision.SetDefaultMaterial();
        HandCollisionChecker.collidingParts.Remove(collision);

        if (HandCollisionChecker.collidingParts.Count == 0) detectionFeedback.SetActive(false);
    }

    //public void ToggleHandInteractionBehaviour()
    //{
    //    isTriggerAvailable = toggleCollision.IsToggled.Active;
    //}

    //public void Init()
    //{
    //    toggleCollision.OnClicked.AddListener(ToggleHandInteractionBehaviour);
    //}

    //private void Start()
    //{
    //    Init();
    //}
}
