using System.IO;
using TMPro;
using UnityEngine;
using Vitrivr.UnityInterface.CineastApi.Model.Registries;
using VitrivrVR.Media.Display;

namespace VitrivrVR.Evaluation
{
  public class MultimediaDrawerEvaluationController : MonoBehaviour
  {
    public CanvasVideoEvaluationDisplay videoPrefab;
    public Transform videoPosition;
    public TMP_InputField configFileInputField;

    private EvaluationConfig _config;
    private int _currentTask;

    public void StartEvaluation()
    {
      _config = LoadEvaluationConfig();
      
      StartTask(0);
    }
    
    private static string GetConfigFilePath(string configFileName)
    {
      string folder;
#if UNITY_EDITOR
      folder = Application.dataPath;
#else
      folder = Application.persistentDataPath;
#endif
      return Path.Combine(folder, configFileName);
    }

    private EvaluationConfig LoadEvaluationConfig()
    {
      var streamReader = File.OpenText(GetConfigFilePath(configFileInputField.text));
      var json = streamReader.ReadToEnd();
      streamReader.Close();
      return JsonUtility.FromJson<EvaluationConfig>(json);
    }

    private async void StartTask(int taskIndex)
    {
      var videoDisplay = Instantiate(videoPrefab, videoPosition.position, Quaternion.identity);

      var videoId = _config.stages[taskIndex].videoId;
      var objectData = ObjectRegistry.GetObject(videoId);
      
      await videoDisplay.Initialize(objectData);
    }
  }
}