using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogFolder : MonoBehaviour
{
    public int Cursor;
    public int CountAll = 0;
    // Start is called before the first frame update
    void Start()
    {
        Cursor = 5;
    }

    // Update is called once per frame
    void Update()
    {
        int count = 0;
        Vector3 pos = new Vector3(6.6f, 0f, -1f);
        GameObject[] logs = FindObjectsOfType<GameObject>();
        for(int i=logs.Length-1; i>=0; i--)
        {
            if (logs[i].name.Contains("GameLog"))
            {
                pos.y = (float)(Cursor - count);
                logs[i].transform.position = pos;
                count++;
            }
        }
        CountAll = count;
    }
}
