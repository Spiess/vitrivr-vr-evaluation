using System.Collections.Generic;
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

    #region startUIVariables

    public TMP_InputField configFileInputField;
    public GameObject startItemsUI;

    #endregion

    #region stageUIVariables

    public GameObject stageItemsUI;
    public TMP_Text stageText;

    #endregion

    private EvaluationConfig _config;
    private int _currentTask;

    private List<GameObject> _currentStageObjects = new();

    public void StartEvaluation()
    {
      _config = LoadEvaluationConfig();

      StartTask(0);

      startItemsUI.SetActive(false);
      stageItemsUI.SetActive(true);
    }

    public void SkipToNextTask()
    {
      _currentTask++;
      StartTask(_currentTask);
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
      // Clear stage objects from the previous stage
      ClearStageObjects();

      stageText.text = $"Task {taskIndex}";
      
      var videoDisplay = Instantiate(videoPrefab, videoPosition.position, Quaternion.identity);
      _currentStageObjects.Add(videoDisplay.gameObject);

      var videoId = _config.stages[taskIndex].videoId;
      var objectData = ObjectRegistry.GetObject(videoId);

      await videoDisplay.Initialize(objectData);
    }

    private void ClearStageObjects()
    {
      foreach (var stageObject in _currentStageObjects)
      {
        Destroy(stageObject);
      }
    }
  }
}