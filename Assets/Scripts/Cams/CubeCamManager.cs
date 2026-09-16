using UnityEngine;
// ---------------------------------------------------------------------------------------------------------------------------------//
// This script is similar to "MainR1CamManager" but for the cube camera, only rotations are allowed.                                //
// ---------------------------------------------------------------------------------------------------------------------------------//
public class CubeCamManager : MonoBehaviour
{
    public GameObject Cube; public Camera_Manager CamSelect;
    public float RotSpeed = 5f; public float TranSpeed = 1f;
    void LateUpdate()
    {
        transform.position = Cube.transform.position;
    }
    void Update() 
    {
        if (CamSelect.nbcam == 1) // Verify it is the camera linked to the cube that is selected
        {
            if (Input.GetMouseButton(1)) { // Right click to orbit
            float rotationX = Input.GetAxis("Mouse X") * RotSpeed;
            float rotationY = Input.GetAxis("Mouse Y") * RotSpeed;
            
            transform.RotateAround(Cube.transform.position, -Vector3.up, rotationX);
            transform.RotateAround(Cube.transform.position, transform.right, rotationY);
            }
            if (Input.GetMouseButton(2)) { // Middle click to "barrel-roll"
                float rotationZ = Input.GetAxis("Mouse X") * RotSpeed;
                
                transform.RotateAround(Cube.transform.position, transform.forward, rotationZ);
            }
        }
    }
}
