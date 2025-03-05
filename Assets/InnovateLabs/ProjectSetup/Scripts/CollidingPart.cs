using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class CollidingPart
{
    public GameObject part;
    public List<Material> defaultMaterials = new List<Material>();
    public bool isChangeMaterial = true;
    public int collisionCount = 0;

    private MeshRenderer meshRenderer;
    public CollidingPart(GameObject part)
    {
        //meshRenderer = part.GetComponent<MeshRenderer>();
        if(part.GetComponent<MeshRenderer>() == null)
        {
            this.part = part.transform.parent.gameObject;
            meshRenderer = part.transform.parent.GetComponent<MeshRenderer>();  
        }
        else
        {
            this.part = part;
            meshRenderer = part.GetComponent<MeshRenderer>();
        }
        defaultMaterials = meshRenderer.materials.ToList();
    }

    public void SetMaterial(Material mat)
    {
        if (!isChangeMaterial) return;
        Debug.Log($"changing material execution : {part.name}");
        var mats = new List<Material>();
        for (int i = 0; i < meshRenderer.materials.Length; i++)
        {
            mats.Add(mat);
        }
        meshRenderer.materials = mats.ToArray();
        isChangeMaterial = false;
    }

    public void SetDefaultMaterial()
    {
        if (defaultMaterials != null || defaultMaterials.Count != 0)
        {
            meshRenderer.materials = defaultMaterials.ToArray();
        }
        isChangeMaterial = true;
    }
}