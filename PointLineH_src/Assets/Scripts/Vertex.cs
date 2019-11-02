using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vertex : MonoBehaviour
{
    public Vector2 XY;
    public string VertexName;
    public int ID;

    Vector3 Position;
    Vector3 Scale;

    public GameObject VertexLog;

    public float StartDragX, StartDragY;

    public bool Selected = false;
    public bool Active = true;
    public bool Fixed = false;
    public Material SelectedVertexMaterial;
    public Material UnselectedVertexMaterial;
    public Material SelectedFixedVertexMaterial;
    public Material UnselectedFixedVertexMaterial;

    void Start()
    {
        Position = new Vector3(0f, 0f, 0f);
        Scale = new Vector3(0.3f, 0.3f, 0.01f);
        //Selected = false;
        
    }

    // Update is called once per frame
    void Update()
    {
        Transform TF = GetComponent<Transform>();
        Position.x = XY.x * World.Scale;//
        Position.y = XY.y * World.Scale;//
        Position.z = -2f;
        if (XY.x * XY.x + XY.y * XY.y < 1f)
        {
            TF.position = Position;
            float metric = 0.4f * (1f - (XY.x * XY.x + XY.y * XY.y));
            if (metric < 0)
            {
                metric = 0f;
            }
            else if (metric < 0.04)
            {
                metric = 0.04f;
            }
            Scale.x = Scale.y = metric;
            TF.localScale = Scale;
        }
        if (Fixed)
        {
            if (Selected)
            {
                GetComponent<MeshRenderer>().material = SelectedFixedVertexMaterial;
            }
            else
            {
                GetComponent<MeshRenderer>().material = UnselectedFixedVertexMaterial;
            }
        }
        else
        {
            if (Selected)
            {
                GetComponent<MeshRenderer>().material = SelectedVertexMaterial;
            }
            else
            {
                GetComponent<MeshRenderer>().material = UnselectedVertexMaterial;
            }
        }

    }

}
