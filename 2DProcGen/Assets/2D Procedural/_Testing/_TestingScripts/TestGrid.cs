using System.Collections;
using System.Collections.Generic;
using jkGenerator;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
[ExecuteInEditMode]
[System.Serializable]
public class TestGrid : MonoBehaviour
{
    [SerializeField] private float gridSize;
    [SerializeField] private int subdivisions;
    [SerializeField] private Image imageTester;
    [SerializeField] private Sprite spriteTester;
    [SerializeField] private JKGrid jkg;
    
    /// <summary>
    /// Create subdivided grid using world coordinates
    /// </summary>
    public void GridDefault()
    {
        var go = gameObject;
        jkg = new JKGrid(gridSize, subdivisions, go.transform.position, go);
    }

    /// <summary>
    /// Create subdivided grid using editor-assigned image
    /// </summary>
    public void GridByImage()
    {
        var go = gameObject;
        jkg = new JKGrid(imageTester, subdivisions, go.transform.position, go);
    }

    /// <summary>
    /// Create subdivided grid using editor-assigned sprite
    /// </summary>
    public void GridBySprite()
    {
        var go = gameObject;
        jkg = new JKGrid(spriteTester, subdivisions, go.transform.position, go);
    }

    public void DestroyGrid()
    {
        jkg = null;
    }

    public void SetSize()
    {
        gridSize = jkg.GetSize();
    }
    
    /// <summary>
    /// Display grid layout in scene view
    /// </summary>
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