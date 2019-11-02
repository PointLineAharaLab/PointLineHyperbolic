using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Preferences : MonoBehaviour
{
    public float WindowLeft = 220;
    public float WindowTop = 10;
    // 200x300 px window will apear in the center of the screen.
    private Rect windowRect = new Rect(0,0 , 200f, 300f);
    // Only show it if needed.
    public bool show = false;
    public GUIStyle PreferenceStyle = null;
    public GUIStyle TextStyle = null;
    public GUIStyle TextFieldStyle = null;
    public GUIStyle ButtonStyle = null;

    public GameObject Object;
    public GameLog LogObject;
    public string Text1="----";
    public string Text2="----";

    //Vertexのオプション
    bool Fixed = false;
    public string ObjectName = "";
    public string CoordX = "", CoordY = "";

    // HLineのオプション
    bool Clipped = false;

    void Init()
    {

    }

    void OnGUI()
    {
        windowRect = new Rect(WindowLeft, WindowTop, 220f, 300f);
        if (show)
            windowRect = GUI.Window(0, windowRect, DialogWindow, "", PreferenceStyle);
    }

    // This is the actual window.
    void DialogWindow(int windowID)
    {
        float AlignX = 15;
        float AlignY = 20;
        float AlignStep = 30;
        float height = 28;
        float align = 60;
        float bigAlign = 72;
        float width = windowRect.width - 30;
        float halfAlign = width / 2f;
        if(LogObject.Mode == MODE.ADD_POINT)
        {
            Vertex VTX = Object.GetComponent<Vertex>();
            if (VTX == null) return;
            Fixed = VTX.Fixed;
            //
            GUI.Label(new Rect(AlignX, AlignY, width, height), Text1, TextStyle);
            AlignY += AlignStep;
            //
            GUI.Label(new Rect(AlignX, AlignY, width, height), "Name ", TextStyle);
            ObjectName = GUI.TextField(new Rect(AlignX+align, AlignY, width - align, height), ObjectName, TextFieldStyle);
            AlignY += AlignStep;
            // １列目
            GUI.Label(new Rect(AlignX, AlignY, windowRect.width - align, height), Text2, TextStyle);
            AlignY += AlignStep;

            GUI.Label(new Rect(AlignX, AlignY, width, height), "X :", TextStyle);
            CoordX = GUI.TextField(new Rect(AlignX+align, AlignY , width - align, height), CoordX, TextFieldStyle);
            AlignY += AlignStep;

            GUI.Label(new Rect(AlignX, AlignY, width, height), "Y : ", TextStyle);
            CoordY = GUI.TextField(new Rect(AlignX + align, AlignY, width - align, height), CoordY, TextFieldStyle);
            AlignY += AlignStep;
            // ２列目
            if (Fixed)
            {
                GUI.Label(new Rect(AlignX, AlignY, width, height), "Fixed ", TextStyle);
                if(GUI.Button(new Rect(AlignX + bigAlign, AlignY, width - bigAlign, height), "Unfixed", ButtonStyle)){
                    Fixed = VTX.Fixed = false;
                }
            }
            else
            {
                GUI.Label(new Rect(AlignX, AlignY, width, height), "Unfixed ", TextStyle);
                if (GUI.Button(new Rect(AlignX + bigAlign, AlignY, width - bigAlign, height), "Fixed", ButtonStyle)){
                    Fixed = VTX.Fixed = true;
                }
            }
            AlignY += AlignStep;

            if (GUI.Button(new Rect(AlignX, AlignY, width, height), "Delete", ButtonStyle))
            {
                Objects.DraggedVertex = VTX;
                Objects.ExecuteDeletePoint();
                show = false;
            }
            AlignY += AlignStep;

            if (GUI.Button(new Rect(AlignX, AlignY, halfAlign, height), "Cancel", ButtonStyle))
            {
                show = false;
            }
            if (GUI.Button(new Rect(AlignX + halfAlign, AlignY, halfAlign, height), "OK", ButtonStyle))
            {
                VTX.VertexName = ObjectName;
                VTX.XY.x = float.Parse(CoordX);
                VTX.XY.y = float.Parse(CoordY);
                show = false;
            }
        }
        else if(LogObject.Mode == MODE.ADD_LINE)
        {
            HLine HLN = Object.GetComponent<HLine>();
            if (HLN == null) return;
            Clipped = HLN.Clipped;

            GUI.Label(new Rect(AlignX, AlignY, width, height), Text1, TextStyle);
            AlignY += AlignStep;

            //
            GUI.Label(new Rect(AlignX, AlignY, width, height), "Name ", TextStyle);
            ObjectName = GUI.TextField(new Rect(AlignX + align, AlignY, width - align, height), ObjectName, TextFieldStyle);
            AlignY += AlignStep;

            GUI.Label(new Rect(AlignX, AlignY, width, height), Text2, TextStyle);
            AlignY += AlignStep;

            if (Clipped)
            {
                GUI.Label(new Rect(AlignX, AlignY, width, height), "Clipped", TextStyle);
                if(GUI.Button(new Rect(AlignX+halfAlign, AlignY, width- halfAlign, height), "Unclipped", ButtonStyle))
                   Clipped = HLN.Clipped = false;
            }
            else
            {
                GUI.Label(new Rect(AlignX, AlignY, width, height), "Unclipped", TextStyle);
                if (GUI.Button(new Rect(AlignX + halfAlign, AlignY, width - halfAlign, height), "Clipped", ButtonStyle))
                    Clipped = HLN.Clipped = true;
            }
            AlignY += AlignStep;

            if (GUI.Button(new Rect(AlignX, AlignY, width, height), "Delete", ButtonStyle))
            {
                Objects.ExecuteDeleteHLine(HLN);
                show = false;
            }
            AlignY += AlignStep;

            if (GUI.Button(new Rect(AlignX, AlignY, halfAlign, height), "Cancel", ButtonStyle))
            {
                show = false;
            }
            if (GUI.Button(new Rect(AlignX + halfAlign, AlignY, halfAlign, height), "OK", ButtonStyle))
            {
                HLN.HLineName = ObjectName;
                show = false;
            }
        }
        else if (LogObject.Mode == MODE.ADD_CIRCLE)
        {
            HCircle HCI = Object.GetComponent<HCircle>();
            if (HCI == null) return;

            GUI.Label(new Rect(AlignX, AlignY, width, height), Text1, TextStyle);
            AlignY += AlignStep;

            GUI.Label(new Rect(AlignX, AlignY, width, height), "Name ", TextStyle);
            ObjectName = GUI.TextField(new Rect(AlignX + align, AlignY, width - align, height), ObjectName, TextFieldStyle);
            AlignY += AlignStep;

            GUI.Label(new Rect(AlignX, AlignY, width, height), Text2, TextStyle);
            AlignY += AlignStep;

            if (GUI.Button(new Rect(AlignX, AlignY, width, height), "Delete", ButtonStyle))
            {
                Objects.ExecuteDeleteHCircle(HCI);
                show = false;
            }
            AlignY += AlignStep;

            if (GUI.Button(new Rect(AlignX, AlignY, halfAlign, height), "Cancel", ButtonStyle))
            {
                show = false;
            }
            if (GUI.Button(new Rect(AlignX + halfAlign, AlignY, halfAlign, height), "OK", ButtonStyle))
            {
                HCI.HCircleName = ObjectName;
                show = false;
            }
        }
        else 
        {
            HModule HMD = Object.GetComponent<HModule>();
            if(HMD != null) {
                GUI.Label(new Rect(AlignX, AlignY, width, height), Text1, TextStyle);
                AlignY += AlignStep;

                GUI.Label(new Rect(AlignX, AlignY, width, height), Text2, TextStyle);
                AlignY += AlignStep;

                if (GUI.Button(new Rect(AlignX, AlignY, width, height), "Delete", ButtonStyle))
                {
                    Objects.ExecuteDeleteHModule(HMD);
                    show = false;
                }
                AlignY += AlignStep;
            }
            if (GUI.Button(new Rect(AlignX, AlignY, halfAlign, height), "Cancel", ButtonStyle))
            {
                show = false;
            }
            if (GUI.Button(new Rect(AlignX + halfAlign, AlignY, halfAlign, height), "OK", ButtonStyle))
            {
                show = false;
            }
        }

    }

    // To open the dialogue from outside of the script.
    public void Open()
    {
        show = true;
    }

    public bool PointInPreferenceDialog(float x, float y)
    {
        float xx = Screen.width * 0.5f + x / World.Width * Screen.width * 0.5f;
        float yy = Screen.height * 0.5f - y / World.Height * Screen.height * 0.5f;
        if(show && WindowLeft < xx && xx < WindowLeft+220f && WindowTop < yy && yy < WindowTop + 300)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
