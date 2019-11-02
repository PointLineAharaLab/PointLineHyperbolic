using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickSpark : MonoBehaviour
{
    public GameObject parent = null;
    public Material material = null;

    public float Pitch = 0f;

    float t;
    Vector3 tt;
    //Start is called before the first frame update
    void Start()
    {
        t = 1f;
        tt = new Vector3(0.01f, 0.01f, 0.01f);
        material.color = new Color(1f, 1f, 0f, 1f);
        AudioSource[] AS = GetComponents<AudioSource>();
        Pitch = Mathf.Floor(Random.value * 13f);
        float rndInt2 = Mathf.Floor(Random.value * 2f) + 3f;

        AS[0].pitch = Mathf.Pow(0.5f,  Pitch / 12f);
        AS[1].pitch = Mathf.Pow(0.5f, (Pitch- rndInt2) / 12f);
    }

    // Update is called once per frame
    void Update()
    {
        tt.x = tt.y = tt.z = 0.25f * (6f - 5f * t);
        t -= (Time.deltaTime * 1f);
        parent.transform.localScale = tt;
        if (t < 0f)
        {
            t = 0f;
            Destroy(parent,1f);
        }
        material.color = new Color(1f, 1f, 1f-t, t);
        parent.GetComponent<MeshRenderer>().material = material;
    }
}
