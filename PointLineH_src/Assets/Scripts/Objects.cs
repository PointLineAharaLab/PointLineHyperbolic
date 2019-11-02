using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

using System.IO;


public class Objects : MonoBehaviour
{
    public GameObject Camera;
    public GameObject WorldObject;
    public GameObject VertexObject;
    public GameObject HLineObject;
    public GameObject HCircleObject;
    public GameObject HModuleObject;
    public GameObject PreferenceObject;

    public GameObject LogFolder;

    Camera Cam;
    bool NotDragged;
    bool Dragging;
    bool NullDrag;
    bool GameLogDrag;
    bool PreferenceDrag;
    
    Vector3 MouseAbsolutePosition;
    Vector3 MousePosition;
    Vector3 StartDragPosition;
    public static Vertex DraggedVertex;// preferenceからの消去を許すため。
    HLine ClickedLine;
    HCircle ClickedCircle;
    float Log0YStart=4.5f;// ログ表示のｙ座標の起点
    float PreferenceWindowLeftStart, PreferenceWindowTopStart;

    Vertex[] vtxs;
    HLine[] hlns;
    HCircle[] hcls;

    GameObject SelectedObject1;
    GameObject SelectedObject2;
    GameObject SelectedObject3;

    char[] LastVertexName = { 'A',(char)0x00 };
    char[] LastHLineName = { 'a', (char)0x00 };

    int LastVertexID = 1;
    int LastHLineID = 1001;
    int LastHCircleID = 2001;
    int LastHModuleID = 3001;
    int LastGameModuleID = 4001;

    //Cursor
    HypLine CursorVLine;
    HypLine CursorHLine;
    public GameObject CursorV;
    public GameObject CursorH;
    readonly int CursorPosLength = 21;
    Vector3[] CursorVPos;
    Vector3[] CursorHPos;
    AnimationCurve CursorVLineAnim;
    AnimationCurve CursorHLineAnim;
    Keyframe[] CursorVLineKs;
    Keyframe[] CursorHLineKs;
    LineRenderer CursorVLR;
    LineRenderer CursorHLR;

    //
    bool MouseLeftButtonDown = false;

    AudioSource WorldSound;

    // Start is called before the first frame update
    void Start()
    {
        Cam = Camera.GetComponent<Camera>();
        World.Height = Cam.orthographicSize;
        World.Width = World.Height * Screen.width / Screen.height;
        //Debug.Log(World.Width);
        Dragging = false;
        MousePosition = new Vector3(0f, 0f, 0f);
        //DraggedVertex = null;        
        // Cursor
        CursorVLine = new HypLine();
        CursorHLine = new HypLine();
        CursorVLR = CursorV.GetComponent<LineRenderer>();
        CursorVLR.positionCount = CursorPosLength;
        CursorHLR = CursorH.GetComponent<LineRenderer>();
        CursorHLR.positionCount = CursorPosLength;
        CursorVPos = new Vector3[CursorPosLength];
        CursorVLineKs = new Keyframe[CursorPosLength];
        CursorVLineAnim = new AnimationCurve(CursorVLineKs);
        CursorHPos = new Vector3[CursorPosLength];
        CursorHLineKs = new Keyframe[CursorPosLength];
        CursorHLineAnim = new AnimationCurve(CursorVLineKs);
        for (int i = 0; i < CursorPosLength; i++)
        {
            CursorVPos[i] = new Vector3(0f, 0f, -1f);//線の位置を決めるためのベクトルの列
            CursorVLineKs[i] = new Keyframe(1f * i / (CursorPosLength - 1), .05f);//線の太さのためのキーフレーム
            CursorHPos[i] = new Vector3(0f, 0f, -1f);//線の位置を決めるためのベクトルの列
            CursorHLineKs[i] = new Keyframe(1f * i / (CursorPosLength - 1), .05f);//線の太さのためのキーフレーム
            CursorVLR.SetPosition(i, CursorVPos[i]);
            CursorHLR.SetPosition(i, CursorHPos[i]);
        }
        WorldSound = WorldObject.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        MouseAbsolutePosition = Cam.ScreenToWorldPoint(Input.mousePosition);
        MouseAbsolutePosition.z = 0f;
        MousePosition = MouseAbsolutePosition * 0.2f;// 0.2 = 1/(World.Scale);

        if (Input.GetMouseButtonDown(0))
        {
            Dragging = true;
            StartDragPosition = MousePosition;
            MouseLeftButtonDown = false;
            OnMouseDown();
        }
        else if (Input.GetMouseButton(0))
        {
            if (Dragging)
            {
                MouseLeftButtonDown = false;
                OnMouseDrag();
            }
            else
            {

                MouseLeftButtonDown = false;
                OnMouseUp();
                Dragging = false;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            MouseLeftButtonDown = false;
            OnMouseUp();
            Dragging = false;
        }
        if (Input.GetMouseButtonDown(1))
        {
            Dragging = true;
            StartDragPosition = MousePosition;
            MouseLeftButtonDown = true;
            OnMouseDown();
        }
        else if (Input.GetMouseButton(1))
        {
            if (Dragging)
            {
                MouseLeftButtonDown = true;
                OnMouseDrag();
            }
            else
            {
                MouseLeftButtonDown = true;
                OnMouseUp();
                Dragging = false;
            }
        }
        else if (Input.GetMouseButtonUp(1))
        {
            MouseLeftButtonDown = true;
            OnMouseUp();
            Dragging = false;
        }        //Cursor
        RenderCursor();

    }

    GameObject AddVertex(Vector3 pos)
    {
        GameObject prefab = Resources.Load<GameObject>("Vertex");
        GameObject obj = Instantiate(prefab, pos, Quaternion.identity, VertexObject.transform);
        Vertex vtx = obj.GetComponent<Vertex>();
        vtx.XY.x = pos.x;
        vtx.XY.y = pos.y;
        //vtx.Z=-2f; //プリセット
        vtx.VertexName = new string(LastVertexName);//名前をつける
        vtx.ID = LastVertexID;
        LastVertexName[0]++;
        LastVertexID++;
        return obj;
    }

    GameObject AddHLine()
    {
        GameObject prefab = Resources.Load<GameObject>("HLine");
        GameObject obj = Instantiate(prefab, Vector3.zero, Quaternion.identity, HLineObject.transform);
        HLine hl = obj.GetComponent<HLine>();
        hl.HLineName = new string(LastHLineName);//名前をつける
        LastHLineName[0]++;
        hl.ID = LastHLineID;
        LastHLineID++;
        return obj;
    }

    GameObject AddHCircle()
    {
        GameObject prefab = Resources.Load<GameObject>("HCircle");
        GameObject obj = Instantiate(prefab, Vector3.zero, Quaternion.identity, HCircleObject.transform);
        HCircle hci = obj.GetComponent<HCircle>();
        hci.HCircleName = new string(LastHLineName);//名前をつける
        LastHLineName[0]++;
        hci.ID = LastHCircleID;
        LastHCircleID++;
        return obj;
    }

    GameObject AddHModule()
    {
        GameObject prefab = Resources.Load<GameObject>("HModule");
        GameObject obj = Instantiate(prefab, Vector3.zero, Quaternion.identity, HModuleObject.transform);
        HModule hmd = obj.GetComponent<HModule>();
        hmd.ID = LastHModuleID;
        LastHModuleID++;
        return obj;
    }

    GameObject AddLog()
    {
        GameObject prefab = Resources.Load<GameObject>("GameLog");
        GameObject obj = Instantiate(prefab, new Vector3(6.5f, 0f, -1f), Quaternion.identity, LogFolder.transform);
        World.LogList.Add(obj);
        GameLog gml = obj.GetComponent<GameLog>();
        gml.ID = LastGameModuleID;
        gml.Active = true;
        LastGameModuleID ++ ;
        return obj;
    }

    private void OnMouseDown()
    {
        NullDrag = false;
        NotDragged = true;
        GameLogDrag = false;
        PreferenceDrag = false;
        vtxs = FindObjectsOfType<Vertex>();
        bool isInWorld = (MousePosition.magnitude < 0.99f);
        float min = 5f;
        Preferences PRF = PreferenceObject.GetComponent<Preferences>();
        if (PRF.PointInPreferenceDialog(MouseAbsolutePosition.x, MouseAbsolutePosition.y))
        {
            PreferenceDrag = true;
            PreferenceWindowLeftStart = PRF.WindowLeft;
            PreferenceWindowTopStart = PRF.WindowTop;
        }
        // 右側のログをドラッグ開始したらGameLogDragをtrueに
        GameLog PushedGameLog = PushedLog(MouseAbsolutePosition);
        if(PreferenceDrag == false && PushedGameLog != null){
            GameLogDrag = true;
            Log0YStart = World.Log0Y;
        }
        for (int i = 0; i < vtxs.Length; i++)
        {
            float dist = (vtxs[i].XY.x - MousePosition.x) * (vtxs[i].XY.x - MousePosition.x) +
                (vtxs[i].XY.y - MousePosition.y) * (vtxs[i].XY.y - MousePosition.y);//magnitudeを使いたい。
            if (dist < min)
            {
                min = dist;
                DraggedVertex = vtxs[i];
            }
        }
        //Debug.Log(min);
        if (min > 0.05f * 0.05f)//ここは検証の余地あり。
        {
            DraggedVertex = null;
            if (isInWorld)
            {
                for (int i = 0; i < vtxs.Length; i++)
                {
                    vtxs[i].StartDragX = vtxs[i].XY.x;
                    vtxs[i].StartDragY = vtxs[i].XY.y;
                }
                NullDrag = true;
            }
        }
        //DraggedVertexの親objectを知りたい。
        //直線の上でマウスダウンしているようだったら、そのGameobjectをClickedLineとして登録しておく。
        min = 5f;
        hlns = FindObjectsOfType<HLine>();
        for (int i = 0; i < hlns.Length; i++)
        {
            float dist1 = 0, dist = 5f;
            if (hlns[i].HL.R > 50)
            {
                dist1 = Mathf.Abs(hlns[i].HL.X * MousePosition.x + hlns[i].HL.Y * MousePosition.y);
                dist = dist1 / Mathf.Sqrt(hlns[i].HL.X * hlns[i].HL.X + hlns[i].HL.Y * hlns[i].HL.Y);
            }
            else
            {
                dist1 = Mathf.Sqrt((hlns[i].HL.X - MousePosition.x) * (hlns[i].HL.X - MousePosition.x) +
                    (hlns[i].HL.Y - MousePosition.y) * (hlns[i].HL.Y - MousePosition.y));
                dist = Mathf.Abs(dist1 - hlns[i].HL.R);
            }
            if (dist < min)
            {
                min = dist;
                ClickedLine = hlns[i];
            }
        }
        if (min > 0.05f)//どの直線とも離れていたら、「直線をクリック」とは認められない。
        {
            ClickedLine = null;
        }
        //円の上でマウスダウンしているようだったら、そのGameobjectをClickedLineとして登録しておく。
        min = 5f;
        hcls = FindObjectsOfType<HCircle>();
        for (int i = 0; i < hcls.Length; i++)
        {
            // 双曲距離ベース
            //float dist1 = HTransform.GetHDistanceOfTwoPoints(MousePosition.x, MousePosition.y,
            //    hcls[i].HCR.HX, hcls[i].HCR.HY);
            //float dist = Mathf.Abs(dist1 - hcls[i].HCR.HR);
            //ユークリッド距離ベース　　　こちらがいいか。
            float dist1 = Mathf.Sqrt((MousePosition.x-hcls[i].HCR.EX) * (MousePosition.x - hcls[i].HCR.EX) +
                (MousePosition.y - hcls[i].HCR.EY) * (MousePosition.y - hcls[i].HCR.EY));
            float dist = Mathf.Abs(dist1- hcls[i].HCR.ER);
            if (dist < min)
            {
                min = dist;
                ClickedCircle = hcls[i];
            }
        }
        if (min > 0.05f)//どの円とも離れていたら、「円をクリック」とは認められない。
        {
            ClickedCircle = null;
        }

    }

    private void OnMouseDrag()
    {
        if((MousePosition - StartDragPosition).magnitude >= 0.05f)
        {
            NotDragged = false;
        }
        if (DraggedVertex != null)
        {
            float d = MousePosition.magnitude;
            if (d > 0.99f)//頂点をドラッグしたまま画面外へ行った場合。
            {
                DraggedVertex.XY.x = MousePosition.x * 0.99f / d;
                DraggedVertex.XY.y = MousePosition.y * 0.99f / d;
            }
            else if (d < 0.01f)//中心に近いとき
            {
                DraggedVertex.XY.x = 0f;
                DraggedVertex.XY.y = 0f;
            }
            else
            {
                DraggedVertex.XY.x = MousePosition.x;
                DraggedVertex.XY.y = MousePosition.y;
            }
        }
        else if (NullDrag)//空ドラッグ-> 画面全体のスライド
        {
            if (StartDragPosition != MousePosition && MousePosition.magnitude < 0.99f)
            {
                HypPoint Pstart = new HypPoint(StartDragPosition.x, StartDragPosition.y);
                HypPoint Pend = new HypPoint(MousePosition.x, MousePosition.y);

                if (HTransform.GetEDistanceOfTwoPoints(Pstart, Pend) > 0.01f) { 
                    HypLine L1 = HTransform.GetHLineThruTwoPoints(Pstart, Pend);
                    HypLine L2 = HTransform.GetHBisectorOfTwoPoints(Pstart, Pend);
                    HypLine L3 = HTransform.GetHPerpendicularThruAPoint(L1, Pend);
                    for (int i = 0; i < vtxs.Length; i++)
                    {
                        HypPoint P = new HypPoint(vtxs[i].StartDragX, vtxs[i].StartDragY);
                        HypPoint P2 = HTransform.GetInversionAlongHLine(L2, P);
                        HypPoint P3 = HTransform.GetInversionAlongHLine(L3, P2);
                        if (P3.InWorld())
                        {
                            vtxs[i].XY.x = P3.GetX();
                            vtxs[i].XY.y = P3.GetY();
                        }
                        //else
                        //{
                        //    Debug.Log("avoid error");
                        //}
                    }
                }
            }
        }
        else if (GameLogDrag)
        {//ログをドラッグする。 //上下の動きに応じてログの表示を動かす。
            World.Log0Y = Log0YStart + (MousePosition.y - StartDragPosition.y)*World.Scale;
        }
        else if (PreferenceDrag)
        {// プリファレンスのダイアログの場所を移動する。
            Preferences PRF = PreferenceObject.GetComponent<Preferences>();
            PRF.WindowLeft = PreferenceWindowLeftStart + (MousePosition.x - StartDragPosition.x) * World.Scale * 0.5f *  Screen.width / World.Width;
            PRF.WindowTop = PreferenceWindowTopStart - (MousePosition.y - StartDragPosition.y) * World.Scale * 0.5f * Screen.height / World.Height;
        }
    }

    private void OnMouseUp()
    {
        if ((MousePosition - StartDragPosition).magnitude < 0.05f && NotDragged)//クリック
        {
            GameObject prefab = Resources.Load<GameObject>("ClickSpark");
            GameObject obj = Instantiate<GameObject>(prefab, Vector3.zero, Quaternion.identity);
            ClickSpark ClickSparkObj = obj.GetComponent<ClickSpark>();
            obj.transform.position = MouseAbsolutePosition;
            //メニューボタンを押しているかどうかを先に判定
            string PushedMenuButton = PushedMenu(MouseAbsolutePosition);
            //ログを押しているかどうかを先に判定
            GameLog PushedGameLog = PushedLog(MouseAbsolutePosition);

            if (PushedMenuButton != "")
            {
                Debug.Log(PushedMenuButton);
                //Debug.Log("メニューボタン"+PushedMenuButton+"が押された！");
                if (PushedMenuButton == "MenuOn")//メニューオンをおしたらメニュー表示
                {
                    WorldObject.GetComponent<World>().MenuOnButtons();
                }
                else if (PushedMenuButton == "MenuOff")//メニューオフをおしたらメニューを消す
                {
                    WorldObject.GetComponent<World>().MenuOffButtons();
                    WorldSound.pitch = Mathf.Pow(0.5f, 3f / 12f)/2f;
                }
                else if (PushedMenuButton == "AddPoint")//点追加をおしたらメニューを消してADD_POINTモードに。
                {
                    WorldObject.GetComponent<World>().MenuOffButtons();
                    World.Mode = MODE.ADD_POINT;
                    WorldSound.pitch = Mathf.Pow(0.5f, 3f / 12f) / 2f;
                }
                else if (PushedMenuButton == "AddCrossing")//点追加をおしたらメニューを消してADD_POINTモードに。
                {
                    WorldObject.GetComponent<World>().MenuOffButtons();
                    World.Mode = MODE.ADD_CROSSING;
                    WorldSound.pitch = Mathf.Pow(0.5f, 3f / 12f) / 2f;
                }
                else if (PushedMenuButton == "AddMidPoint")//点追加をおしたらメニューを消してADD_POINTモードに。
                {
                    WorldObject.GetComponent<World>().MenuOffButtons();
                    World.Mode = MODE.ADD_MIDPOINT;
                    WorldSound.pitch = Mathf.Pow(0.5f, 3f / 12f) / 2f;
                }
                else if (PushedMenuButton == "AddLine")//線追加をおしたらメニューを消してADD_LINEモードに。
                {
                    WorldObject.GetComponent<World>().MenuOffButtons();
                    World.Mode = MODE.ADD_LINE;
                    World.Phase = 0;// ADD_LINEモードのフェーズ0
                    WorldSound.pitch = Mathf.Pow(0.5f, 4f / 12f) / 2f;
                }
                else if (PushedMenuButton == "AddCircle")//円追加をおしたらメニューを消してADD_CIRCLEモードに。
                {
                    WorldObject.GetComponent<World>().MenuOffButtons();
                    World.Mode = MODE.ADD_CIRCLE;
                    World.Phase = 0;// ADD_CIRCLEモードのフェーズ0
                    WorldSound.pitch = Mathf.Pow(0.5f, 2f / 12f) / 2f;
                }
                //2列目
                else if (PushedMenuButton == "PointToPoint")//P2Pをおしたらメニューを消してP2Pモードに。
                {
                    WorldObject.GetComponent<World>().MenuOffButtons();
                    World.Mode = MODE.POINT_TO_POINT;
                    World.Phase = 0;// POINT_TO_POINTモードのフェーズ0
                    WorldSound.pitch = Mathf.Pow(0.5f, 5f / 12f) / 2f;
                }
                else if (PushedMenuButton == "PointToLine")//P2Lをおしたらメニューを消してP2Lモードに。
                {
                    WorldObject.GetComponent<World>().MenuOffButtons();
                    World.Mode = MODE.POINT_TO_LINE;
                    World.Phase = 0;// POINT_TO_LINEモードのフェーズ0
                    WorldSound.pitch = Mathf.Pow(0.5f, 5f / 12f) / 2f;
                }
                else if (PushedMenuButton == "PointToCircle")//P2Cをおしたらメニューを消してP2Cモードに。
                {
                    WorldObject.GetComponent<World>().MenuOffButtons();
                    World.Mode = MODE.POINT_TO_CIRCLE;
                    World.Phase = 0;// POINT_TO_CIRCLEモードのフェーズ0
                    WorldSound.pitch = Mathf.Pow(0.5f, 5f / 12f) / 2f;
                }
                //3列目
                else if (PushedMenuButton == "Isometry")//Isometryをおしたらメニューを消してIsometryモードに。
                {
                    WorldObject.GetComponent<World>().MenuOffButtons();
                    World.Mode = MODE.ISOMETRY;
                    World.Phase = 0;// ISOMETRYモードのフェーズ0
                    WorldSound.pitch = Mathf.Pow(0.5f, 7f / 12f) / 2f;
                }
                else if (PushedMenuButton == "Perpendicular")//Perpendicularをおしたらメニューを消してPerpendicularモードに。
                {
                    WorldObject.GetComponent<World>().MenuOffButtons();
                    World.Mode = MODE.PERPENDICULAR;
                    World.Phase = 0;// PERPENDICULARモードのフェーズ0
                    WorldSound.pitch = Mathf.Pow(0.5f, 7f / 12f) / 2f;
                }
                else if (PushedMenuButton == "Angle")//angleをおしたらメニューを消してParallelモードに。
                {
                    WorldObject.GetComponent<World>().MenuOffButtons();
                    World.Mode = MODE.ANGLE;
                    World.Phase = 0;// ANGLEモードのフェーズ0
                    WorldSound.pitch = Mathf.Pow(0.5f, 7f / 12f) / 2f;
                }
                //4列目
                else if (PushedMenuButton == "TangentC2L")//TangentC2Lをおしたらメニューを消してTangentC2Lモードに。
                {
                    WorldObject.GetComponent<World>().MenuOffButtons();
                    World.Mode = MODE.TANGENT_C2L;
                    World.Phase = 0;// フェーズ0
                    WorldSound.pitch = Mathf.Pow(0.5f, 8f / 12f) / 2f;
                }
                else if (PushedMenuButton == "TangentC2C")//TangentC2Cをおしたらメニューを消してTangentC2Cモードに。
                {
                    WorldObject.GetComponent<World>().MenuOffButtons();
                    World.Mode = MODE.TANGENT_C2C;
                    World.Phase = 0;// フェーズ0
                    WorldSound.pitch = Mathf.Pow(0.5f, 8f / 12f) / 2f;
                }
                //第6列
                else if (PushedMenuButton == "DeletePoint")// 点の消去
                {
                    WorldObject.GetComponent<World>().MenuOffButtons();
                    World.Mode = MODE.DELETE_POINT;
                    World.Phase = 0;// フェーズ0
                    WorldSound.pitch = Mathf.Pow(0.5f, 8f / 12f) / 2f;
                }
                else if (PushedMenuButton == "DeleteAll")// すべての消去
                {
                    ExecuteDeleteAll();
                    WorldSound.pitch = Mathf.Pow(0.5f, 8f / 12f) / 2f;
                }
                //第7列
                else if (PushedMenuButton == "Save")// ファイル保存
                {
                    WorldObject.GetComponent<World>().MenuOffButtons();
                    World.Mode = MODE.ADD_POINT;
                    ExecuteSave();
                    //ClickSparkObj.Pitch = 5f;
                }
                else if (PushedMenuButton == "Open")// ファイル読み込み
                {
                    WorldObject.GetComponent<World>().MenuOffButtons();
                    World.Mode = MODE.ADD_POINT;
                    ExecuteOpen();
                    //ClickSparkObj.Pitch = 5f;
                }
                else if (PushedMenuButton == "SaveTeX")// TeXファイルの保存
                {
                    //ExecuteSaveTeX();
                    //ClickSparkObj.Pitch = 5f;
                }
                else if (PushedMenuButton == "Quit")// 終了
                {
                    World.Mode = MODE.QUIT;
                    //ClickSparkObj.Pitch = 5f;
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_STANDALONE
                    UnityEngine.Application.Quit();
#endif
                }
            }
            else if (PushedGameLog != null)
            {
                //右クリック、またはログの右上の部分の左クリックだったら、設定ダイアログを出す。
                if (MouseLeftButtonDown || PushedLogUpperRight(MouseAbsolutePosition) != null)
                {
                    Preferences PRF = PreferenceObject.GetComponent<Preferences>();
                    PRF.show = true;
                    PRF.Text1 = PushedGameLog.Text1;
                    PRF.Text2 = PushedGameLog.Text2;
                    PRF.LogObject = PushedGameLog;
                    PRF.Object = PushedGameLog.ParentObj;
                    if(PushedGameLog.Mode == MODE.ADD_POINT)
                    {
                        Vertex VTX = PushedGameLog.ParentObj.GetComponent<Vertex>();
                        PRF.ObjectName = VTX.VertexName;
                        PRF.CoordX = ""+(Mathf.Round(1000f * VTX.XY.x) / 1000f);
                        PRF.CoordY = ""+(Mathf.Round(1000f * VTX.XY.y) / 1000f);
                    }
                    else if (PushedGameLog.Mode == MODE.ADD_LINE)
                    {
                        HLine HLN = PushedGameLog.ParentObj.GetComponent<HLine>();
                        PRF.ObjectName = HLN.HLineName;
                    }
                    else if (PushedGameLog.Mode == MODE.ADD_CIRCLE)
                    {
                        HCircle HCI = PushedGameLog.ParentObj.GetComponent<HCircle>();
                        PRF.ObjectName = HCI.HCircleName;
                    }
                }
                //ログが押されていたら、かつそれが点のログだったら、(親を)選択状態にする。
                Debug.Log(PushedGameLog.Text1);
                Vertex vtx;
                if (!Input.GetKey(KeyCode.RightShift) && !Input.GetKey(KeyCode.LeftShift)) {
                    for(int i=0;i<World.LogList.Count; i++)
                    {
                       GameLog GL = World.LogList[i].GetComponent<GameLog>();
                        if (GL == null)
                        {
                            Debug.Log(i+","+GL);
                        }
                        vtx = GL.ParentObj.GetComponent<Vertex>();
                        if (vtx != null)
                        {
                            vtx.Selected = false;
                        }
                    }
                }
                vtx = PushedGameLog.ParentObj.GetComponent<Vertex>();
                if (vtx != null)
                {
                    vtx.Selected = true;
                    //ClickSparkObj.Pitch = 6f;
                }
            }
            else if (PreferenceDrag)
            {
                ;
            }
            else if (World.Mode == MODE.ADD_POINT)
            {
                //ClickSparkObj.Pitch = 7f;
                //print("AddPoint starts.");
                ExecuteAddPoint();
            }
            else if (World.Mode == MODE.ADD_CROSSING)
            {
                //ClickSparkObj.Pitch = 7f;
                //print("AddCrossing starts.");
                ExecuteAddCrossing();
            }
            else if (World.Mode == MODE.ADD_MIDPOINT)
            {
                //ClickSparkObj.Pitch = 7f;
                //print("AddMidPoint starts.");
                ExecuteAddMidPoint();
            }
            else if (World.Mode == MODE.ADD_LINE)
            {
                //ClickSparkObj.Pitch = 8f;
                //print("AddLine starts.");
                ExecuteAddLine();
            }
            else if (World.Mode == MODE.ADD_CIRCLE)
            {
                //ClickSparkObj.Pitch = 9f;
                //print("AddCircle starts.");
                ExecuteAddCircle();
            }
            else if (World.Mode == MODE.POINT_TO_POINT)
            {
                //ClickSparkObj.Pitch = 10f;
                ExecutePointToPoint();
            }
            else if (World.Mode == MODE.POINT_TO_LINE)
            {
                //ClickSparkObj.Pitch = 10f;
                ExecutePointToLine();
            }
            else if (World.Mode == MODE.POINT_TO_CIRCLE)
            {
                //ClickSparkObj.Pitch = 10f;
                ExecutePointToCircle();
            }
            else if (World.Mode == MODE.ISOMETRY)
            {
                //ClickSparkObj.Pitch = 10f;
                ExecuteIsometry();
            }
            else if (World.Mode == MODE.PERPENDICULAR)
            {
                //ClickSparkObj.Pitch = 10f;
                ExecutePerpendicular();
            }
            else if (World.Mode == MODE.ANGLE)
            {
                ExecuteAngle();
            }
            else if (World.Mode == MODE.TANGENT_C2L)
            {
                //ClickSparkObj.Pitch = 10f;
                ExecuteTangentC2L();
            }
            else if (World.Mode == MODE.TANGENT_C2C)
            {
                //ClickSparkObj.Pitch = 10f;
                ExecuteTangentC2C();
            }
            else if (World.Mode == MODE.DELETE_POINT)
            {
                //ClickSparkObj.Pitch = 11f;
                ExecuteDeletePoint();
            }
        }
    }

    void ExecuteAddPoint()
    {
        if (DraggedVertex == null)// 既存の頂点をクリックしたわけではない
        {
            if (MousePosition.magnitude < 1f)//円の内側
                                             //if (MousePosition.x * MousePosition.x + MousePosition.y * MousePosition.y < 1f)//円の内側
            {
                GameObject Vtx = AddVertex(MousePosition);   //新規に頂点を追加する。
                GameObject Lg = AddLog();//新規にログを追加する
                Vtx.GetComponent<Vertex>().VertexLog = Lg;//頂点とログとを紐づけする
                Lg.GetComponent<GameLog>().ParentObj = Vtx;//頂点とログとを紐づけする
                if (ClickedLine != null)//直線の上をクリック//
                {
                    GameObject MD = AddHModule();//モジュールを追加
                    MD.GetComponent<HModule>().VA = Vtx;//点を登録
                    MD.GetComponent<HModule>().VB = ClickedLine.parent;//直線を登録
                    MD.GetComponent<HModule>().Mode = MODE.POINT_TO_LINE;//モードを登録
                    GameObject MDLg = AddLog();                            //新規にログを追加する
                    MDLg.GetComponent<GameLog>().Mode = MODE.POINT_TO_LINE; // ログのモード設定
                    MD.GetComponent<HModule>().HModuleLog = MDLg;  //モジュールとログとを紐づけする
                    MDLg.GetComponent<GameLog>().ParentObj = MD;  //ログとモジュールとを紐づけする
                }
                else if (ClickedCircle)//円の上をクリック//
                {
                    GameObject MD = AddHModule();//モジュールを追加
                    MD.GetComponent<HModule>().VA = Vtx;//点を登録
                    MD.GetComponent<HModule>().VB = ClickedCircle.parent;//円を登録
                    MD.GetComponent<HModule>().Mode = MODE.POINT_TO_CIRCLE;//モードを登録
                    GameObject MDLg = AddLog();                            //新規にログを追加する
                    MDLg.GetComponent<GameLog>().Mode = MODE.POINT_TO_CIRCLE; // ログのモード設定
                    MD.GetComponent<HModule>().HModuleLog = MDLg;  //モジュールとログとを紐づけする
                    MDLg.GetComponent<GameLog>().ParentObj = MD;  //ログとモジュールとを紐づけする
                }
                else
                {
                    SetAllVerticesUnselected();//該当頂点は選択、そのほかは選択解除
                    Vtx.GetComponent<Vertex>().Selected = true;//この文が無効になってしまう?（未解決）
                }
            }
        }
        else//既存の頂点をクリック
        {
            if (DraggedVertex.GetComponent<Vertex>().Selected)//選択されているものは選択解除
            {
                DraggedVertex.GetComponent<Vertex>().Selected = false;
            }
            else//選択されていない頂点をクリックした場合
            {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))// SHIFTキーが押されていれば、「選択追加」
                {
                    ;
                }
                else//SHIFTが押されていなければ、ほかの頂点の選択はキャンセル。
                {
                    SetAllVerticesUnselected();
                }
                DraggedVertex.GetComponent<Vertex>().Selected = true;
            }
        }
    }

    void ExecuteAddCrossing()
    {
        if(World.Phase == 0)
        {
            if(ClickedLine != null)
            {
                SelectedObject1 = ClickedLine.gameObject;
                World.Phase = 1;
            }
            else if(ClickedCircle != null)
            {
                SelectedObject1 = ClickedCircle.gameObject;
            World.Phase = 1;
            }
        }
        else if(World.Phase == 1)
        {
            SelectedObject2 = null;
            if (ClickedLine != null)
            {
                SelectedObject2 = ClickedLine.gameObject;
            }
            else if (ClickedCircle != null)
            {
                SelectedObject2 = ClickedCircle.gameObject;
            }
            if (SelectedObject2 != null)
            {
                //点を追加する
                GameObject NewVTX = AddVertex(MousePosition);//頂点を追加
                                                             //点のログを追加する
                GameObject Lg = AddLog();                            //新規にログを追加する
                NewVTX.GetComponent<Vertex>().VertexLog = Lg;  //頂点とログとを紐づけする
                Lg.GetComponent<GameLog>().ParentObj = NewVTX;  //頂点とログとを紐づけする
                                                                //モジュールを追加する
                GameObject MDa = AddHModule();
                MDa.GetComponent<HModule>().VA = NewVTX;
                MDa.GetComponent<HModule>().VB = SelectedObject1;//線を登録
                                                                 //モジュールのログを追加する
                GameObject MDaLg = AddLog();                            //新規にログを追加する
                MDa.GetComponent<HModule>().HModuleLog = MDaLg;  //頂点とログとを紐づけする
                MDaLg.GetComponent<GameLog>().ParentObj = MDa;  //頂点とログとを紐づけする
                if (SelectedObject1.GetComponent<HLine>() != null)
                {
                    MDa.GetComponent<HModule>().Mode = MODE.POINT_TO_LINE;//モードを登録
                    MDaLg.GetComponent<GameLog>().Mode = MODE.POINT_TO_LINE; // ログのモード設定
                }
                else if (SelectedObject1.GetComponent<HCircle>() != null)
                {
                    MDa.GetComponent<HModule>().Mode = MODE.POINT_TO_CIRCLE;//モードを登録
                    MDaLg.GetComponent<GameLog>().Mode = MODE.POINT_TO_CIRCLE; // ログのモード設定
                }
                else
                {
                    Debug.Log("Add crossing error. SelectedObject1 is illegal;");
                }
                //モジュールを追加する
                GameObject MDb = AddHModule();
                MDb.GetComponent<HModule>().VA = NewVTX;
                MDb.GetComponent<HModule>().VB = SelectedObject2;//線を登録
                                                                 //モジュールのログを追加する
                GameObject MDbLg = AddLog();                            //新規にログを追加する
                MDb.GetComponent<HModule>().HModuleLog = MDbLg;  //頂点とログとを紐づけする
                MDbLg.GetComponent<GameLog>().ParentObj = MDb;  //頂点とログとを紐づけする
                if (SelectedObject2.GetComponent<HLine>() != null)
                {
                    MDb.GetComponent<HModule>().Mode = MODE.POINT_TO_LINE;//モードを登録
                    MDbLg.GetComponent<GameLog>().Mode = MODE.POINT_TO_LINE; // ログのモード設定
                }
                else if (SelectedObject2.GetComponent<HCircle>() != null)
                {
                    MDb.GetComponent<HModule>().Mode = MODE.POINT_TO_CIRCLE;//モードを登録
                    MDbLg.GetComponent<GameLog>().Mode = MODE.POINT_TO_CIRCLE; // ログのモード設定
                }
                else
                {
                    Debug.Log("Add crossing error. SelectedObject2 is illegal;");
                }
                World.Phase = 0;
            }
        }
    }

    void ExecuteAddMidPoint()
    {
        if (World.Phase == 0)
        {
            if (DraggedVertex != null)
            {// 頂点をクリックした
                SelectedObject1 = DraggedVertex.gameObject;
            }
            else if (ClickedLine == null && ClickedCircle == null)
            {//何もないところをクリックした
                //点を追加する
                GameObject NewVTX = AddVertex(MousePosition);//頂点を追加
                SelectedObject1 = NewVTX;
                //点のログを追加する
                GameObject Lg = AddLog();                            //新規にログを追加する
                NewVTX.GetComponent<Vertex>().VertexLog = Lg;  //頂点とログとを紐づけする
                Lg.GetComponent<GameLog>().ParentObj = NewVTX;  //頂点とログとを紐づけする

            }
            World.Phase = 1;
        }
        else if(World.Phase == 1)
        {
            if (DraggedVertex != null)
            {// 頂点をクリックした
                SelectedObject2 = DraggedVertex.gameObject;
            }
            else if (ClickedLine == null && ClickedCircle == null)
            {//何もないところをクリックした
                //点を追加する
                GameObject NewVTX = AddVertex(MousePosition);//頂点を追加
                SelectedObject2 = NewVTX;
                //点のログを追加する
                GameObject Lg = AddLog();                            //新規にログを追加する
                NewVTX.GetComponent<Vertex>().VertexLog = Lg;  //頂点とログとを紐づけする
                Lg.GetComponent<GameLog>().ParentObj = NewVTX;  //頂点とログとを紐づけする
            }
            //点を追加する
            HypPoint P1 = new HypPoint(SelectedObject1.GetComponent<Vertex>().XY);
            HypPoint P2 = new HypPoint(SelectedObject2.GetComponent<Vertex>().XY);
            HypPoint P3 = HTransform.GetMidPointOfTwoPoint(P1, P2);
            SelectedObject3 = AddVertex(new Vector3(P3.GetX(), P3.GetY(), 0f));//頂点を追加
            //点のログを追加する
            GameObject VTXLg = AddLog();                            //新規にログを追加する
            SelectedObject3.GetComponent<Vertex>().VertexLog = VTXLg;  //頂点とログとを紐づけする
            VTXLg.GetComponent<GameLog>().ParentObj = SelectedObject3;  //頂点とログとを紐づけする
            //モジュールを追加する
            GameObject MD = AddHModule();
            MD.GetComponent<HModule>().VA = SelectedObject1;//第1の点を登録
            MD.GetComponent<HModule>().VB = SelectedObject2;//第2の点を登録
            MD.GetComponent<HModule>().VC = SelectedObject3;//第3の点を登録
            MD.GetComponent<HModule>().Mode = MODE.ADD_MIDPOINT;//モードを登録
            //モジュールのログを追加する
            GameObject MDLg = AddLog();                            //新規にログを追加する
            MDLg.GetComponent<GameLog>().Mode = MODE.ADD_MIDPOINT; // ログのモード設定
            MD.GetComponent<HModule>().HModuleLog = MDLg;  //頂点とログとを紐づけする
            MDLg.GetComponent<GameLog>().ParentObj = MD;  //頂点とログとを紐づけする
            World.Phase = 0;
        }
    }

    void ExecuteAddLine()
    {
        if (DraggedVertex == null)// 何もないところをクリックした
        {
            if (MousePosition.magnitude < 1f)//円の内側
            {
                if (World.Phase == 0)
                {
                    Debug.Log("Now Mode:AddLine, Phase 0");
                    //新規に頂点を追加して、これを「一つ目の頂点」にする
                    SelectedObject1 = AddVertex(MousePosition);//頂点を追加
                    GameObject Lg = AddLog();                            //新規にログを追加する
                    SelectedObject1.GetComponent<Vertex>().VertexLog = Lg;  //頂点とログとを紐づけする
                    Lg.GetComponent<GameLog>().ParentObj = SelectedObject1;  //頂点とログとを紐づけする
                    SetAllVerticesUnselected();
                    SelectedObject1.GetComponent<Vertex>().Selected = true;//なぜ無効？
                    World.Phase = 1;
                }
                else if (World.Phase == 1)
                {
                    Debug.Log("Now Mode:AddLine, Phase 1");
                    //新規に頂点を追加して、これを「２つ目の頂点」にする
                    SelectedObject2 = AddVertex(MousePosition);//頂点を追加
                    GameObject Lg = AddLog();                            //新規にログを追加する
                    SelectedObject2.GetComponent<Vertex>().VertexLog = Lg;  //頂点とログとを紐づけする
                    Lg.GetComponent<GameLog>().ParentObj = SelectedObject2;  //頂点とログとを紐づけする
                    SelectedObject2.GetComponent<Vertex>().Selected = true;//なぜ無効？
                    GameObject HL = AddHLine();//直線を追加
                    HL.GetComponent<HLine>().SetVertex(SelectedObject1, SelectedObject2);//直線を通る点を登録
                    GameObject HLLg = AddLog();                            //新規にログを追加する
                    HLLg.GetComponent<GameLog>().Mode = MODE.ADD_LINE; // ログのモード設定
                    HL.GetComponent<HLine>().HLineLog = HLLg;  //頂点とログとを紐づけする
                    HLLg.GetComponent<GameLog>().ParentObj = HL;  //頂点とログとを紐づけする
                    World.Phase = 0;//フェーズを戻す
                }
            }
        }
        else//既存の頂点をクリック
        {
            if (World.Phase == 0)
            {
                //「一つ目の頂点」にする
                Debug.Log("Now Mode:AddLine, Phase 0");
                //該当頂点は選択、そのほかは選択解除//未実装
                SelectedObject1 = FindParentOfVertex(DraggedVertex);//頂点を登録
                SetAllVerticesUnselected();
                DraggedVertex.Selected = true;
                World.Phase = 1;
            }
            else if (World.Phase == 1)
            {
                //新規に頂点を追加して、これを「２つ目の頂点」にする
                Debug.Log("Now Mode:AddLine, Phase 1");
                //該当頂点は選択、そのほかは選択解除//未実装
                SelectedObject2 = FindParentOfVertex(DraggedVertex);//頂点を登録
                if (SelectedObject1 != SelectedObject2)
                {
                    DraggedVertex.Selected = true;
                    GameObject HL = AddHLine();//直線を追加
                    HL.GetComponent<HLine>().SetVertex(SelectedObject1, SelectedObject2);//直線を通る点を登録
                    GameObject HLLg = AddLog();                            //新規にログを追加する
                    HLLg.GetComponent<GameLog>().Mode = MODE.ADD_LINE; // ログのモード設定
                    HL.GetComponent<HLine>().HLineLog = HLLg;  //頂点とログとを紐づけする
                    HLLg.GetComponent<GameLog>().ParentObj = HL;  //頂点とログとを紐づけする
                    World.Phase = 0;//フェーズを戻す
                }
            }
        }

    }

    void ExecuteAddCircle()
    {
        if (World.Phase == 0)
        {
            if (DraggedVertex != null)
            {
                //該当頂点は選択、そのほかは選択解除
                SelectedObject1 = FindParentOfVertex(DraggedVertex);//頂点を登録
                SetAllVerticesUnselected();
                DraggedVertex.Selected = true;
                World.Phase = 1;
            }
            else if (ClickedLine != null)
            {

            }
            else if (ClickedCircle != null)
            {

            }
            else
            {
                //新規に頂点を追加して、これを「一つ目の頂点」にする
                SelectedObject1 = AddVertex(MousePosition);//頂点を追加
                GameObject Lg = AddLog();                            //新規にログを追加する
                SelectedObject1.GetComponent<Vertex>().VertexLog = Lg;  //頂点とログとを紐づけする
                Lg.GetComponent<GameLog>().ParentObj = SelectedObject1;  //頂点とログとを紐づけする
                SetAllVerticesUnselected();
                SelectedObject1.GetComponent<Vertex>().Selected = true;//なぜ無効？
                World.Phase = 1;
            }
        }
        else if (World.Phase == 1)
        {
            if (DraggedVertex != null)
            {
                GameObject HC = AddHCircle();//円を追加
                HC.GetComponent<HCircle>().SetVertex(SelectedObject1);//円の中心を登録
                Vertex vtx = SelectedObject1.GetComponent<Vertex>();
                float radius = HTransform.GetHDistanceOfTwoPoints(new HypPoint(MousePosition.x, MousePosition.y), new HypPoint(vtx.XY.x, vtx.XY.y));
                HCircle Hci = HC.GetComponent<HCircle>();
                Hci.SetRadius(radius);//円の半径を登録

                GameObject HCLg = AddLog();                          //新規にログを追加する
                HCLg.GetComponent<GameLog>().Mode = MODE.ADD_CIRCLE; // ログのモード設定
                HC.GetComponent<HCircle>().HCircleLog = HCLg;  //頂点とログとを紐づけする
                HCLg.GetComponent<GameLog>().ParentObj = HC;  //頂点とログとを紐づけする
                
                GameObject MD = AddHModule();//モジュールを追加
                MD.GetComponent<HModule>().VA = FindParentOfVertex(DraggedVertex);//点を登録
                MD.GetComponent<HModule>().VB = HC;//円を登録
                MD.GetComponent<HModule>().Mode = MODE.POINT_TO_CIRCLE;//モードを登録
                GameObject MDLg = AddLog();                            //新規にログを追加する
                MDLg.GetComponent<GameLog>().Mode = MODE.POINT_TO_CIRCLE; // ログのモード設定
                MD.GetComponent<HModule>().HModuleLog = MDLg;  //頂点とログとを紐づけする
                MDLg.GetComponent<GameLog>().ParentObj = MD;  //頂点とログとを紐づけする
                World.Phase = 0;//フェーズを戻す
            }
            else if (ClickedLine != null)
            {

            }
            else if (ClickedCircle != null)
            {

            }
            else
            {
                GameObject HC = AddHCircle();//円を追加
                HC.GetComponent<HCircle>().SetVertex(SelectedObject1);//円の中心を登録
                Vertex vtx = SelectedObject1.GetComponent<Vertex>();
                float radius = HTransform.GetHDistanceOfTwoPoints(new HypPoint(MousePosition.x, MousePosition.y), new HypPoint(vtx.XY.x, vtx.XY.y));
                HC.GetComponent<HCircle>().SetRadius(radius);//円の半径を登録
                GameObject HCLg = AddLog();                            //新規にログを追加する
                HCLg.GetComponent<GameLog>().Mode = MODE.ADD_CIRCLE; // ログのモード設定
                HC.GetComponent<HCircle>().HCircleLog = HCLg;  //頂点とログとを紐づけする
                HCLg.GetComponent<GameLog>().ParentObj = HC;  //頂点とログとを紐づけする
                World.Phase = 0;//フェーズを戻す
            }
        }
    }

    void ExecutePointToPoint()
    {
        if (DraggedVertex != null)// 既存の頂点をクリックした
        {
            if (World.Phase == 0)//フェーズ０
            {
                //「一つ目の頂点」にする
                Debug.Log("Now Mode:P2P, Phase 0");
                //該当頂点は選択、そのほかは選択解除
                SelectedObject1 = FindParentOfVertex(DraggedVertex);//頂点を登録
                SetAllVerticesUnselected();
                DraggedVertex.Selected = true;
                World.Phase = 1;
            }
            else if (World.Phase == 1)
            {
                //新規に頂点を追加して、これを「２つ目の頂点」にする
                Debug.Log("Now Mode:AddLine, Phase 1");
                //該当頂点は選択、そのほかは選択解除
                SelectedObject2 = FindParentOfVertex(DraggedVertex);//頂点を登録
                DraggedVertex.Selected = true;
                GameObject MD = AddHModule();//モジュールを追加
                MD.GetComponent<HModule>().VA = SelectedObject1;//点を登録
                MD.GetComponent<HModule>().VB = SelectedObject2;//点を登録
                MD.GetComponent<HModule>().Mode = MODE.POINT_TO_POINT;//モードを登録
                GameObject MDLg = AddLog();                            //新規にログを追加する
                MDLg.GetComponent<GameLog>().Mode = MODE.POINT_TO_POINT; // ログのモード設定
                MD.GetComponent<HModule>().HModuleLog = MDLg;  //頂点とログとを紐づけする
                MDLg.GetComponent<GameLog>().ParentObj = MD;  //頂点とログとを紐づけする
                World.Phase = 0;//フェーズを戻す
            }
        }
    }

    void ExecutePointToLine()
    {
        if (DraggedVertex != null)// 既存の頂点をクリックした
        {
            if (World.Phase == 0)//フェーズ0のみ
            {
                //「一つ目の頂点」にする
                Debug.Log("Now Mode:P2P, Phase 0");
                //該当頂点は選択、そのほかは選択解除
                SelectedObject1 = FindParentOfVertex(DraggedVertex);//頂点を登録
                SetAllVerticesUnselected();
                SelectedObject1.GetComponent<Vertex>().Selected = true;//なぜ無効？
                World.Phase = 1;
            }
        }
        else if (ClickedLine != null)//既存の直線（のうち、頂点でないところ）をクリックした
        {
            if (World.Phase == 1)//フェーズ1のみ
            {
                Debug.Log("Now Mode:AddLine, Phase 1");
                SelectedObject2 = FindParentOfHLine(ClickedLine);//直線を登録
                //ClickedLine.Selected = true;//直線を選択状態に
                GameObject MD = AddHModule();//モジュールを追加
                MD.GetComponent<HModule>().VA = SelectedObject1;//点を登録
                MD.GetComponent<HModule>().VB = SelectedObject2;//直線を登録
                MD.GetComponent<HModule>().Mode = MODE.POINT_TO_LINE;//モードを登録
                GameObject MDLg = AddLog();                            //新規にログを追加する
                MDLg.GetComponent<GameLog>().Mode = MODE.POINT_TO_LINE; // ログのモード設定
                MD.GetComponent<HModule>().HModuleLog = MDLg;  //頂点とログとを紐づけする
                MDLg.GetComponent<GameLog>().ParentObj = MD;  //頂点とログとを紐づけする
                World.Phase = 0;//フェーズを戻す
            }
        }
        else// 何もないところをクリックした
        {
            if (World.Phase == 0)//フェーズ0のみ
            {
                if (MousePosition.magnitude < 1f)//円の内側
                {
                    Debug.Log("Now Mode:P2L, Phase 0");
                    //「一つ目の頂点」にする
                    SelectedObject1 = AddVertex(MousePosition);//頂点を追加
                    GameObject Lg = AddLog();                            //新規にログを追加する
                    Lg.GetComponent<GameLog>().Mode = MODE.ADD_POINT; // ログのモード設定
                    SelectedObject1.GetComponent<Vertex>().VertexLog = Lg;  //頂点とログとを紐づけする
                    Lg.GetComponent<GameLog>().ParentObj = SelectedObject1;  //頂点とログとを紐づけする
                    SetAllVerticesUnselected();
                    SelectedObject1.GetComponent<Vertex>().Selected = true;//なぜ無効？
                    World.Phase = 1;
                }
            }
        }
    }

    void ExecutePointToCircle()
    {
        if (World.Phase == 0)//フェーズごとに処理
        {
            if (DraggedVertex != null)// 既存の頂点をクリックした
            {
                Debug.Log("Now Mode:P2C, Phase 0");
                SelectedObject1 = FindParentOfVertex(DraggedVertex);//頂点を登録
                SetAllVerticesUnselected();
                SelectedObject1.GetComponent<Vertex>().Selected = true;//該当頂点を選択状態に//なぜ無効？
                World.Phase = 1;
            }
            else if (ClickedLine != null)
            {

            }
            else if(ClickedCircle != null)
            {

            }
            else
            {//何もないところをクリック
                SelectedObject1 = AddVertex(MousePosition);//頂点を追加
                GameObject Lg = AddLog();                            //新規にログを追加する
                Lg.GetComponent<GameLog>().Mode = MODE.ADD_POINT; // ログのモード設定
                SelectedObject1.GetComponent<Vertex>().VertexLog = Lg;  //頂点とログとを紐づけする
                Lg.GetComponent<GameLog>().ParentObj = SelectedObject1;  //頂点とログとを紐づけする
                SetAllVerticesUnselected();
                SelectedObject1.GetComponent<Vertex>().Selected = true;//なぜ無効？
                World.Phase = 1;
            }
        }
        else if (World.Phase == 1)
        {
            if (DraggedVertex != null)
            {
                ;
            }
            else if (ClickedLine != null)
            {
                ;
            }
            else if(ClickedCircle != null)
            {
                Debug.Log("Now Mode:P2C, Phase 1");
                SelectedObject2 = FindParentOfHCircle(ClickedCircle);//円を確定
                //ClickedCircle.Selected = true;//円を選択状態に
                GameObject MD = AddHModule();//モジュールを追加
                MD.GetComponent<HModule>().VA = SelectedObject1;//点を登録
                MD.GetComponent<HModule>().VB = SelectedObject2;//円を登録
                MD.GetComponent<HModule>().Mode = MODE.POINT_TO_CIRCLE;//モードを登録
                GameObject MDLg = AddLog();                            //新規にログを追加する
                MDLg.GetComponent<GameLog>().Mode = MODE.POINT_TO_CIRCLE; // ログのモード設定
                MD.GetComponent<HModule>().HModuleLog = MDLg;  //頂点とログとを紐づけする
                MDLg.GetComponent<GameLog>().ParentObj = MD;  //頂点とログとを紐づけする
                World.Phase = 0;//フェーズを戻す
            }
            else
            {
                ;
            }
        }
    }

    void ExecuteIsometry()
    {
        if (World.Phase == 0)
        {
            if (ClickedLine != null)//既存の直線（のうち、頂点でないところ）をクリックした
            {
                SelectedObject1 = FindParentOfHLine(ClickedLine);//直線を登録
                World.Phase = 1;
            }
        }
        else if(World.Phase == 1)
        {
            if (ClickedLine != null)//既存の直線（のうち、頂点でないところ）をクリックした
            {
                SelectedObject2 = FindParentOfHLine(ClickedLine);//直線を登録
                GameObject MD = AddHModule();//モジュールを追加
                MD.GetComponent<HModule>().VA = SelectedObject1;//直線１を登録
                MD.GetComponent<HModule>().VB = SelectedObject2;//直線２を登録
                MD.GetComponent<HModule>().Mode = MODE.ISOMETRY;//モードを登録
                GameObject MDLg = AddLog();                            //新規にログを追加する
                MDLg.GetComponent<GameLog>().Mode = MODE.ISOMETRY; // ログのモード設定
                MD.GetComponent<HModule>().HModuleLog = MDLg;  //頂点とログとを紐づけする
                MDLg.GetComponent<GameLog>().ParentObj = MD;  //頂点とログとを紐づけする
                World.Phase = 0;
            }
        }
    }

    void ExecutePerpendicular()
    {
        if (World.Phase == 0)
        {
            if (ClickedLine != null)//既存の直線（のうち、頂点でないところ）をクリックした
            {
                SelectedObject1 = FindParentOfHLine(ClickedLine);//直線を登録
                World.Phase = 1;
            }
        }
        else if (World.Phase == 1)
        {
            if (ClickedLine != null)//既存の直線（のうち、頂点でないところ）をクリックした
            {
                SelectedObject2 = FindParentOfHLine(ClickedLine);//直線を登録
                GameObject MD = AddHModule();//モジュールを追加
                MD.GetComponent<HModule>().VA = SelectedObject1;//直線１を登録
                MD.GetComponent<HModule>().VB = SelectedObject2;//直線２を登録
                MD.GetComponent<HModule>().Mode = MODE.PERPENDICULAR;//モードを登録
                GameObject MDLg = AddLog();                            //新規にログを追加する
                MDLg.GetComponent<GameLog>().Mode = MODE.PERPENDICULAR; // ログのモード設定
                MD.GetComponent<HModule>().HModuleLog = MDLg;  //頂点とログとを紐づけする
                MDLg.GetComponent<GameLog>().ParentObj = MD;  //頂点とログとを紐づけする
                World.Phase = 0;
            }
        }
    }

    void ExecuteAngle()
    {
        if (World.Phase == 0)
        {
            if ( DraggedVertex != null)//既存の点をクリックした
            {
                SelectedObject1 = FindParentOfVertex(DraggedVertex);//点を登録
                SetAllVerticesUnselected();
                SelectedObject1.GetComponent<Vertex>().Selected = true;//該当頂点を選択状態に
                World.Phase = 1;
            }
        }
        else if (World.Phase == 1)
        {
            if (DraggedVertex != null)//既存の点をクリックした
            {
                SelectedObject2 = FindParentOfVertex(DraggedVertex);//点を登録
                SelectedObject2.GetComponent<Vertex>().Selected = true;//該当頂点を選択状態に
                World.Phase = 2;
            }
        }
        else if (World.Phase == 2)
        {
            if (DraggedVertex != null)//既存の点をクリックした
            {
                SelectedObject3 = FindParentOfVertex(DraggedVertex);//点を登録
                SelectedObject3.GetComponent<Vertex>().Selected = true;//該当頂点を選択状態に
                //モジュールを作る
                GameObject MD = AddHModule();//モジュールを追加
                MD.GetComponent<HModule>().VA = SelectedObject1;//点１を登録
                MD.GetComponent<HModule>().VB = SelectedObject2;//点２を登録
                MD.GetComponent<HModule>().VC = SelectedObject3;//点３を登録
                MD.GetComponent<HModule>().ParaA = 45f;//パラメータAを登録//デフォルトは45度
                MD.GetComponent<HModule>().Mode = MODE.ANGLE;//モードを登録
                //ログを作る
                GameObject MDLg = AddLog(); //新規にログを追加する
                MDLg.GetComponent<GameLog>().Mode = MODE.ANGLE; // ログのモード設定
                MD.GetComponent<HModule>().HModuleLog = MDLg;  //頂点とログとを紐づけする
                MDLg.GetComponent<GameLog>().ParentObj = MD;  //頂点とログとを紐づけする
                World.Phase = 0;
            }
        }
    }

    void ExecuteTangentC2L()
    {
        if (World.Phase == 0 && ClickedCircle != null)
        {// フェーズ０で円をクリックした
            Debug.Log("Now Mode:TanC2L, Phase 0");
            SelectedObject1 = FindParentOfHCircle(ClickedCircle);//円を確定
            //ClickedCircle.Selected = true;//円を選択状態に
            World.Phase = 1;
        }
        else if (World.Phase == 1 && ClickedLine != null)
        {// フェーズ１で直線をクリックした
            Debug.Log("Now Mode:TanC2L, Phase 1");
            SelectedObject2 = FindParentOfHLine(ClickedLine);//円を確定
            //ClickedLine.Selected = true;// 直線を選択状態に
            GameObject MD = AddHModule();//モジュールを追加
            MD.GetComponent<HModule>().VA = SelectedObject1;//円を登録
            MD.GetComponent<HModule>().VB = SelectedObject2;//直線を登録
            MD.GetComponent<HModule>().Mode = MODE.TANGENT_C2L;//モードを登録
            GameObject MDLg = AddLog();                            //新規にログを追加する
            MDLg.GetComponent<GameLog>().Mode = MODE.TANGENT_C2L; // ログのモード設定
            MD.GetComponent<HModule>().HModuleLog = MDLg;  //頂点とログとを紐づけする
            MDLg.GetComponent<GameLog>().ParentObj = MD;  //頂点とログとを紐づけする
            World.Phase = 0;//フェーズを戻す        }
        }
    }

    void ExecuteTangentC2C()
    {
        if (World.Phase == 0 && ClickedCircle != null)
        {// フェーズ０で円をクリックした
            Debug.Log("Now Mode:TanC2C, Phase 0");
            SelectedObject1 = FindParentOfHCircle(ClickedCircle);//円を確定
            //ClickedCircle.Selected = true;//円を選択状態に
            World.Phase = 1;
        }
        else if (World.Phase == 1 && ClickedCircle != null)
        {// フェーズ１で直線をクリックした
            Debug.Log("Now Mode:TanC2L, Phase 1");
            SelectedObject2 = FindParentOfHCircle(ClickedCircle);//円を確定
            //ClickedCircle.Selected = true;// 円を選択状態に
            GameObject MD = AddHModule();//モジュールを追加
            MD.GetComponent<HModule>().VA = SelectedObject1;//円を登録
            MD.GetComponent<HModule>().VB = SelectedObject2;//直線を登録
            MD.GetComponent<HModule>().Mode = MODE.TANGENT_C2C;//モードを登録
            GameObject MDLg = AddLog();                            //新規にログを追加する
            MDLg.GetComponent<GameLog>().Mode = MODE.TANGENT_C2C; // ログのモード設定
            MD.GetComponent<HModule>().HModuleLog = MDLg;  //頂点とログとを紐づけする
            MDLg.GetComponent<GameLog>().ParentObj = MD;  //頂点とログとを紐づけする
            World.Phase = 0;//フェーズを戻す        }
        }
    }



    public static void ExecuteDeleteHLine(HLine LN)
    {
        HModule[] MDs = FindObjectsOfType<HModule>();
        for(int i = MDs.Length - 1; i >= 0; i--)
        {
            if(MDs[i].VA.GetComponent<HLine>() == LN)
            {
                MDs[i].Active = false;
                MDs[i].HModuleLog.GetComponent<GameLog>().Active = false;
            }
            else if (MDs[i].VB.GetComponent<HLine>() == LN)
            {
                MDs[i].Active = false;
                MDs[i].HModuleLog.GetComponent<GameLog>().Active = false;
            }
            else if (MDs[i].VC.GetComponent<HLine>() == LN)
            {
                MDs[i].Active = false;
                MDs[i].HModuleLog.GetComponent<GameLog>().Active = false;
            }
        }
        // LN のログをnonactiveに
        LN.HLineLog.GetComponent<GameLog>().Active = false ;
        // LNをnonactiveに
        LN.Active = false;
        DeleteAllNonactiveObjects();
        World.ClearLogList();
    }

    public static void ExecuteDeleteHCircle(HCircle CI)
    {
        HModule[] MDs = FindObjectsOfType<HModule>();
        for (int i = MDs.Length - 1; i >= 0; i--)
        {
            if (MDs[i].VA.GetComponent<HCircle>() == CI)
            {
                MDs[i].Active = false;
                MDs[i].HModuleLog.GetComponent<GameLog>().Active = false;
            }
            else if (MDs[i].VB.GetComponent<HCircle>() == CI)
            {
                MDs[i].Active = false;
                MDs[i].HModuleLog.GetComponent<GameLog>().Active = false;
            }
            else if (MDs[i].VC.GetComponent<HCircle>() == CI)
            {
                MDs[i].Active = false;
                MDs[i].HModuleLog.GetComponent<GameLog>().Active = false;
            }
        }
        // CI のログをnonactiveに
        CI.HCircleLog.GetComponent<GameLog>().Active = false;
        // CIをnonactiveに
        CI.Active = false;
        DeleteAllNonactiveObjects();
        World.ClearLogList();
    }

    public static void ExecuteDeleteHModule(HModule MD)
    {
        // MD のログをnonactiveに
        MD.HModuleLog.GetComponent<GameLog>().Active = false;
        // CIをnonactiveに
        MD.Active = false;
        //モジュールの種類によっては、さらに消去すべき案件がある。
        // MODE.PERPENDICULAR : 直角のマークを消す
        // MODE.ISOMETRY : 等長のカラーを消す
        // MODE.POINT_TO_LINE : （引くかどうかはわからぬが）補助延長線を消す
        DeleteAllNonactiveObjects();
        World.ClearLogList();
    }


    public static void ExecuteDeletePoint()
    {
        GameObject[] objs;
        if (DraggedVertex != null)//既存の点をクリックした
        {
            Debug.Log("DeletePoint starts.");
            DraggedVertex.Active = false;
            DraggedVertex.VertexLog.GetComponent<GameLog>().Active = false;
            //GameObject obj = FindParentOfVertex(DraggedVertex);
            ////この点を使っているHLine, HCircle, HModuleをすべて消す
            bool cont = false;
            do
            {
                print("Loop starts");
                cont = false;
                objs = FindObjectsOfType<GameObject>();
                for (int i = 0; i< objs.Length; i++)
                {
                    if (objs[i].name.Contains("HLine(Clone)"))
                    {
                        HLine HL = objs[i].GetComponent<HLine>();
                        if (HL.Active)
                        {
                            if (!HL.VA.GetComponent<Vertex>().Active || !HL.VB.GetComponent<Vertex>().Active)
                            {
                                print("missing line");
                                HL.HLineLog.GetComponent<GameLog>().Active = false;
                                HL.Active = false;
                                cont = true;
                            }
                        }
                    }
                }
                objs = FindObjectsOfType<GameObject>();
                for (int i = objs.Length - 1; i >= 0; i--)
                {
                    if (objs[i].name.Contains("HCircle(Clone)"))
                    {
                        HCircle CI = objs[i].GetComponent<HCircle>();
                        if (CI.Active)
                        {
                            print("missing circle");
                            CI.HCircleLog.GetComponent<GameLog>().Active = false;
                            CI.Active = false;
                            cont = true;
                        }
                    }
                }
                objs = FindObjectsOfType<GameObject>();
                for (int i = objs.Length - 1; i >= 0; i--)
                {
                    if (objs[i].name.Contains("HModule(Clone)"))
                    {
                        HModule MD = objs[i].GetComponent<HModule>();
                        if (MD.Active)
                        {
                            if (!BranchIsActive(MD.VA) || !BranchIsActive(MD.VB) || !BranchIsActive(MD.VC))
                            {
                                print("missing module");
                                MD.HModuleLog.GetComponent<GameLog>().Active = false;
                                MD.Active = false;
                                cont = true;
                            }
                        }
                    }
                }
            }
            while (cont) ;
            DeleteAllNonactiveObjects();
            //ログの並びを整理する
            World.ClearLogList();
        }
    }

    public static void DeleteAllNonactiveObjects()
    {
        GameObject[] objs = FindObjectsOfType<GameObject>();
        for (int i = objs.Length - 1; i >= 0; i--)
        {
            if (objs[i].name.Contains("Vertex(Clone)"))
            {
                if (!objs[i].GetComponent<Vertex>().Active)
                {
                    Destroy(objs[i]);
                }
            }
            if (objs[i].name.Contains("HLine(Clone)"))
            {
                if (!objs[i].GetComponent<HLine>().Active)
                {
                    Destroy(objs[i]);
                }
            }
            if (objs[i].name.Contains("HCircle(Clone)"))
            {
                if (!objs[i].GetComponent<HCircle>().Active)
                {
                    Destroy(objs[i]);
                }
            }
            else if (objs[i].name.Contains("HModule(Clone)"))
            {
                if (!objs[i].GetComponent<HModule>().Active)
                {
                    Destroy(objs[i]);
                }
            }
            //消去対象となるオブジェクトのログも消去が必要
            if (objs[i].name.Contains("GameLog(Clone)"))
            {
                if (!objs[i].GetComponent<GameLog>().Active)
                {
                    Destroy(objs[i]);
                }
            }
        }
    }

    public static bool BranchIsActive(GameObject obj)
    {
        if (obj == null)
        {
            return true;
        }
        if (obj.GetComponent<Vertex>() != null)
        {
            return obj.GetComponent<Vertex>().Active;
        }
        else if(obj.GetComponent<HLine>() != null)
        {
            return obj.GetComponent<HLine>().Active;
        }
        else if (obj.GetComponent<HCircle>() != null)
        {
            return obj.GetComponent<HCircle>().Active;
        }
        return true;// must return false?
    }

    void ExecuteDeleteAll()
    {
        GameObject[] objs = FindObjectsOfType<GameObject>();
        for (int i = objs.Length - 1; i >= 0; i--)
        {
            if (objs[i].name.Contains("HCircle(Clone)"))
            {
                Destroy(objs[i]);
            }
        }
        objs = FindObjectsOfType<GameObject>();
        for (int i = objs.Length - 1; i >= 0; i--)
        {
            if (objs[i].name.Contains("HLine(Clone)"))
            {
                Destroy(objs[i]);
            }
        }
        objs = FindObjectsOfType<GameObject>();
        for (int i = objs.Length - 1; i >= 0; i--)
        {
            if (objs[i].name.Contains("HModule(Clone)"))
            {
                Destroy(objs[i]);
            }
        }
        objs = FindObjectsOfType<GameObject>();
        for (int i = objs.Length - 1; i >= 0; i--)
        {
            if (objs[i].name.Contains("Vertex(Clone)"))
            {
                Destroy(objs[i]);
            }
        }
        objs = FindObjectsOfType<GameObject>();
        for (int i = objs.Length - 1; i >= 0; i--)
        {
            if (objs[i].name.Contains("GameLog(Clone)"))
            {
                Destroy(objs[i]);
            }
        }
        vtxs = FindObjectsOfType<Vertex>();//からのデータをvtxsに入れておく
        World.LogList.Clear(); // ログリストをクリア
        WorldObject.GetComponent<World>().MenuOffButtons();
        World.Mode = MODE.ADD_POINT;
    }

    void ExecuteSave()
    {
        string path = Crosstales.FB.FileBrowser.SaveFile("Save a PointLine file","","PointLineSample.txt","txt");
        try
        {
            using (StreamWriter writer = new StreamWriter(path, false))
            {
                //Debug.Log("start savelog : logLength = " + LogLength);
                for (int i = 0; i < World.LogList.Count; i++)
                {
                    if (World.LogList[i].GetComponent<GameLog>().Active)
                    {
                        writer.WriteLine(World.LogList[i].GetComponent<GameLog>().ToString());
                    }
                }
                writer.Flush();
                writer.Close();
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
        //WorldObject.GetComponent<World>().MenuOffButtons();
        World.Mode = MODE.ADD_POINT;
    }

    void ExecuteOpen()
    {
        ExecuteDeleteAll();
        string path = Crosstales.FB.FileBrowser.OpenSingleFile("Open a PointLine file", "",  "txt");
        try
        {
            using (StreamReader reader = new StreamReader(path, false))
            {
                World.LogList.Clear();
                World.LogCount = 0;
                string str;
                int PId = -1, LId = -1, CId = -1, MId = -1;
                LastVertexName[0] = 'A';
                LastHLineName[0] = 'a';
                do
                {
                    str = reader.ReadLine();
                    if (str != null) 
                    {
                        GameLog GML = FromString(str);
                        //AddLog(GML);
                        if(GML != null) { 
                            if (GML.Mode  == MODE.ADD_POINT)
                            {
                                if (PId < GML.ID)  PId = GML.ID;
                            }
                            else if (GML.Mode == MODE.ADD_LINE)
                            {
                                if (LId < GML.ID) LId = GML.ID;
                            }
                            else if (GML.Mode == MODE.ADD_CIRCLE)
                            {
                                if (CId < GML.ID) CId = GML.ID;
                            }
                            else 
                            {
                                if (MId < GML.ID) MId = GML.ID;
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                while (str != null);
                reader.Close();
                // LastVertexName[0]を整える。（どうやるんだろう？）
                vtxs = MonoBehaviour.FindObjectsOfType<Vertex>();
                hlns = MonoBehaviour.FindObjectsOfType<HLine>();
                hcls = MonoBehaviour.FindObjectsOfType<HCircle>();
                LastVertexName[0] = (char)('A' + (PId + 1));
                LastHLineName[0] = (char)('a' + (LId + 1) + (CId + 1));
                //AppMgr.mds = MonoBehaviour.FindObjectsOfType<Module>();
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
        //Debug.Log("End of ExecuteOpen");
        //WorldObject.GetComponent<World>().MenuOffButtons();
        World.Mode = MODE.ADD_POINT;
    }

    void ExecuteSaveTeXFile()
    {
        string path = Crosstales.FB.FileBrowser.SaveFile("Save a PointLine file", "", "PointLineSample.tex", "tex");
        try
        {
            //using (StreamWriter writer = new StreamWriter(path, false))
            //{
            //    //Debug.Log("start savelog : logLength = " + LogLength);
            //    for (int i = 0; i < World.LogList.Count; i++)
            //    {
            //        if (World.LogList[i].GetComponent<GameLog>().Active)
            //        {
            //            writer.WriteLine(World.LogList[i].GetComponent<GameLog>().ToString());
            //        }
            //    }
            //    writer.Flush();
            //    writer.Close();
            //}
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
        WorldObject.GetComponent<World>().MenuOffButtons();
        World.Mode = MODE.ADD_POINT;

    }

    void SetAllVerticesUnselected()
    {
        vtxs = FindObjectsOfType<Vertex>();
        for (int i = 0; i < vtxs.Length; i++)
        {
            vtxs[i].Selected = false;
        }
    }

    GameObject FindParentOfVertex(Vertex V)
    {
        return V.gameObject;
    }

    GameObject FindParentOfHLine(HLine L)
    {
        return L.gameObject;
    }

    GameObject FindParentOfHCircle(HCircle C)
    {
        return C.gameObject;
    }

    string PushedMenu(Vector3 pos)
    {
        for (int i = 0; i < World.MenuList.Count; i++)
        {
            string result = World.MenuList[i].GetComponent<MenuButton>().MouseCursorInMenuButton(pos.x, pos.y);
            if (result != "")
            {
                return result;
            }
        }
        return "";
    }

    GameLog PushedLog(Vector3 pos)
    {
        for (int i = 0; i < World.LogList.Count; i++)
        {
            GameLog result = World.LogList[i].GetComponent<GameLog>().MouseCursorInGameLog(pos.x, pos.y);
            if (result != null)
            {
                return result;
            }
        }
        return null;
    }

    GameLog PushedLogUpperRight(Vector3 pos)
    {
        for (int i = 0; i < World.LogList.Count; i++)
        {
            GameLog result = World.LogList[i].GetComponent<GameLog>().MouseCursorInGameLogUpperRight(pos.x, pos.y);
            if (result != null)
            {
                return result;
            }
        }
        return null;
    }

    void RenderCursor()
    {
        Vector3 MouseAbsolutePosition = Cam.ScreenToWorldPoint(Input.mousePosition);
        MouseAbsolutePosition.z = 0f;
        Vector3 MousePosition = MouseAbsolutePosition / World.Scale;
        if (MousePosition.magnitude >= 0.99f) { return; }

        HTransform.GetHLineFromVector(
            MousePosition.x, MousePosition.y, 
            0f, 1f, 
            out float VX, out float VY, out float VR);
        CursorVLine.X = VX;
        CursorVLine.Y = VY;
        CursorVLine.R = VR;
        if (CursorVLine.R > 300f)//まっすぐすぎるので直径を描く
        {
            float xx = -CursorVLine.Y;
            float yy = CursorVLine.X;
            float mag = Mathf.Sqrt(xx * xx + yy * yy);
            xx *= (World.Scale / mag);
            yy *= (World.Scale / mag);
            for (int i = 0; i < CursorPosLength; i++)
            {
                float xxx = -xx + (2 * xx) * i / (CursorPosLength - 1);
                float yyy = -yy + (2 * yy) * i / (CursorPosLength - 1);
                float dr = 0.01f * (World.Scale * World.Scale - xxx * xxx - yyy * yyy) / World.Scale / World.Scale;
                CursorVPos[i].x = xxx;
                CursorVPos[i].y = yyy;
                CursorVPos[i].z = -1f;
                CursorVLineKs[i].value = dr * 2f * World.Scale;
            }
            CursorVLineAnim.keys = CursorVLineKs;
            CursorVLR.widthCurve = CursorVLineAnim;
        }
        else
        {
            float midT = Mathf.Atan2(CursorVLine.Y, CursorVLine.X);
            float widT = Mathf.Asin(1f / Mathf.Sqrt(CursorVLine.X * CursorVLine.X + CursorVLine.Y * CursorVLine.Y));
            for (int i = 0; i < CursorPosLength; i++)
            {
                float t = midT - widT + (2 * widT) * i / (CursorPosLength - 1);
                float xx = CursorVLine.X - CursorVLine.R * Mathf.Cos(t);
                float yy = CursorVLine.Y - CursorVLine.R * Mathf.Sin(t);
                float dr = 0.01f * (1f - xx * xx - yy * yy);
                CursorVPos[i].x = World.Scale * (CursorVLine.X - CursorVLine.R * Mathf.Cos(t));
                CursorVPos[i].y = World.Scale * (CursorVLine.Y - CursorVLine.R * Mathf.Sin(t));
                CursorVPos[i].z = -1f;
                CursorVLineKs[i].value = dr * 2f * World.Scale;
            }
            CursorVLineAnim.keys = CursorVLineKs;
            CursorVLR.widthCurve = CursorVLineAnim;
            for (int i = 0; i < CursorPosLength; i++)
            {
                CursorVLR.SetPosition(i, CursorVPos[i]);
            }
        }

        HTransform.GetHLineFromVector(MousePosition.x, MousePosition.y, 1f, 0f, out VX, out VY, out VR);
        CursorHLine.X = VX;
        CursorHLine.Y = VY;
        CursorHLine.R = VR;
        if (CursorHLine.R > 300f)//まっすぐすぎるので直径を描く
        {
            float xx = -CursorHLine.Y;
            float yy = CursorHLine.X;
            float mag = Mathf.Sqrt(xx * xx + yy * yy);
            xx *= (World.Scale / mag);
            yy *= (World.Scale / mag);
            for (int i = 0; i < CursorPosLength; i++)
            {
                float xxx = -xx + (2 * xx) * i / (CursorPosLength - 1);
                float yyy = -yy + (2 * yy) * i / (CursorPosLength - 1);
                float dr = 0.01f * (World.Scale * World.Scale - xxx * xxx - yyy * yyy) / World.Scale / World.Scale;
                CursorHPos[i].x = xxx;
                CursorHPos[i].y = yyy;
                CursorHPos[i].z = -1f;
                CursorHLineKs[i].value = dr * 2f * World.Scale;
            }
            CursorHLineAnim.keys = CursorHLineKs;
            CursorHLR.widthCurve = CursorHLineAnim;
        }
        else
        {
            float midT = Mathf.Atan2(CursorHLine.Y, CursorHLine.X);
            float widT = Mathf.Asin(1f / Mathf.Sqrt(CursorHLine.X * CursorHLine.X + CursorHLine.Y * CursorHLine.Y));
            for (int i = 0; i < CursorPosLength; i++)
            {
                float t = midT - widT + (2 * widT) * i / (CursorPosLength - 1);
                float xx = CursorHLine.X - CursorHLine.R * Mathf.Cos(t);
                float yy = CursorHLine.Y - CursorHLine.R * Mathf.Sin(t);
                float dr = 0.01f * (1f - xx * xx - yy * yy);
                CursorHPos[i].x = World.Scale * (CursorHLine.X - CursorHLine.R * Mathf.Cos(t));
                CursorHPos[i].y = World.Scale * (CursorHLine.Y - CursorHLine.R * Mathf.Sin(t));
                CursorHPos[i].z = -1f;
                CursorHLineKs[i].value = dr * 2f * World.Scale;
            }
            CursorHLineAnim.keys = CursorHLineKs;
            CursorHLR.widthCurve = CursorHLineAnim;
            for (int i = 0; i < CursorPosLength; i++)
            {
                CursorHLR.SetPosition(i, CursorHPos[i]);
            }
        }
    }

    GameLog FromString(string str)
    {// GameObject返しという設計もありうる。
        string[] item = str.Split(',');
        char[] name = { 'a', (char)0x00 };
        if (item[0] == "Point")
        {
            float vx = float.Parse(item[1]);
            float vy = float.Parse(item[2]);
            float vz = float.Parse(item[3]);// maybe 1
            Vector3 pos = new Vector3(vx, vy, -2);
            int id = int.Parse(item[4]);
            bool fxd = bool.Parse(item[5]);
            bool act = bool.Parse(item[6]);
            GameObject obj = AddVertex(pos);//新規に頂点を追加する。
            Vertex vtx = obj.GetComponent<Vertex>();
            vtx.ID = id;
            vtx.Fixed = fxd;
            vtx.Active = act;
            name[0] = (char)('A'+id);
            vtx.VertexName = (item.Length == 7) ? new string(name) : item[7];
            //pt.PTobject.GetComponent<Text>().text = pname;
            GameObject Lg = AddLog();//新規にログを追加する
            vtx.VertexLog = Lg;//頂点とログとを紐づけする
            Lg.GetComponent<GameLog>().ParentObj = obj;//頂点とログとを紐づけする
            return Lg.GetComponent<GameLog>();
        }
        else if (item[0] == "Line")
        {
            int o1 = int.Parse(item[1]);
            int o2 = int.Parse(item[2]);
            int id = int.Parse(item[3]);
            bool act = bool.Parse(item[4]);
            GameObject obj = AddHLine();
            HLine hln = obj.GetComponent<HLine>();
            GameObject[] objs = FindObjectsOfType<GameObject>();
            for (int i = 0; i < objs.Length; i++)
            {
                Vertex vtx = objs[i].GetComponent<Vertex>();
                if (vtx != null)
                {
                    if (vtx.ID == o1)
                    {
                        hln.VA = objs[i];
                    }
                    if (vtx.ID == o2)
                    {
                        hln.VB = objs[i];
                    }
                }
            }
            hln.ID = id;
            hln.Active = act;
            hln.HLineName = (item.Length == 5) ? "L" + (id - 1000) : item[5];
            GameObject Lg = AddLog();//新規にログを追加する
            Lg.GetComponent<GameLog>().Mode = MODE.ADD_LINE;
            hln.HLineLog = Lg;//頂点とログとを紐づけする
            Lg.GetComponent<GameLog>().ParentObj = obj;//頂点とログとを紐づけする
            return Lg.GetComponent<GameLog>();
        }
        else if (item[0] == "Circle")
        {
            int o1 = int.Parse(item[1]);
            float rad = float.Parse(item[2]);
            int id = int.Parse(item[3]);
            bool act = bool.Parse(item[4]);
            GameObject obj = AddHCircle();
            HCircle hci = obj.GetComponent<HCircle>();
            GameObject[] objs = FindObjectsOfType<GameObject>();
            for (int i = 0; i < objs.Length; i++)
            {
                Vertex vtx = objs[i].GetComponent<Vertex>();
                if (vtx != null)
                {
                    if (vtx.ID == o1)
                    {
                        hci.VA = objs[i];
                    }
                }
            }
            hci.HCR = new HypCircle();
            hci.SetRadius(rad);
            hci.ID = id;
            hci.Active = act;
            hci.HCircleName = (item.Length == 5) ? "C" + (id - 2000) : item[5];

            GameObject Lg = AddLog();//新規にログを追加する
            Lg.GetComponent<GameLog>().Mode = MODE.ADD_CIRCLE;
            hci.HCircleLog = Lg;//頂点とログとを紐づけする
            Lg.GetComponent<GameLog>().ParentObj = obj;//頂点とログとを紐づけする
            return Lg.GetComponent<GameLog>();
        }
        else if (item[0] == "Module")
        {
            //int mt = int.Parse(item[1]);
            int o1 = int.Parse(item[2]);
            int o2 = int.Parse(item[3]);
            int o3 = int.Parse(item[4]);
            int id = int.Parse(item[5]);
            bool act = bool.Parse(item[6]);
            //オブジェクト作成
            GameObject MD = AddHModule();
            HModule hmd = MD.GetComponent<HModule>();
            string mode = item[1];
            if (mode == "ADD_MIDPOINT")
            {
                hmd.Mode = MODE.ADD_MIDPOINT;
                hmd.VA = FindVertexByID(o1);
                hmd.VB = FindVertexByID(o2);
                hmd.VC = FindVertexByID(o3);
                hmd.ID = id;
                hmd.Active = act;
                //ログ作成
                GameObject Lg = AddLog();
                Lg.GetComponent<GameLog>().Mode = MODE.ADD_MIDPOINT; // ログのモード設定
                MD.GetComponent<HModule>().HModuleLog = Lg;  //頂点とログとを紐づけする
                Lg.GetComponent<GameLog>().ParentObj = MD;  //頂点とログとを紐づけする
                return Lg.GetComponent<GameLog>();
            }
            else if(mode == "ISOMETRY")
            {
            }
            else if (mode == "POINT_TO_POINT")
            {
            }
            else if (mode == "POINT_TO_LINE")
            {
            }
            else if (mode == "POINT_TO_CIRCLE")
            {
            }
            else if (mode == "PERPENDICULAR")
            {
            }
            else if (mode == "ISOMETRY")
            {
            }
            else if (mode == "TANGENT_C2L")
            {
            }
            else if (mode == "TANGENT_C2C")
            {
            }
            
            //    if (mt == MENU.LINES_PERPENDICULAR)
            //    {// 直交モジュールの時には直角マークを付ける。
            //        Util.AddAngleMark(o1, o2);
            //    }
            //    if (mt == MENU.POINT_ON_LINE)
            //    {// 点を直線上に、のときには補助線を付ける。
            //        Util.AddThinLine(o1, o2);
            //    }
            //    return log;
            //}
            //return new Log();
        }
        return null;
    }

    GameObject FindVertexByID(int ID)
    {
        Vertex[] VTXs = FindObjectsOfType<Vertex>();
        for(int i=0; i<VTXs.Length; i++)
        {
            if(VTXs[i].ID == ID)
            {
                return FindParentOfVertex(VTXs[i]);
            }
        }
        return null;
    }

    GameObject FindHLineByID(int ID)
    {
        HLine[] HLNs = FindObjectsOfType<HLine>();
        for (int i = 0; i < HLNs.Length; i++)
        {
            if (HLNs[i].ID == ID)
            {
                return FindParentOfHLine(HLNs[i]);
            }
        }
        return null;
    }

    GameObject FindHCircleByID(int ID)
    {
        HCircle[] HCIs = FindObjectsOfType<HCircle>();
        for (int i = 0; i < HCIs.Length; i++)
        {
            if (HCIs[i].ID == ID)
            {
                return FindParentOfHCircle(HCIs[i]);
            }
        }
        return null;
    }
}
