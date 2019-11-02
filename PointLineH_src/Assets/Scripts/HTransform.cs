using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class HypPoint
{
    public float X, Y, Z;
    public HypPoint()
    {
        X = Y = 0f;
        Z = 1f;
    }
    public HypPoint(HypPoint P)
    {
        X = P.X;
        Y = P.Y;
        Z = P.Z;
    }
    public HypPoint(Vector2 v)
    {
        X = v.x;
        Y = v.y;
        Z = 1f;
    }
    public HypPoint(float _x, float _y, float _z = 1f)
    {
        X = _x;
        Y = _y;
        Z = _z;
    }

    public void Normalized()
    {
        float d = Mathf.Sqrt(X * X + Y * Y + Z * Z);
        if (d != 0)
        {
            X /= d;
            Y /= d;
            Z /= d;
        }
    }

    public static HypPoint Zero()
    {
        return new HypPoint(Vector2.zero);
    }

    /// <summary>
    /// ディスクモデルの空間内に入るかどうか
    /// </summary>
    public bool InWorld()
    {
        if (float.IsInfinity(X) || float.IsNaN(X)) return false;
        if (float.IsInfinity(Y) || float.IsNaN(Y)) return false;
        if (float.IsInfinity(Z) || float.IsNaN(Z)) return false;
        return (X * X + Y * Y < Z * Z * 0.9999f);
    }

    public static HypPoint operator /(HypPoint p, float d)
    {
        HypPoint ret = new HypPoint
        {
            X = p.X,
            Y = p.Y,
            Z = p.Z * d
        };
        return ret;
    }

    public static HypPoint operator - (HypPoint P)
    {
        return HTransform.GetInversionImage(P);
    }

    public static EucLine operator * (HypPoint P1, HypPoint P2)
    {
        return HTransform.GetLineByTwoPoints(P1, P2);
    }

    public static EucLine operator | (HypPoint P1, HypPoint P2)
    {
        return HTransform.GetBisectorLine(P1, P2);
    }

    public static bool operator == (HypPoint P1, HypPoint P2)
    {
        P1.Normalized();
        P2.Normalized();
        if (Mathf.Abs(P1.X * P2.Z - P2.X * P1.Z) < 0.00001f)
        {
            if (Mathf.Abs(P1.Y * P2.Z - P2.Y * P1.Z) < 0.00001f)
            {
                return true;
            }
        }
        return false;
    }

    public static bool operator !=(HypPoint P1, HypPoint P2)
    {
        P1.Normalized();
        P2.Normalized();
        if (Mathf.Abs(P1.X * P2.Z - P2.X * P1.Z) < 0.000001f)
        {
            if (Mathf.Abs(P1.Y * P2.Z - P2.Y * P1.Z) < 0.000001f)
            {
                return false;
            }
        }
        return true;
    }

    public void Println(string str)
    {
            Debug.Log(str + "=(" + GetX() + "," + GetY()+ ")[" + X + " : " + Y + " : " + Z+"]");
    }

    public float GetX()
    {
        return Z == 0f ? Mathf.Infinity : X / Z;
    }

    public float GetY()
    {
        return Z == 0f ? Mathf.Infinity : Y / Z;
    }

    public override bool Equals(object obj)
    {
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override string ToString()
    {
        return base.ToString();
    }
}

public class EucLine
{
    public float a, b, p;
    public EucLine()
    {
        a = b = 1f;
        p = 0f;
    }
    public EucLine(float _a, float _b, float _p)
    {
        a = _a;
        b = _b;
        p = _p;
    }

    public static HypPoint operator * (EucLine L1, EucLine L2)
    {
        return HTransform.GetCrossingPointOfTwoLines(L1, L2);
    }

}

public class HypLine
{
    // Z(x^2+y^2)-2Xx-2Yy=-Z
    // 半径：R=
    public float X, Y, R, Z;
    public HypLine()
    {
        X = Y = 0f;
        R = Z = 1f;
    }
    public HypLine(HypLine L)
    {
        X = L.X;
        Y = L.Y;
        Z = L.Z;
        R = L.R;
    }

    public void SetXY(float x, float y)
    {
        X = x;
        Y = y;
        R = Mathf.Sqrt(X * X + Y * Y - 1);
        Z = 1f;
    }

    public void SetRadius()
    {
        if(Z != 0f)
        {
            R = Mathf.Sqrt(X * X + Y * Y - Z * Z)/Z;

        }
        else
        {
            R = Mathf.Infinity;
        }
    }

    public float GetX()
    {
        return Z == 0f ? Mathf.Infinity : X / Z;
    }

    public float GetY()
    {
        return Z == 0f ? Mathf.Infinity : Y / Z;
    }

    public HypPoint GetECenter()
    {
        return new HypPoint(X, Y, Z);
    }

    public void Println(string str)
    {
        Debug.Log(str + "=(" + GetX() + "," + GetY()+";" + R + ")[" +  X + ":" + Y + ":" + Z + "]");
    }
}

public class HypCircle
{
    public float EX = 0f, EY = 0f, ER = 0.2f;
    public float HX = 0f, HY = 0f, HR = 0.2f;

    public HypCircle()
    {

    }

    public void GetHCircleFromECircle()
    {
        if (EX == 0f && EY == 0f)
        {
            HX = 0f;
            HY = 0f;
            HR = Mathf.Log((1 + ER) / (1 - ER),2.718281828459f);
            return;
        }
        float r = Mathf.Sqrt(EX * EX + EY * EY);
        float nx = EX / r;
        float ny = EY / r;
        float d1 = HTransform.E2HFromOrigin(r + ER);
        float d2 = HTransform.E2HFromOrigin(r - ER);

        float d = HTransform.H2EFromOrigin(0.5f * (d1 + d2));
        HX = d * nx;
        HY = d * ny;
        HR = 0.5f * (d1 - d2);
    }


    public void GetECircleFromHCircle()
    {
        if (HX == 0f && HY == 0f)
        {
            EX = 0f;
            EY = 0f;
            ER = HTransform.H2EFromOrigin(HR);
            return;
        }
        float r = Mathf.Sqrt(HX * HX + HY * HY);
        float nx = HX / r;
        float ny = HY / r;
        float d = HTransform.E2HFromOrigin(r);
        float r1 = HTransform.H2EFromOrigin(d + HR);
        float r2 = HTransform.H2EFromOrigin(d - HR);
        EX = 0.5f * (r1 + r2) * nx;
        EY = 0.5f * (r1 + r2) * ny;
        ER = 0.5f * (r1 - r2);
    }

    public Vector2 GetHCenter()
    {
        return new Vector2(HX, HY);
    }

    public HypPoint GetECenter()
    {
        return new HypPoint(EX, EY);
    }
}
class HTransform
{
    /// <summary>
    /// 特定の点を特定の点へ移す双曲変換
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="P"></param>
    /// <returns></returns>
    public static HypPoint ParallelTransform(Vector2 start, Vector2 end, HypPoint P)
    {
        if((start-end).magnitude < 0.01f)
        {
            return P;
        }
        HypPoint Pstart = new HypPoint(start);
        HypPoint Pend = new HypPoint(end);
        HypLine L1 = GetHLineThruTwoPoints(Pstart, Pend);
        HypLine L2 = GetHBisectorOfTwoPoints(Pstart, Pend);
        HypLine L3 = GetHPerpendicularThruAPoint(L1, Pend);
        HypPoint P1 = GetInversionAlongHLine(L2, P);
        HypPoint P2 = GetInversionAlongHLine(L3, P);
        //if (!P2.InWorld())
        //{
        //    L1.Println("L1");
        //    L2.Println("L2");
        //    L3.Println("L3");
        //    P1.Println("P1");
        //    P2.Println("P2");
        //    return P;
        //}
        return P2;
    }

    public static HypPoint ParallelTransform(HypPoint Pstart, HypPoint Pend, HypPoint P)
    {
        if(!Pstart.InWorld() || !Pend.InWorld() || !P.InWorld())
        {
            Debug.Log("error occurs at ParallelTransform: A");
            Debug.Log("Pstart:" + Pstart.GetX() + "," + Pstart.GetY() + "(" + Pstart.X + ":" + Pstart.Y + ":" + Pstart.Z + ")");
            Debug.Log("Pend:" + Pend.GetX() + "," + Pend.GetY() + "(" + Pend.X + ":" + Pend.Y + ":" + Pend.Z + ")");
            Debug.Log("error occurs at ParallelTransform ここまで");
            return P;
        }
        if (GetEDistanceOfTwoPoints(Pstart, Pend) < 0.001f)
        {
            //Debug.Log("error occurs at ParallelTransform: B");
            //Debug.Log("Pstart:" + Pstart.GetX() + "," + Pstart.GetY() + "(" + Pstart.X + ":" + Pstart.Y + ":" + Pstart.Z + ")");
            //Debug.Log("Pend:" + Pend.GetX() + "," + Pend.GetY() + "(" + Pend.X + ":" + Pend.Y + ":" + Pend.Z + ")");
            //Debug.Log("error occurs at ParallelTransform ここまで");
            return P;
        }
        HypLine L1 = GetHLineThruTwoPoints(Pstart, Pend);
        HypLine L2 = GetHBisectorOfTwoPoints(Pstart, Pend);
        //L2.Println("---L2");
        HypLine L3 = GetHPerpendicularThruAPoint(L1, Pend);
        //L3.Println("---L3");
        HypPoint P1 = GetInversionAlongHLine(L2, P);
        HypPoint P2 = GetInversionAlongHLine(L3, P1);
        if (P2.InWorld())
        {
            return P2;
        }
        else
        {
            Debug.Log("error occurs at ParallelTransform: C");
            Debug.Log("Pstart:" + Pstart.GetX() + "," + Pstart.GetY() + "(" + Pstart.X + ":" + Pstart.Y + ":" + Pstart.Z + ")");
            Debug.Log("Pend:" + Pend.GetX() + "," + Pend.GetY() + "(" + Pend.X + ":" + Pend.Y + ":" + Pend.Z + ")");
            Debug.Log("L1:" + L1.GetX() + "," + L1.GetY() + "," + L1.R + "(" + L1.X + ":" + L1.Y + ":" + L1.Z + ")");
            Debug.Log("L2:" + L2.GetX() + "," + L2.GetY() + "," + L2.R + "(" + L2.X + ":" + L2.Y + ":" + L2.Z + ")");
            Debug.Log("L3:" + L3.GetX() + "," + L3.GetY() + "," + L3.R + "(" + L3.X + ":" + L3.Y + ":" + L3.Z + ")");
            Debug.Log("P1:" + P1.GetX() + "," + P1.GetY() + "(" + P1.X + ":" + P1.Y + ":" + P1.Z + ")");
            Debug.Log("P2:" + P2.GetX() + "," + P2.GetY() + "(" + P2.X + ":" + P2.Y + ":" + P2.Z + ")");
            P1 = GetInversionAlongHLine(L2, P);
            P2 = GetInversionAlongHLine(L3, P1);
            Debug.Log("P2:" + P2.GetX() + "," + P2.GetY() + "(" + P2.X + ":" + P2.Y + ":" + P2.Z + ")");
            if (P2.InWorld())
            {
                return P2;
            }
            Debug.Log("error occurs at ParallelTransform ここまで");
            return P;
        }
    }

    //public static void ParallelTransform(Vector2 start, Vector2 end, float X0, float Y0, out float newX, out float newY)
    //old version
    //{
    //    GetHLineThruTwoPoints(start.x, start.y, end.x, end.y, out float HL1X, out float HL1Y, out float HL1R);
    //    GetHBisectorOfTwoPoints(start.x, start.y, end.x, end.y, out float HL2X, out float HL2Y, out float HL2R);
    //    GetHyperbolicPerpendicularLineThruAPoint(HL1X, HL1Y, HL1R, end.x, end.y, out float HL3X, out float HL3Y, out float HL3R);
    //    GetInversionAlongHLine(HL2X, HL2Y, HL2R, X0, Y0, out float X1, out float Y1);
    //    GetInversionAlongHLine(HL3X, HL3Y, HL3R, X1, Y1, out float X2, out float Y2);
    //    newX = X2;
    //    newY = Y2;
    //}

    /// <summary>
    /// 単位円の反転像
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public static HypPoint GetInversionImage(HypPoint p)//new
    {
        float d = p.GetX() * p.GetX() + p.GetY() * p.GetY();
        return p / d;
    }

    //old
    public static void GetInversionImage(float x, float y, out float u, out float v)//old
    //old
    {
        float d = x * x + y * y;
        if (d == 0f)
        {
            Debug.Log("d=0 in GetInversionImage()");
            d = 0.00001f;
        }
        u = x / d;
        v = y / d;
    }

    /// <summary>
    /// 2点のユークリッド垂直二等分線
    /// </summary>
    /// <param name="P1"></param>
    /// <param name="P2"></param>
    /// <returns></returns>
    public static EucLine GetBisectorLine(HypPoint P1, HypPoint P2)//new
    {
        EucLine ret = new EucLine();
        float z1 = P1.Z;
        float z2 = P2.Z;
        if (z1 * z2 != 0)
        {
            ret.a = P1.X * z1 * z2 * z2
                - P2.X * z1 * z1 * z2;
            ret.b = P1.Y * z1 * z2 * z2
                - P2.Y * z1 * z1 * z2;
            float v = (P1.X * P1.X * z2 * z2
                + P1.Y * P1.Y * z2 * z2
                - P2.X * P2.X * z1 * z1
                - P2.Y * P2.Y * z1 * z1);
            ret.p = 0.5f * v;
        }
        else if (z1 == 0f)
        {
            ret.a = P1.X;
            ret.b = P1.Y;
            ret.p = Mathf.Infinity;
        }
        else
        {
            ret.a = P2.X;
            ret.b = P2.Y;
            ret.p = Mathf.Infinity;
        }
        return ret;
    }

    //old
    public static void GetBisectorLine(float x1, float y1, float x2, float y2, out float a, out float b, out float c)
    //old
    {
        //float A1, B1, C1;
        a = x1 - x2;
        b = y1 - y2;
        c = 0.5f * (x1 * x1 + y1 * y1 - x2 * x2 - y2 * y2);
    }

    /// <summary>
    /// ２点を通るユークリッド直線
    /// </summary>
    /// <param name="P1"></param>
    /// <param name="P2"></param>
    /// <returns></returns>
    public static EucLine GetLineByTwoPoints(HypPoint P1, HypPoint P2)
    //new
    {
        // (x-x1)*(y2-y1)=(x2-x1)*(y-y1)
        // (y2-y1)*x - (x2-x1)*y = -x2*y1 + x1*y2
        EucLine ret = new EucLine();
        float z1 = P1.Z;
        float z2 = P2.Z;
        ret.a = P2.Y * z1 - P1.Y * z2;
        ret.b = P1.X * z2 - P2.X * z1;
        ret.p = -P2.X * P1.Y + P1.X * P2.Y;
        return ret;
    }

    ////old
    //public static void GetLineByTwoPoints(float x1, float y1, float x2, float y2, out float a, out float b, out float c)
    ////old
    //{
    //    a = y2 - y1;
    //    b = x1 - x2;
    //    c = -x2 * y1 + x1 * y2;
    //}

    /// <summary>
    /// ２つのユークリッド直線の交点
    /// </summary>
    /// <param name="L1"></param>
    /// <param name="L2"></param>
    /// <returns></returns>
    public static HypPoint GetCrossingPointOfTwoLines(EucLine L1, EucLine L2)
    {
        HypPoint ret = new HypPoint
        {
            X = L1.p * L2.b - L1.b * L2.p,// remark that .p is minus than that in usual case
            Y = L1.a * L2.p - L1.p * L2.a,//
            Z = L1.a * L2.b - L1.b * L2.a
        };
        return ret;
    }

    //old
    public static void GetCrossingPointOfTwoLines(float a, float b, float c, float d, float p, float q, out float x, out float y)
    //old
    {
        float den = a * d - b * c;
        if (den == 0f)
        {
            //Debug.Log("den=0");
            den = 0.00001f;
        }
        x = (p * d - b * q) / den;
        y = (a * q - p * c) / den;
    }

    /// <summary>
    /// 二つの双曲直線の交点を求める。
    /// </summary>
    /// <param name="L1"></param>
    /// <param name="L2"></param>
    /// <returns></returns>
    public static HypPoint GetCrossingPointOfTwoHLines(HypLine L1, HypLine L2)
    {
        bool debug = false;
        float X1 = L1.X, Y1 = L1.Y, Z1 = L1.Z;
        float X2 = L2.X, Y2 = L2.Y, Z2 = L2.Z;
        float WX = Y1 * Z2 - Y2 * Z1, WY = X1 * Z2 - X2 * Z1, WZ = X1 * Y2 - X2 * Y1;
        float s1 = - WZ - Mathf.Sqrt(WZ * WZ - WX * WX - WY * WY);
        float s2 = - WZ + Mathf.Sqrt(WZ * WZ - WX * WX - WY * WY);
        if (debug)
        {
            Debug.Log(X1 + "," + Y1 + "," + Z1 + "," + X2 + "," + Y2 + "," + Z2);
            Debug.Log(WX + "," + WY + "," + WZ);
            Debug.Log("s=" + s1 + "," + s2);
        }
        float s = 0;
        if (Mathf.Abs(s1) < Mathf.Abs(s2))
        {
            s = s2;
        }
        else
        {
            s = s1;
        }
        if (Z1 != 0f || Z2 != 0f)
        {
            return new HypPoint
            {
                X = WX,
                Y = -WY,
                Z = s
            };
        }
        else
        {
            return new HypPoint
            {
                X = 0,
                Y = 0,
                Z = - 2 * WZ
            };
        }
    }

    public static HypPoint GetCrossingPointOfTwoHLines2(HypLine L1, HypLine L2)
    {
        // Center (L1.X : L1.Y : L1.Z ) Radius L1.R
        // RadicalLine 
        float a = -L1.X * L2.Z + L1.Z * L2.X;
        float b = -L1.Y * L2.Z + L1.Z * L2.Y;
        //Debug.Log(a + "," + b);
        if (a == 0f && b == 0f)
        {
            return new HypPoint(0f, 0f);
        }
        float X, Y, Z;
        if (L1.Z != 0f)
        {
            X = L1.X;
            Y = L1.Y;
            Z = L1.Z;
        }
        else
        {
            X = L2.X;
            Y = L2.Y;
            Z = L2.Z;
        }
        float d = (b * X - a * Y) * (b * X - a * Y) - (a * a + b * b) * Z * Z;
        if (d <= 0f)
        {
            return new HypPoint(1f, 1f, 0f); ;
        }
        float x1 = b * (b * X - a * Y + Mathf.Sqrt(d)) / (a * a + b * b) / Z;
        float x2 = b * (b * X - a * Y - Mathf.Sqrt(d)) / (a * a + b * b) / Z;
        float y1 = -a * (b * X - a * Y + Mathf.Sqrt(d)) / (a * a + b * b) / Z;
        float y2 = -a * (b * X - a * Y - Mathf.Sqrt(d)) / (a * a + b * b) / Z;

        if (x1 * x1 + y1 * y1 < 1f)
        {
            return new HypPoint(x1, y1);
        }
        else
        {
            return new HypPoint(x2, y2);
        }
    }

    /// <summary>
    /// 中心点から双曲直線を得る
    /// </summary>
    /// <param name="P"></param>
    /// <returns></returns>
    public static HypLine GetHLineFromCenter(HypPoint P)
    {
        HypLine ret = new HypLine();
        if (P.Z != 0f)
        {
            //float radius2 = P.X * P.X / P.Z / P.Z + P.Y * P.Y / P.Z / P.Z - 1f;
            float radius2 = P.GetX()*P.GetX() + P.GetY()*P.GetY() - 1f;
            if (radius2 > 0f)
            {
                ret.X = P.GetX();
                ret.Y = P.GetY();
                ret.Z = 1f;
                ret.R = Mathf.Sqrt(radius2);
            }
            else
            {
                ret.X = P.X;
                ret.Y = P.Y;
                ret.Z = 0f;
                ret.R = Mathf.Infinity;
            }
        }
        else
        {
            ret.X = P.X;
            ret.Y = P.Y;
            ret.Z = 0f;
            ret.R = Mathf.Infinity;
        }
        return ret;
    }

    public static HypLine GetHLineThruTwoPoints(HypPoint P1, HypPoint P2)
    {
        //HypPoint p1 = GetInversionImage(P1);
        //HypPoint p2 = GetInversionImage(P2);
        //EucLine l1 = GetBisectorLine(p1, P1);
        //EucLine l2 = GetBisectorLine(p2, P2);
        //HypPoint p3 = GetCrossingPointOfTwoLines(l1, l2);
        //HypLine ret = GetHLineFromCenter(p3);
        float x1 = P1.X, y1 = P1.Y, z1 = P1.Z;
        float x2 = P2.X, y2 = P2.Y, z2 = P2.Z;
        float a1 = 2 * x1 * z1, b1 = 2 * y1 * z1, p1 = x1 * x1 + y1 * y1 + z1 * z1;
        float a2 = 2 * x2 * z2, b2 = 2 * y2 * z2, p2 = x2 * x2 + y2 * y2 + z2 * z2;

        HypPoint HP =  new HypPoint{
            X = p1 * b2 - p2 * b1,
            Y = a1 * p2 - a2 * p1,
            Z = a1 * b2 - a2 * b1
        };
        return GetHLineFromCenter(HP);
    }

    ////old
    //public static void GetHLineThruTwoPoints(float x1, float y1, float x2, float y2, out float X, out float Y, out float R)
    ////old
    //{
    //    GetInversionImage(x1, y1, out float u1, out float v1);
    //    GetBisectorLine(x1, y1, u1, v1, out float a, out float b, out float p);
    //    GetInversionImage(x2, y2, out float u2, out float v2);
    //    GetBisectorLine(x2, y2, u2, v2, out float c, out float d, out float q);
    //    GetCrossingPointOfTwoLines(a, b, c, d, p, q, out float X0, out float Y0);
    //    X = X0;
    //    Y = Y0;
    //    R = Mathf.Sqrt(X * X + Y * Y - 1);
    //}

    public static HypLine GetHBisectorOfTwoPoints2(HypPoint P1, HypPoint P2)
    {
        HypLine ret = new HypLine();
        if (P1 != P2)
        {
            if (P1.X == 0f && P1.Y == 0f)
            {
                float x2 = P2.X;
                float y2 = P2.Y;
                float z2 = P2.Z;
                HypPoint P = new HypPoint(x2 * z2, y2 * z2, x2 * x2 + y2 * y2);
                ret = GetHLineFromCenter(P);
            }
            else if (P2.X == 0f && P2.Y == 0f)
            {
                float x1 = P1.X;
                float y1 = P1.Y;
                float z1 = P1.Z;
                HypPoint P = new HypPoint(x1*z1, y1*z1, x1 * x1 + y1 * y1);
                ret = GetHLineFromCenter(P);
            }
            else
            {
                EucLine L = GetLineByTwoPoints(P1, P2);
                HypLine L1 = GetHLineThruTwoPoints(P1, P2);
                EucLine L2 = new EucLine(L1.X, L1.Y, 1f);
                HypPoint P = GetCrossingPointOfTwoLines(L, L2);
                ret = GetHLineFromCenter(P);
            }
        }
        return ret;
    }

    public static HypLine GetHBisectorOfTwoPoints(HypPoint P1, HypPoint P2)
    {
        HypLine ret = new HypLine
        {
            X = 1f,
            Y = 1f,
            Z = 0f,
            R = 0f
        };
        if (P1 != P2)
        {
            EucLine L = GetLineByTwoPoints(P1, P2);
            HypPoint P3 = new HypPoint (L.a, L.b, Mathf.Sqrt(L.a * L.a + L.b * L.b));
            EucLine L12 = GetBisectorLine(P1, P2);
            EucLine L23 = GetBisectorLine(P2, P3);
            HypPoint P123 = GetCrossingPointOfTwoLines(L12, L23);
            float C123R = GetEDistanceOfTwoPoints(P123, P1);
            HypCircle C1 = new HypCircle
            {
                EX = P123.GetX(),
                EY = P123.GetY(),
                ER = C123R
            };// P1,P2,P3を通る円。
            EucLine L2 = new EucLine
            {
                a = -2 * C1.EX,
                b = -2 * C1.EY,
                p = C1.ER * C1.ER - C1.EX * C1.EX - C1.EY * C1.EY - 1f
            };//単位円とC1の共通弦（接するならば共通接線）
            HypPoint P = GetCrossingPointOfTwoLines(L, L2);//L1とL2の交点（無限遠点もありうる）
            ret = GetHLineFromCenter(P);//Pを中心とする双曲直線
        }else
        {
            Debug.Log("GetHBisectorOfTwoPoints error: P1==P2");
            P1.Println("P1");
            P2.Println("P2");
        }
        return ret;
    }

    ////old
    //public static void GetHBisectorOfTwoPoints(float x1, float y1, float x2, float y2, out float X, out float Y, out float R)
    ////old
    //{
    //    if (x1 == 0f && y1 == 0f)
    //    {
    //        X = x2 / (x2 * x2 + y2 * y2);
    //        Y = y2 / (x2 * x2 + y2 * y2);
    //        R = Mathf.Sqrt(X * X + Y * Y - 1f);
    //    }
    //    else if (x2 == 0f && y2 == 0f)
    //    {
    //        X = x1 / (x1 * x1 + y1 * y1);
    //        Y = y1 / (x1 * x1 + y1 * y1);
    //        R = Mathf.Sqrt(X * X + Y * Y - 1f);
    //    }
    //    else
    //    {
    //        GetLineByTwoPoints(x1, y1, x2, y2, out float A1, out float B1, out float C1);
    //        GetHLineThruTwoPoints(x1, y1, x2, y2, out float X1, out float Y1, out float R1);
    //        float A2 = X1, B2 = Y1, C2 = 1;
    //        GetCrossingPointOfTwoLines(A1, B1, A2, B2, C1, C2, out X, out Y);
    //        R = Mathf.Sqrt(X * X + Y * Y - 1);
    //    }
    //}

    public static HypLine GetHPerpendicularThruAPoint(HypLine L, HypPoint P)
    {
        //L.Println("------L");
        HypPoint P2 = GetInversionImage(P);
        if (P2.Z != 0f) {
            EucLine L1 = new EucLine(L.X, L.Y, L.Z);
            EucLine L2 = GetBisectorLine(P, P2);
            HypPoint P3 = GetCrossingPointOfTwoLines(L1, L2);
            return GetHLineFromCenter(P3);
        }
        else
        {
            HypLine HL1 = new HypLine
            {
                X = L.Y,
                Y = -L.X,
                Z = 0f,
                R = 0f
            };
            return HL1;
        }
    }


    public static HypPoint GetInversionAlongHLine(HypLine L, HypPoint P)
    {
        if (P.Z==0f) return new HypPoint(L.X, L.Y, L.Z);
        if (L.Z != 0f && L.R!= Mathf.Infinity) { 
            float X3 = P.X * L.Z - L.X * P.Z;
            float Y3 = P.Y * L.Z - L.Y * P.Z;
            float Z3 = P.Z * L.Z;
            //Debug.Log("XYZ3=" + X3 / Z3 + "," + Y3 / Z3);
            float X4 = X3 * Z3 * L.R * L.R ;
            float Y4 = Y3 * Z3 * L.R * L.R ;
            float Z4 = X3 * X3 + Y3 * Y3  ;
            //Debug.Log("P4x=" + X4 / Z4 + "," + Y4 / Z4);
            return new HypPoint( 
                X4 * L.Z + L.X * Z4, 
                Y4 * L.Z + L.Y * Z4, 
                L.Z * Z4);
        }
        else
        {
            float a = L.X;
            float b = L.Y;       //L: aX+by=0;
            float ab = Mathf.Sqrt(a * a + b * b);
            a /= ab;
            b /= ab; // Normalize
            float x = P.GetX();
            float y = P.GetY();
            float dist = a * x + b * y;
            return new HypPoint(
                x - 2 * dist * a,
                y - 2 * dist * b
                );
        }
    }

    public static float E2HFromOrigin(float Er)
    {
        if (Er <= -1 || 1 <= Er)
        {
            return 0f;
        }
        float Hr = Mathf.Log((1f + Er) / (1f - Er),2.718281828459f);
        return Hr;
    }

    public static float H2EFromOrigin(float Hr)
    {
        float Er = (Mathf.Exp(Hr / 2) - Mathf.Exp(-Hr / 2)) / (Mathf.Exp(Hr / 2) + Mathf.Exp(-Hr / 2));
        return Er;
    }

    public static void GetIdealPointsFromTwoPoints(HypPoint P1, HypPoint P2, out HypPoint Out1, out HypPoint Out2)
    {
        HypLine L = GetHLineThruTwoPoints(P1, P2);
        //L.Println("L");
        if (L.R > 300f || L.Z==0f)
        {
            float X1 = -L.Y;
            float Y1 = L.X;
            float mag = Mathf.Sqrt(X1 * X1 + Y1 * Y1);
            Out1 = new HypPoint( X1, Y1, mag);
            Out2 = new HypPoint( -X1, -Y1, mag);
        }
        else
        {
            float midT = Mathf.Atan2(L.Y, L.X);
            float widT = Mathf.Asin(1f / Mathf.Sqrt(L.GetX() * L.GetX() + L.GetY() * L.GetY()));
            float t1 = midT - widT;
            Out1 = new HypPoint(L.GetX() - L.R * Mathf.Cos(t1), L.GetY() - L.R * Mathf.Sin(t1));
            float t2 = midT + widT;
            Out2 = new HypPoint(L.GetX() - L.R * Mathf.Cos(t2), L.GetY() - L.R * Mathf.Sin(t2));
        }
    }
    //public static void GetIdealPointsFromTwoPoints(float x1, float y1, float x2, float y2, out float X1, out float Y1, out float X2, out float Y2)
    //{
    //    HypLine L = GetHLineThruTwoPoints(new HypPoint(x1,y1), new HypPoint(x2,y2));
    //    float X = L.GetX();
    //    float Y = L.GetY();
    //    float R = L.R;
    //    //Debug.Log(X + " " + Y + " " + R);
    //    X1 = Y1 = X2 = Y2 = 0f;
    //    if (R > 300f)
    //    {
    //        X1 = -Y;
    //        Y1 = X;
    //        float mag = Mathf.Sqrt(X * X + Y * Y);
    //        X1 /= mag;
    //        Y1 /= mag;
    //        X2 = -X1;
    //        Y2 = -Y1;
    //    }
    //    else
    //    {
    //        float midT = Mathf.Atan2(Y, X);
    //        float widT = Mathf.Asin(1f / Mathf.Sqrt(X * X + Y * Y));
    //        float t1 = midT - widT;
    //        X1 = X - R * Mathf.Cos(t1);
    //        Y1 = Y - R * Mathf.Sin(t1);
    //        float t2 = midT + widT;
    //        X2 = X - R * Mathf.Cos(t2);
    //        Y2 = Y - R * Mathf.Sin(t2);
    //    }
    //}

    public static float GetEDistanceOfTwoPoints(HypPoint P1, HypPoint P2)
    {
        if (P1.Z == 0f || P2.Z == 0f)
        {
            return Mathf.Infinity;
        }
        else
        {
            float X = (P1.GetX()) - (P2.GetX());
            float Y = (P1.GetY()) - (P2.GetY());
            return Mathf.Sqrt(X * X + Y * Y);
        }
    }

    public static float GetEDistanceOfPointAndELine(HypPoint P, EucLine L)
    {
        if(P.Z == 0f)
        {
            return Mathf.Infinity;
        }
        float Num = Mathf.Abs(L.a * (P.X / P.Z) + L.b * (P.Y / P.Z) - (L.p));
        float Den = Mathf.Sqrt(L.a * L.a + L.b * L.b);
        return Num / Den;
    }

    public static float GetHDistanceOfTwoPoints(HypPoint P1, HypPoint P2)
    {
        GetIdealPointsFromTwoPoints(P1, P2, out HypPoint O1, out HypPoint O2);
        //O1.Println("O1");
        //O2.Println("O2");
        float d11 = GetEDistanceOfTwoPoints(P1, O1);
        float d12 = GetEDistanceOfTwoPoints(P1, O2);
        float d21 = GetEDistanceOfTwoPoints(P2, O1);
        float d22 = GetEDistanceOfTwoPoints(P2, O2);
        float ret1 =  Mathf.Abs(Mathf.Log(d11 * d22 / d12 / d21,2.718281828459f));
        return ret1;
    }

    public static void GetHLineFromVector(float x1, float y1, float u1, float v1, out float X, out float Y, out float R)
    {
        // u1(x-x1) + v1(y-y1) = 0
        float A1 = u1, B1 = v1, C1 = u1 * x1 + v1 * y1;
        GetInversionImage(x1, y1, out float x2, out float y2);
        GetBisectorLine(x1, y1, x2, y2, out float A2, out float B2, out float C2);

        GetCrossingPointOfTwoLines(A1, B1, A2, B2, C1, C2, out X, out Y);
        R = Mathf.Sqrt(X * X + Y * Y - 1f);
    }

    public static HypLine GetHLineFromVector(HypPoint P, Vector2 V)
    {
        // u1(x-x1) + v1(y-y1) = 0
        float x1 = P.GetX();
        float y1 = P.GetY();
        float u1 = V.x;
        float v1 = V.y;
        EucLine E1 = new EucLine(u1, v1, u1 * x1 + v1 * y1);
        //GetInversionImage(x1, y1, out float x2, out float y2);
        HypPoint P2 = GetInversionImage(P);
        //        GetBisectorLine(x1, y1, x2, y2, out float A2, out float B2, out float C2);
        EucLine E2 = GetBisectorLine(P, P2);
        HypPoint P3 = GetCrossingPointOfTwoLines(E1, E2);
        //GetCrossingPointOfTwoLines(A1, B1, A2, B2, C1, C2, out float x3, out float y3);

        //float r3 = Mathf.Sqrt(x3 *x3 + y3 * y3 - 1f);
        //HypLine L = new HypLine();
        //L.X = x3;
        //L.Y = y3;
        //L.Z = 1f;
        //L.R = r3;
        return GetHLineFromCenter(P3);
    }

    //public static void GetHMoveAlongTwoPoints(float x1, float y1, float x2, float y2, float d, out float X, out float Y)
    public static HypPoint GetHMoveAlongTwoPoints(HypPoint P1, HypPoint P2, float d)
    {
        if (!P1.InWorld() || !P2.InWorld())
        {
            return P1;
        }
        if (P1 != HypPoint.Zero())
        {
            //X1をOへ移す直線L1
            HypLine L1 = GetHBisectorOfTwoPoints(P1, HypPoint.Zero());
            //点X2をL1で線対称移動
            HypPoint XY2 = GetInversionAlongHLine(L1, P2);
            if (XY2.Z == 0f)
            {
                P1.Println("---P1");
                P2.Println("---P2");
                L1.Println("---L1");
                XY2.Println("--XY2");
            }
            float magXY2 = Mathf.Sqrt(XY2.GetX() * XY2.GetX() + XY2.GetY() * XY2.GetY());
            float ed = H2EFromOrigin(d);
            //原点からX2へ向かう線分で、双曲長がdであるような点
            XY2.X *= ed;
            XY2.Y *= ed;
            XY2.Z *= magXY2;
            //点X2をL1で線対称移動
            HypPoint P3 = GetInversionAlongHLine(L1, XY2);
            if (P3.InWorld())
            {
                return P3;
            }
            else
            {
                Debug.Log("error occurs at GetHMoveAlongTwoPoints");
                P1.Println("P1");
                P2.Println("P2");
                Debug.Log("magXY2=" + magXY2);
                Debug.Log("ed=" + ed);
                XY2.Println("XY2");
                P3.Println("P3");
                return P1;
            }
        }
        else
        {
            HypPoint XY2 = P2;
            float magXY2 = Mathf.Sqrt(XY2.GetX() * XY2.GetX() + XY2.GetY() * XY2.GetY());
            float ed = H2EFromOrigin(d);
            //原点からX2へ向かう線分で、双曲長がdであるような点
            XY2.X *= ed;
            XY2.Y *= ed;
            XY2.Z *= magXY2;
            HypPoint P3 = XY2;
            return P3;
        }
    }

    public static HypPoint GetMidPointOfTwoPoint(HypPoint P1, HypPoint P2)
    {
        HypLine L1 = GetHLineThruTwoPoints(P1, P2);
        HypLine L2 = GetHBisectorOfTwoPoints(P1, P2);
        return GetCrossingPointOfTwoHLines(L1, L2);
    }

    public static HypPoint GetRotationOfPoint(HypPoint C, float theta, HypPoint P1)
    {
        if (C.GetX()!=0f || C.GetY()!=0f) { 
            HypLine L1 = GetHBisectorOfTwoPoints(C, HypPoint.Zero());
            HypPoint P2 = GetInversionAlongHLine(L1, P1);
            HypPoint P3 = new HypPoint(
                Mathf.Cos(theta) * P2.X - Mathf.Sin(theta) * P2.Y,
                Mathf.Sin(theta) * P2.X + Mathf.Cos(theta) * P2.Y,
                P2.Z
                );
            return GetInversionAlongHLine(L1, P3);
        }
        else
        {
            HypPoint P3 = new HypPoint(
                Mathf.Cos(theta) * P1.X - Mathf.Sin(theta) * P1.Y,
                Mathf.Sin(theta) * P1.X + Mathf.Cos(theta) * P1.Y,
                P1.Z
                );
            return P3;
        }
    }

    /// <summary>
    /// P1 = crossing point if exists. 
    /// returns between 0 to PI/2
    /// </summary>
    /// <param name="L1"></param>
    /// <param name="L2"></param>
    /// <param name="P1"></param>
    /// <returns></returns>
    public static float GetAngleOfTwoLines(HypLine L1, HypLine L2, HypPoint P1)
    { // P1 = crossing point if exists
        if (L1.Z!=0f && L2.Z != 0f)
        {
            float u1 = L1.GetX() - P1.GetX();
            float v1 = L1.GetY() - P1.GetY();
            float u2 = L2.GetX() - P1.GetX();
            float v2 = L2.GetY() - P1.GetY();
            return Mathf.Acos(Mathf.Abs(u1 * u2 + v1 * v2) / Mathf.Sqrt((u1 * u1 + v1 * v1) * (u2 * u2 + v2 * v2)));
        }
        else if (L1.Z == 0f && L2.Z != 0f)
        {
            float u1 = L2.GetX() - P1.GetX();
            float v1 = L2.GetY() - P1.GetY();
            float u2 = L1.X;
            float v2 = L1.Y;
            return Mathf.Acos(Mathf.Abs(u1 * u2 + v1 * v2) / Mathf.Sqrt((u1 * u1 + v1 * v1) * (u2 * u2 + v2 * v2)));
        }
        else if (L1.Z != 0f && L2.Z == 0f)
        {
            float u1 = L1.GetX() - P1.GetX();
            float v1 = L1.GetY() - P1.GetY();
            float u2 = L2.X;
            float v2 = L2.Y;
            return Mathf.Acos(Mathf.Abs(u1 * u2 + v1 * v2) / Mathf.Sqrt((u1 * u1 + v1 * v1) * (u2 * u2 + v2 * v2)));
        }
        else if (L1.Z == 0f && L2.Z == 0f)
        {
            float u1 = L1.X;
            float v1 = L1.Y;
            float u2 = L2.X;
            float v2 = L2.Y;
            return Mathf.Acos(Mathf.Abs(u1 * u2 + v1 * v2) / Mathf.Sqrt((u1 * u1 + v1 * v1) * (u2 * u2 + v2 * v2)));
        }

        return 0f;
    }


    /// <summary>
    /// P1 = crossing point if exists. 
    /// returns between -PI to PI
    /// </summary>
    public static float GetAngleOfTwoLines(HypPoint P11, HypPoint P12, HypPoint P21, HypPoint P22, HypPoint C)
    {
        HypLine L1 = GetHBisectorOfTwoPoints(C, HypPoint.Zero());
        HypPoint Q11 = GetInversionAlongHLine(L1, P11);
        HypPoint Q12 = GetInversionAlongHLine(L1, P12);
        HypPoint Q21 = GetInversionAlongHLine(L1, P21);
        HypPoint Q22 = GetInversionAlongHLine(L1, P22);
        float V1x = Q12.GetX() - Q11.GetX();
        float V1y = Q12.GetY() - Q11.GetY();
        float V2x = Q22.GetX() - Q21.GetX();
        float V2y = Q22.GetY() - Q21.GetY();
        float InnerProd = V1x * V2x + V1y * V2y;
        float NormV1 = Mathf.Sqrt(V1x * V1x + V1y * V1y);
        float NormV2 = Mathf.Sqrt(V2x * V2x + V2y * V2y);
        float Area = V1x * V2y - V1y * V2x;
        float Theta = Mathf.Acos(InnerProd/NormV1/NormV2);
        if (Area < 0)
        {
            Theta *= -1;
        }
        return Theta;
    }
}
