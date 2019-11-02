using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HLine : MonoBehaviour
{
    LineRenderer LR;
    public HypLine HL;
    public GameObject parent=null;
    public GameObject VA=null, VB=null;
    public string HLineName;
    public int ID;

    public bool Clipped;

    public GameObject HLineLog;

    Vector3[] Pos;
    readonly int PosLength = 21;
    float WorldScale;

    private AnimationCurve anim;
    private Keyframe[] ks;

    public bool Active = true;

    // Start is called before the first frame update
    void Start()
    {
        LR = GetComponent<LineRenderer>();
        LR.positionCount = PosLength;
        HL = new HypLine();
        Pos = new Vector3[PosLength];
        ks = new Keyframe[PosLength];
        anim = new AnimationCurve(ks);
        for (int i = 0; i < PosLength; i++)
        {
            Pos[i] = new Vector3(0f, 0f, -1f);//線の位置を決めるためのベクトルの列
            ks[i] = new Keyframe(1f * i / (PosLength - 1), .05f);//線の太さのためのキーフレーム
        }
        LineRendererSetPosition();
        Clipped = false;
    }

    // Update is called once per frame
    void Update()
    {
        WorldScale = World.Scale;
        if (VA != null && VB != null)
        {
            GetHLineFromTwoVertices();
        }
        else// 何もなければ円弧を描く
        {
            HL.SetRadius();
            if (Clipped)
            {
                DrawClippedLine();
            }
            else
            {
                DrawAllLine();
            }
            LineRendererSetPosition();
        }

    }

    public void SetVertex(GameObject va, GameObject vb)
    {
        VA = va;
        VB = vb;
    }

    public HypLine GetHL()
    {
        if (HL == null) HL = new HypLine();
        if (VA != null && VB != null)
        {
            GetHLineFromTwoVertices();
        }
        return HL;
    }

    private void GetInversionImage(float x, float y, out float  u, out float v)
    {
        float d = x * x + y * y;
        if (d == 0f)
        {
            //Debug.Log("d=0 in GetInversionImage()");
            d = 0.00001f;
            u = 10000f;
            v = 0;
            return;
        }
        u = x / d;
        v = y / d;
    }

    void GetEBisectorLine(float x1, float y1, float x2, float y2, out float a, out float b, out float c)
    {
        a = x1 - x2;
        b = y1 - y2;
        c = 0.5f * (x1 * x1 + y1 * y1 - x2 * x2 - y2 * y2 );
    }

    void GetCrossingPointOfTwoLines(float a, float b, float c, float d, float p, float q, out float x, out float y)
    {
        float den = a * d - b * c;
        if (den == 0f)
        {
            //Debug.Log("den=0");
            den = 0.00001f;
            x = b*10000f;
            y = -a*10000f;
            return;
        }
        x = (p * d - b * q) / den;
        y = (a * q - p * c) / den;
    }

    public void GetHLineFromTwoVertices()
    {
        Vertex PVA = VA.GetComponent<Vertex>();
        HypPoint P1 = new HypPoint(PVA.XY);
        Vertex PVB = VB.GetComponent<Vertex>();
        HypPoint P2 = new HypPoint(PVB.XY);

        HL = HTransform.GetHLineThruTwoPoints(P1, P2);
        if (Clipped)
        {
            DrawClippedLine();
        }
        else
        {
            DrawAllLine();
        }
        LineRendererSetPosition();
        //print(HTransform.GetHDistanceOfTwoPoints(x1,y1,x2,y2));
    }

    void LineRendererSetPosition()
    {
        for (int i = 0; i < PosLength; i++)
        {
            LR.SetPosition(i, Pos[i]);
        }
    }

    /// <summary>
    /// 無限遠まで直線を引く。
    /// </summary>
    void DrawAllLine()
    {
        if (HL.R > 300f || HL.Z == 0f)//まっすぐすぎるので直径を描く
        {
            //Debug.Log("R>300");
            float xx = -HL.Y;
            float yy = HL.X;
            float mag = Mathf.Sqrt(xx * xx + yy * yy);
            xx *= (WorldScale / mag);
            yy *= (WorldScale / mag);
            for (int i = 0; i < PosLength; i++)
            {
                float xxx = -xx + (2 * xx) * i / (PosLength - 1);
                float yyy = -yy + (2 * yy) * i / (PosLength - 1);
                float dr = 0.01f * (WorldScale* WorldScale - xxx * xxx - yyy * yyy)/ WorldScale/ WorldScale;
                Pos[i].x = xxx;
                Pos[i].y = yyy;
                Pos[i].z = -1f;
                ks[i].value = dr * World.StrokeWeight * World.Scale;
            }
            anim.keys = ks;
            LR.widthCurve = anim;
        }
        else// 普通に円弧を描く
        {
            float hlx = HL.X / HL.Z;
            float hly = HL.Y / HL.Z;
            float midT = Mathf.Atan2(hly, hlx);
            float widT = Mathf.Asin(1f / Mathf.Sqrt(hlx * hlx + hly * hly));
            for (int i = 0; i < PosLength; i++)
            {
                float t = midT - widT + (2 * widT) * i / (PosLength - 1);
                float xx = hlx - HL.R * Mathf.Cos(t);
                float yy = hly - HL.R * Mathf.Sin(t);
                float dr = 0.01f* (1f - xx * xx - yy * yy);
                Pos[i].x = WorldScale * xx;
                Pos[i].y = WorldScale * yy;
                Pos[i].z = -1f;
                ks[i].value =  dr * World.StrokeWeight * World.Scale;
            }
            anim.keys = ks;
            LR.widthCurve=anim;
        }

    }

    void DrawClippedLine()
    {
        if (HL.R > 300f || HL.Z == 0f)//まっすぐすぎるので直径を描く
        {
            Vertex P1 = VA.GetComponent<Vertex>();
            Vertex P2 = VB.GetComponent<Vertex>();
            float x1 = P1.XY.x*World.Scale;
            float y1 = P1.XY.y * World.Scale;
            float x2 = (P2.XY.x - x1) ;
            float y2 = (P2.XY.y - y1) ;
            for (int i = 0; i < PosLength; i++)
            {
                float xxx = x1 + x2 * i / (PosLength - 1);
                float yyy = y1 + y2 * i / (PosLength - 1);
                float dr = 0.01f * (1f - xxx * xxx - yyy * yyy) ;
                Pos[i].x = xxx * World.Scale;
                Pos[i].y = yyy * World.Scale;
                Pos[i].z = -1f;
                ks[i].value = dr * World.StrokeWeight * World.Scale;
            }
            anim.keys = ks;
            LR.widthCurve = anim;
        }
        else// 普通に円弧を描く
        {
            Vertex P1 = VA.GetComponent<Vertex>();
            Vertex P2 = VB.GetComponent<Vertex>();
            float hlx = HL.X / HL.Z;
            float hly = HL.Y / HL.Z;
            float T1 = Mathf.Atan2(P1.XY.y - hly, P1.XY.x - hlx);
            float T2 = Mathf.Atan2(P2.XY.y - hly, P2.XY.x - hlx);
            float StartT=0f, WidT=0f;
            if (T1 <= T2 && T2 < T1 + Mathf.PI)
            {
                StartT = T1;
                WidT = T2 - T1;
            }
            else if (T1 + Mathf.PI <= T2  && T2 < T1 + 2f * Mathf.PI)
            {
                StartT = T2;
                WidT = T1 - T2 + 2 * Mathf.PI ;
            }
            else if (T1 - Mathf.PI <= T2 && T2 < T1)
            {
                StartT = T2;
                WidT = T1 - T2;
            }
            else if (T1 - 2f *  Mathf.PI <= T2 && T2 < T1 - Mathf.PI)
            {
                StartT = T1;
                WidT = T2 - T1 + 2 * Mathf.PI;
            }
            for (int i = 0; i < PosLength; i++)
            {
                float t = StartT + WidT * i / (PosLength - 1);
                float xx = hlx + HL.R * Mathf.Cos(t);
                float yy = hly + HL.R * Mathf.Sin(t);
                float dr = 0.01f * (1f - xx * xx - yy * yy);
                Pos[i].x = WorldScale * xx;
                Pos[i].y = WorldScale * yy;
                Pos[i].z = -1f;
                ks[i].value = dr * World.StrokeWeight * World.Scale;
            }
            anim.keys = ks;
            LR.widthCurve = anim;
        }

    }

}

//public class HyperbolicLine
//{
//    public float X, Y, R;

//    public HyperbolicLine()
//    {
//        X = 1.5f;
//        Y = 0f;
//        R = Mathf.Sqrt(X * X + Y * Y - 1);
//    }

//    public HyperbolicLine(float x, float y)
//    {
//        X = x;
//        Y = y;
//        R = Mathf.Sqrt(X * X + Y * Y - 1);
//    }
//    public void SetXY(float x, float y)
//    {
//        X = x;
//        Y = y;
//        R = Mathf.Sqrt(X * X + Y * Y - 1);
//    }
//    public void SetXY(HypLine L)
//    {
//        X = L.X;
//        Y = L.Y;
//        R = L.R;
//    }

//    public void SetRadius()
//    {
//        R = Mathf.Sqrt(X * X + Y * Y - 1);
//    }

//}
