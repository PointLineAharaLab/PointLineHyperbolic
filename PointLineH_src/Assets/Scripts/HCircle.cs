using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HCircle : MonoBehaviour
{
    LineRenderer LR;
    public HypCircle HCR = null;
    public GameObject parent = null;
    public GameObject VA = null, VB = null;
    public string HCircleName;
    public int ID;

    public GameObject HCircleLog;// ログの参照
    //描画関係
    readonly int PosLength = 61;//折れ線の長さ
    Vector3[] Pos;//（折れ線としての）座標
    private AnimationCurve anim;// LineRendererのWidth設定のため
    private Keyframe[] ks;// LineRendererのWidth設定のため

    public bool Active = true;

    // Start is called before the first frame update
    void Start()
    {
        LR = GetComponent<LineRenderer>();
        LR.positionCount = PosLength;

        if(HCR==null) HCR = new HypCircle();
        Pos = new Vector3[PosLength];
        ks = new Keyframe[PosLength];
        anim = new AnimationCurve(ks);
        for (int i = 0; i < PosLength; i++)
        {
            Pos[i] = new Vector3(0f, 0f, -1f);//線の位置を決めるためのベクトルの列
            ks[i] = new Keyframe(1f * i / (PosLength - 1), .05f);//線の太さのためのキーフレーム
        }
        LineRendererSetPosition();
    }

    // Update is called once per frame
    void Update()
    {
        if (HCR != null && VA != null)
        {
            HCR.HX = VA.GetComponent<Vertex>().XY.x;
            HCR.HY = VA.GetComponent<Vertex>().XY.y;
            HCR.GetECircleFromHCircle();
            RenderHCircle();
        }
    }

    public void SetVertex(GameObject obj)
    {
        VA = obj;
        if (HCR == null) HCR = new HypCircle();
        HCR.HX = VA.GetComponent<Vertex>().XY.x;
        HCR.HY = VA.GetComponent<Vertex>().XY.y;
        HCR.GetECircleFromHCircle();

    }

    public void SetRadius(float r)
    {
        if (HCR == null) HCR = new HypCircle();
        HCR.HR = r;
    }

    public HypCircle GetHCR()
    {
        if (HCR == null) HCR = new HypCircle();
        HCR.HX = VA.GetComponent<Vertex>().XY.x;
        HCR.HY = VA.GetComponent<Vertex>().XY.y;
        HCR.GetECircleFromHCircle();
        return HCR;
    }

    void LineRendererSetPosition()
    {
        for (int i = 0; i < PosLength; i++)
        {
            LR.SetPosition(i, Pos[i]);
        }
    }

    void RenderHCircle()
    {
        for (int i = 0; i < PosLength; i++)
        {
            float x = HCR.EX + HCR.ER * Mathf.Cos(i * 2 * Mathf.PI / (PosLength - 1));
            float y = HCR.EY + HCR.ER * Mathf.Sin(i * 2 * Mathf.PI / (PosLength - 1));
            float dr = 0.01f * (1f - x * x - y * y);

            Vector3 pos = new Vector3(x*World.Scale, y*World.Scale, -1f );
            LR.SetPosition(i, pos);
            ks[i].value = dr * World.StrokeWeight * World.Scale;
        }
        anim.keys = ks;
        LR.widthCurve = anim;

    }
}

