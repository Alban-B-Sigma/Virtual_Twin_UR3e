using UnityEngine;
using System.Collections.Generic;
using System;
// -------------------------------------------------------------------------------------------------------------------------------------- //
// This script contains the functions that extract the Joints angles from Unity in different formats (List of doubles or Array of floats) //
// -------------------------------------------------------------------------------------------------------------------------------------- //
public class Cobot_Command : MonoBehaviour
{
    public ArticulationBody ab;
    void Start()
    {
        ArticulationBody[] allBodies = ab.GetComponentsInChildren<ArticulationBody>();
        // foreach(ArticulationBody ab in allBodies){Debug.Log(ab.name + " : " + ab.dofCount.ToString());}
    }
    public List<double> GetAllJointAngles()
    {
        List<double> Q = new List<double>{};
        if (ab == null) Debug.Log("No base");
        ArticulationBody[] allBodies = ab.GetComponentsInChildren<ArticulationBody>();
        
        for (int i = 1; i < 7; i++)
        {
            Q.Add(Mathf.Deg2Rad * Math.Round(allBodies[i].xDrive.target, 4));
        }
        return Q;
    }
    public float[] GetAllJointAnglesFloat()
    {
        float[] Q = new float[6];
        if (ab == null) Debug.Log("No base");
        ArticulationBody[] allBodies = ab.GetComponentsInChildren<ArticulationBody>();
        for (int i = 1; i < Q.Length+1; i++)
        {
            Q[i-1] = Mathf.Deg2Rad * allBodies[i].xDrive.target;
        }
        return Q;
    }
}