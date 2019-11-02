using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    int WorldPtNumber = 61;
    public Vector3[] pts;
    LineRenderer LR;
    static public float Scale = 5f;
    static public bool MenuOn = false;
    // Start is called before the first frame update
    public static float Width = 9f, Height = 6f;
    public static float StrokeWeight = 2.5f;


    //static public int Mode;
    public static MODE Mode;
    public static int Phase;
    public static List<GameObject> MenuList;
    public GameObject MenuObjects;
    public GameObject MessageOnWorldObject;
    public Material MenuOnMaterial = null;
    //第1列
    public Material MenuOffMaterial = null;
    public Material AddPointMaterial = null;
    public Material AddCrossingMaterial = null;
    public Material AddMidPointMaterial = null;
    public Material AddLineMaterial = null;
    public Material AddCircleMaterial = null;
    //第2列
    public Material P2PMaterial = null;
    public Material P2LMaterial = null;
    public Material P2CMaterial = null;
    //第3列
    public Material IsometryMaterial = null;
    public Material PerpendicularMaterial = null;
    public Material AngleMaterial = null;
    //第4列
    public Material TangentC2LMaterial = null;
    public Material TangentC2CMaterial = null;
    //第5列
    public Material DeletePointMaterial = null;
    public Material DeleteAllMaterial = null;
    //第6列
    public Material SaveMaterial = null;
    public Material OpenMaterial = null;
    public Material SaveTeXMaterial = null;
    public Material QuitMaterial = null;

    static public string MessageOnWorld;

    // ログ表示管理（旧LogFolder.cs）
    public static List<GameObject> LogList;
    public static int LogCount;
    public static float Log0Y = 4.5f;
    public static float  LogCenterX=6.6f;
    public static float  LogTopY = 4.5f;



    void Start()
    {
        LR = GetComponent<LineRenderer>();
        LR.positionCount = 61;
        pts = new Vector3[61];
        for (int i=0; i<WorldPtNumber; i++)
        {
            pts[i] = new Vector3(
                Scale * Mathf.Cos(2f * Mathf.PI * i / 60f),
                Scale * Mathf.Sin(2f * Mathf.PI * i / 60f),
                1f
                );
            LR.SetPosition(i, pts[i]);
        }
        Mode = MODE.ADD_POINT;
        MenuList = new List<GameObject>();
        MenuOffButtons();
        LogList = new List<GameObject>();


        //  Debug area
        // Debug area
    }

    void Update()
    {
        // Message
        if(Mode == MODE.ADD_POINT)
        {
            MessageOnWorld = MESSAGE_ON_WORLD.ADD_POINT_MESSAGE[0]; 
        }
        else if (Mode == MODE.ADD_CROSSING)
        {
            MessageOnWorld = MESSAGE_ON_WORLD.ADD_CROSSING_MESSAGE[Phase];
        }
        else if (Mode == MODE.ADD_MIDPOINT)
        {
            MessageOnWorld = MESSAGE_ON_WORLD.ADD_MIDPOINT_MESSAGE[Phase];
        }
        else if (Mode == MODE.ADD_LINE)
        {
            MessageOnWorld = MESSAGE_ON_WORLD.ADD_LINE_MESSAGE[Phase];
        }
        else if (Mode == MODE.ADD_CIRCLE)
        {
            MessageOnWorld = MESSAGE_ON_WORLD.ADD_CIRCLE_MESSAGE[Phase];
        }
        else if (Mode == MODE.POINT_TO_POINT)
        {
            MessageOnWorld = MESSAGE_ON_WORLD.POINT_TO_POINT_MESSAGE[Phase];
        }
        else if (Mode == MODE.POINT_TO_LINE)
        {
            MessageOnWorld = MESSAGE_ON_WORLD.POINT_TO_LINE_MESSAGE[Phase];
        }
        else if (Mode == MODE.POINT_TO_CIRCLE)
        {
            MessageOnWorld = MESSAGE_ON_WORLD.POINT_TO_CIRCLE_MESSAGE[Phase];
        }
        else if (Mode == MODE.ISOMETRY)
        {
            MessageOnWorld = MESSAGE_ON_WORLD.ISOMETRY_MESSAGE[Phase];
        }
        else if (Mode == MODE.PERPENDICULAR)
        {
            MessageOnWorld = MESSAGE_ON_WORLD.PERPENDICULAR_MESSAGE[Phase];
        }
        else if (Mode == MODE.ANGLE)
        {
            MessageOnWorld = MESSAGE_ON_WORLD.ANGLE_MESSAGE[Phase];
        }
        else if (Mode == MODE.TANGENT_C2L)
        {
            MessageOnWorld = MESSAGE_ON_WORLD.TANGENT_C2L_MESSAGE[Phase];
        }
        else if (Mode == MODE.TANGENT_C2C)
        {
            MessageOnWorld = MESSAGE_ON_WORLD.TANGENT_C2C_MESSAGE[Phase];
        }
        else if (Mode == MODE.DELETE_POINT)
        {
            MessageOnWorld = MESSAGE_ON_WORLD.DELETE_POINT_MESSAGE[0];
        }
        else
        {
            MessageOnWorld = "";
        }
        MessageOnWorldObject.GetComponent<TextMesh>().text = "[ " + MESSAGE_ON_WORLD.MODES[(int)Mode] + " ] " + MessageOnWorld;
        GameLogUpdate();
        ModuleUpdate();
    }

    public void MenuOffButtons()
    {
        for(int i=MenuList.Count-1; i>=0; i--)
        {
            Destroy(MenuList[i]);
        }
        MenuList.Clear();
        GameObject prefab = Resources.Load<GameObject>("MenuButton");
        GameObject obj = Instantiate<GameObject>(prefab, Vector3.zero, Quaternion.identity, MenuObjects.transform);
        MenuButton MB = obj.GetComponent<MenuButton>();
        MB.SetMenuName("MenuOn", MenuOnMaterial, -7f, 4.5f);
        MenuList.Add(obj);
        MenuOn = false;
    }

    float GetMenuButtonPosX(int X)
    {
        float offsetX = -7;
        return offsetX + 1.5f * X;
    }
    float GetMenuButtonPosY(int Y)
    {
        float offsetY = 4.5f;
        return offsetY - 1.5f * Y;
    }

    public void MenuOnButtons()
    {
        for (int i = MenuList.Count - 1; i >= 0; i--)
        {
            Destroy(MenuList[i]);
        }
        MenuList.Clear();
        //第1列
        GameObject prefab = Resources.Load<GameObject>("MenuButton");
        GameObject obj = Instantiate<GameObject>(prefab, Vector3.zero, Quaternion.identity, MenuObjects.transform);
        MenuButton MB = obj.GetComponent<MenuButton>();
        MB.SetMenuName("MenuOff", MenuOffMaterial, GetMenuButtonPosX(0), GetMenuButtonPosY(0));
        MenuList.Add(obj);

        prefab = Resources.Load<GameObject>("MenuButton");
        obj = Instantiate<GameObject>(prefab, Vector3.zero, Quaternion.identity, MenuObjects.transform);
        MB = obj.GetComponent<MenuButton>();
        MB.SetMenuName("AddPoint", AddPointMaterial, GetMenuButtonPosX(1), GetMenuButtonPosY(0));
        MenuList.Add(obj);

        prefab = Resources.Load<GameObject>("MenuButton");
        obj = Instantiate<GameObject>(prefab, Vector3.zero, Quaternion.identity, MenuObjects.transform);
        MB = obj.GetComponent<MenuButton>();
        MB.SetMenuName("AddCrossing", AddCrossingMaterial, GetMenuButtonPosX(2), GetMenuButtonPosY(0));
        MenuList.Add(obj);

        prefab = Resources.Load<GameObject>("MenuButton");
        obj = Instantiate<GameObject>(prefab, Vector3.zero, Quaternion.identity, MenuObjects.transform);
        MB = obj.GetComponent<MenuButton>();
        MB.SetMenuName("AddMidPoint", AddMidPointMaterial, GetMenuButtonPosX(3), GetMenuButtonPosY(0));
        MenuList.Add(obj);

        prefab = Resources.Load<GameObject>("MenuButton");
        obj = Instantiate<GameObject>(prefab, Vector3.zero, Quaternion.identity, MenuObjects.transform);
        MB = obj.GetComponent<MenuButton>();
        MB.SetMenuName("AddLine", AddLineMaterial, GetMenuButtonPosX(4), GetMenuButtonPosY(0));
        MenuList.Add(obj);

        prefab = Resources.Load<GameObject>("MenuButton");
        obj = Instantiate<GameObject>(prefab, Vector3.zero, Quaternion.identity, MenuObjects.transform);
        MB = obj.GetComponent<MenuButton>();
        MB.SetMenuName("AddCircle", AddCircleMaterial, GetMenuButtonPosX(5), GetMenuButtonPosY(0));
        MenuList.Add(obj);
        //第2列
        prefab = Resources.Load<GameObject>("MenuButton");
        obj = Instantiate<GameObject>(prefab, Vector3.zero, Quaternion.identity, MenuObjects.transform);
        MB = obj.GetComponent<MenuButton>();
        MB.SetMenuName("PointToPoint", P2PMaterial, GetMenuButtonPosX(0), GetMenuButtonPosY(1));
        MenuList.Add(obj);

        prefab = Resources.Load<GameObject>("MenuButton");
        obj = Instantiate<GameObject>(prefab, Vector3.zero, Quaternion.identity, MenuObjects.transform);
        MB = obj.GetComponent<MenuButton>();
        MB.SetMenuName("PointToLine", P2LMaterial, GetMenuButtonPosX(1), GetMenuButtonPosY(1));
        MenuList.Add(obj);

        prefab = Resources.Load<GameObject>("MenuButton");
        obj = Instantiate<GameObject>(prefab, Vector3.zero, Quaternion.identity, MenuObjects.transform);
        MB = obj.GetComponent<MenuButton>();
        MB.SetMenuName("PointToCircle", P2CMaterial, GetMenuButtonPosX(2), GetMenuButtonPosY(1));
        MenuList.Add(obj);
        //第3列
        prefab = Resources.Load<GameObject>("MenuButton");
        obj = Instantiate<GameObject>(prefab, Vector3.zero, Quaternion.identity, MenuObjects.transform);
        MB = obj.GetComponent<MenuButton>();
        MB.SetMenuName("Isometry", IsometryMaterial, GetMenuButtonPosX(0), GetMenuButtonPosY(2));
        MenuList.Add(obj);

        prefab = Resources.Load<GameObject>("MenuButton");
        obj = Instantiate<GameObject>(prefab, Vector3.zero, Quaternion.identity, MenuObjects.transform);
        MB = obj.GetComponent<MenuButton>();
        MB.SetMenuName("Perpendicular", PerpendicularMaterial, GetMenuButtonPosX(1), GetMenuButtonPosY(2));
        MenuList.Add(obj);

        prefab = Resources.Load<GameObject>("MenuButton");
        obj = Instantiate<GameObject>(prefab, Vector3.zero, Quaternion.identity, MenuObjects.transform);
        MB = obj.GetComponent<MenuButton>();
        MB.SetMenuName("Angle", AngleMaterial, GetMenuButtonPosX(2), GetMenuButtonPosY(2));
        MenuList.Add(obj);

        //第4列
        prefab = Resources.Load<GameObject>("MenuButton");
        obj = Instantiate<GameObject>(prefab, Vector3.zero, Quaternion.identity, MenuObjects.transform);
        MB = obj.GetComponent<MenuButton>();
        MB.SetMenuName("TangentC2L", TangentC2LMaterial, GetMenuButtonPosX(0), GetMenuButtonPosY(3));
        MenuList.Add(obj);

        prefab = Resources.Load<GameObject>("MenuButton");
        obj = Instantiate<GameObject>(prefab, Vector3.zero, Quaternion.identity, MenuObjects.transform);
        MB = obj.GetComponent<MenuButton>();
        MB.SetMenuName("TangentC2C", TangentC2CMaterial, GetMenuButtonPosX(1), GetMenuButtonPosY(3));
        MenuList.Add(obj);

        //第6列
        prefab = Resources.Load<GameObject>("MenuButton");
        obj = Instantiate<GameObject>(prefab, Vector3.zero, Quaternion.identity, MenuObjects.transform);
        MB = obj.GetComponent<MenuButton>();
        MB.SetMenuName("DeletePoint", DeletePointMaterial, GetMenuButtonPosX(0), GetMenuButtonPosY(5));
        MenuList.Add(obj);

        prefab = Resources.Load<GameObject>("MenuButton");
        obj = Instantiate<GameObject>(prefab, Vector3.zero, Quaternion.identity, MenuObjects.transform);
        MB = obj.GetComponent<MenuButton>();
        MB.SetMenuName("DeleteAll", DeleteAllMaterial, GetMenuButtonPosX(1), GetMenuButtonPosY(5));
        MenuList.Add(obj);
        //第7列;
        prefab = Resources.Load<GameObject>("MenuButton");
        obj = Instantiate<GameObject>(prefab, Vector3.zero, Quaternion.identity, MenuObjects.transform);
        MB = obj.GetComponent<MenuButton>();
        MB.SetMenuName("Save", SaveMaterial, GetMenuButtonPosX(0), GetMenuButtonPosY(6));
        MenuList.Add(obj);

        prefab = Resources.Load<GameObject>("MenuButton");
        obj = Instantiate<GameObject>(prefab, Vector3.zero, Quaternion.identity, MenuObjects.transform);
        MB = obj.GetComponent<MenuButton>();
        MB.SetMenuName("Open", OpenMaterial, GetMenuButtonPosX(1), GetMenuButtonPosY(6));
        MenuList.Add(obj);

        prefab = Resources.Load<GameObject>("MenuButton");
        obj = Instantiate<GameObject>(prefab, Vector3.zero, Quaternion.identity, MenuObjects.transform);
        MB = obj.GetComponent<MenuButton>();
        MB.SetMenuName("SaveTeX", SaveTeXMaterial, GetMenuButtonPosX(2), GetMenuButtonPosY(6));
        MenuList.Add(obj);

        prefab = Resources.Load<GameObject>("MenuButton");
        obj = Instantiate<GameObject>(prefab, Vector3.zero, Quaternion.identity, MenuObjects.transform);
        MB = obj.GetComponent<MenuButton>();
        MB.SetMenuName("Quit", QuitMaterial, GetMenuButtonPosX(3), GetMenuButtonPosY(6));
        MenuList.Add(obj);

        MenuOn = true;
    }

    public static void GameLogUpdate()
    {
        LogCount = LogList.Count;
        //Debug.Log("LogCount =" + LogCount);
        LogCenterX = (5f + Width) * 0.5f;
        Vector3 pos = new Vector3(LogCenterX, 0f, -1f);
        for(int i=0; i<LogCount; i++)
        {
            pos.y = Log0Y - 1.0f * i;
            if(Log0Y< -LogTopY - 0.5f)// 下方向に消えないための
            {
                Log0Y = -LogTopY - 0.4f;
            }
            if (Log0Y - 1f * (LogCount - 1) > LogTopY + 0.5f)//上方向に消えないための
            {
                Log0Y = 1f * (LogCount - 1) + LogTopY + 0.4f;
            }
            //Debug.Log(Log0Y);
            if (-LogTopY - 0.5f < pos.y && pos.y < LogTopY + 0.5f)
            {
                LogList[i].transform.position = pos;
                LogList[i].GetComponent<GameLog>().Rendered = true;
            }
            else
            {
                //この瞬間に「表示をやめたい」のだが・・・あまりに原始的な方法・・・
                pos.y = -100f;
                LogList[i].transform.position = pos;
                LogList[i].GetComponent<GameLog>().Rendered = false;
            }
        }
    }

    public static void ModuleUpdate()
    {
        HModule[] MODs = FindObjectsOfType<HModule>();
        for (int repeat = 0; repeat < 20; repeat++)
        {
            for (int i = 0; i < MODs.Length; i++)
            {
                MODs[i].ModuleUpdate();
            }
        }
    }
    public static void ClearLogList()
    {
        //Debug.Log("ClearLogList()");
        LogList.Clear();
        GameObject[] objs = FindObjectsOfType<GameObject>();
        for(int i= objs.Length-1; i>=0; i--)// 裏技。なぜかこれでうまくいく。
        {
            if (objs[i].name.Contains("GameLog(Clone)") && objs[i].GetComponent<GameLog>().Active)
            {
                LogList.Add(objs[i]);
            }
        }
        GameLogUpdate();
    }
}

public enum MODE
{ 
    ADD_POINT=0,ADD_CROSSING, ADD_MIDPOINT, ADD_LINE, ADD_CIRCLE,
    POINT_TO_POINT,POINT_TO_LINE,POINT_TO_CIRCLE,
    ISOMETRY, PERPENDICULAR, ANGLE,
    TANGENT_C2L,TANGENT_C2C,
    DELETE_POINT,DELETE_ALL,
    SAVE, OPEN, SAVE_TEX, QUIT
}

public class MESSAGE_ON_WORLD
{
    static public string[] MODES = {//MODEの番号付けと同じ順番である必要がある。
        "Add Point", "Add Crossing", "Add MidPoint", "Add Line","Add Circle",
        "Point on Point","Point on Line","Point on Circle",
        "Isometry", "Perpendicular", "Angle",
        "Tan Circle on Line","Tan Circle on Circle",
        "Delete Point", "Delete All",
        "Save", "Open", "Save TeX", "Quit"
    };
    static public string[] ADD_POINT_MESSAGE = { "Click and Add a point" };
    static public string[] ADD_CROSSING_MESSAGE = { "Click an object", "Click another object" };
    static public string[] ADD_MIDPOINT_MESSAGE = { "Click a point", "Click another point" };
    static public string[] ADD_LINE_MESSAGE = { "Click a point", "Click another point" };
    static public string[] ADD_CIRCLE_MESSAGE = { "Click a point for center", "Click anywhere" };
    static public string[] ISOMETRY_MESSAGE = { "Click a line", "Click another line" };
    static public string[] PERPENDICULAR_MESSAGE = { "Click a line", "Click another line" };
    static public string[] ANGLE_MESSAGE = { "Click one point", "Click another point", "Click the last point" };
    static public string[] POINT_TO_POINT_MESSAGE = { "Click a point", "Click another point" };
    static public string[] POINT_TO_LINE_MESSAGE = { "Click a point", "Click a line" };
    static public string[] POINT_TO_CIRCLE_MESSAGE = { "Click a point", "Click a circle" };
    static public string[] TANGENT_C2L_MESSAGE = { "Click a circle", "Click a line" };
    static public string[] TANGENT_C2C_MESSAGE = { "Click a circle", "Click another circle" };
    static public string[] DELETE_POINT_MESSAGE = { "Click a point" };
}

