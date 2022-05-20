using UnityEngine;

namespace VitrivrVR.Input.Text
{
  /// <summary>
  /// MonoBehaviour helper to input text into <see cref="TextInputManager"/>.
  /// </summary>
  public class SceneTextInputController : MonoBehaviour
  {
    public void InputText(string text)
    {
      TextInputManager.InputText(text);
    }

    public void InputBackspace()
    {
      TextInputManager.InputBackspace();
    }
    
    public void InputReturn()
    {
      TextInputManager.InputReturn();
    }
    
    public void InputLeftArrow()
    {
      TextInputManager.InputLeftArrow();
    }
    
    public void InputRightArrow()
    {
      TextInputManager.InputRightArrow();
    }
    public void InputTabulator()
    {
      TextInputManager.InputTabulator();
    }

    public void ReceiveDictationResult(string text)
    {
      InputText(text);
    }
  }
}