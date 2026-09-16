using UnityEngine;
using UnityEngine.InputSystem;
using System;
// ---------------------------------------------------------------------------------------------------------------------------------//
// With this script, the user is able to switch cameras, to have a better/different point of vue, by pressing "left Ctrl" + "+" or  //
// "left Ctrl" + "-".                                                                                                               //
// ---------------------------------------------------------------------------------------------------------------------------------//
public class Camera_Manager : MonoBehaviour
{
    public Camera[] cameras;
    public int nbcam = 0; private int nbcaminit = -1; 
    public void Start()
    {
        SelectCamera();
    }
    void Update()
    {
        nbcaminit = nbcam;
        if (Keyboard.current.numpadPlusKey.wasPressedThisFrame && Keyboard.current.leftCtrlKey.isPressed)  nbcam += 1;
        if (Keyboard.current.numpadMinusKey.wasPressedThisFrame && Keyboard.current.leftCtrlKey.isPressed)  nbcam -= 1;

        if (nbcam < 0) nbcam = cameras.Length - 1;
        if (nbcam >= cameras.Length) nbcam = 0;

        Invoke(nameof(SelectCamera), 0.5f);
    }
    
    public void SelectCamera()
    {
        if (nbcam != nbcaminit)
        {
            // Disable all cameras
            foreach (Camera cam in cameras)
            { cam.enabled = false; }
            
            // Enable the selected camera
            if (nbcam >= 0 && nbcam < cameras.Length)
            { cameras[nbcam].enabled = true; }
        }
    }
    
}
