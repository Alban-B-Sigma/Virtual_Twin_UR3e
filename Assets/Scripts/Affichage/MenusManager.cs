using UnityEngine;
using UnityEngine.UIElements;
// ---------------------------------------------------------------------------------------------------------------------------------//
// This script manages the change between the ProgramMenu and the MoveMenu.                                                         //
// ---------------------------------------------------------------------------------------------------------------------------------//
public class MenusManager : MonoBehaviour
{
    private VisualElement root; private Button Program; private Button Move;
    private VisualElement ProgramMenu; private VisualElement MoveMenu;
    private ProgCreation ProgScript;
    void Start()
    {
        // First it pick up all the usefull elements for this script from the scene
        root = GetComponent<UIDocument>().rootVisualElement; Program = root.Q<Button>("ProgramMenuButton"); Move = root.Q<Button>("MoveMenuButton");
        ProgramMenu = root.Q<VisualElement>("ProgramMenu"); MoveMenu = root.Q<VisualElement>("MoveMenu");
        ProgScript = GetComponent<ProgCreation>();
        Program.SetEnabled(true); Move.SetEnabled(true); 
        Program.RegisterCallback<ClickEvent>(ProgramSelected);
        Move.RegisterCallback<ClickEvent>(MoveSelected);

        // initialise as MoveMenu selected
        ProgramMenu.visible = false; MoveMenu.visible = true; ProgScript.enabled = false;

        // Hide some other independant visual elements used for the ProgramMenu, that will be displayed in other scripts.
        VisualElement WpNodeWindow = root.Q<VisualElement>("WpNodeWindow"); DropdownField WpList = root.Q<DropdownField>("WPlist");
        Button CloseNodeWindow = root.Q<Button>("CloseNodeWindow");
        Button ConfirmWpPose = root.Q<Button>("ConfirmWpPose"); Button UndoWpPose = root.Q<Button>("UndoWpPose");
        WpNodeWindow.visible = false; WpList.visible = false; CloseNodeWindow.visible = false; ConfirmWpPose.visible = false; UndoWpPose.visible = false;
    }

    void ProgramSelected(ClickEvent evt)
    {
        ProgramMenu.visible = true; MoveMenu.visible = false; ProgScript.enabled = true;
    }
    void MoveSelected(ClickEvent evt)
    {
        ProgramMenu.visible = false; MoveMenu.visible = true; ProgScript.enabled = false;
    }
}
