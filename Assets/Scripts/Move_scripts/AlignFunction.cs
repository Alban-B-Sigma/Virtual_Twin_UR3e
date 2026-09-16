using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

// -------------------------------------------------------------------------------------------------------------------------------------- //
// This script try to reproduce the "Align" feature of the cobot, by aligning the z axis of the TCP (or of the IK point) with the nearest //
//  base axis. This script could be improved by making the robot reaching its targeted position progressively, not instantely as now.     //
// -------------------------------------------------------------------------------------------------------------------------------------- //
public class AlignFunction : MonoBehaviour
{
    private VisualElement root; private Button AlignButton;
    public Transform IKpt; public Transform basePos;
    private float aX; private float aXneg; private float aY; private float aYneg; private float aZ; private float aZneg; 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        root = GetComponent<UIDocument>().rootVisualElement; AlignButton = root.Q<Button>("AlignButton");
        AlignButton.RegisterCallback<ClickEvent>(AlignNearest);
    }

    // Calculate each frame the angle between the TCP z-axis and each base axis :
    void Update()
    {
        aX = Vector3.SignedAngle(IKpt.forward, basePos.right, basePos.right); aXneg = Vector3.SignedAngle(IKpt.forward, -basePos.right, -basePos.right);
        aY = Vector3.SignedAngle(IKpt.forward, basePos.up, basePos.up); aYneg = Vector3.SignedAngle(IKpt.forward, -basePos.up, -basePos.up);
        aZ = Vector3.SignedAngle(IKpt.forward, basePos.forward, basePos.forward); aZneg = Vector3.SignedAngle(IKpt.forward, -basePos.forward, -basePos.forward);
    }
    // Check axis is the nearest, and apply the new position to the IK point, adapted to the Unity coordinate convention:
    void AlignNearest(ClickEvent clickEvent)
    {
        float[] allAbsAngles = {math.abs(aX), math.abs(aXneg), math.abs(aY), math.abs(aYneg), math.abs(aZ), math.abs(aZneg)};
        // Debug.Log(Mathf.Min(allAbsAngles).ToString("0.000"));
        // for (int i = 0; i < allAbsAngles.Length; i++) if (Mathf.Min(allAbsAngles) == math.abs(allAbsAngles[i])){Debug.Log(i);}
        if (Mathf.Min(allAbsAngles) == math.abs(aX))
        {
            IKpt.forward = basePos.right;
        }
        if (Mathf.Min(allAbsAngles) == math.abs(aXneg))
        {
            IKpt.forward = -basePos.right;
        }
        if (Mathf.Min(allAbsAngles) == math.abs(aY))
        {
            IKpt.forward = basePos.up;
        }
        if (Mathf.Min(allAbsAngles) == math.abs(aYneg))
        {
            IKpt.forward = -basePos.up;
        }
        if (Mathf.Min(allAbsAngles) == math.abs(aZ))
        {
            IKpt.forward = basePos.forward;
        }
        if (Mathf.Min(allAbsAngles) == math.abs(aZneg))
        {
            IKpt.forward = -basePos.forward;
        }
    }
}
