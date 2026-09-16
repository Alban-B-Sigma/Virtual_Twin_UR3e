using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.XR.CoreUtils;
using Unity.VisualScripting;
// -------------------------------------------------------------------------------------------------------------------------------------- //
// This script manages the program creation, with the management of the Buttons creation in the program tree for each node and the        //
// management of node's save and stock in Lists and Dictionnaries, to be used in other scripts.                                           //
// -------------------------------------------------------------------------------------------------------------------------------------- //

public class ProgCreation : MonoBehaviour
{
    private VisualElement root; public Cobot_Command Cmd; public Transform ik; public Transform TCP; public Transform Base; private MOVEJv1 MvJ;

    private VisualElement MoveMenu; private VisualElement ProgMenu;
    private ScrollView ProgTree; private Foldout[] ListFoldout = new Foldout[4];
    private Foldout BasicMenu; private Foldout AdvancedMenu; private Foldout TemplatesMenu; private Foldout URCAPSMenu;

    private Button CloseNodeWindow;

    private VisualElement MoveNodeWindow;
    private DropdownField MoveType; private FloatField MoveSpeed; private FloatField MoveAccel;
    private Label MoveSpeedUnits; private Label MoveAccelUnits;
    private bool MoveSpeedChanging = false; private bool MoveAccelChanging = false;


    private VisualElement WpNodeWindow;
    private TextField WpName; private Button BindWp; private Button SetWpPose; private DropdownField WpList; private Button WpMoveHereButton;
    private Button ConfirmWpPose; private Button UndoWpPose;
    private bool WpNameChanging = false;


    private VisualElement GripNodeWindow;
    private FloatField GripWidth; private Slider GripForce; private Slider GripSpeed;
    private bool GripWidthChanging = false;


    private Button MoveNode; private Button WaypointNode; private Button GripNode; private Button ReleaseNode;
    private Button DelButton; private Button StartNode;

    public List<Button> ListNodes = new List<Button>();
    public Dictionary<string, float[]> MoveNodeDict = new Dictionary<string, float[]>(); // value [type : 0 for MoveJ, 1 for MoveL | speed (in right units) | acceleration (in right units) | number of waypoint nodes in it]
    public Dictionary<string, float[]> WpNodeDict = new Dictionary<string, float[]>();   // value [joints angles (6 floats) | pose of the tool (3 floats for position, 3 floats for orientation) | 1 float for the index of the MoveNode parent in the ListNodes]
    public Dictionary<string, float[]> TwoFGNodeDict = new Dictionary<string, float[]>(); // value [targeted Lenght, speed, force], force = 0 for Release nodes.
    // public Dictionary<string, string> WpNameDict = new Dictionary<string, string>();
    private int nbMoveNode; private int nbWpNode; private int nbGripNode; 
    const float FixNodeHeight = 50;
    private Vector3 Tab = new Vector3(80, 0, 0);
    private bool isCreatingNode = false; private bool onNodeClicked = false; private bool isDisplayInfo = false; private Button nodeSelected = null;
    
    void Start()
    {
        root = GetComponent<UIDocument>().rootVisualElement; MvJ = GetComponent<MOVEJv1>();
        MoveMenu = root.Q<VisualElement>("MoveMenu");       ProgMenu = root.Q<VisualElement>("ProgramMenu");
        ProgTree = root.Q<ScrollView>("ProgTree");  
        BasicMenu = root.Q<Foldout>("BasicMenu");           AdvancedMenu = root.Q<Foldout>("AdvancedMenu");
        TemplatesMenu = root.Q<Foldout>("TemplatesMenu");   URCAPSMenu = root.Q<Foldout>("URCapsMenu");
        ListFoldout[0] = BasicMenu; ListFoldout[1] = AdvancedMenu; ListFoldout[2] =  TemplatesMenu; ListFoldout[3] = URCAPSMenu;
        BasicMenu.RegisterCallback<ClickEvent>(FoldoutHideOrShow);      AdvancedMenu.RegisterCallback<ClickEvent>(FoldoutHideOrShow);
        TemplatesMenu.RegisterCallback<ClickEvent>(FoldoutHideOrShow);  URCAPSMenu.RegisterCallback<ClickEvent>(FoldoutHideOrShow);

        CloseNodeWindow = root.Q<Button>("CloseNodeWindow");
        CloseNodeWindow.RegisterCallback<ClickEvent>(CloseAllNodeWindow);
        
        MoveNode = root.Q<Button>("MoveButton"); WaypointNode = root.Q<Button>("WaypointButton");
        GripNode = root.Q<Button>("GripButton"); ReleaseNode = root.Q<Button>("ReleaseButton");
        DelButton = root.Q<Button>("DeleteButton");
        
        MoveNodeWindow = root.Q<VisualElement>("MoveNodeWindow");
        MoveType = root.Q<DropdownField>("MoveType"); MoveSpeed = root.Q<FloatField>("MoveJointSpeed"); MoveAccel = root.Q<FloatField>("MoveJointAccel");
        MoveSpeedUnits = root.Q<Label>("MoveSpeedUnits"); MoveAccelUnits = root.Q<Label>("MoveAccelUnits");
        MoveNodeWindow.visible = false; 
        MoveType.RegisterValueChangedCallback(MoveTypeChanged);
        MoveSpeed.RegisterValueChangedCallback(MoveSpeedChanged); MoveAccel.RegisterValueChangedCallback(MoveAccelChanged);
        // Initialisation in MoveJ type :
        MoveSpeed.value = 60f; MoveSpeedUnits.text = "°/s"; MoveAccel.value = 80f; MoveAccelUnits.text = "°/s²";

        WpNodeWindow = root.Q<VisualElement>("WpNodeWindow");
        WpName = root.Q<TextField>("WpName");               BindWp = root.Q<Button>("BindWp");          SetWpPose = root.Q<Button>("SetWpButton"); 
        WpList = root.Q<DropdownField>("WPlist");           WpMoveHereButton = root.Q<Button>("WpMoveHere");
        ConfirmWpPose = root.Q<Button>("ConfirmWpPose");    UndoWpPose = root.Q<Button>("UndoWpPose");
        WpName.RegisterValueChangedCallback(WpNameChanged);             SetWpPose.RegisterCallback<ClickEvent>(WpPoseSetting);
        BindWp.RegisterCallback<PointerUpEvent>(WpBinding);             WpList.RegisterValueChangedCallback(WpBindChanged);
        WpMoveHereButton.RegisterCallback<ClickEvent>(WpMoveHere);
        ConfirmWpPose.RegisterCallback<ClickEvent>(WpPoseConfirmed);    UndoWpPose.RegisterCallback<ClickEvent>(WpPoseNotSet);
        
        GripNodeWindow = root.Q<VisualElement>("GripNodeWindow");
        GripWidth = root.Q<FloatField>("GripWidth"); GripForce = root.Q<Slider>("GripForce"); GripSpeed = root.Q<Slider>("GripSpeed");
        GripNodeWindow.visible = false;
        GripWidth.RegisterValueChangedCallback(GripWidthChanged); 
        GripForce.RegisterValueChangedCallback(GripForceChanged); GripSpeed.RegisterValueChangedCallback(GripSpeedChanged);

        nbMoveNode = 0; nbWpNode = 0; nbGripNode = 0;
        
        StartNode = new Button{name = "StartRobotProgram", text = "Robot Program" };
        ProgTree.Add(StartNode); nodeSelected = StartNode;

        MoveNode.RegisterCallback<ClickEvent>(CreateMoveNode);
        WaypointNode.RegisterCallback<ClickEvent>(CreateWpNode);
        GripNode.RegisterCallback<ClickEvent>(CreateGripNode);
        ReleaseNode.RegisterCallback<ClickEvent>(CreateReleaseNode);

        DelButton.RegisterCallback<ClickEvent>(DelNode);
    }

    // Update is called once per frame
    void Update()
    {
        CloseNodeWindow.visible = MoveNodeWindow.visible || WpNodeWindow.visible || GripNodeWindow.visible;
        foreach(Button node in ListNodes)
        {
            if (nodeSelected == node)
            {node.style.backgroundColor = new Color(0.23f, 0.28f, 1f, 1f);}
            else
            {node.style.backgroundColor = new Color(0.737f, 0.737f, 0.737f, 1f);}
        }
        if (nodeSelected == StartNode) {StartNode.style.backgroundColor = new Color(0.23f, 0.28f, 1f, 1f);}
        else {StartNode.style.backgroundColor = new Color(0.737f, 0.737f, 0.737f, 1f);}

        if (onNodeClicked)
        {
            if (nodeSelected.name.Contains("Move"))
            {
                MoveNodeWindow.visible = true; WpNodeWindow.visible = false; GripNodeWindow.visible = false; GripForce.visible = false; WpList.visible = false;
                if (MoveSpeedChanging && Keyboard.current.enterKey.isPressed)
                {MoveNodeDict[nodeSelected.name][1] = MoveSpeed.value; MoveSpeedChanging = false;}
                if (MoveAccelChanging && Keyboard.current.enterKey.isPressed)
                {MoveNodeDict[nodeSelected.name][2] = MoveAccel.value; MoveAccelChanging = false;}
            }
            else if (nodeSelected.name.Contains("Wp"))
            {
                WpNodeWindow.visible = true; MoveNodeWindow.visible = false; GripNodeWindow.visible = false; GripForce.visible = false;
                if (WpNameChanging && Keyboard.current.enterKey.isPressed)
                {
                    float[] oldNameInfo = WpNodeDict[nodeSelected.text];
                    if (!WpNodeDict.Keys.Contains(WpName.value))
                    {
                        nodeSelected.text = WpName.value;
                        WpNodeDict[nodeSelected.text] = oldNameInfo;
                    }
                }
            }
            else if (nodeSelected.name.Contains("Grip") || nodeSelected.name.Contains("Release"))
            {
                if (nodeSelected.name.Contains("Release")) GripForce.visible = false;
                else GripForce.visible = true;

                GripNodeWindow.visible = true; MoveNodeWindow.visible = false; WpNodeWindow.visible = false; WpList.visible = false;
                if (GripWidthChanging && Keyboard.current.enterKey.isPressed)
                {
                    TwoFGNodeDict[nodeSelected.name][0] = GripWidth.value;
                }

            }
        }
        UpdateNodesPos(ProgTree);
    }

    void FoldoutHideOrShow(ClickEvent clickEvent)
    {
        if (!isCreatingNode)
        {
            foreach(Foldout menu in ListFoldout)
            {
                if (clickEvent.currentTarget == menu) {menu.value = true;}
                else{menu.value = false;}
            }
        }
    }

    void CloseAllNodeWindow(ClickEvent clickEvent)
    {
        onNodeClicked = false; MoveNodeWindow.visible = false; WpNodeWindow.visible = false; GripNodeWindow.visible = false; GripForce.visible = false;
    }
    
    void CreateMoveNode(ClickEvent clickEvent)
    {
        ExternalMoveNodeCreation();
    }
    private void ExternalMoveNodeCreation()
    {
        var nodeParent = nodeSelected.parent;
        if (nodeSelected.name.Contains("Move"))
        {
            nodeParent = nodeSelected.parent.parent;
        }
        
        isCreatingNode = true; nbMoveNode ++; nbWpNode ++;
        
        var newMoveNodeContainer = CreateMoveNodeContainer();
        nodeParent.Add(newMoveNodeContainer);
        
        newMoveNodeContainer.style.translate = Tab * GetNumberTab(nodeParent);
        

        var newMoveNode = new Button{name = "MoveNode"+nbMoveNode.ToString(), text = "MoveJ" };
        newMoveNode.style.height = FixNodeHeight;
        ListNodes.Add(newMoveNode);
        MoveNodeDict.Add(newMoveNode.name, new float[]{0, 60f, 80f, 1});
        
        newMoveNodeContainer.Add(newMoveNode); 
        
        newMoveNode.RegisterCallback<ClickEvent>(ModifNode);

        nodeSelected = newMoveNode; onNodeClicked = true;
        MoveType.index = (int)MoveNodeDict[nodeSelected.name][0];
        MoveSpeed.value = MoveNodeDict[nodeSelected.name][1]; MoveAccel.value = MoveNodeDict[nodeSelected.name][2];

        var newWpNode = new Button{name = "WpNode"+nbWpNode.ToString(), text = "Waypoint " + nbWpNode.ToString()};
        newWpNode.style.height = FixNodeHeight;
        newMoveNodeContainer.Add(newWpNode);
        newWpNode.style.translate = Tab * GetNumberTab(nodeParent); newWpNode.style.translate = Tab;
        newWpNode.RegisterCallback<ClickEvent>(ModifNode);
        ListNodes.Add(newWpNode); WpList.choices.Add(newWpNode.text);
        WpName.value = newWpNode.text;
        float[] Qwp = Cmd.GetAllJointAnglesFloat(); float[] pose = GetToolPose();
        WpNodeDict.Add(newWpNode.text, Qwp.Concat(pose).ToArray());
        
        Invoke(nameof(NodeCreationFinished), 0.1f);
    }
    void MoveTypeChanged(ChangeEvent<string> evt)
    {
        if (!isDisplayInfo)
        {
            nodeSelected.text = MoveType.value;
            if (MoveType.value == "MoveJ")
            {
                MoveSpeed.value = 60f; MoveSpeedUnits.text = "°/s"; MoveAccel.value = 80f; MoveAccelUnits.text = "°/s²";
                MoveNodeDict[nodeSelected.name][0] = 0;
            }
            else if (MoveType.value == "MoveL")
            {
                MoveSpeed.value = 250f; MoveSpeedUnits.text = "mm/s"; MoveAccel.value = 1200f; MoveAccelUnits.text = "mm/s²";
                MoveNodeDict[nodeSelected.name][0] = 1;
            }
            else if (MoveType.value == "MoveP")
            {
                MoveSpeed.value = 250f; MoveSpeedUnits.text = "mm/s"; MoveAccel.value = 1200f; MoveAccelUnits.text = "mm/s²";
                MoveNodeDict[nodeSelected.name][0] = 2;
            }
            else{Debug.Log("Move type uncorrect");}
            MoveNodeDict[nodeSelected.name][1] = MoveSpeed.value; MoveNodeDict[nodeSelected.name][2] = MoveAccel.value;
        }
    }
    void MoveSpeedChanged(ChangeEvent<float> evt) {MoveSpeedChanging = true;}
    void MoveAccelChanged(ChangeEvent<float> evt) {MoveAccelChanging = true;}
    

    void CreateWpNode(ClickEvent clickEvent)
    {
        isCreatingNode = true;
        if (nodeSelected.name.Contains("Move") || nodeSelected.name.Contains("Wp"))
        {
            var nodeParent = nodeSelected.parent;
            nbWpNode ++;

            var newWpNode = new Button{name = "WpNode"+nbWpNode.ToString(), text = "Waypoint " + nbWpNode.ToString()};
            newWpNode.style.height = FixNodeHeight;
            nodeParent.Add(newWpNode);
            newWpNode.style.translate = Tab * GetNumberTab(nodeParent); newWpNode.style.translate = Tab;

            newWpNode.RegisterCallback<ClickEvent>(ModifNode);
            
            ListNodes.Add(newWpNode); WpList.choices.Add(newWpNode.text);
            WpName.value = newWpNode.text;
            nodeSelected = newWpNode; onNodeClicked = true;

            float[] Qwp = Cmd.GetAllJointAnglesFloat(); float[] pose = GetToolPose();
            WpNodeDict.Add(newWpNode.text, Qwp.Concat(pose).ToArray());
        }
        else
        { ExternalMoveNodeCreation();}

        Invoke(nameof(NodeCreationFinished), 0.1f);
    }
    void WpNameChanged(ChangeEvent<string> evt) {WpNameChanging = true;}
    void WpBinding(PointerUpEvent evt) {WpList.visible = true;}
    void WpBindChanged(ChangeEvent<string> evt)
    {
        if (WpList.value != null) 
        {
            WpList.visible = false;
            nodeSelected.text = WpList.value; WpName.value = WpList.value;
        }
    }
    void WpPoseSetting(ClickEvent clickEvent)
    {ProgMenu.visible = false; MoveMenu.visible = true; ConfirmWpPose.visible = true; UndoWpPose.visible = true;}
    void WpPoseConfirmed(ClickEvent clickEvent)
    {
        ProgMenu.visible = true; MoveMenu.visible = false; ConfirmWpPose.visible = false; UndoWpPose.visible = false;
        float[] Qcurrent = Cmd.GetAllJointAnglesFloat();
        float[] CurrentPos = GetToolPose();
        WpNodeDict[nodeSelected.text] = Qcurrent.Concat(CurrentPos).ToArray();

    }
    void WpPoseNotSet(ClickEvent clickEvent)
    {ProgMenu.visible = true; MoveMenu.visible = false; ConfirmWpPose.visible = false; UndoWpPose.visible = false;}
    void WpMoveHere(ClickEvent clickEvent)
    {
        float[] WpInfo = WpNodeDict[nodeSelected.text];
        float[] Qtarget = new float[]{WpInfo[0], WpInfo[1], WpInfo[2], WpInfo[3], WpInfo[4], WpInfo[5]};
        float[] Qcurrent = Cmd.GetAllJointAnglesFloat();
        if (Qtarget != Qcurrent){MvJ.ExternalExecution(Qtarget);}
        else {Debug.Log("Already in position !");}
    }

    
    void CreateGripNode(ClickEvent clickEvent)
    {
        var nodeParent = nodeSelected.parent;
        if (nodeSelected.name.Contains("Move")) {nodeParent = nodeSelected.parent.parent;}

        isCreatingNode = true; nbGripNode++;
        var newGripNode = new Button{name = "GripNode"+nbGripNode.ToString(), text = "2FG Grip" };
        ListNodes.Add(newGripNode);
        TwoFGNodeDict.Add(newGripNode.name, new float[]{40f, 50f, 100f});
        newGripNode.style.translate = Tab * GetNumberTab(nodeParent);
        nodeParent.Add(newGripNode);

        newGripNode.RegisterCallback<ClickEvent>(ModifNode);
        nodeSelected = newGripNode; onNodeClicked = true;
    }
    void GripWidthChanged(ChangeEvent<float> evt){GripWidthChanging = true;}
    void GripForceChanged(ChangeEvent<float> evt)
    {TwoFGNodeDict[nodeSelected.name][1] = GripForce.value;}
    void GripSpeedChanged(ChangeEvent<float> evt)
    {TwoFGNodeDict[nodeSelected.name][2] = GripSpeed.value;}

    void CreateReleaseNode(ClickEvent clickEvent)
        {
            var nodeParent = nodeSelected.parent;
            if (nodeSelected.name.Contains("Move")) {nodeParent = nodeSelected.parent.parent;}

            isCreatingNode = true; nbGripNode++;
            var newGripNode = new Button{name = "ReleaseNode"+nbGripNode.ToString(), text = "2FG Release" };
            ListNodes.Add(newGripNode);
            TwoFGNodeDict.Add(newGripNode.name, new float[]{40f, 50f, 100f});
            newGripNode.style.translate = Tab * GetNumberTab(nodeParent);
            nodeParent.Add(newGripNode);

            newGripNode.RegisterCallback<ClickEvent>(ModifNode);
            nodeSelected = newGripNode; onNodeClicked = true;
        }

    void ModifNode(ClickEvent clickEvent)
    {
        onNodeClicked = true; 
        foreach(Button node in ListNodes)
            {
                if (clickEvent.currentTarget == node) {nodeSelected = node;}
            }
        isDisplayInfo = true;
        if (nodeSelected.name.Contains("Move"))
        {
            MoveType.index = (int)MoveNodeDict[nodeSelected.name][0];
            MoveSpeed.value = MoveNodeDict[nodeSelected.name][1];       MoveAccel.value = MoveNodeDict[nodeSelected.name][2];
        }
        else if (nodeSelected.name.Contains("Wp"))
        {
            WpName.value = nodeSelected.text;
        }
        else if (nodeSelected.name.Contains("Grip"))
        {
            GripWidth.value = TwoFGNodeDict[nodeSelected.name][0]; GripForce.value = TwoFGNodeDict[nodeSelected.name][1]; GripSpeed.value = TwoFGNodeDict[nodeSelected.name][2]; 
        }
        else if (nodeSelected.name.Contains("Release"))
        {
            GripWidth.value = TwoFGNodeDict[nodeSelected.name][0]; GripSpeed.value = TwoFGNodeDict[nodeSelected.name][2]; 
        }
        Invoke(nameof(DisplayInfoFinished), 0.1f);
    }
    private void DelNode(ClickEvent clickEvent)
    {
        var ArrayNodes = ListNodes.ToArray();
        var newSelectedNode = StartNode;
        for (int i=1; i<ArrayNodes.Length; i++)
        {if (ArrayNodes[i] == nodeSelected) newSelectedNode = ArrayNodes[i-1];}

        if (nodeSelected == null || nodeSelected.name == "StartRobotProgram")   return;
        else if (nodeSelected.name.Contains("Move"))                            DelMoveChildren(nodeSelected.parent);
        else
        {
            nodeSelected.parent.Remove(nodeSelected);
            ListNodes.Remove(nodeSelected);
            MoveNodeDict.Remove(nodeSelected.name);
        }

        if (ProgTree.Contains(newSelectedNode)){nodeSelected = newSelectedNode;}
        else {nodeSelected = StartNode;}
    }
    private void DelMoveChildren(VisualElement MvNdContainer)
    {
        if (MvNdContainer.name.Contains("Container"))
        {
            var childNodeList = MvNdContainer.Children().ToList();
            foreach (var childElem in childNodeList)
            {
                if (childElem is VisualElement)
                {
                    MvNdContainer.Remove(childElem);
                    DelMoveChildren(childElem);  
                }
                else if (childElem is Button)
                {
                    foreach(Button node in ListNodes)
                        {if (node.name == childElem.name) ListNodes.Remove(node);}
                    if (childElem.name.Contains("Move"))
                    {MoveNodeDict.Remove(childElem.name);}
                    if (childElem.name.Contains("Wp"))
                    {WpNodeDict.Remove(childElem.name);}
                    if (childElem.name.Contains("Grip") || childElem.name.Contains("Grip"))
                    {TwoFGNodeDict.Remove(childElem.name);}
                    
                }
            }
        }
        else return;
        
    }
    private VisualElement CreateMoveNodeContainer()
    {
        var res = new VisualElement{name = "MoveNodeContainer" + nbMoveNode.ToString()};
        // res.style.width = 400f;
        // res.style.borderBottomColor = new Color(0f, 0f, 0f, 1f); res.style.borderBottomWidth = 3f;
        // res.style.borderTopColor    = new Color(0f, 0f, 0f, 1f); res.style.borderTopWidth = 3f;
        // Color rdmColor = Random.ColorHSV();
        // res.style.backgroundColor = rdmColor;
        return res;
    }
    private void UpdateNodesPos(VisualElement parent)
    {
        var NodeList = parent.Children().ToList();
        
        foreach(var node in NodeList)
        {
            if (node.name.Contains("Container"))
            {
                var childrenNodeList = node.Children().ToList();
                foreach(var childNode in childrenNodeList)
                {
                    if (childNode.name.Contains("Container"))
                    {UpdateNodesPos(childNode);}
                }

                node.style.translate = Vector3.zero;
                UpdateMoveNodeContainerHeight(node);
            }
        }
    }
    private void UpdateMoveNodeContainerHeight(VisualElement MNC)
    {
        MNC.style.height = 4 + CalcOffsetHeight(MNC);
        MNC.style.backgroundSize = new BackgroundSize(MNC.style.width.value, MNC.style.height.value);
    }
    private float CalcOffsetHeight(VisualElement elem)
    {
        float res = 0;
        var childList = elem.Children();
        foreach (VisualElement child in childList)
        {
            if (child.name.Contains("Container")) {res += CalcOffsetHeight(child);}
            else {res += FixNodeHeight + 4;}
        }
        return res;
    }
    void NodeCreationFinished() {isCreatingNode = false;}
    void DisplayInfoFinished() {isDisplayInfo = false;}
    public Button GetCurrentButton()
    {
        Button res = null;
        foreach(Button node in ListNodes)
        {
            if (nodeSelected == node)
            {res = node;}
        }
        return res;
    }
    

    private int GetNumberTab(VisualElement elem)
    {
        if (elem.name == "ProgTree")    return 0;
        else 
        {
            int res = 1 + GetNumberTab(elem.parent); 
            return res;
        }   
    }
    public float[] GetToolPose()
    {
        float[] res = new float[6];
        Vector3 pos = ik.localPosition; var ang = TCP.rotation.eulerAngles; //Debug.Log("EulerAngles : " + ang.x.ToString("0.00") + "; " + ang.y.ToString("0.00") + "; " + ang.z.ToString("0.00") + ";");
        res[0] = pos.x; res[1] = pos.y; res[2] = pos.z; res[3] = ang.x; res[4] = ang.y; res[5] = ang.z;
        return res;
    }
}