using UnityEngine;
using UnityEngine.UIElements;
// -------------------------------------------------------------------------------------------------------------------------------------- //
// This script allows the user to open and close the clamp with the related buttons.                                                      //
// -------------------------------------------------------------------------------------------------------------------------------------- //
public class GripperActivator : MonoBehaviour
{
    private VisualElement root; private Slider_Manager sldMan; private Button GripButton; private Button ReleaseButton;
    public ArticulationBody finger1; public ArticulationBody finger2;  
    private float direction = 0f; private float rotationSpeed = 0f;
    public bool movableFinger;
    void Start()
    {
        root = GetComponent<UIDocument>().rootVisualElement; sldMan = GetComponentInChildren<Slider_Manager>();
        GripButton = root.Q<Button>("ActivateGrip"); ReleaseButton = root.Q<Button>("ActivateRelease");

        GripButton.RegisterCallback<PointerDownEvent>(CloseClamp, TrickleDown.TrickleDown); GripButton.RegisterCallback<PointerUpEvent>(StopClampMovement, TrickleDown.TrickleDown);
        ReleaseButton.RegisterCallback<PointerDownEvent>(OppenClamp, TrickleDown.TrickleDown); ReleaseButton.RegisterCallback<PointerUpEvent>(StopClampMovement, TrickleDown.TrickleDown);
    }

    void FixedUpdate()
    {
        if (direction == 0f) return;

        if (movableFinger)
        {
            rotationSpeed = sldMan.TrSpeed;
            ArticulationDrive drive1 = finger1.yDrive; ArticulationDrive drive2 = finger2.yDrive;

            // Calculate the new target angle based on time and speed
            float newTarget1 = drive1.target + (direction * rotationSpeed * Time.fixedDeltaTime);
            float newTarget2 = drive2.target - (direction * rotationSpeed * Time.fixedDeltaTime);

            // Clamp the target to stay within the joint's limits
            newTarget1 = Mathf.Clamp(newTarget1, drive1.lowerLimit, drive1.upperLimit);
            newTarget2 = Mathf.Clamp(newTarget2, drive2.lowerLimit, drive2.upperLimit);

            // Apply the new target back to the ArticulationBody
            drive1.target = newTarget1; drive2.target = newTarget2;
            finger1.yDrive = drive1;    finger2.yDrive = drive2;
        }
    }

    void CloseClamp(PointerDownEvent evt) 
    {
        direction = 1f;
    }
    void OppenClamp(PointerDownEvent evt) 
    {
        direction = -1f;
    }
    void StopClampMovement(PointerUpEvent evt)
    {
        direction = 0;
    }
    public float GetClampWidth()
    {
        return 0.7f - (finger1.yDrive.target - finger2.yDrive.target);
    }
}
