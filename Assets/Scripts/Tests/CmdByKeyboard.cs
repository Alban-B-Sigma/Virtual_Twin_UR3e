using UnityEngine;
using UnityEngine.InputSystem;
// -------------------------------------------------------------------------------------------------------------------------------- //
// This script was used for commanding the joints of the 2 robot with the keyboard. One key is assigned to each joint, it allow to  //
// move the articulation in one direction when pressed alone, and in the other direction when pressed with the left Shift key.      //
// -------------------------------------------------------------------------------------------------------------------------------- //
public class CmdByKeyboard : MonoBehaviour
{
    public Slider_Manager sldMan; private ArticulationBody ab;
    private string jointName; private float rotationSpeed = 0f; private float direction = 0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ab = GetComponent<ArticulationBody>();
        if (ab == null) Debug.LogError("Need an ArticulationBody component on the same GameObject!");
        jointName = ab.name;
    }

    // Check if the keys are pressed and modify the "direction" variable depending of the joint :
    void Update()
    {
        rotationSpeed = sldMan.RotSpeed * 5;
        direction = 0f;
        if (jointName == "Shoulder_R1")
        {
            if (Keyboard.current.aKey.isPressed && ! Keyboard.current.leftShiftKey.isPressed)  direction = 1f;
            if (Keyboard.current.aKey.isPressed && Keyboard.current.leftShiftKey.isPressed)    direction = -1f;
        }
        else if (jointName == "Elbow_R1")
        {
            if (Keyboard.current.sKey.isPressed && ! Keyboard.current.leftShiftKey.isPressed)  direction = 1f;
            if (Keyboard.current.sKey.isPressed && Keyboard.current.leftShiftKey.isPressed)    direction = -1f;
        }
        else if (jointName == "Wrist1_R1")
        {
            if (Keyboard.current.dKey.isPressed && ! Keyboard.current.leftShiftKey.isPressed)  direction = 1f;
            if (Keyboard.current.dKey.isPressed && Keyboard.current.leftShiftKey.isPressed)    direction = -1f;
        }
        else if (jointName == "Wrist2_R1")
        {
            if (Keyboard.current.fKey.isPressed && ! Keyboard.current.leftShiftKey.isPressed)  direction = 1f;
            if (Keyboard.current.fKey.isPressed && Keyboard.current.leftShiftKey.isPressed)    direction = -1f;
        }
        else if (jointName == "Wrist3_R1")
        {
            if (Keyboard.current.gKey.isPressed && ! Keyboard.current.leftShiftKey.isPressed)  direction = 1f;
            if (Keyboard.current.gKey.isPressed && Keyboard.current.leftShiftKey.isPressed)    direction = -1f;
        }
        else if (jointName == "Effector_R1")
        {
            if (Keyboard.current.hKey.isPressed && ! Keyboard.current.leftShiftKey.isPressed)  direction = 1f;
            if (Keyboard.current.hKey.isPressed && Keyboard.current.leftShiftKey.isPressed)    direction = -1f;
        }
        else if (jointName == "Shoulder_R2")
        {
            if (Keyboard.current.zKey.isPressed && ! Keyboard.current.leftShiftKey.isPressed)  direction = 1f;
            if (Keyboard.current.zKey.isPressed && Keyboard.current.leftShiftKey.isPressed)    direction = -1f;
        }
        else if (jointName == "Elbow_R2")
        {
            if (Keyboard.current.xKey.isPressed && ! Keyboard.current.leftShiftKey.isPressed)  direction = 1f;
            if (Keyboard.current.xKey.isPressed && Keyboard.current.leftShiftKey.isPressed)    direction = -1f;
        }
        else if (jointName == "Wrist1_R2")
        {
            if (Keyboard.current.cKey.isPressed && ! Keyboard.current.leftShiftKey.isPressed)  direction = 1f;
            if (Keyboard.current.cKey.isPressed && Keyboard.current.leftShiftKey.isPressed)    direction = -1f;
        }
        else if (jointName == "Wrist2_R2")
        {
            if (Keyboard.current.vKey.isPressed && ! Keyboard.current.leftShiftKey.isPressed)  direction = 1f;
            if (Keyboard.current.vKey.isPressed && Keyboard.current.leftShiftKey.isPressed)    direction = -1f;
        }
        else if (jointName == "Wrist3_R2")
        {
            if (Keyboard.current.bKey.isPressed && ! Keyboard.current.leftShiftKey.isPressed)  direction = 1f;
            if (Keyboard.current.bKey.isPressed && Keyboard.current.leftShiftKey.isPressed)    direction = -1f;
        }
        else if (jointName == "Effector_R2")
        {
            if (Keyboard.current.nKey.isPressed && ! Keyboard.current.leftShiftKey.isPressed)  direction = 1f;
            if (Keyboard.current.nKey.isPressed && Keyboard.current.leftShiftKey.isPressed)    direction = -1f;
        }
    }
    // Apply the input with the "direction" variable to the articulation :
    void FixedUpdate()
    {
        if (ab == null || direction == 0f) return;
        ArticulationDrive drive = ab.xDrive;
        
        // Calculate the new target angle based on time and speed
        float newTarget = drive.target + (direction * rotationSpeed * Time.fixedDeltaTime);

        // Clamp the target to stay within the joint's limits
        newTarget = Mathf.Clamp(newTarget, drive.lowerLimit, drive.upperLimit);

        // Apply the new target back to the ArticulationBody
        drive.target = newTarget;
        ab.xDrive = drive;
    }
}
