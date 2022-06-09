using UnityEngine;

namespace VitrivrVR.Evaluation
{
  public class FrameTaskController : MonoBehaviour
  {
    public void Initialize(Texture2D frame)
    {
      var rend = GetComponent<Renderer>();
      rend.material.mainTexture = frame;
      float factor = Mathf.Max(frame.width, frame.height);
      var scale = new Vector3(frame.width / factor, frame.height / factor, 1);
      rend.transform.localScale = scale;
    }
  }
}