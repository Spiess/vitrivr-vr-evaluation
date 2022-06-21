using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace VitrivrVR.Evaluation
{
  public class KeyboardAction : MonoBehaviour
  {
    public InputAction action;
    public UnityEvent effect;

    private void Start()
    {
      action.performed += _ => effect.Invoke();
    }

    private void OnEnable()
    {
      action.Enable();
    }

    private void OnDisable()
    {
      action.Disable();
    }
  }
}