using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuButton : MonoBehaviour
{
    public string MenuName;

    float X, Y;

    //bool Selected=false;

    // Start is called before the first frame update
    void Start()
    {
        X = Y = 0f;
        //Selected = false;
    }

    public void SetMenuName(string str,Material mat, float x, float y)
    {
        MenuName = str;
        X = x;
        Y = y;
        GetComponent<Transform>().position = new Vector3(X, Y, -1.5f);
        GetComponent<MeshRenderer>().material = mat;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public string MouseCursorInMenuButton(float x, float y)
    {
        Transform TF = GetComponent<Transform>();
        Vector3 Position = TF.position;
        Vector3 Scale = TF.localScale;
        if (Position.x - 0.5f*Scale.x < x && x < Position.x + 0.5f * Scale.x)
        {
            if (Position.y - 0.5f * Scale.y < y && y < Position.y + 0.5f * Scale.y)
            {
                return MenuName;
            }
        }
        return "";
    }
}
