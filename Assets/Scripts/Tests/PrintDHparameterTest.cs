using System.Threading;
using UnityEngine;
// -------------------------------------------------------------------------------------------------------------------------------------- //
// This script try to get the length between each joint, to verify the Unity Denavit-Hartenberg parameters are the sames as on the real   //
// cobots.                                                                                                                                //
// -------------------------------------------------------------------------------------------------------------------------------------- //
public class PrintDHparameterTest : MonoBehaviour
{
    public GameObject joint;

    void Start()
    {
        var ab = joint.GetComponent<ArticulationBody>();
        var transf = joint.GetComponent<Transform>();
        var dist = ab.anchorPosition + transf.localPosition;
        var parent = joint.GetComponentInParent<Transform>();
        var reference = parent.position;
        Debug.Log("Pos of the joint : " + (dist.x).ToString("0.00") + "; "+ (dist.y).ToString("0.00") + "; "+ (dist.z).ToString("0.00") + ";");
    }

}
