using UnityEngine;
using UnityEngine.InputSystem; 
using UnityEngine.UIElements;
// -------------------------------------------------------------------------------------------------------------------------------------- //
// This script is used to pick and release the cube, even if the gripper fingers are not movable, or if they struggles to do it properly, //
// by fixing the cube to the TCP, as the RobotDK simulation can do it.                                                                    //
// -------------------------------------------------------------------------------------------------------------------------------------- //
public class PaPcube : MonoBehaviour
{
    public GameObject cube; private Rigidbody cubeRb; public UIDocument UiDoc; private VisualElement root;
    private Label GripMessage; private Collider cubeCollider; private Collider tcpCollider;
    private bool isGrabbed = false;
    public bool wantToGrip = false; public bool wantToRelease = false;

    void Start()
    {
        root = UiDoc.rootVisualElement; GripMessage = root.Q<Label>("GripMessage");
        GripMessage.visible = false;
        cubeRb = cube.GetComponent<Rigidbody>();
        cubeCollider = cube.GetComponent<Collider>();
        tcpCollider = GetComponent<Collider>();
    }

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject == cube)
        {
            GripMessage.visible = true;

            // e Key : Grip the cube
            if ((Keyboard.current.eKey.wasPressedThisFrame || wantToGrip) && !isGrabbed)
            {
                GrabCube();
                // Debug.Log("cube grapped");
            }
        }
    }

    void Update()
    {
        tcpCollider.isTrigger = true;
        // r Key : Release the cube
        if ((Keyboard.current.rKey.wasPressedThisFrame || wantToRelease) && isGrabbed)
        {
            ReleaseCube();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject == cube && !isGrabbed)
        {
            GripMessage.visible = false;
        }
    }

    private void GrabCube()
    {
        isGrabbed = true;
        
        // Desactivate the cube physic to avoid the unwanted collisions and forces
        cubeRb.useGravity = false;
        cubeRb.linearVelocity = Vector3.zero;
        cubeRb.angularVelocity = Vector3.zero;

        Physics.IgnoreCollision(cubeCollider, tcpCollider, true);
        
        cubeCollider.isTrigger = true;
        // Attache the cube to the TCP
        cube.transform.SetParent(transform);
        GripMessage.visible = false;
        
    }

    private void ReleaseCube()
    {
        isGrabbed = false;

        Physics.IgnoreCollision(cubeCollider, tcpCollider, true);
        cubeCollider.isTrigger = true;

        // Detache the cube
        cube.transform.SetParent(null);

        // Activate again the physics
        cubeRb.useGravity = true;

        GripMessage.visible = false;
        Invoke(nameof(ResetReleaseTrigger), 0.1f);
    }

    private void ResetReleaseTrigger()
    {
        cubeCollider.isTrigger = false;
        Physics.IgnoreCollision(cubeCollider, tcpCollider, false);
        cubeRb.linearVelocity = Vector3.zero;
        cubeRb.angularVelocity = Vector3.zero;
        cubeRb.useGravity = true;
    }
}
