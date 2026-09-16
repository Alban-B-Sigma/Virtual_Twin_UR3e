using UnityEngine;
using System;
using System.IO;


public class Excel_Test : MonoBehaviour
{

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {   
        File.WriteAllText("File_created_"+DateTime.Now.Day + DateTime.Now.Month + DateTime.Now.Year + "_" +
                          DateTime.Now.Hour + DateTime.Now.Minute + DateTime.Now.Second + ".script", "Ceci est un test");

        // StreamWriter sw = new StreamWriter("C:/Users/alban/Virtual_Twin_UR3e/Assets/Test.script");
        // sw.WriteLine("Ceci est une modification");
        // sw.Close();
    }
}
