using UnityEngine;
using UnityEngine.UIElements;
// -------------------------------------------------------------------------------------------------------------------------------- //
// This script manages the buttons that allow the user to move the IK point manually. Each button correspond to a specific move.    //
// -------------------------------------------------------------------------------------------------------------------------------- //
public class IKMove : MonoBehaviour
{
    private Button TZplus; private Button TZmoins; private Button RZplus; private Button RZmoins;
    private Button TXplus; private Button TXmoins; private Button RXplus; private Button RXmoins;
    private Button TYplus; private Button TYmoins; private Button RYplus; private Button RYmoins;

    private DropdownField Feature; private Slider_Manager Slider;

    public Transform IKpoint; private float speed; private Vector3 moveInput; private Vector3 rotInput;
    void Start()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        TZplus = root.Q<Button>("TZplus"); TZmoins = root.Q<Button>("TZmoins"); RZplus = root.Q<Button>("RZplus"); RZmoins = root.Q<Button>("RZmoins");
        TXplus = root.Q<Button>("TXplus"); TXmoins = root.Q<Button>("TXmoins"); RXplus = root.Q<Button>("RXplus"); RXmoins = root.Q<Button>("RXmoins");
        TYplus = root.Q<Button>("TYplus"); TYmoins = root.Q<Button>("TYmoins"); RYplus = root.Q<Button>("RYplus"); RYmoins = root.Q<Button>("RYmoins");
        Feature = root.Q<DropdownField>("ChooseFeature"); Slider = GetComponentInChildren<Slider_Manager>();

        moveInput = Vector3.zero;
        rotInput =  Vector3.zero;

        TZplus.RegisterCallback<PointerDownEvent>(TZplusActiv, TrickleDown.TrickleDown); TZplus.RegisterCallback<PointerUpEvent>(TZplusDisactiv, TrickleDown.TrickleDown);
        TZmoins.RegisterCallback<PointerDownEvent>(TZmoinsActiv, TrickleDown.TrickleDown); TZmoins.RegisterCallback<PointerUpEvent>(TZmoinsDisactiv, TrickleDown.TrickleDown);
        TXplus.RegisterCallback<PointerDownEvent>(TXplusActiv, TrickleDown.TrickleDown); TXplus.RegisterCallback<PointerUpEvent>(TXplusDisactiv, TrickleDown.TrickleDown);
        TXmoins.RegisterCallback<PointerDownEvent>(TXmoinsActiv, TrickleDown.TrickleDown); TXmoins.RegisterCallback<PointerUpEvent>(TXmoinsDisactiv, TrickleDown.TrickleDown);
        TYplus.RegisterCallback<PointerDownEvent>(TYplusActiv, TrickleDown.TrickleDown); TYplus.RegisterCallback<PointerUpEvent>(TYplusDisactiv, TrickleDown.TrickleDown);
        TYmoins.RegisterCallback<PointerDownEvent>(TYmoinsActiv, TrickleDown.TrickleDown); TYmoins.RegisterCallback<PointerUpEvent>(TYmoinsDisactiv, TrickleDown.TrickleDown);
        
        RZplus.RegisterCallback<PointerDownEvent>(RZplusActiv, TrickleDown.TrickleDown); RZplus.RegisterCallback<PointerUpEvent>(RZplusDisactiv, TrickleDown.TrickleDown);
        RZmoins.RegisterCallback<PointerDownEvent>(RZmoinsActiv, TrickleDown.TrickleDown); RZmoins.RegisterCallback<PointerUpEvent>(RZmoinsDisactiv, TrickleDown.TrickleDown);
        RXplus.RegisterCallback<PointerDownEvent>(RXplusActiv, TrickleDown.TrickleDown); RXplus.RegisterCallback<PointerUpEvent>(RXplusDisactiv, TrickleDown.TrickleDown);
        RXmoins.RegisterCallback<PointerDownEvent>(RXmoinsActiv, TrickleDown.TrickleDown); RXmoins.RegisterCallback<PointerUpEvent>(RXmoinsDisactiv, TrickleDown.TrickleDown);
        RYplus.RegisterCallback<PointerDownEvent>(RYplusActiv, TrickleDown.TrickleDown); RYplus.RegisterCallback<PointerUpEvent>(RYplusDisactiv, TrickleDown.TrickleDown);
        RYmoins.RegisterCallback<PointerDownEvent>(RYmoinsActiv, TrickleDown.TrickleDown); RYmoins.RegisterCallback<PointerUpEvent>(RYmoinsDisactiv, TrickleDown.TrickleDown);

    }

    void FixedUpdate()
    {
        
        Vector3 movement = moveInput * Slider.TrSpeed * Time.fixedDeltaTime;
        Vector3 rot = rotInput;
        if (Feature.value == "Base")
        {
            IKpoint.Translate(movement, Space.World);
            IKpoint.RotateAround(IKpoint.position, rot, Slider.RotSpeed);
        }
        else if (Feature.value == "Tool")
        {
            movement = new Vector3(movement.z, movement.x, movement.y);
            rot = new Vector3(rot.z, rot.x, rot.y);
            IKpoint.Translate(movement, Space.Self);
            IKpoint.Rotate(rot, Slider.RotSpeed);
        }
    }
    
    private void TZplusActiv(PointerDownEvent evt)  { moveInput.y = 1f; }
    private void TZplusDisactiv(PointerUpEvent evt) { moveInput.y = 0f; }
    private void TZmoinsActiv(PointerDownEvent evt) { moveInput.y = -1f;}
    private void TZmoinsDisactiv(PointerUpEvent evt){ moveInput.y = 0f; }

    private void TXplusActiv(PointerDownEvent evt)  { moveInput.z = 1f; }
    private void TXplusDisactiv(PointerUpEvent evt) { moveInput.z = 0f; }
    private void TXmoinsActiv(PointerDownEvent evt) { moveInput.z = -1f;}
    private void TXmoinsDisactiv(PointerUpEvent evt){ moveInput.z = 0f; }

    private void TYplusActiv(PointerDownEvent evt)  { moveInput.x = -1f;}
    private void TYplusDisactiv(PointerUpEvent evt) { moveInput.x = 0f; }
    private void TYmoinsActiv(PointerDownEvent evt) { moveInput.x = 1f; }
    private void TYmoinsDisactiv(PointerUpEvent evt){ moveInput.x = 0f; }

    private void RZplusActiv(PointerDownEvent evt)  { rotInput.y = 1f; }
    private void RZplusDisactiv(PointerUpEvent evt) { rotInput.y = 0f; }
    private void RZmoinsActiv(PointerDownEvent evt) { rotInput.y = -1f;}
    private void RZmoinsDisactiv(PointerUpEvent evt){ rotInput.y = 0f; }

    private void RXplusActiv(PointerDownEvent evt)  { rotInput.z = -1f;}
    private void RXplusDisactiv(PointerUpEvent evt) { rotInput.z = 0f; }
    private void RXmoinsActiv(PointerDownEvent evt) { rotInput.z = 1f; }
    private void RXmoinsDisactiv(PointerUpEvent evt){ rotInput.z = 0f; }

    private void RYplusActiv(PointerDownEvent evt)  { rotInput.x = -1f;}
    private void RYplusDisactiv(PointerUpEvent evt) { rotInput.x = 0f; }
    private void RYmoinsActiv(PointerDownEvent evt) { rotInput.x = 1f; }
    private void RYmoinsDisactiv(PointerUpEvent evt){ rotInput.x = 0f; }
}
