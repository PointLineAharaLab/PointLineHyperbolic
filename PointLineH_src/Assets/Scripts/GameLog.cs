using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLog : MonoBehaviour
{
    public MODE Mode;
    public float X, Y;
    public int ID;


    public Material PointLogMaterial;
    public Material LineLogMaterial;
    public Material CircleLogMaterial;
    public Material ModuleLogMaterial;
    public Material SelectedLogMaterial;

    public GameObject ParentObj;

    public GameObject LogText1, LogText2;
    public string Text1, Text2;

    public bool Selected = false;
    public bool Active = true;
    public bool Rendered = true;

    // Start is called before the first frame update
    void Start()
    {
        Text1 = MESSAGE_ON_WORLD.MODES[(int)Mode];
        Text2 = "----";
    }

    // Update is called once per frame
    void Update()
    {
        SetLogText();
        SetLogBoxColor();
        SetXY();
    }

    void SetXY()
    {
        Vector3 XY = this.transform.position;
        X = XY.x;
        Y = XY.y;
    }
    void SetLogText()
    {
        Vertex vtx;
        HLine ln;
        HCircle cl;
        HModule md;
        switch (Mode)
        {
            case MODE.ADD_POINT:
                vtx = ParentObj.GetComponent<Vertex>();
                Text1 = "Point " + vtx.VertexName;
                if (vtx.Fixed) Text1 = Text1 + " (Fixed)";
                Text2 = "( " + (Mathf.Round(1000f * vtx.XY.x) / 1000f) + ", " + (Mathf.Round(1000f * vtx.XY.y) / 1000f) + ")";
                Selected = vtx.Selected;
                break;
            case MODE.ADD_LINE:
                ln = ParentObj.GetComponent<HLine>();
                Text1 = "Line " + ln.HLineName;
                if (ln.Clipped)
                {
                    Text1 += ", Clipped";
                }
                Text2 = ln.HLineName+" = " +ln.VA.GetComponent<Vertex>().VertexName+""+ ln.VB.GetComponent<Vertex>().VertexName+"";
                break;
            case MODE.ADD_CIRCLE:
                cl = ParentObj.GetComponent<HCircle>();
                Text1 = "Circle " + cl.HCircleName;
                Text2 = cl.HCircleName + " = ( " + cl.VA.GetComponent<Vertex>().VertexName + " , " + (Mathf.Round(cl.HCR.HR*1000f)/1000f) + " )";
                break;
            case MODE.ADD_MIDPOINT:
                md = ParentObj.GetComponent<HModule>();
                Vertex P1 = md.VA.GetComponent<Vertex>();
                Vertex P2 = md.VB.GetComponent<Vertex>();
                Vertex P3 = md.VC.GetComponent<Vertex>();
                Text1 = "MidPoint " + P3.VertexName;
                Text2 = "MidPoint (" + P1.VertexName + ", " + P2.VertexName+")";
                    break;
            case MODE.POINT_TO_POINT:
                md = ParentObj.GetComponent<HModule>();
                Text1 = "Point to Point";
                Text2 = md.VA.GetComponent<Vertex>().VertexName + "=" + md.VB.GetComponent<Vertex>().VertexName;
                break;
            case MODE.POINT_TO_LINE:
                md = ParentObj.GetComponent<HModule>();
                Text1 = "Point on Line";
                Text2 = md.VA.GetComponent<Vertex>().VertexName + " is on " + md.VB.GetComponent<HLine>().HLineName;
                break;
            case MODE.POINT_TO_CIRCLE:
                md = ParentObj.GetComponent<HModule>();
                Text1 = "Point on Circle";
                Text2 = md.VA.GetComponent<Vertex>().VertexName + " is on " + md.VB.GetComponent<HCircle>().HCircleName;
                break;
            case MODE.ISOMETRY:
                md = ParentObj.GetComponent<HModule>();
                Text1 = "Isometry";
                Text2 = md.VA.GetComponent<HLine>().HLineName + " and " + md.VB.GetComponent<HLine>().HLineName;
                break;
            case MODE.PERPENDICULAR:
                md = ParentObj.GetComponent<HModule>();
                Text1 = "Perpendicular";
                Text2 = md.VA.GetComponent<HLine>().HLineName + " and " + md.VB.GetComponent<HLine>().HLineName;
                break;
            case MODE.ANGLE:
                md = ParentObj.GetComponent<HModule>();
                Text1 = "Angle:" + (Mathf.FloorToInt(md.ParaA*10f) / 10f) + " degree ";
                Text2 = md.VA.GetComponent<Vertex>().VertexName + "-" + md.VB.GetComponent<Vertex>().VertexName + "-" + md.VC.GetComponent<Vertex>().VertexName;
                break;
            case MODE.TANGENT_C2L:
                md = ParentObj.GetComponent<HModule>();
                Text1 = "Tan Circle to Line";
                Text2 = md.VA.GetComponent<HCircle>().HCircleName + " tangents to " + md.VB.GetComponent<HLine>().HLineName;
                break;
            case MODE.TANGENT_C2C:
                md = ParentObj.GetComponent<HModule>();
                Text1 = "Tan Circle to Circle";
                Text2 = md.VA.GetComponent<HCircle>().HCircleName + " tangents to " + md.VB.GetComponent<HCircle>().HCircleName;
                break;
        }
        LogText1.GetComponent<TextMesh>().text = Text1;
        LogText2.GetComponent<TextMesh>().text = Text2;
    }
    void SetLogBoxColor()
    {
        switch (Mode)
        {
            case MODE.ADD_POINT:
                if (Selected)
                {
                    GetComponent<MeshRenderer>().material = SelectedLogMaterial;
                }
                else
                {
                    GetComponent<MeshRenderer>().material = PointLogMaterial;
                }

                break;
            case MODE.ADD_LINE:
                GetComponent<MeshRenderer>().material = LineLogMaterial;
                break;
            case MODE.ADD_CIRCLE:
                GetComponent<MeshRenderer>().material = CircleLogMaterial;
                break;
            default:
                GetComponent<MeshRenderer>().material = ModuleLogMaterial;
                break;
        }
    }

    public GameLog MouseCursorInGameLog(float x, float y)
    {
        //Debug.Log(x+","+y);
        //Debug.Log(X+","+ Y);
        if (X - 1.5f < x && x < X + 1.5)
        {
            if (Y - 0.5f < y && y < Y + 0.5)
            {
                return this;
            }
        }
        return null;
    }

    public GameLog MouseCursorInGameLogUpperRight(float x, float y)
    {
        //Debug.Log(x+","+y);
        //Debug.Log(X+","+ Y);
        if (X + 1f < x && x < X + 1.5)
        {
            if (Y < y && y < Y + 0.5)
            {
                return this;
            }
        }
        return null;
    }

    public override string ToString()
    {
        if (Mode == MODE.ADD_POINT)
        {
        Vertex VTX = ParentObj.GetComponent<Vertex>();
            return "Point," + VTX.XY.x + "," + VTX.XY.y + "," + "1" + "," + VTX.ID + "," + VTX.Fixed + "," + Active + "," + VTX.VertexName;
        }
        else if (Mode == MODE.ADD_LINE)
        {
            HLine HLN = ParentObj.GetComponent<HLine>();
        int Id1 = HLN.VA.GetComponent<Vertex>().ID;
        int Id2 = HLN.VB.GetComponent<Vertex>().ID;
        return "Line," + Id1 + "," + Id2 + "," + HLN.ID + "," + Active+","+HLN.HLineName;
        }
        else if (Mode == MODE.ADD_CIRCLE)
        {
        HCircle HCI = ParentObj.GetComponent<HCircle>();
        int Id1 = HCI.VA.GetComponent<Vertex>().ID;
        return "Circle," + Id1 + "," + HCI.HCR.HR + "," + HCI.ID + "," + Active + ","+HCI.HCircleName;
        }
        else if (Mode == MODE.ADD_MIDPOINT)
        {
            HModule HMD = ParentObj.GetComponent<HModule>();
            return "Module," + Mode + "," + HMD.VA.GetComponent<Vertex>().ID
                + "," + HMD.VB.GetComponent<Vertex>().ID + "," + HMD.VC.GetComponent<Vertex>().ID + "," + HMD.ID + "," + Active;
        }
        else if (Mode == MODE.POINT_TO_POINT)
        {
            HModule HMD = ParentObj.GetComponent<HModule>();
            return "Module," + Mode + "," + HMD.VA.GetComponent<Vertex>().ID
                + "," + HMD.VB.GetComponent<Vertex>().ID + "," + "ID3" + "," + HMD.ID + "," + Active;
        }
        else if (Mode == MODE.POINT_TO_LINE)
        {
            HModule HMD = ParentObj.GetComponent<HModule>();
            return "Module," + Mode + "," + HMD.VA.GetComponent<Vertex>().ID
                + "," + HMD.VB.GetComponent<HLine>().ID + "," + "ID3" + "," + HMD.ID + "," + Active;
        }
        else if (Mode == MODE.POINT_TO_CIRCLE)
        {
            HModule HMD = ParentObj.GetComponent<HModule>();
            return "Module," + Mode + "," + HMD.VA.GetComponent<Vertex>().ID
                + "," + HMD.VB.GetComponent<HCircle>().ID + "," + "ID3" + "," + HMD.ID + "," + Active;
        }
        else if (Mode == MODE.ISOMETRY)
        {
            HModule HMD = ParentObj.GetComponent<HModule>();
            return "Module," + Mode + "," + HMD.VA.GetComponent<HLine>().ID
                + "," + HMD.VB.GetComponent<HLine>().ID + "," + "ID3" + "," + HMD.ID + "," + Active;
        }
        else if (Mode == MODE.PERPENDICULAR)
        {
            HModule HMD = ParentObj.GetComponent<HModule>();
            return "Module," + Mode + "," + HMD.VA.GetComponent<HLine>().ID
                + "," + HMD.VB.GetComponent<HLine>().ID + "," + "ID3" + "," + HMD.ID + "," + Active;
        }
        else if (Mode == MODE.TANGENT_C2L)
        { 
            HModule HMD = ParentObj.GetComponent<HModule>();
            return "Module," + Mode + "," + HMD.VA.GetComponent<HCircle>().ID 
                + "," + HMD.VB.GetComponent<HLine>().ID + "," + "ID3" + "," + HMD.ID + "," + Active ;
        }
        else if (Mode == MODE.TANGENT_C2C)
        {
            HModule HMD = ParentObj.GetComponent<HModule>();
            return "Module," + Mode + "," + HMD.VA.GetComponent<HCircle>().ID
                + "," + HMD.VB.GetComponent<HCircle>().ID + "," + "ID3" + "," + HMD.ID + "," + Active;
        }
        else
        {
            return "";
        }
    }

}
