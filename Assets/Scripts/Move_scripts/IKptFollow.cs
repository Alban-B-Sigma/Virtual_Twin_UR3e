using UnityEngine;
// -------------------------------------------------------------------------------------------------------------------------------- //
// This script fixs the IK point to the TCP, it is used when the MoveL mode is disabled.                                            //
// -------------------------------------------------------------------------------------------------------------------------------- //
public class IKptFollow : MonoBehaviour
{
    public Transform IKpoint; public Transform TCP;
    private Vector3 offsetPos; private Quaternion offsetOrientation;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        offsetPos = IKpoint.position - TCP.position;
        offsetOrientation = IKpoint.rotation * Quaternion.Inverse(TCP.rotation);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        IKpoint.position = TCP.position + offsetPos;
        IKpoint.rotation = TCP.rotation * offsetOrientation;
    }
}
