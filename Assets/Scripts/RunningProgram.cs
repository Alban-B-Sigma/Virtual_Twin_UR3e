using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Mathematics;
// -------------------------------------------------------------------------------------------------------------------------------------- //
// This script manages the program execution, by reading the node list and executing the related action.                                  //
// -------------------------------------------------------------------------------------------------------------------------------------- //

public class RunningProgram : MonoBehaviour
{
    private Button RunButton; private ProgCreation PC; private MOVEJv1 MvJ;
    public Transform IkPt; public PaPcube PaP; private Cobot_Command Cmd;
    private Label PrgInfo;
    private List<Button> ListNodes = new List<Button>(); private Button[] ArrayNodes = new Button[]{};
    public Dictionary<string, float[]> MoveNodeDict = new Dictionary<string, float[]>();
    public Dictionary<string, float[]> WpNodeDict = new Dictionary<string, float[]>();   
    public Dictionary<string, float[]> TwoFGNodeDict = new Dictionary<string, float[]>();
    private bool isRunning = false; private bool StartProg = false; private int i = 0; private string Print;
    private List<float[]> QwpList = new List<float[]>{}; private float[] WpInfo = new float[]{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
    private float[] Q0 = new float[]{0, 0, 0, 0, 0, 0}; private float[] Qf = new float[]{0, 0, 0, 0, 0, 0}; private float[] dQ = new float[]{0, 0, 0, 0, 0, 0}; private float[] Qc = new float[]{0, 0, 0, 0, 0, 0};
    private float[] Pos0 = new float[]{0, 0, 0, 0, 0, 0}; private float[] Posf = new float[]{0, 0, 0, 0, 0, 0}; private float[] dpos = new float[]{0, 0, 0, 0, 0, 0}; private float[] PosC = new float[]{0, 0, 0, 0, 0, 0};
    private int N = 200; private int k;
    public ArticulationBody ab; private ArticulationBody[] allBodies; 
    private ArticulationBody finger1; private ArticulationBody finger2; private ArticulationDrive drive1; private ArticulationDrive drive2;
    private IKptFollow IKptFollow; private IKCalculator IKcalc;
    private Button node; private float[] moveInfo = new float[]{};
    void Start()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement; PC = GetComponent<ProgCreation>(); MvJ = GetComponent<MOVEJv1>();
        RunButton = root.Q<Button>("RunProg"); PrgInfo = root.Q<Label>("PrgInfoTxt");
        allBodies = ab.GetComponentsInChildren<ArticulationBody>(); IKptFollow = ab.GetComponentInChildren<IKptFollow>(); 
        Cmd = ab.GetComponent<Cobot_Command>(); IKcalc = ab.GetComponent<IKCalculator>();
        
        finger1 = allBodies[9]; finger2 = allBodies[10];
        // Debug.Log(finger1.name + "  |  " + finger2.name);
        if (finger1.name != "Finger1_R1" || finger2.name != "Finger2_R1") Debug.LogError("Clamp fingers not defined correctly. Change the numbers in the RunningProgram script in the line 35 : finger1 = allBodies[Number 1]; finger2 = allBodies[Number 2];");
        drive1 = finger1.yDrive; drive2 = finger2.yDrive;

        RunButton.RegisterCallback<ClickEvent>(RunCurrentProgram);
        Print = "Current program nodes :\n\n";
    }
    void FixedUpdate()
    {
        if (StartProg)
        {   
            Debug.Log("program running");
            if (!isRunning)                 // The "isRunning" boolean is used to switch to the next node when the current one is executed.
            {                               // The variable represent the index of the current program.
                if (i < ArrayNodes.Length)  // While their are nodes, the program continue, the i variable is iterrated each time.  
                {
                    node = ArrayNodes[i];
                    // Debug.Log(node.name);
                    isRunning = true;
                    i++;
                }
                else {StartProg = false; i=0;} // When all nodes are executed, the program stops.
            }
            else
            {
                if (node.name.Contains("Move")) // For a Move node, its data is extracted to be used in its Waypoint nodes.
                {
                    moveInfo = MoveNodeDict[node.name];
                    
                    if(i==1) // for the first node, get the current position data :
                    {
                        Q0 = Cmd.GetAllJointAnglesFloat();
                        var pos = IkPt.localPosition; var ang = IkPt.localRotation.eulerAngles;
                        for (int j=0; j<3; j++)
                        {
                            Pos0[j] = pos[j]; Pos0[j+3] = ang[j];
                        }
                    }
                    else // for the other nodes, get the previous node data :
                    {
                        for (int n=0; n<6; n++)
                        {
                            Q0[n] = WpInfo[n]; Pos0[n] = WpInfo[n+6];
                        }

                    }

                    isRunning = false;
                }
                else if (node.name.Contains("Wp"))
                {
                    
                    WpInfo = WpNodeDict[node.text];
                    if (moveInfo[0] == 0) // Move J :
                    {
                        IKptFollow.enabled = true; IKcalc.enabled = false;
                        Qf = new float[]{WpInfo[0], WpInfo[1], WpInfo[2], WpInfo[3], WpInfo[4], WpInfo[5]};
                        for (int j=0; j<6; j++)
                        {
                            dQ[j] = (Qf[j] - Q0[j])/N;
                            Qc[j] = Q0[j] + k*dQ[j];
                            if (Q0[j] < Qf[j]) Qc[j] = math.clamp(Qc[j], Q0[j], Qf[j]);
                            else Qc[j] = math.clamp(Qc[j], Qf[j], Q0[j]);
                        }
                        
                        SetJointAngles(Qc);
                        
                        if (k==N)   Invoke(nameof(FinishWpMoveJ), 0.5f);

                        k++;
                    }
                    else if (moveInfo[0] == 1) // Move L :
                    {
                        IKptFollow.enabled = false; IKcalc.enabled = true;
                        Posf = new float[]{WpInfo[6], WpInfo[7], WpInfo[8], WpInfo[9], WpInfo[10], WpInfo[11]};
                        // Debug.Log("Pos0 : " + Pos0[0].ToString("0.00") + "; " + Pos0[1].ToString("0.00") + "; " + Pos0[2].ToString("0.00") + "; " + Pos0[3].ToString("0.00") + "; " + Pos0[4].ToString("0.00") + "; " + Pos0[5].ToString("0.00") + "; ");
                        // Debug.Log("Posf : " + Posf[0].ToString("0.00") + "; " + Posf[1].ToString("0.00") + "; " + Posf[2].ToString("0.00") + "; " + Posf[3].ToString("0.00") + "; " + Posf[4].ToString("0.00") + "; " + Posf[5].ToString("0.00") + "; ");
                        
                        if (k<N)
                        {
                            for (int j=0; j<3; j++)
                            {
                                dpos[j] = 2*Time.fixedDeltaTime*(Posf[j] - Pos0[j])/N;
                                float gap = Posf[j+3] - Pos0[j+3];
                                // Debug.Log("gap = " + gap.ToString("0.00"));
                                if (math.abs(gap) < 180f) dpos[j+3] =  gap/N;
                                else
                                {
                                    // Debug.Log("Angle n°" + j);
                                    dpos[j+3] = (-360f * math.sign(gap) + gap)/N;
                                } 
                            }
                            // Debug.Log("dpos : " + dpos[0].ToString("0.00") + "; " + dpos[1].ToString("0.00") + "; " + dpos[2].ToString("0.00") + "; " + dpos[3].ToString("0.00") + "; " + dpos[4].ToString("0.00") + "; " + dpos[5].ToString("0.00") + "; ");

                            IkPt.Translate(new Vector3(dpos[1], dpos[2], dpos[0]), Space.World);
                            IkPt.Rotate(new Vector3(1, 0, 0), dpos[3]); 
                            IkPt.Rotate(new Vector3(0, 1, 0), dpos[4]);
                            IkPt.Rotate(new Vector3(0, 0, 1), dpos[5]);
                        }
                        
                        if (k==N) 
                        {
                            Invoke(nameof(FinishWpMoveL), 0.5f);
                        }
                        
                        k++;
                    }
                }
                else if (node.name.Contains("Grip"))
                {
                    if (k < N)
                    {
                        float newTarget1 = drive1.target + math.abs(drive1.upperLimit-drive1.lowerLimit)/N;
                        float newTarget2 = drive2.target - math.abs(drive2.upperLimit-drive2.lowerLimit)/N;

                        // Clamp the target to stay within the joint's limits
                        newTarget1 = Mathf.Clamp(newTarget1, drive1.lowerLimit, drive1.upperLimit);
                        newTarget2 = Mathf.Clamp(newTarget2, drive2.lowerLimit, drive2.upperLimit);

                        // Apply the new target back to the ArticulationBody
                        drive1.target = newTarget1; drive2.target = newTarget2;
                        finger1.yDrive = drive1;    finger2.yDrive = drive2;
                    }
                    if(k<=N && (drive1.target == drive1.lowerLimit || drive1.target == drive1.upperLimit || drive2.target == drive2.lowerLimit || drive2.target == drive2.upperLimit)) 
                    {k = N; Invoke(nameof(FinishGrip), 0.2f);}
                    k++;
                }
                else if (node.name.Contains("Release"))
                {
                    if (k < N)
                    {
                        float newTarget1 = drive1.target - math.abs(drive1.upperLimit-drive1.lowerLimit)/N;
                        float newTarget2 = drive2.target + math.abs(drive2.upperLimit-drive2.lowerLimit)/N;

                        // Clamp the target to stay within the joint's limits
                        newTarget1 = Mathf.Clamp(newTarget1, drive1.lowerLimit, drive1.upperLimit);
                        newTarget2 = Mathf.Clamp(newTarget2, drive2.lowerLimit, drive2.upperLimit);

                        // Apply the new target back to the ArticulationBody
                        drive1.target = newTarget1; drive2.target = newTarget2;
                        finger1.yDrive = drive1;    finger2.yDrive = drive2;
                    }
                    if(k<=N && (drive1.target == drive1.lowerLimit || drive1.target == drive1.upperLimit || drive2.target == drive2.lowerLimit || drive2.target == drive2.upperLimit)) 
                    {k = N; Invoke(nameof(FinishGrip), 0.2f);}
                    k++;
                }
            }
        }
    }
    void RunCurrentProgram(ClickEvent evt) // When the user execute the program, the node list and all Dictionnaries are extracted :
    {
        ListNodes = PC.ListNodes; ArrayNodes = ListNodes.ToArray(); MoveNodeDict = PC.MoveNodeDict; WpNodeDict = PC.WpNodeDict; TwoFGNodeDict = PC.TwoFGNodeDict;
        PrgInfo.text = Print;
        StartProg = true;
    }
    public void SetJointAngles(float[] Q2apply) // A copy of the MOVEJv1 script function, but with a float[] as input.
    {
        if(MvJ.GetRobotAllDof() == 6)
        {
            allBodies[0].SetDriveTargets(new List<float>{   Q2apply[0],
                                                            Q2apply[1],
                                                            Q2apply[2],
                                                            Q2apply[3],
                                                            Q2apply[4],
                                                            Q2apply[5],                 
                                                            });
        }
        else if (MvJ.GetRobotAllDof() == 8)
        {
            ArticulationBody f1 = MvJ.GetArticulationBodyByName("Finger1_R1");
            ArticulationBody f2 = MvJ.GetArticulationBodyByName("Finger2_R1");

            allBodies[0].SetDriveTargets(new List<float>{   Q2apply[0],
                                                            Q2apply[1],
                                                            Q2apply[2],
                                                            Q2apply[3],
                                                            Q2apply[4],
                                                            Q2apply[5],
                                                            f1.yDrive.target,
                                                            f2.yDrive.target                 
                                                            });
        }
    }
    void FinishWpMoveJ()
    {isRunning = false; Q0 = Qf; k=0; IKptFollow.enabled = false; IKcalc.enabled = true;}
    void FinishWpMoveL()
    {isRunning = false; Pos0 = Posf; k=0; IKptFollow.enabled = false; IKcalc.enabled = true;}
    void FinishGrip()
    {isRunning = false; k=0; PaP.wantToGrip = false; PaP.wantToRelease = false;}
}
