using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;
using System;
using System.IO;
using UnityEngine.InputSystem;
using Unity.Mathematics;
// -------------------------------------------------------------------------------------------------------------------------------------- //
// This script is used to create a URscript file, when the current simulation program is saved.                                           //
// -------------------------------------------------------------------------------------------------------------------------------------- //
public class URscriptCreation : MonoBehaviour
{
    private Button SaveProg; private ProgCreation PC;
    private List<Button> ListNodes = new List<Button>(); private Button[] ArrayNodes = new Button[]{};
    public Dictionary<string, float[]> MoveNodeDict = new Dictionary<string, float[]>();
    public Dictionary<string, float[]> WpNodeDict = new Dictionary<string, float[]>();   
    public Dictionary<string, float[]> TwoFGNodeDict = new Dictionary<string, float[]>();
    private bool isWriting = false; private bool SavingProg = false; private bool enteringName = false; private int i = 0;
    private float[] WpInfo = new float[]{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
    private float[] Qwp = new float[]{0, 0, 0, 0, 0, 0}; private float[] Poswp = new float[]{0, 0, 0, 0, 0, 0};
    private TextField ProgName;
    private string fileName = ""; private string content = ""; // containers for the name and the content of the program
    private Button node; private float[] moveInfo = new float[]{};
    void Start()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement; PC = GetComponent<ProgCreation>();
        SaveProg = root.Q<Button>("SaveProg"); ProgName = root.Q<TextField>("ProgramName");
        
        SaveProg.RegisterCallback<ClickEvent>(SaveCurrentProgram);
    }
    void Update()
    {
        // when the user is saving the program, the text field for the program name appears, and the program will be saved after the name will be written
        if (enteringName)
        {
            ProgName.visible = true;
            // After entering the program name, the user has to press the "Enter" key to save the program :
            if (Keyboard.current.enterKey.isPressed)
            {
                // If the user really wrote a program name, this one will be used
                if (ProgName.value != "")
                {
                    fileName = ProgName.value + ".script";
                }
                // else, a name with the current date and hour will be created
                else
                {    
                    fileName += DateTime.Now.Day.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Year.ToString() + "_" +
                                DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + ".script";
                }
                // Create the file with the created name and content
                File.WriteAllText(fileName, content);

                Debug.Log("Program saved");
                enteringName = false;
            }
        }
        else{ProgName.visible = false;}
    }
    void FixedUpdate() // The writing process follows the structure of the "RunningProgram" script to execute programs.
    {
        if (SavingProg)
        {   
            Debug.Log("writing program");
            if (!isWriting)
            {
                if (i < ArrayNodes.Length) 
                {
                    node = ArrayNodes[i];
                    isWriting = true;
                    i++;
                }
                else 
                {
                    SavingProg = false;
                    enteringName = true; 
                    // Finalise the script : end the main_program function and call it to allows the program to be executed
                    content += "\nend\nmain_program()"; 
                    i=0;
                }
            }
            else
            {
                if (node.name.Contains("Move")) 
                {
                    moveInfo = MoveNodeDict[node.name];
                    isWriting = false;
                }
                else if (node.name.Contains("Wp"))
                {
                    WpInfo = WpNodeDict[node.text];
                    Qwp = new float[]{WpInfo[0], WpInfo[1], WpInfo[2], WpInfo[3], WpInfo[4], WpInfo[5]};
                    Poswp = new float[]{WpInfo[6], WpInfo[7], WpInfo[8], WpInfo[9], WpInfo[10], WpInfo[11]};
                    // var angG = new float[]{WpInfo[12], WpInfo[13], WpInfo[14]};
                    string NewWp = "";  // Initialise the string that will be added to the program content

                    if (moveInfo[0] == 0) // Move J :
                    {
                        NewWp += "\n\t# MoveJ to the waypoint : " + node.text; // write a comment to inform wich movement type is used and which waypoint will be reached
                        NewWp += "\n\tmovej(["; // Initialise the movement function
                        for (int i=0; i<Qwp.Length; i++)   // Add in the function parameters the joint angles corresponding to the selected waypoint
                        {
                            NewWp += Qwp[i].ToString("0.000").Replace(",", ".");
                            if (i < Qwp.Length-1)  {NewWp += ",";}
                            else                   {NewWp += "], a=1.2, v=1)";} // Add at the end of the function standard values for the acceleration (a) and the speed (v) 
                        }
                    }
                    else if (moveInfo[0] == 1) // Move L :
                    {
                        Vector3 angUR = URrotFromEuler(Poswp[4]*Mathf.Deg2Rad, Poswp[3]*Mathf.Deg2Rad, Poswp[5]*Mathf.Deg2Rad);
                        NewWp += "\n\t# MoveL to the waypoint : " + node.text;
                        NewWp += "\n\tangRPY = rotvec2rpy(["
                                + angUR[0].ToString("0.000").Replace(",", ".") + ", "
                                + angUR[1].ToString("0.000").Replace(",", ".") + ", " 
                                + angUR[2].ToString("0.000").Replace(",", ".") +  "])";
                        NewWp += "popup(str(angRPY[0]) + str(angRPY[1]) + str(angRPY[2]))";
                        NewWp += "\n\tmovel(p[";

                        for (int i=0; i<Poswp.Length; i++)
                        {
                            if (i < 3) 
                            {
                                Poswp[i] = Poswp[i] / 1000;
                                if (i==1) {Poswp[i] = - Poswp[i];}
                                NewWp += Poswp[i].ToString("0.000").Replace(",", ".");
                            }
                            else 
                            {
                                Poswp[i] = angUR[i-3]; 
                                // Debug.Log("angle " + (i-2) + " : " + Poswp[i].ToString("0.000"));
                                NewWp += "angRPY[" + (i-3) + "]";
                            
                            }
                            
                            if (i < Poswp.Length-1)  {NewWp += ",";}
                            else                     {NewWp += "], a=1.2, v=0.75)";} 
                        }
                    }
                    content += NewWp;
                    isWriting = false;
                }
                else if (node.name.Contains("Grip"))
                {
                    content += "\n\ttwofg_grip(40, 30, 2)";
                    isWriting = false;
                }
                else if (node.name.Contains("Release"))
                {
                    content += "\n\ttwofg_grip(80, 30, 2)";
                    isWriting = false;
                }
            }
        }
    }
    void SaveCurrentProgram(ClickEvent evt)
    {
        ListNodes = PC.ListNodes; ArrayNodes = ListNodes.ToArray(); MoveNodeDict = PC.MoveNodeDict; WpNodeDict = PC.WpNodeDict; TwoFGNodeDict = PC.TwoFGNodeDict;
        fileName = "Created_File_"; content = "def main_program():";
        SavingProg = true;
    }
    // Here two test to convert the Euler angles to the UR convention angles:
    private Vector3 ConvertToURRotationVector(Quaternion q)
    {
        // Convertir le quaternion Unity en un angle-axis compatible
        q.ToAngleAxis(out float angleInDegrees, out Vector3 axis);
        
        float angleInRad = angleInDegrees * Mathf.Deg2Rad;

        // Normaliser l'axe et appliquer l'angle (Vecteur de rotation Rx, Ry, Rz)
        Vector3 rotVector = axis.normalized * angleInRad;
        
        return rotVector;
    }
    public Vector3 URrotFromEuler(float heading, float attitude, float bank) 
    {
        // Assuming the angles are in radians.
        float c1 = math.cos(heading/2);
        float s1 = math.sin(heading/2);
        float c2 = math.cos(attitude/2);
        float s2 = math.sin(attitude/2);
        float c3 = math.cos(bank/2);
        float s3 = math.sin(bank/2);
        float c1c2 = c1*c2;
        float s1s2 = s1*s2;
        float w = c1c2*c3 - s1s2*s3;
        float x = c1c2*s3 + s1s2*c3;
        float y = s1*c2*c3 + c1*s2*s3;
        float z = c1*s2*c3 - s1*c2*s3;
        float angle = 2 * math.acos(w);
        float norm = x*x+y*y+z*z;
        if (norm < 0.001) { // when all euler angles are zero angle = 0 so
            x=1;            // we can set axis to anything to avoid divide by zero
            y=z=0;
        } 
        else 
        {
            norm = math.sqrt(norm);
            x /= norm;
            y /= norm;
            z /= norm;
        }
        return new Vector3(x * angle, y * angle, z * angle);
    }
}
