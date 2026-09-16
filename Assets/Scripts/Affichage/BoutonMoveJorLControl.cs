using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
// ---------------------------------------------------------------------------------------------------------------------------------//
// This script manages the change between the MoveJ mode and the MoveL mode, when the player click on the "SwitchMoveType" button.  //
// ---------------------------------------------------------------------------------------------------------------------------------//
public class BoutonMoveJorLControl : MonoBehaviour
{   
    private VisualElement root; private VisualElement moveWindow; private VisualElement jointWindow; 
    private Button MoveHere;
    public Object baseRob; private IKCalculator ikScript; private IKptFollow fixIkPoint;
    public bool isMoveJ;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // First it pick up all the usefull elements for this script from the scene
        root = GetComponent<UIDocument>().rootVisualElement; moveWindow = root.Q<VisualElement>("MoveWindow"); jointWindow = root.Q<VisualElement>("JointWindow");
        MoveHere = root.Q<Button>("WpMoveHere");
        ikScript = baseRob.GetComponentInChildren<IKCalculator>(); fixIkPoint = baseRob.GetComponentInChildren<IKptFollow>();
        
        moveWindow.RegisterCallback<FocusInEvent>(ToMoveL);
        jointWindow.RegisterCallback<FocusInEvent>(ToMoveJ);
        MoveHere.RegisterCallback<ClickEvent>(ToMoveJforMoveHere);
        // Initialise in MoveJ mode to allow the initialisation movement
        MoveJMode();
        // Then go to moveL mode (1 second later to make sure the initialisation movement is completely made)
        Invoke(nameof(MoveLMode), 1f);
    }
    void ToMoveJ(FocusInEvent evt)
    {
        MoveJMode();
    }
    void ToMoveJforMoveHere(ClickEvent evt)
    {
        MoveJMode();
    }
    void ToMoveL(FocusInEvent evt)
    {
        MoveLMode();
    }
    // In the MoveJ mode, the player can enter the joints angles manually witg the mvJ button. The player cannot move in a linear way the
    // IK point, this one is attached to the TCP pointwith the script fixIkPoint.  
    void MoveJMode()
    {   
        ikScript.enabled = false; fixIkPoint.enabled = true;
        isMoveJ = true;
    }
    // For the MoveL mode we will have the opposite of the MoveJ mode.
    void MoveLMode()
    {   
        ikScript.enabled = true; fixIkPoint.enabled = false;
        isMoveJ = false;
    }
}
