using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using Microsoft.MixedReality.Toolkit.UX;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(TapToPlace))]
public class TapToPlaceManager : MonoBehaviour
{
    #region SerializeField

    [SerializeField]
    Material visible;

    [SerializeField]
    Material notVisible;

    [SerializeField]
    PressableButton startPlacement;


    [SerializeField]
    private List<Vector3> offsets;
    [SerializeField]
    private List<Vector3> offsetAngles;
    [SerializeField]
    public List<GameObject> designElements;


    public int siblingIndex = 0;


    private bool isPlacing = true;
    private bool isPlaced = false;
    private bool isAnchored = false;
    private Vector3 anchorPosition;
    private Vector3 anchorRotation;

    public static List<MeshCollider> colliders = new();

    private AssemblyManager _assemblyManager;
    public Transform ParentObject;

    #endregion SerializeField

    public static TapToPlaceManager Instance { get; private set; }

    private void ToggleSpatialMesh(bool isSpatialMeshVisible)
    {
        ARMeshManager meshVisualizer = FindObjectOfType<ARMeshManager>();

        if (isSpatialMeshVisible)
        {
            foreach (var mesh in meshVisualizer.meshes)
            {
                mesh.GetComponent<MeshRenderer>().material = visible;
            }
        }
        else
        {
            foreach (var mesh in meshVisualizer.meshes)
            {
                mesh.GetComponent<MeshRenderer>().material = notVisible;
            }
        }

    }

    private void Awake()
    {
        _assemblyManager = FindObjectOfType<AssemblyManager>();
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }
    private void ToggleMarkers(bool isMarkerActive)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(isMarkerActive);
        }
    }
    private void ToggleTapToPlace(bool isPlacing)
    {

        var tapToPlace = GetComponent<TapToPlace>();
        if (isPlacing)
        {
            tapToPlace.enabled = true;
            tapToPlace.StartPlacement();
            SuppressGameObjects();
        }
        else
        {
            tapToPlace.StopPlacement();
            tapToPlace.enabled = false;
        }

    }

    [ContextMenu("Get Offset")]
    public void GetTransformOffset() //  i removed a reference from build window. search kar lena
    {
        var currentScene = gameObject.scene;
        designElements = currentScene.GetRootGameObjects().ToList();

        offsets.Clear();
        offsetAngles.Clear();
        if (offsets == null || offsets.Count <= 0)
        {
            for (int i = siblingIndex + 2; i < designElements.Count; i++)
            {
                var offset = designElements[i].transform.position - transform.position;
                var angle = Vector3.SignedAngle(designElements[i].transform.position - transform.position, designElements[i].transform.position, Vector3.right);
                offsets.Add(offset);
                offsetAngles.Add(angle * Vector3.up);
            }
        }

        siblingIndex = this.transform.GetSiblingIndex();
    }
    [ContextMenu("Set Offset")]
    public void SetTransformOffset
    (
        //bool shouldSet = true
    )
    {
        var currentScene = gameObject.scene;


        if (isAnchored)// && shouldSet)
        {
            transform.localPosition = anchorPosition;
            transform.eulerAngles = anchorRotation;
        }

        for (int i = 0; i < offsets.Count; i++)
        {
            var element = designElements[siblingIndex + 2 + i];
            element.transform.localPosition = transform.localPosition + offsets[i];
            element.transform.eulerAngles = new Vector3(element.transform.eulerAngles.x, element.transform.eulerAngles.y - anchorRotation.y, element.transform.eulerAngles.z);
            element.transform.RotateAround(transform.position, Vector3.up, offsetAngles[i].magnitude + transform.eulerAngles.y);
            element.SetActive(true);

        }

        anchorPosition = transform.localPosition;
        anchorRotation = transform.eulerAngles;

        var manipulator = FindObjectOfType<AdjustPosition>(true);
        if (manipulator != null)
        {
            manipulator.ResetManipulator(transform);
        }

        isPlacing = false;
        isPlaced = true;

    }

    private void SuppressGameObjects()
    {
        var currentScene = gameObject.scene;

        for (int i = 0; i < offsets.Count; i++)
        {
            var element = designElements[siblingIndex + 2 + i];

            element.SetActive(false);
        }
    }
    private void OnPlacingStart()
    {
        SetHandTracker();
        ToggleSpatialMesh(true);
        ToggleMarkers(true);
    }
    private void OnPlacingStoped()
    {
        
        ToggleSpatialMesh(false);
        ToggleMarkers(false);
        startPlacement.ForceSetToggled(false);
        SetTransformOffset
        (
            //false
        );
        var objects = GetObjects();
        //foreach (var obj in objects)
        //{
        //    AddCollidersDFS(obj);
        //}
        //_assemblyManager.FirstPhase();
        // ParentObject.rotation = Quaternion.Euler(ParentObject.rotation.x, 90, ParentObject.rotation.z);
        //ParentObject.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, transform.rotation.z);
        //ParentObject.rotation = Quaternion.Euler(0, 90, 0);
    }

    private void AddCollidersDFS(Transform parent)
    {
        foreach (Transform child in parent)
        {
            if(child.GetComponent<MeshCollider>() != null && !colliders.Contains(child.GetComponent<MeshCollider>()))
                colliders.Add(child.GetComponent<MeshCollider>());
            AddCollidersDFS(child);
        }
    }
    public int GetElementCount()
    {
        return designElements.Count;
    }
    private bool IsPlacing()
    {
        if (isPlacing && !isPlaced)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public Vector3 GetObjectPosition(int index)
    {
        if (index < 0) return Vector3.zero;
        return offsets[index];
    }
    public Vector3 GetObjectAngle(int index)
    {
        return offsetAngles[index];
    }

    public List<Transform> GetObjects()
    {
        var elements = new List<Transform>();
        for (int i = 0; i < offsets.Count; i++)
        {
            elements.Add(designElements[siblingIndex + 2 + i].transform);
        }
        return elements;
    }
    public Vector3 CalculatePositionOffset(Transform part)
    {
        return part.transform.position - transform.position;
    }
    public Vector3 CalculateAngleOffset(Transform part)
    {
        var angle = Vector3.SignedAngle(part.position - transform.position, part.transform.position, Vector3.right);
        return angle * Vector3.up;
    }
    private void SetHandTracker()
    {
        var solver = GetComponent<SolverHandler>();
        solver.TrackedHandedness = Microsoft.MixedReality.Toolkit.Handedness.Right;
        solver.TrackedTargetType = TrackedObjectType.ControllerRay;
    }
    private void InitTapToPlace()
    {
        SetHandTracker();

        var tap = GetComponent<TapToPlace>();

        tap.OnPlacingStarted.AddListener(delegate { OnPlacingStart(); });
        tap.OnPlacingStopped.AddListener(delegate { OnPlacingStoped(); });

        startPlacement.OnClicked.AddListener(
            delegate {

                if (IsPlacing())
                {
                    isAnchored = true;

                    ToggleTapToPlace(false);
                }
                else
                {
                    isAnchored = false;
                    isPlacing = true;
                    isPlaced = false;

                    ToggleTapToPlace(true);
                }
            });

        SuppressGameObjects();
    }

    #region Unity Method
    private void Start()
    {
        InitTapToPlace();
    }
    #endregion Unity Method

}

