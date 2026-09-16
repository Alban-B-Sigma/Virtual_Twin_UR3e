using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;
// -------------------------------------------------------------------------------------------------------------------------------------- //
// This script manages the Buttons used to move each joints seperately.                                                                   //
// -------------------------------------------------------------------------------------------------------------------------------------- //
public class JointsCommand : MonoBehaviour
{
    private Button Q1dec; private Button Q1inc; private Button Q2dec; private Button Q2inc; private Button Q3dec; private Button Q3inc;
    private Button Q4dec; private Button Q4inc; private Button Q5dec; private Button Q5inc; private Button Q6dec; private Button Q6inc;
    public ArticulationBody ab; private ArticulationBody[] allBodies; private Slider_Manager slider;
    private float[] Qinput; private float rotationSpeed;
     
    void Start()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        Q1dec = root.Q<Button>("Q1dec"); Q1inc = root.Q<Button>("Q1inc"); Q2dec = root.Q<Button>("Q2dec"); Q2inc = root.Q<Button>("Q2inc"); Q3dec = root.Q<Button>("Q3dec"); Q3inc = root.Q<Button>("Q3inc");
        Q4dec = root.Q<Button>("Q4dec"); Q4inc = root.Q<Button>("Q4inc"); Q5dec = root.Q<Button>("Q5dec"); Q5inc = root.Q<Button>("Q5inc"); Q6dec = root.Q<Button>("Q6dec"); Q6inc = root.Q<Button>("Q6inc");
        Q1dec.RegisterCallback<PointerDownEvent>(Q1Decrease, TrickleDown.TrickleDown); Q1dec.RegisterCallback<PointerUpEvent>(Q1STOP, TrickleDown.TrickleDown);
        Q1inc.RegisterCallback<PointerDownEvent>(Q1Increase, TrickleDown.TrickleDown); Q1inc.RegisterCallback<PointerUpEvent>(Q1STOP, TrickleDown.TrickleDown);
        Q2dec.RegisterCallback<PointerDownEvent>(Q2Decrease, TrickleDown.TrickleDown); Q2dec.RegisterCallback<PointerUpEvent>(Q2STOP, TrickleDown.TrickleDown);
        Q2inc.RegisterCallback<PointerDownEvent>(Q2Increase, TrickleDown.TrickleDown); Q2inc.RegisterCallback<PointerUpEvent>(Q2STOP, TrickleDown.TrickleDown);
        Q3dec.RegisterCallback<PointerDownEvent>(Q3Decrease, TrickleDown.TrickleDown); Q3dec.RegisterCallback<PointerUpEvent>(Q3STOP, TrickleDown.TrickleDown);
        Q3inc.RegisterCallback<PointerDownEvent>(Q3Increase, TrickleDown.TrickleDown); Q3inc.RegisterCallback<PointerUpEvent>(Q3STOP, TrickleDown.TrickleDown);
        Q4dec.RegisterCallback<PointerDownEvent>(Q4Decrease, TrickleDown.TrickleDown); Q4dec.RegisterCallback<PointerUpEvent>(Q4STOP, TrickleDown.TrickleDown);
        Q4inc.RegisterCallback<PointerDownEvent>(Q4Increase, TrickleDown.TrickleDown); Q4inc.RegisterCallback<PointerUpEvent>(Q4STOP, TrickleDown.TrickleDown);
        Q5dec.RegisterCallback<PointerDownEvent>(Q5Decrease, TrickleDown.TrickleDown); Q5dec.RegisterCallback<PointerUpEvent>(Q5STOP, TrickleDown.TrickleDown);
        Q5inc.RegisterCallback<PointerDownEvent>(Q5Increase, TrickleDown.TrickleDown); Q5inc.RegisterCallback<PointerUpEvent>(Q5STOP, TrickleDown.TrickleDown);
        Q6dec.RegisterCallback<PointerDownEvent>(Q6Decrease, TrickleDown.TrickleDown); Q6dec.RegisterCallback<PointerUpEvent>(Q6STOP, TrickleDown.TrickleDown);
        Q6inc.RegisterCallback<PointerDownEvent>(Q6Increase, TrickleDown.TrickleDown); Q6inc.RegisterCallback<PointerUpEvent>(Q6STOP, TrickleDown.TrickleDown);
        allBodies = ab.GetComponentsInChildren<ArticulationBody>(); slider = GetComponentInChildren<Slider_Manager>();
        rotationSpeed = slider.JointSpeed;
        Qinput = new float[]{0f, 0f, 0f, 0f, 0f, 0f};
    }
    void FixedUpdate()
    {
        rotationSpeed = slider.JointSpeed;
        for (int i = 1; i < 7; i++)
        {
            ArticulationDrive drive = allBodies[i].xDrive;
            drive.target = math.clamp(drive.target + Qinput[i-1] * rotationSpeed * Time.fixedDeltaTime, -360, 360);
            allBodies[i].xDrive = drive;
        }
    }
    void Q1Decrease(PointerDownEvent evt)   {Qinput[0] = -1f;}
    void Q1Increase(PointerDownEvent evt)   {Qinput[0] = 1f;}
    void Q1STOP(PointerUpEvent evt)         {Qinput[0] = 0f;}
    void Q2Decrease(PointerDownEvent evt)   {Qinput[1] = -1f;}
    void Q2Increase(PointerDownEvent evt)   {Qinput[1] = 1f;}
    void Q2STOP(PointerUpEvent evt)         {Qinput[1] = 0f;}
    void Q3Decrease(PointerDownEvent evt)   {Qinput[2] = -1f;}
    void Q3Increase(PointerDownEvent evt)   {Qinput[2] = 1f;}
    void Q3STOP(PointerUpEvent evt)         {Qinput[2] = 0f;}
    void Q4Decrease(PointerDownEvent evt)   {Qinput[3] = -1f;}
    void Q4Increase(PointerDownEvent evt)   {Qinput[3] = 1f;}
    void Q4STOP(PointerUpEvent evt)         {Qinput[3] = 0f;}
    void Q5Decrease(PointerDownEvent evt)   {Qinput[4] = -1f;}
    void Q5Increase(PointerDownEvent evt)   {Qinput[4] = 1f;}
    void Q5STOP(PointerUpEvent evt)         {Qinput[4] = 0f;}
    void Q6Decrease(PointerDownEvent evt)   {Qinput[5] = -1f;}
    void Q6Increase(PointerDownEvent evt)   {Qinput[5] = 1f;}
    void Q6STOP(PointerUpEvent evt)         {Qinput[5] = 0f;}
}
