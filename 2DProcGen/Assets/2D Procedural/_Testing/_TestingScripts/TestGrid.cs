using System.Collections;
using System.Collections.Generic;
using jkGenerator;
using UnityEngine;
#if UNITY_EDITOR
[ExecuteInEditMode]
[System.Serializable]
public class TestGrid : MonoBehaviour
{
    [SerializeField] private float gridSize;
    [SerializeField] private int subdivisions;
    [SerializeField] private JKGrid jkg;
    
    public void CreateGrid()
    {
        var go = gameObject;
        jkg = new JKGrid(gridSize, subdivisions, go.transform.position, go);
    }

    public void DestroyGrid()
    {
        jkg = null;
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        if (jkg == null) return;
        foreach (var element in jkg.GetIndexes())
            Gizmos.DrawSphere(element.Item2, .25f);
        //Horizontal
        Gizmos.DrawLine(new Vector3(jkg.GetWorldPos().x - .5f*jkg.GetSize(), jkg.GetWorldPos().y + .5f*jkg.GetSize()), 
                        new Vector3(jkg.GetWorldPos().x + .5f*jkg.GetSize(), jkg.GetWorldPos().y + .5f*jkg.GetSize()));
        Gizmos.DrawLine(new Vector3(jkg.GetWorldPos().x - .5f*jkg.GetSize(), jkg.GetWorldPos().y - .5f*jkg.GetSize()), 
                        new Vector3(jkg.GetWorldPos().x + .5f*jkg.GetSize(), jkg.GetWorldPos().y - .5f*jkg.GetSize()));
        //Vertical
        Gizmos.DrawLine(new Vector3(jkg.GetWorldPos().x - .5f*jkg.GetSize(), jkg.GetWorldPos().y + .5f*jkg.GetSize()), 
                        new Vector3(jkg.GetWorldPos().x - .5f*jkg.GetSize(), jkg.GetWorldPos().y - .5f*jkg.GetSize()));
        Gizmos.DrawLine(new Vector3(jkg.GetWorldPos().x + .5f*jkg.GetSize(), jkg.GetWorldPos().y + .5f*jkg.GetSize()), 
                        new Vector3(jkg.GetWorldPos().x + .5f*jkg.GetSize(), jkg.GetWorldPos().y - .5f*jkg.GetSize()));
    }
}
#endif