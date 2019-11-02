using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HModule : MonoBehaviour
{
    public GameObject VA = null, VB = null, VC = null, VD = null;
    public float ParaA = 0f, ParaB = 0f;
    public MODE Mode = 0;
    public int ID;

    public GameObject HModuleLog;

    public bool Active = true;

    void Start()
    {
        
    }


    private void Update()
    {
        
    }
    public void ModuleUpdate()
    {
        for (int repeat = 0; repeat < 10; repeat++)
        {
            if (Mode == MODE.ADD_MIDPOINT)
            {
                HypPoint V1 = new HypPoint(VA.GetComponent<Vertex>().XY);
                HypPoint V2 = new HypPoint(VB.GetComponent<Vertex>().XY);
                HypPoint V3 = new HypPoint(VC.GetComponent<Vertex>().XY);
                HypPoint W1 = HTransform.ParallelTransform(V2, V3, V3);
                HypPoint W2 = HTransform.ParallelTransform(V1, V3, V3);
                HypPoint W3 = HTransform.GetMidPointOfTwoPoint(V1, V2);
                float dist1 = HTransform.GetHDistanceOfTwoPoints(V1, W1);
                float dist2 = HTransform.GetHDistanceOfTwoPoints(V2, W2);
                float dist3 = HTransform.GetHDistanceOfTwoPoints(V3, W3);
                HypPoint U1 = HTransform.GetHMoveAlongTwoPoints(V1, W1, dist1 * 0.1f);
                HypPoint U2 = HTransform.GetHMoveAlongTwoPoints(V2, W2, dist2 * 0.1f);
                HypPoint U3 = HTransform.GetHMoveAlongTwoPoints(V3, W3, dist3 * 0.1f);
                //if (!U1.InWorld() || !U2.InWorld() || !U3.InWorld())
                //{
                //    V1.Println("V1:");
                //    V2.Println("V2:");
                //    V3.Println("V3:");
                //    W1.Println("W1:");
                //    Debug.Log("dist1:" + dist1);
                //    U1.Println("U1:");
                //}
                if (U1.InWorld() && U2.InWorld() && U3.InWorld())
                {
                    if (!VA.GetComponent<Vertex>().Fixed)
                    {
                        VA.GetComponent<Vertex>().XY.x = U1.GetX();
                        VA.GetComponent<Vertex>().XY.y = U1.GetY();
                    }
                    if (!VB.GetComponent<Vertex>().Fixed)
                    {
                        VB.GetComponent<Vertex>().XY.x = U2.GetX();
                        VB.GetComponent<Vertex>().XY.y = U2.GetY();
                    }
                    if (!VC.GetComponent<Vertex>().Fixed)
                    {
                        VC.GetComponent<Vertex>().XY.x = U3.GetX();
                        VC.GetComponent<Vertex>().XY.y = U3.GetY();
                    }
                }
            }
            else if (Mode == MODE.POINT_TO_POINT)
            {
                if (VA != null && VB != null)
                {
                    Vector2 V1 = VA.GetComponent<Vertex>().XY;
                    Vector2 V2 = VB.GetComponent<Vertex>().XY;
                    Vector2 NewV1 = 0.75f * V1 + 0.25f * V2;
                    Vector2 NewV2 = 0.25f * V1 + 0.75f * V2;
                    if (!VA.GetComponent<Vertex>().Fixed)
                    {
                        VA.GetComponent<Vertex>().XY = NewV1;
                    }
                    if (!VB.GetComponent<Vertex>().Fixed)
                    {
                        VB.GetComponent<Vertex>().XY = NewV2;
                    }
                }
            }
            else if (Mode == MODE.POINT_TO_LINE)
            {
                if (VA != null && VB != null)
                {
                    ModuleUpdatePointToLine2();
                }
            }
            else if (Mode == MODE.POINT_TO_CIRCLE)
            {
                if (VA != null && VB != null)
                {
                    ModuleUpdatePointToCircle();
                }
            }
            else if (Mode == MODE.ISOMETRY)
            {
                if (VA != null && VB != null)
                {
                    ModuleUpdateIsometry();
                }
            }
            else if (Mode == MODE.PERPENDICULAR)
            {
                if (VA != null && VB != null)
                {
                    ModuleUpdatePerpendicular();
                }
            }
            else if (Mode == MODE.ANGLE)
            {
                if (VA != null && VB != null && VC != null)
                {
                    ModuleUpdateAngle();
                }
            }
            else if (Mode == MODE.TANGENT_C2L)
            {
                if (VA != null && VB != null)
                {
                    ModuleUpdateTangentCircleToLine();
                }
            }
            else if (Mode == MODE.TANGENT_C2C)
            {
                if (VA != null && VB != null)
                {
                    HCircle hcircle1 = VA.GetComponent<HCircle>();//対象となる円1
                    HCircle hcircle2 = VB.GetComponent<HCircle>();//対象となる円2
                    if (hcircle1 != null && hcircle2 != null)
                    {
                        HypCircle C1 = hcircle1.HCR;
                        HypCircle C2 = hcircle2.HCR;
                        HypPoint P1 = new HypPoint(hcircle1.VA.GetComponent<Vertex>().XY);
                        HypPoint P2 = new HypPoint(hcircle2.VA.GetComponent<Vertex>().XY);
                        //HypLine L1 = HTransform.GetHLineThruTwoPoints(P1,P2);
                        float dist = HTransform.GetHDistanceOfTwoPoints(P1, P2);
                        float errorOut = dist - (C1.HR + C2.HR);
                        float errorIn = dist - Mathf.Abs(C1.HR - C2.HR);
                        if(Mathf.Abs(errorOut)< Mathf.Abs(errorIn))
                        {// 外接と思われる
                            float error = errorOut * 0.1f;
                            //円C1を寄せる
                            HypPoint P3 = HTransform.GetHMoveAlongTwoPoints(P1, P2, error);
                            Vertex C1V = hcircle1.VA.GetComponent<Vertex>();
                            if (!C1V.Fixed && P3.InWorld())
                            {
                                C1V.XY.x = P3.GetX();
                                C1V.XY.y = P3.GetY();
                            }
                            //円C1の半径を調節する。
                            C1.HR += error;
                            //円C2を寄せる
                            HypPoint P4 = HTransform.GetHMoveAlongTwoPoints(P2, P1, error);
                            Vertex C2V = hcircle2.VA.GetComponent<Vertex>();
                            if (!C2V.Fixed && P4.InWorld())
                            {
                                C2V.XY.x = P4.GetX();
                                C2V.XY.y = P4.GetY();
                            }
                            //円C2の半径を調節する。
                            C2.HR += error;
                        }
                        else
                        {//内接と思われる
                            float error = errorIn * 0.1f;
                            bool C1Out = false;
                            if (C1.HR > C2.HR)
                            {// C1が外側と思われる
                                C1Out = true;
                            }
                            //円C1を寄せる
                            HypPoint P3 = HTransform.GetHMoveAlongTwoPoints(P1, P2, error);
                            Vertex C1V = hcircle1.VA.GetComponent<Vertex>();
                            if (!C1V.Fixed && P3.InWorld())
                            {
                                C1V.XY.x = P3.GetX();
                                C1V.XY.y = P3.GetY();
                            }
                            if(C1Out){
                                C1.HR += error;
                            }
                            else
                            {
                                C1.HR -= error;
                            }
                            //円C2を寄せる
                            HypPoint P4 = HTransform.GetHMoveAlongTwoPoints(P2, P1, error);
                            Vertex C2V = hcircle2.VA.GetComponent<Vertex>();
                            if (!C2V.Fixed && P4.InWorld())
                            {
                                C2V.XY.x = P4.GetX();
                                C2V.XY.y = P4.GetY();
                            }
                            //円C2の半径を調節する。
                            if (C1Out)
                            {
                                C2.HR -= error;
                            }
                            else
                            {
                                C2.HR += error;
                            }
                        }
                    }
                }
            }
        }
    }

    void ModuleUpdateIsometry()
    {
        HLine HLN1 = VA.GetComponent<HLine>();//対象となる直線1
        HLine HLN2 = VB.GetComponent<HLine>();//対象となる直線2
        Vertex V11 = HLN1.VA.GetComponent<Vertex>();
        Vertex V12 = HLN1.VB.GetComponent<Vertex>();
        Vertex V21 = HLN2.VA.GetComponent<Vertex>();
        Vertex V22 = HLN2.VB.GetComponent<Vertex>();
        HypPoint P11 = new HypPoint(V11.XY);
        HypPoint P12 = new HypPoint(V12.XY);
        HypPoint P21 = new HypPoint(V21.XY);
        HypPoint P22 = new HypPoint(V22.XY);
        float Dist1 = HTransform.GetHDistanceOfTwoPoints(P11, P12);
        float Dist2 = HTransform.GetHDistanceOfTwoPoints(P21, P22);
        float Error = (Dist2 - Dist1)*0.1f;
        HypPoint NewP11 = HTransform.GetHMoveAlongTwoPoints(P11, P12, -Error);
        HypPoint NewP12 = HTransform.GetHMoveAlongTwoPoints(P12, P11, -Error);
        HypPoint NewP21 = HTransform.GetHMoveAlongTwoPoints(P21, P22, Error);
        HypPoint NewP22 = HTransform.GetHMoveAlongTwoPoints(P22, P21, Error);
        if (NewP11.InWorld() && !V11.Fixed)
        {
            V11.XY.x = NewP11.GetX();
            V11.XY.y = NewP11.GetY();
        }
        if (NewP12.InWorld() && !V12.Fixed)
        {
            V12.XY.x = NewP12.GetX();
            V12.XY.y = NewP12.GetY();
        }
        if (NewP21.InWorld() && !V21.Fixed)
        {
            V21.XY.x = NewP21.GetX();
            V21.XY.y = NewP21.GetY();
        }
        if (NewP22.InWorld() && !V22.Fixed)
        {
            V22.XY.x = NewP22.GetX();
            V22.XY.y = NewP22.GetY();
        }

    }

    void ModuleUpdatePerpendicular()
    {
        HLine HLN1 = VA.GetComponent<HLine>();//対象となる直線1
        HLine HLN2 = VB.GetComponent<HLine>();//対象となる直線2
        HypLine HL1 = HLN1.HL;
        HypLine HL2 = HLN2.HL;
        Vertex V11 = HLN1.VA.GetComponent<Vertex>();
        Vertex V12 = HLN1.VB.GetComponent<Vertex>();
        Vertex V21 = HLN2.VA.GetComponent<Vertex>();
        Vertex V22 = HLN2.VB.GetComponent<Vertex>();
        HypPoint P11 = new HypPoint(V11.XY);
        HypPoint P12 = new HypPoint(V12.XY);
        HypPoint P21 = new HypPoint(V21.XY);
        HypPoint P22 = new HypPoint(V22.XY);

        HypPoint C = HTransform.GetCrossingPointOfTwoHLines2(HL1, HL2);
        if (C != new HypPoint(1f,1f,0f))
        {
            //float angle = HTransform.GetAngleOfTwoLines(HL1, HL2, C);
            float angle = HTransform.GetAngleOfTwoLines(P11,P12,P21,P22, C);
            //Debug.Log("angle="+angle);
            float error = (Mathf.PI / 2f - angle) * 0.25f;
            if(angle < 0f)
            {
                error = (-Mathf.PI / 2f - angle) * 0.25f;
            }
            HypPoint P1m = HTransform.GetMidPointOfTwoPoint(P11, P12);
            HypPoint P11T = HTransform.GetRotationOfPoint(P1m, -error, P11);
            HypPoint P12T = HTransform.GetRotationOfPoint(P1m, -error, P12);
            //HypLine L1T = HTransform.GetHLineThruTwoPoints(P11T, P12T);
            HypPoint P2m = HTransform.GetMidPointOfTwoPoint(P21, P22);
            HypPoint P21T = HTransform.GetRotationOfPoint(P2m, error, P21);
            HypPoint P22T = HTransform.GetRotationOfPoint(P2m, error, P22);
            //HypLine L2T = HTransform.GetHLineThruTwoPoints(P21T, P22T);
            if (!V11.Fixed && P11T.InWorld())
            { 
                V11.XY.x = P11T.GetX();
                V11.XY.y = P11T.GetY();
            }
            if (!V12.Fixed && P12T.InWorld())
            {
                V12.XY.x = P12T.GetX();
                V12.XY.y = P12T.GetY();
            }
            if (!V21.Fixed && P21T.InWorld())
            {
                V21.XY.x = P21T.GetX();
                V21.XY.y = P21T.GetY();
            }
            if (!V22.Fixed && P22T.InWorld())
            {
                V22.XY.x = P22T.GetX();
                V22.XY.y = P22T.GetY();
            }
        }
    }

    void ModuleUpdateAngle()
    {
        Vertex VtxA = VA.GetComponent<Vertex>();// 対象となる点A
        Vertex VtxB = VB.GetComponent<Vertex>();// 対象となる点B
        Vertex VtxC = VC.GetComponent<Vertex>();// 対象となる点C
        float FinalAngle = Mathf.Abs(ParaA * Mathf.PI / 180f);// 対象となる角
        HypPoint PA = new HypPoint(VtxA.XY);
        HypPoint PB = new HypPoint(VtxB.XY);
        HypPoint PC = new HypPoint(VtxC.XY);
        HypPoint PO = new HypPoint(0, 0);
        // Bを原点に移す写像を求める。
        HypLine LA = HTransform.GetHBisectorOfTwoPoints(PB, PO);
        HypPoint PAA = HTransform.GetInversionAlongHLine(LA, PA);
        HypPoint PCC = HTransform.GetInversionAlongHLine(LA, PC);
        //角ABCを求める
        float DeclineA = Mathf.Atan2(PAA.GetY(), PAA.GetX());
        float DeclineC = Mathf.Atan2(PCC.GetY(), PCC.GetX());
        float AngleAC = DeclineC - DeclineA;
        if (AngleAC > Mathf.PI) AngleAC -= (Mathf.PI * 2f);
        if (AngleAC < -Mathf.PI) AngleAC += (Mathf.PI * 2f);
        // 修正する角度をもとめる。
        float error = (AngleAC - FinalAngle) * 0.1f;
        if (AngleAC < 0)
            error = (AngleAC + FinalAngle) * 0.1f;
        //今一度点の取り直し。
        {//線分ABのとりなおし
            PA = new HypPoint(VtxA.XY);
            PB = new HypPoint(VtxB.XY);
            HypPoint PM = HTransform.GetMidPointOfTwoPoint(PA, PB);
            LA = HTransform.GetHBisectorOfTwoPoints(PM, PO);
            HypPoint PPA = HTransform.GetInversionAlongHLine(LA, PA);
            HypPoint PPB = HTransform.GetInversionAlongHLine(LA, PB);
            HypPoint NewPPA = HTransform.GetRotationOfPoint(PO, error, PPA);
            HypPoint NewPPB = HTransform.GetRotationOfPoint(PO, error, PPB);
            HypPoint NewPA = HTransform.GetInversionAlongHLine(LA, NewPPA);
            HypPoint NewPB = HTransform.GetInversionAlongHLine(LA, NewPPB);
            if (!VtxA.Fixed && NewPA.InWorld())
            {
                VtxA.XY.x = NewPA.GetX();
                VtxA.XY.y = NewPA.GetY();
            }
            if (!VtxB.Fixed && NewPB.InWorld())
            {
                VtxB.XY.x = NewPB.GetX();
                VtxB.XY.y = NewPB.GetY();
            }
        }
        {//線分BCの取り直し
            PC = new HypPoint(VtxC.XY);
            PB = new HypPoint(VtxB.XY);
            HypPoint PM = HTransform.GetMidPointOfTwoPoint(PC, PB);
            LA = HTransform.GetHBisectorOfTwoPoints(PM, PO);
            HypPoint PPC = HTransform.GetInversionAlongHLine(LA, PC);
            HypPoint PPB = HTransform.GetInversionAlongHLine(LA, PB);
            HypPoint NewPPC = HTransform.GetRotationOfPoint(PO, -error, PPC);
            HypPoint NewPPB = HTransform.GetRotationOfPoint(PO, -error, PPB);
            HypPoint NewPC = HTransform.GetInversionAlongHLine(LA, NewPPC);
            HypPoint NewPB = HTransform.GetInversionAlongHLine(LA, NewPPB);
            if (!VtxC.Fixed && NewPC.InWorld())
            {
                VtxC.XY.x = NewPC.GetX();
                VtxC.XY.y = NewPC.GetY();
            }
            if (!VtxB.Fixed && NewPB.InWorld())
            {
                VtxB.XY.x = NewPB.GetX();
                VtxB.XY.y = NewPB.GetY();
            }
        }
    }

    void ModuleUpdatePointToCircle()
    {
        Vector2 vtx = VA.GetComponent<Vertex>().XY;//対象となる点
        HCircle hcircle = VB.GetComponent<HCircle>();//対象となる円
        if (vtx != null && hcircle != null)
        {
            HypCircle cr = hcircle.GetHCR();//円の円データ
            HypPoint HPcr = new HypPoint(cr.HX, cr.HY);//円の中心
            HypPoint Pvtx = new HypPoint(vtx.x, vtx.y);//対象となる点
            float dist = HTransform.GetHDistanceOfTwoPoints(HPcr, Pvtx) - cr.HR;//誤差
            //Debug.Log(HTransform.GetHDistanceOfTwoPoints(HPcr, Pvtx)+"->");
            if (Mathf.Abs(dist) > 0.0001f)
            {
                //円を点に寄せる
                HypPoint P1 = HTransform.GetHMoveAlongTwoPoints(HPcr, Pvtx, dist * 0.5f);
                //点を円に寄せる
                HypPoint P2 = HTransform.GetHMoveAlongTwoPoints(Pvtx, HPcr, dist * 0.5f);
                //Debug.Log("*:"+dist+" "+ X+" "+ Y);
                if (!VB.GetComponent<HCircle>().VA.GetComponent<Vertex>().Fixed && P1.InWorld())
                {
                    VB.GetComponent<HCircle>().VA.GetComponent<Vertex>().XY.x = P1.GetX();
                    VB.GetComponent<HCircle>().VA.GetComponent<Vertex>().XY.y = P1.GetY();
                }
                if (!VA.GetComponent<Vertex>().Fixed && P2.InWorld())
                {
                    VA.GetComponent<Vertex>().XY.x = P2.GetX();
                    VA.GetComponent<Vertex>().XY.y = P2.GetY();
                }
                //円の半径を調整する
                VB.GetComponent<HCircle>().HCR.HR += (dist * 0.1f);
                cr = hcircle.HCR;//円の円データ
                HPcr = new HypPoint(cr.HX, cr.HY);//円の中心
                Pvtx = new HypPoint(vtx.x, vtx.y);//対象となる点
            }
        }
    }

    void ModuleUpdatePointToLine()
    {
        Vector2 vtx = VA.GetComponent<Vertex>().XY;//対象となる点
        HLine hline = VB.GetComponent<HLine>();//対象となる直線
        if (vtx != null && hline != null)
        {

            HypLine ln = hline.HL;//直線の円データ
            Vector2 hlnCenter = new Vector2(ln.GetX(), ln.GetY());//直線の円データの中心座標
            Vector2 direction = vtx - hlnCenter;//円の中心から対象となる点の方向
            float dist = direction.magnitude - ln.R;//誤差
            if (Mathf.Abs(dist) > 0.001f)
            {
                direction.Normalize();
                //Vector2 newVtx = hlnCenter + (ln.R + 0.75f * dist) * direction;//新しい点の座標
                Vector2 newVtx = vtx  - (0.1f * dist) * direction;//新しい点の座標
                HypPoint newPt = new HypPoint(newVtx);
                if (!VA.GetComponent<Vertex>().Fixed && newPt.InWorld())
                {
                    VA.GetComponent<Vertex>().XY = newVtx;
                }
                Vector2 startPos = vtx - dist * direction;//平行移動スタート点
                Vector2 endPos = vtx - (0.8f * dist) * direction;//平行移動ゴール点
                Vertex lineVertex1 = hline.VA.GetComponent<Vertex>();//動かすべき点1
                Vertex lineVertex2 = hline.VB.GetComponent<Vertex>();//動かすべき点2
                //new HypPoint(lineVertex1.XY).Println("LV1");
                //new HypPoint(lineVertex2.XY).Println("LV2");
                Vector2 XY1 = lineVertex1.XY;
                Vector2 XY2 = lineVertex2.XY;
                if (!lineVertex1.Fixed)
                {
                    HypPoint HP1 = new HypPoint(XY1);
                    HypPoint HPnew1 = HTransform.ParallelTransform(startPos, endPos, HP1);//点１を平行移動する
                    if (HPnew1.InWorld())
                    {
                        lineVertex1.XY.x = HPnew1.GetX();
                        lineVertex1.XY.y = HPnew1.GetY();
                    }
                    else
                    {
                        Debug.Log("error occurs at module P2L - 1A:" + HPnew1.X + "," + HPnew1.Y + "," + HPnew1.Z);
                        HP1.Println("HP1");
                        HPnew1.Println("HPnew1");
                        Debug.Log("dist " + dist);
                        Debug.Log("ln.R " + ln.R);
                        Debug.Log("direction " + direction.x + "," + direction.y);
                        Debug.Log("hlnCenter " + hlnCenter.x + "," + hlnCenter.y);
                        Debug.Log("startPos" + startPos.x + "," + startPos.y);
                        Debug.Log("endPos" + endPos.x + "," + endPos.y);
                        Debug.Log(XY1);

                    }
                }
                if (!lineVertex2.Fixed)
                {
                    HypPoint HP2 = new HypPoint(XY2);
                    HypPoint HPnew2 = HTransform.ParallelTransform(startPos, endPos, HP2);//点2を平行移動する
                    if (HPnew2.InWorld())
                    {
                        lineVertex2.XY.x = HPnew2.GetX();
                        lineVertex2.XY.y = HPnew2.GetY();
                    }
                    else
                    {
                        Debug.Log("error occurs at module P2L - 2A:" + HPnew2.X + "," + HPnew2.Y + "," + HPnew2.Z);
                    }
                }
                hline.GetHLineFromTwoVertices();
            }
        }
    }

    //string MyDebug_Backup="";
    void ModuleUpdatePointToLine2()
    {
        Vertex VTX = VA.GetComponent<Vertex>();//対象となる点
        HLine HLN = VB.GetComponent<HLine>();//対象となる直線
        if (VTX != null && HLN != null)
        {
            HypLine L1 = HLN.HL;
            HypPoint P1 = new HypPoint(VTX.XY);
            HypLine L2 = HTransform.GetHPerpendicularThruAPoint(L1, P1);
            HypPoint P2 = HTransform.GetCrossingPointOfTwoHLines(L1, L2);
            float error = HTransform.GetHDistanceOfTwoPoints(P1, P2) * 0.2f;
            //Debug.Log(error);
            if (0.001f < error || error < -0.001f)
            {
                //string MyDebug = "";
                HypPoint P3 = HTransform.GetHMoveAlongTwoPoints(P2, P1, error);
                HypPoint P4 = HTransform.GetHMoveAlongTwoPoints(P1, P2, error);
                ////点を直線に寄せる
                if (!VTX.Fixed && P4.InWorld())
                {
                    //MyDebug += "P4:" + P4.GetX() + "," + P4.GetY();
                    VTX.XY.x = P4.GetX();
                    VTX.XY.y = P4.GetY();
                }
                //直線を円に寄せる
                Vertex L1V1 = HLN.VA.GetComponent<Vertex>();//動かすべき点1
                Vertex L1V2 = HLN.VB.GetComponent<Vertex>();//動かすべき点2
                HypPoint PL1 = new HypPoint(L1V1.XY);
                HypPoint PL2 = new HypPoint(L1V2.XY);
                HypPoint NewPL1 = HTransform.ParallelTransform(P2, P3, PL1);
                HypPoint NewPL2 = HTransform.ParallelTransform(P2, P3, PL2);
                //NewPL1.println("NewPL1");
                if (!L1V1.Fixed && NewPL1.InWorld())
                {
                    //MyDebug += "L1:" + NewPL1.GetX() + "," + NewPL1.GetY();
                    L1V1.XY.x = NewPL1.GetX();
                    L1V1.XY.y = NewPL1.GetY();
                }
                if (!L1V2.Fixed && NewPL2.InWorld())
                {
                    //MyDebug += "L2:" + NewPL2.GetX() + "," + NewPL2.GetY();
                    L1V2.XY.x = NewPL2.GetX();
                    L1V2.XY.y = NewPL2.GetY();
                }
                HLN.GetHLineFromTwoVertices();
                //if (MyDebug != MyDebug_Backup)
                //{
                //    Debug.Log(MyDebug);
                //    MyDebug_Backup = MyDebug;
                //}
            }
        }
    }


    void ModuleUpdateTangentCircleToLine()
    {
        HCircle hcircle = VA.GetComponent<HCircle>();//対象となる円
        HLine hline = VB.GetComponent<HLine>();//対象となる直線
        if (hcircle != null && hline != null)
        {
            HypCircle C1 = hcircle.GetHCR();
            HypLine L1 = hline.GetHL();
            HypPoint P1 = new HypPoint(hcircle.VA.GetComponent<Vertex>().XY);
            HypLine L2 = HTransform.GetHPerpendicularThruAPoint(L1, P1);
            HypPoint P2 = HTransform.GetCrossingPointOfTwoHLines(L1, L2);
            //P2.println("P2");
            float error = (HTransform.GetHDistanceOfTwoPoints(P1, P2) - C1.HR) * 0.1f;
            //Debug.Log(error);
            if (0.001f < error || error < -0.001f)
            {
                HypPoint P3 = HTransform.GetHMoveAlongTwoPoints(P2, P1, error);
                HypPoint P4 = HTransform.GetHMoveAlongTwoPoints(P1, P2, error);
                //円の半径を変える
                C1.HR += error;
                ////円を直線に寄せる
                Vertex C1V = hcircle.VA.GetComponent<Vertex>();
                if (!C1V.Fixed && P4.InWorld())
                {
                    C1V.XY.x = P4.GetX();
                    C1V.XY.y = P4.GetY();
                }
                //直線を円に寄せる
                Vertex L1V1 = hline.VA.GetComponent<Vertex>();//動かすべき点1
                Vertex L1V2 = hline.VB.GetComponent<Vertex>();//動かすべき点2
                HypPoint PL1 = new HypPoint(L1V1.XY);
                HypPoint PL2 = new HypPoint(L1V2.XY);
                HypPoint NewPL1 = HTransform.ParallelTransform(P2, P3, PL1);
                HypPoint NewPL2 = HTransform.ParallelTransform(P2, P3, PL2);
                //NewPL1.println("NewPL1");
                if (!L1V1.Fixed && NewPL1.InWorld())
                {
                        L1V1.XY.x = NewPL1.GetX();
                        L1V1.XY.y = NewPL1.GetY();
                }
                if (!L1V2.Fixed && NewPL2.InWorld())
                {
                        L1V2.XY.x = NewPL2.GetX();
                        L1V2.XY.y = NewPL2.GetY();
                }
                hline.GetHLineFromTwoVertices();
            }
        }

    }



    bool Missing(GameObject obj)
    {
        try
        {
            Transform TF = obj.GetComponent<Transform>();
        }
        catch 
        {
            return true;
        }
        return false;
    }
}
