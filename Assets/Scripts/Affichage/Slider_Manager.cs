using UnityEngine;
using UnityEngine.UIElements;
// ---------------------------------------------------------------------------------------------------------------------------------//
// This script manages the slider that is used to variate the general speed of the robot. It also display its value in real time.   //
// The RotSpeed variable is used in other programs to obtain the slider value.                                                      //
// ---------------------------------------------------------------------------------------------------------------------------------//
public class Slider_Manager : MonoBehaviour
{
    private Slider slider; private Label sliderValue;
    public float TrSpeed = 0f; public float RotSpeed = 0f; public float JointSpeed = 0f;
    private float coeff = 0.03f;
    void Start()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        slider = root.Q<Slider>("GenSpeed");
        sliderValue = root.Q<Label>("GenSpeedValue");
        slider.visible = true; 
        
        TrSpeed = slider.value * coeff;
        RotSpeed = TrSpeed / 4f; JointSpeed = TrSpeed * 5f; 
        sliderValue.text = slider.value.ToString("0.00");
    }

    void Update()
    {
        slider.RegisterValueChangedCallback(evt => 
        {
            TrSpeed = evt.newValue*coeff;
            RotSpeed = TrSpeed / 3f; JointSpeed = TrSpeed * 16f;
            sliderValue.text = evt.newValue.ToString("0.00");
            
        });
    }
}
