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
    public GameObject multimediaDrawerPrefab;
    public Transform drawerPosition;

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

    private readonly List<GameObject> _currentStageObjects = new();

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

    private void InstantiateDrawer(CanvasVideoEvaluationDisplay display)
    {
      var drawer = Instantiate(multimediaDrawerPrefab, drawerPosition.position, drawerPosition.rotation);
      _currentStageObjects.Add(drawer);
      drawer.GetComponentInChildren<MediaObjectSegmentView>()
        .Initialize(display.GetObjectData(), (i, _) => display.SkipToSegment(i));
    }

    private async void StartTask(int taskIndex)
    {
      var stage = _config.stages[taskIndex];

      // Clear stage objects from the previous stage
      ClearStageObjects();

      stageText.text = $"Task {taskIndex}";

      // Instantiate video display
      var videoDisplay = Instantiate(videoPrefab, videoPosition.position, Quaternion.identity);
      _currentStageObjects.Add(videoDisplay.gameObject);

      var objectData = ObjectRegistry.GetObject(stage.videoId);

      await videoDisplay.Initialize(objectData);

      // Instantiate multimedia drawer view
      if (stage.multimediaDrawer)
      {
        InstantiateDrawer(videoDisplay);
      }
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