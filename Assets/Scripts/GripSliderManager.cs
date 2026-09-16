using UnityEngine;
using UnityEngine.UIElements;
// -------------------------------------------------------------------------------------------------------------------------------------- //
// This script manages the 2 sliders of the Grip Node window, in a same way as the general speed slider is managed.                       //
// -------------------------------------------------------------------------------------------------------------------------------------- //
public class GripSliderManager : MonoBehaviour
{
    private Slider forceSlider; private Slider speedSlider; private Label forceSliderValue; private Label speedSliderValue;
    void Start()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        forceSlider = root.Q<Slider>("GripForce"); speedSlider = root.Q<Slider>("GripSpeed");
        forceSliderValue = root.Q<Label>("GripForceValue"); speedSliderValue = root.Q<Label>("GripSpeedValue");
        forceSlider.value = forceSlider.highValue; forceSliderValue.text = forceSlider.value.ToString("0.0");
        speedSlider.value = speedSlider.highValue; speedSliderValue.text = speedSlider.value.ToString("0.0");
    }

    void Update()
    {
        forceSlider.RegisterValueChangedCallback(evt => 
        {
            forceSliderValue.text = evt.newValue.ToString("0.0");
        });
        speedSlider.RegisterValueChangedCallback(evt => 
        {
            speedSliderValue.text = evt.newValue.ToString("0.0");
        });
    }
}
