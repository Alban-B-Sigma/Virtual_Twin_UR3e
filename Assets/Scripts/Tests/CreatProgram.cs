using UnityEngine;
using System;
using System.IO;
using UnityEngine.UIElements;
using System.Collections.Generic;
using UnityEngine.InputSystem;
// -------------------------------------------------------------------------------------------------------------------------------------- //
// This script was a first version of the "ProgCreation" script but without the possibily to modify the nodes after their creation. It    //
// managed also the URscript creation.                                                                                                    //
// -------------------------------------------------------------------------------------------------------------------------------------- //
public class CreatProgram : MonoBehaviour
{
    // root : initial element of the UIDocument (User Interface Document) that contains all buttons and displays windows of the simulation 
    private VisualElement root; 
    private VisualElement WPwindow; // window in which are the buttons for create and save Waypoints and select already saved Waypoint
    // MoveWindow and JointWindow : windows in which are buttons for moving manually the robot
    private VisualElement MoveWindow; private VisualElement JointWindow;
    // StartCreation : button to begin to write a program and save it | ShowProg : Button to display the program information/nodes 
    private Button StartCreation; private Button ShowProg; 
    private Button RcWdw; // Button used to add a Waypoint to the program, it appear by right-clicking on a waypoint (RcWdw -> )
    private DropdownField WPlist; // stock the list of already saved waypoints
    private Label PrintPrg; // displays the program information
    private TextField ProgName; // appears when the user want to save the program, it allows to create a custom program name
    public Cobot_Command Cmd; //program in which the function to extract the joints angles is stocked
    private WPsave WPsave; // program that allows the user to save Waypoint and in which you can extract the list of the Waypoints saved 
    private BoutonMoveJorLControl JorL; // program that controls if the robot move in MoveJ or MoveL type

    private Vector3 RcOffset = new Vector3(-350, -170, 0);
    private Dictionary<string, float[]> WPdict = new Dictionary<string, float[]>(); // stocks for each saved waypoint the 6 joint angles
    // booleans that will be used to block or run some functions :
    private bool isCreating = false; private bool isSavingPrg = false;
    private string fileName = ""; private string content = ""; // containers for the name and the content of the program
    private int clickCount; // manages the StartCreation button : the first click creates a program, the second saves the current program

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        clickCount = 0;
        // Link the variable to the corresponding game element / script :
        root = GetComponent<UIDocument>().rootVisualElement; WPwindow = root.Q<VisualElement>("WPwindow");
        MoveWindow = root.Q<VisualElement>("MoveWindow"); JointWindow = root.Q<VisualElement>("JointWindow");
        StartCreation = root.Q<Button>("ProgramMenuButton"); ShowProg = root.Q<Button>("ShowProg");
        RcWdw = root.Q<Button>("Wp2Prg");
        WPlist = root.Q<DropdownField>("WPlist"); PrintPrg = root.Q<Label>("PrgInfoTxt"); ProgName = root.Q<TextField>("ProgName");
        WPsave = GetComponentInChildren<WPsave>();
        JorL = GetComponent<BoutonMoveJorLControl>();
        // Initialise the visibility of the different elements in the game window at the start of the simulation
        WPwindow.visible = false; ShowProg.visible = false;
        RcWdw.visible = false;      PrintPrg.visible = false; ProgName.visible = false;
        StartCreation.visible = true; MoveWindow.visible = true; JointWindow.visible = true;
        // Create activators for when the player click on buttons
        StartCreation.RegisterCallback<ClickEvent>(CreatePrg);
        WPlist.RegisterCallback<PointerDownEvent>(OnListClicked);
        RcWdw.RegisterCallback<ClickEvent>(AddWP2Prg);
    }

    // Update is called once per frame
    void Update()
    {
        // when the user is creating the program, the waypoint window is displayed to allow the user to set them in the program 
        if (isCreating && ! isSavingPrg)
        {
            WPwindow.visible = true; ProgName.visible = false;
        }
        // when the user is saving the program, the text field for the program name appears, and the program will be saved after the name will be written
        else if (isCreating && isSavingPrg)
        {
            WPwindow.visible = false; ProgName.visible = true;
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
                // Finalise the script : end the main_program function and call it to allows the program to be executed
                content += "\nend\nmain_program()";
                // Create the file with the created name and content
                File.WriteAllText(fileName, content);

                Debug.Log("Program saved");
                isCreating = false; isSavingPrg = false;
            }
        }
    }
    void CreatePrg(ClickEvent clickEvent)
    {
        MoveWindow.visible = false; JointWindow.visible = false;
        // Creation of the program : 
        if (!isCreating && clickCount%2 == 0)
        {
            ShowProg.visible = true;
            // Change the text displayed on the StartCreation button
            StartCreation.text = "Save Program";
            // Initialise the script information
            fileName = "Created_File_"; content = "def main_program():";
            isCreating = true;
            // Initialise the program information display window
            PrintPrg.text = "Current Program nodes :\n";
        }
        // Allow the save of the program, managed in the Update window : 
        else if (isCreating && clickCount%2 != 0)
        {
            isSavingPrg = true;
        }
        // add 1 to the clickCount after 0.5s to avoid conflict in the two conditions above
        Invoke(nameof(PlusOneClick), 0.5f);
    }
    private void OnListClicked(PointerDownEvent evt)
    {
        if (evt.button == 1) // check if it's a right click
        {
            // Set the button to a correct position and display it : 
            RcWdw.style.translate = evt.position + RcOffset;
            RcWdw.visible = true;
        }
    }
    private void AddWP2Prg(ClickEvent clickEvent)
    {   
        // By clicking on the RcWdw button, the user can add the selected waypoint to the program :
        if (isCreating)
        {
            WPdict = WPsave.GetSavedWP(); // Update the list of the waypoints and their information
            foreach (string key in WPdict.Keys) //
            {                                   // Search the selected waypoint (WPlist.value) in the dictionary :
                if (key == WPlist.value)        //
                {
                    float[] Qext = WPdict[key]; // extract the selected waypoint information from the dictionary 
                    string NewWp = "";  // Initialise the string that will be added to the program content
                    if (JorL.isMoveJ)   // For a MoveJ movement :
                    {
                        NewWp += "\n\t# MoveJ to the waypoint : " + key; // write a comment to inform wich movement type is used and which waypoint will be reached
                        NewWp += "\n\tmovej(["; // Initialise the movement function
                        for (int i=0; i<Qext.Length; i++)   // Add in the function parameters the joint angles corresponding to the selected waypoint
                        {
                            NewWp += Qext[i].ToString("0.00").Replace(",", ".");
                            if (i < Qext.Length-1)  {NewWp += ",";}
                            else                    {NewWp += "], a=1.2, v=1)";} // Add at the end of the function standard values for the acceleration (a) and the speed (v) 
                        }
                    }
                    else    // Same thing for a MoveL movement :
                    {
                        NewWp += "\n\t# MoveL to the waypoint : " + key;
                        NewWp += "\n\tmovel([";
                        for (int i=0; i<Qext.Length; i++)
                        {
                            NewWp += Qext[i].ToString("0.00").Replace(",", ".");
                            if (i < Qext.Length-1)  {NewWp += ",";}
                            else                    {NewWp += "], a=1.2, v=0.75)";} 
                        }
                    }
                    // Add this new move function to the program content and to the program display window :
                    PrintPrg.text += NewWp; content += NewWp;
                    
                    
                }
            }
        }
        else {Debug.Log("Start a Program !");}
        // After that, the button that appears with a right-click is hidden
        RcWdw.visible = false;
    }
    void PlusOneClick() {clickCount++;}
}
