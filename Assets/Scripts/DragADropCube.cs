using UnityEngine;
// -------------------------------------------------------------------------------------------------------------------------------------- //
// This script allows the user to move the cube by left-clicking on it, to set it in a specific position. However the script doesn't      //
// manage the cube rotation.                                                                                                              //
// -------------------------------------------------------------------------------------------------------------------------------------- //
public class DragADropCube : MonoBehaviour
{
    public Camera_Manager CM;
    Vector3 mousePos; 
    // RigidBody and Collider of the cube :
    Rigidbody rb; Collider col;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }
    private Vector3 GetMousePos()
    {
        return CM.cameras[CM.nbcam].WorldToScreenPoint(transform.position);
    }
    private void OnMouseDown()
    {
        mousePos = Input.mousePosition - GetMousePos();
    }
    private void OnMouseDrag()
    {
        transform.position = CM.cameras[CM.nbcam].ScreenToWorldPoint(Input.mousePosition - mousePos);
        rb.useGravity = false;
        // col.isTrigger = true;
    }
    // Re-initialize the RigidBody and the Collider when the click is released :
    private void OnMouseUp()
    {
        rb.useGravity = true;
        // col.isTrigger = false;
    }
}
