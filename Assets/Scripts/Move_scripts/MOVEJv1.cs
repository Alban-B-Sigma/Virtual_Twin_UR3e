using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;
using System;
using UnityEngine.InputSystem;
using Unity.Mathematics;
// -------------------------------------------------------------------------------------------------------------------------------------- //
// This script purpose is to execute a MoveJ movement, by entering numbers in the FloatField, or in the Program Menu scripts, with the    //
// "Move Here" Button or by executing a program.                                                                                          //
// -------------------------------------------------------------------------------------------------------------------------------------- //
public class MOVEJv1 : MonoBehaviour
{
    public Calculs Calc; public Cobot_Command Cmd; public ArticulationBody ab; 
    private ArticulationBody[] allBodies; private IKptFollow IKptFollow;
    private FloatField InpQ1; private FloatField InpQ2; private FloatField InpQ3; private FloatField InpQ4; private FloatField InpQ5; private FloatField InpQ6;
    private List<double> Q0; private List<double> Qf; private List<double> dQ; private List<double> Qc;
    // N is the number of iteration to execute the complete movement, it can be changed to adjust the speed.
    public bool execute = false; private int N = 200; private int k; private bool manualModif = false;
    void Start()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        InpQ1 = root.Q<FloatField>("Q1input"); InpQ2 = root.Q<FloatField>("Q2input"); InpQ3 = root.Q<FloatField>("Q3input");
        InpQ4 = root.Q<FloatField>("Q4input"); InpQ5 = root.Q<FloatField>("Q5input"); InpQ6 = root.Q<FloatField>("Q6input");
        k = 0;
        if (ab == null) Debug.Log("No base");
        allBodies = ab.GetComponentsInChildren<ArticulationBody>(); IKptFollow = ab.GetComponentInChildren<IKptFollow>();
        Q0 = Cmd.GetAllJointAngles(); Qc = Q0; dQ = new List<double>{0, 0, 0, 0, 0, 0};
        InpQ1.RegisterCallback<FocusInEvent>(ValueIsBeingEntered); InpQ2.RegisterCallback<FocusInEvent>(ValueIsBeingEntered); InpQ3.RegisterCallback<FocusInEvent>(ValueIsBeingEntered);
        InpQ4.RegisterCallback<FocusInEvent>(ValueIsBeingEntered); InpQ5.RegisterCallback<FocusInEvent>(ValueIsBeingEntered); InpQ6.RegisterCallback<FocusInEvent>(ValueIsBeingEntered);
    }

    // Fonction used in the other scripts :
    public void ExternalExecution(float[] Qext)
    {
        IKptFollow.enabled = true;
        Qf = new List<double>{Qext[0], Qext[1], Qext[2], Qext[3], Qext[4], Qext[5]};
        execute = true;
    }
    // This function avoid interference between the "JointCommand" script and this one:
    void ValueIsBeingEntered(FocusInEvent evt) {manualModif = true;}

    public void FixedUpdate()
    {
        if (!execute)
        {
        Q0 = Cmd.GetAllJointAngles();
        }
        else
        {
            IKptFollow.enabled = true;
            for (int i=0; i<6; i++)
            {
                dQ[i] = (Qf[i] - Q0[i])/N;
            
                Qc[i] = Q0[i] + k*dQ[i];
            }

            SetJointAngles(Qc);
            
            if (k<N){k++;}
            else{execute = false; Q0 = Qf; k=0; IKptFollow.enabled = false;} //Debug.Log("Movement finished");
        }
    
        if (!manualModif)
        {
            float[] Qcurrent = Cmd.GetAllJointAnglesFloat(); int digit = 3;
            InpQ1.value = (float)Math.Round((double)(Qcurrent[0] * Mathf.Rad2Deg), digit);
            InpQ2.value = (float)Math.Round((double)(Qcurrent[1] * Mathf.Rad2Deg), digit);
            InpQ3.value = (float)Math.Round((double)(Qcurrent[2] * Mathf.Rad2Deg), digit);
            InpQ4.value = (float)Math.Round((double)(Qcurrent[3] * Mathf.Rad2Deg), digit);
            InpQ5.value = (float)Math.Round((double)(Qcurrent[4] * Mathf.Rad2Deg), digit);
            InpQ6.value = (float)Math.Round((double)(Qcurrent[5] * Mathf.Rad2Deg), digit);
        }
        else
        {
            if (Keyboard.current.enterKey.isPressed)
            {
                Qf = new List<double>{  math.clamp(InpQ1.value*Mathf.Deg2Rad, -math.TAU, math.TAU),
                                        math.clamp(InpQ2.value*Mathf.Deg2Rad, -math.TAU, math.TAU),
                                        math.clamp(InpQ3.value*Mathf.Deg2Rad, -math.TAU, math.TAU), 
                                        math.clamp(InpQ4.value*Mathf.Deg2Rad, -math.TAU, math.TAU),
                                        math.clamp(InpQ5.value*Mathf.Deg2Rad, -math.TAU, math.TAU),
                                        math.clamp(InpQ6.value*Mathf.Deg2Rad, -math.TAU, math.TAU)};
                execute = true; manualModif = false;
            }
        }
    
    }
    // Apply the current angle for each joint in 2 possible ways, depending if the gripper has movable fingers or not 
    public void SetJointAngles(List<double> Q2apply)
    {
        if(GetRobotAllDof() == 6)
        {
            allBodies[0].SetDriveTargets(new List<float>{   (float)Q2apply[0],
                                                            (float)Q2apply[1],
                                                            (float)Q2apply[2],
                                                            (float)Q2apply[3],
                                                            (float)Q2apply[4],
                                                            (float)Q2apply[5]  });
        }
        else if (GetRobotAllDof() == 8) // if the fingers are movable, we apply the 6 angles and the current target of each finger, to not
        {                               // modify their position.
            ArticulationBody f1 = GetArticulationBodyByName("Finger1_R1");
            ArticulationBody f2 = GetArticulationBodyByName("Finger2_R1");

            allBodies[0].SetDriveTargets(new List<float>{   (float)Q2apply[0],
                                                            (float)Q2apply[1],
                                                            (float)Q2apply[2],
                                                            (float)Q2apply[3],
                                                            (float)Q2apply[4],
                                                            (float)Q2apply[5],
                                                            f1.yDrive.target,
                                                            f2.yDrive.target    });
        }
    }
    public int GetRobotAllDof()
    {
        int res = 0;
        foreach(ArticulationBody ab in allBodies)
        {res += ab.dofCount;}
        return res;
    }
    public ArticulationBody GetArticulationBodyByName(string name)
    {
        ArticulationBody res = null;
        foreach(ArticulationBody ab in allBodies)
        {
            if(ab.name == name) res = ab;
        }
        return res;
    }
}
