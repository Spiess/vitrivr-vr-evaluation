using UnityEngine;
using Vitrivr.UnityInterface.CineastApi.Model.Registries;
using VitrivrVR.Media.Display;

namespace VitrivrVR.Evaluation
{
  public class MultimediaDrawerEvaluationController : MonoBehaviour
  {
    public CanvasVideoEvaluationDisplay videoPrefab;
    public Transform videoPosition;
    public string testVideo;

    public async void StartEvaluation()
    {
      var videoDisplay = Instantiate(videoPrefab, videoPosition.position, Quaternion.identity);

      var objectData = ObjectRegistry.GetObject(testVideo);
      
      await videoDisplay.Initialize(objectData);
    }
  }
}