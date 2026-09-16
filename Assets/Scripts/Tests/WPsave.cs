using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
// -------------------------------------------------------------------------------------------------------------------------------------- //
// This script was used with the "CreatProgram" script test, to save the waypoints in the program.                                        //
// -------------------------------------------------------------------------------------------------------------------------------------- //
public class WPsave : MonoBehaviour
{
    public Cobot_Command Cmd; public Slider_Manager slider; private MOVEJv1 MoveJ;
    private Button SaveWP; private TextField WPname; private DropdownField WPlist; private Button RcWdw;
    private bool isSaving = false; private bool saved = false; private float[] savedPos; private float oldRotSpeed;
    private Vector3 RcOffset = new Vector3(20, 20, 0);
    private Dictionary<string, float[]> WPdict = new Dictionary<string, float[]>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        MoveJ = GetComponent<MOVEJv1>();
        SaveWP = root.Q<Button>("SaveWP"); WPname = root.Q<TextField>("WPname"); WPlist = root.Q<DropdownField>("WPlist");
        RcWdw = root.Q<Button>("Wp2Prg");
        WPname.visible = false; RcWdw.visible = false;
        SaveWP.RegisterCallback<ClickEvent>(OnSaveClicked);
        WPlist.RegisterValueChangedCallback(NewWpSelected);
    }

    // Update is called once per frame
    void Update()
    {
        if (!isSaving){WPname.visible = false; oldRotSpeed = slider.RotSpeed;}
        else
        {
            WPname.visible = true;
            slider.RotSpeed = 0;
            if (Keyboard.current.enterKey.isPressed)
            {
                if (WPname.value != "")
                {
                    WPdict.Add(WPname.value, savedPos);
                    WPlist.choices.Add(WPname.value);
                    isSaving = false; saved = true;
                }
                else
                {
                    Debug.Log("Enter WP name");
                }
            }
        }
        if (saved)
        {
            slider.RotSpeed = oldRotSpeed;
            WPname.value = "";
            saved = false;
            }
    }
    private void OnSaveClicked(ClickEvent clickEvent)
    {
        savedPos = Cmd.GetAllJointAnglesFloat();
        isSaving = true;
    }
    private void NewWpSelected(ChangeEvent<string> evt)
    {
        if (WPlist.value != "")
        {
            foreach (string key in WPdict.Keys)
            {
                if (key == WPlist.value)
                {
                    float[] Qext = WPdict[key];
                    MoveJ.ExternalExecution(Qext);
                }
            }
        }
    }
    public Dictionary<string, float[]> GetSavedWP()
    {
        return WPdict;
    }
}
