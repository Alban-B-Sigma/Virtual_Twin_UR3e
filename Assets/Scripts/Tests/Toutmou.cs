using UnityEngine;
// -------------------------------------------------------------------------------------------------------------------------------------- //
// This script was an attempt to make the TCP following the mouse trigger when the left click was pressed, without success.               //
// -------------------------------------------------------------------------------------------------------------------------------------- //
public class Toutmou : MonoBehaviour
{
    private Camera mainCamera;
    private Vector3 mOffset;
    private float mZCoord;
    private bool isDragging = false;

    [Header("Référence à la cible IK")]
    public Transform ikTarget; 
    public IKCalculator solverScript;

    void Start()
    {
        mainCamera = Camera.main;
        if (ikTarget != null && solverScript != null)
        {
            // ikTarget.position = solverScript.endEffector.position;
            // ikTarget.rotation = solverScript.endEffector.rotation;
        }
    }
    void Update()
    {
        if (!isDragging)
        {
            // ikTarget.position = solverScript.endEffector.position;
            // ikTarget.rotation = solverScript.endEffector.rotation;
        }
    } 

    void OnMouseDown()
    {
        mZCoord = mainCamera.WorldToScreenPoint(transform.position).z;
        mOffset = transform.position - GetMouseWorldPos();
        isDragging = true;
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = mZCoord;
        return mainCamera.ScreenToWorldPoint(mousePoint);
    }

    void OnMouseDrag()
    {
        if (isDragging && ikTarget != null)
        {
            ikTarget.position = GetMouseWorldPos() + mOffset;
        }
    }

    void OnMouseUp()
    {
        isDragging = false;
    }
    public void SetJointTargetDegree(ArticulationBody joint, float targetAngleDeg)
    {
        ArticulationDrive drive = joint.xDrive;
        drive.target = targetAngleDeg;
        joint.xDrive = drive; // Réassignation nécessaire sous Unity
    }
}

