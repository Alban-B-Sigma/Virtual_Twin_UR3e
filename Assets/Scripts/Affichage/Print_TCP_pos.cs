using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;
using System;
using UnityEngine.UIElements;
using UnityEditor;
// -----------------------------------------------------------------------------------------------------------------------------------------//
// This script display the joints angles and the TCP information when the button "ShowInfos" is clicked, and the program information when   //
// one program is been crating and the "ShowPrg" button is clicked.                                                                         //
// -----------------------------------------------------------------------------------------------------------------------------------------//
public class Print_TCP_pos : MonoBehaviour
{
    public ArticulationBody ab; private VisualElement root;
    private Button ShowInfos; private Button ShowPrg; private Label ToolInfo; private Label JointsInfo;  
    private ScrollView PrgView; private Label PrgInfo;
    public Transform IKpt; public Cobot_Command Cmd; public Transform TCP; public Transform Base;
    private Vector3 pos0; private Vector3 ang0; private Matrix4x4 Mtcp0;
    private Vector3 Ikpos0; private Vector3 Ikang0; private Matrix4x4 IkMtcp0;
    private List<double> Qu0;
    private List<Button> ListNodes = new List<Button>();
    public Dictionary<string, float[]> MoveNodeDict = new Dictionary<string, float[]>();
    public Dictionary<string, float[]> WpNodeDict = new Dictionary<string, float[]>();   
    public Dictionary<string, float[]> TwoFGNodeDict = new Dictionary<string, float[]>();
    private int clickCountInfos; private int clickCountProg;
    
    void Start()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        ToolInfo = root.Q<Label>("ToolInfoText"); JointsInfo = root.Q<Label>("JointInfoText"); PrgView = root.Q<ScrollView>("ProgView"); PrgInfo = root.Q<Label>("PrgInfoTxt");
        ShowInfos = root.Q<Button>("ShowCobotInfo"); ShowPrg = root.Q<Button>("ShowProg");
        // Initialisation of the Joints and TCP informations
        // Joints information :
        Qu0 = Cmd.GetAllJointAngles();
        // TCP information : The TCP and the IK point positions will be displayed, to check if there is any difference between them.
        Ikpos0 = IKpt.localPosition; Ikang0 = IKpt.rotation.eulerAngles;
        pos0 = TCP.position - Base.position; ang0 = TCP.rotation.eulerAngles;
        pos0 = new Vector3(pos0.z*100, -pos0.x*100, pos0.y*100); // translate the Unity position in a scientific coordinate system.
        // In the current state, the transform matrices are not used anymore, but it can be another way to display the TCP and IK point information.
        IkMtcp0 = GetTransformMatrix(IKpt); Mtcp0 = GetTransformMatrix(TCP);

        // Set the information in the display windows
        ToolInfo.text = ChangeToolValues(pos0, ang0, Mtcp0, Ikpos0, Ikang0, IkMtcp0);    JointsInfo.text = ChangeJointsValues(Qu0);
        // Make the display window hidden at the start. The clickCount int are use to know if the informations have to be displayed (fisrt click),
        // or have to be hidden (second click on the same button).
        ToolInfo.visible = false; JointsInfo.visible = false; PrgView.visible = false; PrgInfo.visible = false; clickCountInfos = 0; clickCountProg = 0;
        // Set up the actions of the buttons.
        ShowInfos.RegisterCallback<ClickEvent>(PrintInfos);
        ShowPrg.RegisterCallback<ClickEvent>(PrintProg);
    }
    // In the Update loop, the Joints and TCP information are constantly updated and if at least one value changes, the new information will
    // be displayed. The update of the program information will be updated in another script.
    void Update()
    {
        // Current joints information :
        List<double> Qu = Cmd.GetAllJointAngles();
        // Current TCP and Ik point information :
        Vector3 Ikpos = IKpt.localPosition; Vector3 Ikang = IKpt.rotation.eulerAngles; Matrix4x4 IkMtcp = GetTransformMatrix(IKpt);
        Vector3 pos = TCP.position - Base.position; Vector3 ang = TCP.rotation.eulerAngles; Matrix4x4 Mtcp = GetTransformMatrix(TCP);
        pos = new Vector3(pos.z*100, -pos.x*100, pos.y*100);

        // Verify if the values are changing in the current frame :
        if (Ikpos != Ikpos0 || Ikang != Ikang0 || pos != pos0 || ang != ang0 || Qu != Qu0)
        {
            ToolInfo.text = ChangeToolValues(pos, ang, Mtcp, Ikpos, Ikang, IkMtcp);
            JointsInfo.text = ChangeJointsValues(Qu);
        }
        // update the old values as the current ones for the next frame :
        Ikpos0 = Ikpos;Ikang0 = Ikang; pos0 = pos; ang0 = ang; Qu0 = Qu;
    }
    // This function print the Tool (TCP) information in its display window in the right format
    public string ChangeToolValues(Vector3 pos, Vector3 eA, Matrix4x4 Mtcp, Vector3 Ikpos, Vector3 IkeA, Matrix4x4 IkMtcp)
    {
        string Print =  " TCP pos :\t\t\t IkPt pos : \n x = " + pos.x.ToString("0.0000") + ";\t\t x = " + Ikpos.x.ToString("0.0000") 
                        + ";\n y = " + pos.y.ToString("0.0000") + ";\t\t y = " + (-Ikpos.y).ToString("0.0000")  + ";\n z = " 
                        + pos.z.ToString("0.0000") + ";\t\t z = " + Ikpos.z.ToString("0.0000")  + ";";
        
        Print +="\n\n Angles d'Euler : \n psi = " + eA.x.ToString("0.00") + ";\t\t psi = " + IkeA.x.ToString("0.00") 
                + ";\n theta = " + eA.y.ToString("0.00") + ";\t\t theta = " + IkeA.y.ToString("0.00")
                + ";\n phi = " + eA.z.ToString("0.00") + ";\t\t phi = " + IkeA.z.ToString("0.00") + ";";
        // Print += "\n\n Matrice de rotation : \n" + PrintMat4(Mtcp);
        return Print;
    }
    // Same thing for the Joints information
    public string ChangeJointsValues(List<double> Qu)
    {
        string Print = " Joints angles :  ";
        for(int i=0; i<6; i++)
        {
            if (i%2 == 0){Print += "\n\t";}
            else {Print += "\t";}
            double ai_u = Math.Round(Qu[i] * Mathf.Rad2Deg, 3);
            Print += "q" + (i+1).ToString() + " = " + ai_u.ToString("0.000");
        }
        return Print;
    }
    // The functions "PrintInfos" and "PrintProg" manage the display of the information depending of the number of click on the related button.
    void PrintInfos(ClickEvent clickEvent)
    {
        ToolInfo.visible = clickCountInfos%2 == 0; JointsInfo.visible = clickCountInfos%2 == 0;
        
        clickCountInfos++;
    }
    void PrintProg(ClickEvent clickEvent)
    {
        PrgView.visible = clickCountProg%2 == 0; PrgInfo.visible = clickCountProg%2 == 0;
        
        clickCountProg++;
    }
    // This function get the transformation matrix of the IK point (that should correspond to the TCP) in the base feature.
    public static Matrix4x4 GetTransformMatrix(Transform IkPt)
    {
        return Matrix4x4.TRS(IkPt.localPosition, 
                             IkPt.localRotation,
                             new Vector3(1, 1, 1));
    }
    // The functions "PrintDbl4" and "PrintMat4" return the string that will be used to display a matrix 4x4, depending of its format (double4x4
    // or Matrix4x4)
    public string PrintDbl4(double4x4 M)
    {
        string prcs = "0.000";
        string  exit = "[ " + M.c0.x.ToString(prcs) + "  \t" + M.c1.x.ToString(prcs) + "  \t" + M.c2.x.ToString(prcs) + "  \t" + M.c3.x.ToString(prcs) + "\n";
                exit+= "  " + M.c0.y.ToString(prcs) + "  \t" + M.c1.y.ToString(prcs) + "  \t" + M.c2.y.ToString(prcs) + "  \t" + M.c3.y.ToString(prcs) + "\n";
                exit+= "  " + M.c0.z.ToString(prcs) + "  \t" + M.c1.z.ToString(prcs) + "  \t" + M.c2.z.ToString(prcs) + "  \t" + M.c3.z.ToString(prcs) + "\n";
                exit+= "  " + M.c0.w.ToString(prcs) + "  \t" + M.c1.w.ToString(prcs) + "  \t" + M.c2.w.ToString(prcs) + "  \t" + M.c3.w.ToString(prcs) + "\t]";
        return exit;        
    }
    public string PrintMat4(Matrix4x4 M)
    {
        string prcs = "0.00";
        string  exit = "[ " + M.m00.ToString(prcs) + "  \t" + M.m01.ToString(prcs) + "  \t" + M.m02.ToString(prcs) + "  \t" + M.m03.ToString(prcs) + "\n";
                exit+= "  " + M.m10.ToString(prcs) + "  \t" + M.m11.ToString(prcs) + "  \t" + M.m12.ToString(prcs) + "  \t" + M.m13.ToString(prcs) + "\n";
                exit+= "  " + M.m20.ToString(prcs) + "  \t" + M.m21.ToString(prcs) + "  \t" + M.m22.ToString(prcs) + "  \t" + M.m23.ToString(prcs) + "\n";
                exit+= "  " + M.m30.ToString(prcs) + "  \t" + M.m31.ToString(prcs) + "  \t" + M.m32.ToString(prcs) + "  \t" + M.m33.ToString(prcs) + "\t]";
        return exit;        
    }
    
}