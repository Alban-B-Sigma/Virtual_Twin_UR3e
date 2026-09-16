using UnityEngine;
// -----------------------------------------------------------------------------------------------------------------------------------------//
// This script allow the user to move the main camera attached to the robot1. To move the camera, use the right-click and the middle-click  //
// (scroll wheel), the different movement are explained below.                                                                              //
// -----------------------------------------------------------------------------------------------------------------------------------------//
public class MainR1CamManager : MonoBehaviour
{
    public GameObject R1; public Camera_Manager CamSelect;
    public float RotSpeed = 5f; public float TranSpeed = 1f;

    void Update() 
    {
        if (CamSelect.nbcam == 0) // Verify it is the main camera that is selected
        {
            if (Input.GetMouseButton(1) && !Input.GetMouseButton(2))// Right click to orbit
            {
                float rotationX = - Input.GetAxis("Mouse X") * RotSpeed;
                float rotationY = - Input.GetAxis("Mouse Y") * RotSpeed;
                
                transform.RotateAround(R1.transform.position, -Vector3.up, rotationX);
                transform.RotateAround(R1.transform.position, transform.right, rotationY);
            }
            if (Input.GetMouseButton(2) && Input.GetMouseButton(1)) // Middle click + right click to barrel roll
            {
                float rotationZ = - Input.GetAxis("Mouse X") * RotSpeed;

                transform.RotateAround(R1.transform.position, transform.up, rotationZ);
            }
            if (Input.GetMouseButton(2) && !Input.GetMouseButton(1))// Middle click to translate
            {
                float TrX = - Input.GetAxis("Mouse X") * TranSpeed;
                float TrY = - Input.GetAxis("Mouse Y") * TranSpeed;
                transform.Translate(Vector3.forward * TrY);
                transform.Translate(Vector3.right * TrX);
            }
            if (Input.GetAxis("Mouse ScrollWheel") != 0)            // Use the scroll wheel to go forward and backward
            {
                float forward = - Input.GetAxis("Mouse ScrollWheel") * TranSpeed;
                transform.Translate(Vector3.up * forward);
            }
        }
    }
}

