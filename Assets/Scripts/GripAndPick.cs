using UnityEngine;
// -------------------------------------------------------------------------------------------------------------------------------------- //
// This script try to manage the collision between the fingers and the cube when the robot try to grip a cube, to avoid the fingers going //
// inside the cube.                                                                                                                       //
// -------------------------------------------------------------------------------------------------------------------------------------- //
public class GripAndPick : MonoBehaviour
{
    public GameObject cube; public GameObject Print; public GripperActivator GpAct;
    public ArticulationBody finger1; public ArticulationBody finger2; 
    private float f1UpLim; private float f2LoLim;

    void Start()
    {
        Print.SetActive(false);
        ArticulationDrive drive1 = finger1.yDrive; ArticulationDrive drive2 = finger2.yDrive;
        f1UpLim = drive1.upperLimit; f2LoLim = drive2.lowerLimit;
    }
    void OnTriggerEnter(Collider other)
        {
            if (other.gameObject == cube)
            {
                Debug.Log("Collision between fingers and cube");
                ArticulationDrive drive1 = finger1.yDrive; ArticulationDrive drive2 = finger2.yDrive;
                drive1.upperLimit = drive1.target; drive2.lowerLimit = drive2.target;
            }
        }
    void OnTriggerExit(Collider other)
        {
            if (other.gameObject == cube)
            {
                Debug.Log("End of collision between fingers and cube");
                ArticulationDrive drive1 = finger1.yDrive; ArticulationDrive drive2 = finger2.yDrive;
                drive1.upperLimit = f1UpLim; drive2.lowerLimit = f2LoLim;
            }
        }
}
