using System;
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

    #region Start UI Variables

    public TMP_InputField configFileInputField;
    public GameObject startItemsUI;

    #endregion

    #region Stage UI Variables

    public GameObject stageItemsUI;
    public TMP_Text stageText;
    public GameObject userRevealButton;
    public GameObject userSubmitButton;

    #endregion

    #region End Variables

    public GameObject endMessage;

    #endregion

    private static readonly Vector3 LoadingLocation = new(0, -10, 0);

    private EvaluationConfig _config;
    private int _currentTask;

    /// <summary>
    /// All objects used for the current stage, that should be destroyed when moving on.
    /// </summary>
    private readonly List<GameObject> _currentStageObjects = new();

    private readonly List<Action> _revealActions = new();
    private CanvasVideoEvaluationDisplay _currentDisplay;

    public void StartEvaluation()
    {
      _config = LoadEvaluationConfig();

      StartTask(0);

      startItemsUI.SetActive(false);
      stageItemsUI.SetActive(true);
    }

    public void NextTask()
    {
      var nextTask = _currentTask + 1;
      if (nextTask >= _config.stages.Count)
      {
        Debug.Log("Completed all tasks!");
        ClearStageObjects();
        stageItemsUI.SetActive(false);
        endMessage.SetActive(true);
        return;
      }

      _currentTask = nextTask;
      StartTask(nextTask);
    }

    /// <summary>
    /// Reveals the current task, if a new task has been loaded.
    /// </summary>
    public void Reveal()
    {
      userRevealButton.SetActive(false);
      foreach (var revealAction in _revealActions)
      {
        revealAction();
      }

      userSubmitButton.SetActive(true);

      _revealActions.Clear();
    }

    /// <summary>
    /// Called when the user attempts to submit the current position of the current display.
    /// </summary>
    public void Submit()
    {
      var videoProgress = _currentDisplay.GetCurrentTime();

      var stage = _config.stages[_currentTask];

      if (videoProgress >= stage.targetStart && videoProgress <= stage.targetEnd)
      {
        // Correct submission
        // TODO: Move to questions and log time
        Debug.Log("Correct submission.");
        userSubmitButton.SetActive(false);
        NextTask();
      }
      else
      {
        // Incorrect submission
        // TODO: Notify user & log
        Debug.Log($"Incorrect submission: {videoProgress}");
      }
    }

    private static string GetConfigFilePath(string configFileName)
    {
#if UNITY_EDITOR
      var folder = Application.dataPath;
#else
      var folder = Application.persistentDataPath;
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
      var drawer = Instantiate(multimediaDrawerPrefab, LoadingLocation, drawerPosition.rotation);
      _currentStageObjects.Add(drawer);
      drawer.GetComponentInChildren<MediaObjectSegmentView>()
        .Initialize(display.GetObjectData(), (i, _) => display.SkipToSegment(i));
      _revealActions.Add(() => drawer.transform.position = drawerPosition.position);
    }

    private async void StartTask(int taskIndex)
    {
      var stage = _config.stages[taskIndex];

      // Clear stage objects from the previous stage
      ClearStageObjects();

      stageText.text = $"Task {taskIndex}";

      // Instantiate video display
      var videoDisplay = Instantiate(videoPrefab, LoadingLocation, Quaternion.identity);
      _currentStageObjects.Add(videoDisplay.gameObject);
      _revealActions.Add(() => videoDisplay.transform.position = videoPosition.position);

      var objectData = ObjectRegistry.GetObject(stage.videoId);

      await videoDisplay.Initialize(objectData);
      _currentDisplay = videoDisplay;

      // Instantiate multimedia drawer view
      if (stage.multimediaDrawer)
      {
        InstantiateDrawer(videoDisplay);
      }

      userRevealButton.SetActive(true);
    }

    private void ClearStageObjects()
    {
      _revealActions.Clear();
      _currentDisplay = null;
      foreach (var stageObject in _currentStageObjects)
      {
        Destroy(stageObject);
      }
    }
  }
}