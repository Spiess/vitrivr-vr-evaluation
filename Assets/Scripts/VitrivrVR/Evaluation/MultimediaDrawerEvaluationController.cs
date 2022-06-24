using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
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
    public GameObject textTaskPrefab;
    public FrameTaskController frameTaskPrefab;
    public SequenceTaskController sequenceTaskPrefab;
    public Transform taskHintPosition;

    public Button likertButtonPrefab;

    #region Start UI Variables

    public TMP_InputField configFileInputField;
    public GameObject startItemsUI;
    public InputAction selectConfigInputFieldAction;

    #endregion

    #region Stage UI Variables

    public GameObject stageItemsUI;
    public TMP_Text stageText;
    public TMP_Text stageTimer;
    public GameObject userRevealButton;
    public GameObject userSubmitButton;

    #endregion

    #region Question UI Variables

    public GameObject questionCanvas;
    public TMP_Text questionText;
    public TMP_Text minLabel;
    public TMP_Text maxLabel;
    public Transform likertButtonHolder;

    #endregion

    #region End Variables

    public GameObject endMessage;

    #endregion

    private static readonly Vector3 LoadingLocation = new(0, -10, 0);

    private EvaluationConfig _config;
    private int _currentTask;
    private int _currentQuestion;

    /// <summary>
    /// All objects used for the current stage, that should be destroyed when moving on.
    /// </summary>
    private readonly List<GameObject> _currentStageObjects = new();

    private readonly List<Action> _revealActions = new();
    private CanvasVideoEvaluationDisplay _currentDisplay;

    private float _taskStartTime = -1;

    private const string SubmissionsFile = "submissions.csv";
    private const string SurveyFile = "survey.csv";
    private const string SkippedFile = "skipped.csv";

    public async void StartEvaluation()
    {
      _config = LoadEvaluationConfig();

      if (Directory.Exists(_config.outputPath))
      {
        Debug.LogWarning($"Output directory {_config.outputPath} already exists.");
        // TODO: Append current time string
      }
      else
      {
        Directory.CreateDirectory(_config.outputPath);
      }

      await WriteSubmissionHeaderRow();
      await WriteSurveyHeaderRow();
      await WriteSkippedHeaderRow();

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

    public async void SkipToNext()
    {
      var time = Time.time - _taskStartTime;
      await LogSkipped(_currentTask, time);
      _taskStartTime = -1;
      NextTask();
    }

    private void Start()
    {
      selectConfigInputFieldAction.Enable();
      selectConfigInputFieldAction.performed += SelectConfigFileInputField;
    }

    private void Update()
    {
      if (_taskStartTime < 0)
      {
        stageTimer.text = "0.00 s";
      }
      else
      {
        var time = Time.time - _taskStartTime;
        stageTimer.text = $"{(int) time} s";
      }
    }

    private void NextQuestion()
    {
      var nextQuestion = _currentQuestion + 1;
      if (nextQuestion >= _config.stages[_currentTask].questions.Count)
      {
        Debug.Log($"Questions complete for stage {_currentTask}.");
        NextTask();
        return;
      }

      _currentQuestion = nextQuestion;
      StartQuestion(nextQuestion);
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

      _taskStartTime = Time.time;

      _revealActions.Clear();
    }

    /// <summary>
    /// Called when the user attempts to submit the current position of the current display.
    /// </summary>
    public async void Submit()
    {
      var videoProgress = _currentDisplay.GetCurrentTime();
      var time = Time.time - _taskStartTime;

      var stage = _config.stages[_currentTask];

      if (videoProgress >= stage.targetStart && videoProgress <= stage.targetEnd)
      {
        // Correct submission
        var button = userSubmitButton.GetComponentInChildren<Button>();
        var buttonColors = button.colors;
        buttonColors.selectedColor = Color.green;
        button.colors = buttonColors;
        Debug.Log($"Correct submission after {time} s.");
        await LogSubmission(_currentTask, time, videoProgress, true);
        userSubmitButton.SetActive(false);
        if (stage.questions.Count > 0)
        {
          StartQuestion(0);
        }
        else
        {
          NextTask();
        }

        _taskStartTime = -1;
      }
      else
      {
        // Incorrect submission
        var button = userSubmitButton.GetComponentInChildren<Button>();
        var buttonColors = button.colors;
        buttonColors.selectedColor = new Color(1, 0.3f, 0.3f);
        button.colors = buttonColors;
        Debug.Log($"Incorrect submission: {videoProgress}");
        await LogSubmission(_currentTask, time, videoProgress, false);
      }
    }

    public void Quit()
    {
      Application.Quit();
    }

    private static string GetConfigFilePath(string configFileName)
    {
#if UNITY_EDITOR
      var folder = Application.dataPath;
      return Path.Combine(folder, configFileName);
#else
      return configFileName;
#endif
    }

    private EvaluationConfig LoadEvaluationConfig()
    {
      var streamReader = File.OpenText(GetConfigFilePath(configFileInputField.text));
      var json = streamReader.ReadToEnd();
      streamReader.Close();
      return JsonUtility.FromJson<EvaluationConfig>(json);
    }

    private void SelectConfigFileInputField(InputAction.CallbackContext callbackContext)
    {
      configFileInputField.Select();
    }

    public void EnableConfigFileInputFieldSelect()
    {
      selectConfigInputFieldAction.Enable();
    }

    public void DisableConfigFileInputFieldSelect()
    {
      selectConfigInputFieldAction.Disable();
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

      await videoDisplay.Initialize(objectData, !stage.multimediaDrawer);
      _currentDisplay = videoDisplay;

      // Instantiate multimedia drawer view
      if (stage.multimediaDrawer)
      {
        InstantiateDrawer(videoDisplay);
      }

      // Instantiate task description
      if (stage.textTask != null)
      {
        var textTask = Instantiate(textTaskPrefab, taskHintPosition.position, taskHintPosition.rotation);
        _currentStageObjects.Add(textTask);
        textTask.GetComponentInChildren<TMP_Text>().text = stage.textTask;
      }
      else if (stage.frameTask >= 0)
      {
        var frameTask = Instantiate(frameTaskPrefab, taskHintPosition.position, taskHintPosition.rotation);
        _currentStageObjects.Add(frameTask.gameObject);

        var frame = await videoDisplay.GetFrameAtTime(stage.frameTask);

        frameTask.Initialize(frame);
      }
      else
      {
        var sequenceTask = Instantiate(sequenceTaskPrefab, taskHintPosition.position, taskHintPosition.rotation);
        _currentStageObjects.Add(sequenceTask.gameObject);

        await sequenceTask.Initialize(videoDisplay.GetMediaURL(), stage.targetStart, stage.targetEnd);
      }

      userRevealButton.SetActive(true);
    }

    private void StartQuestion(int questionIndex)
    {
      var question = _config.stages[_currentTask].questions[questionIndex];

      ClearStageObjects();

      questionText.text = question.questionText;
      minLabel.text = question.minLabel;
      maxLabel.text = question.maxLabel;

      for (var i = 1; i <= question.max; i++)
      {
        var button = Instantiate(likertButtonPrefab, likertButtonHolder);
        button.GetComponentInChildren<TMP_Text>().text = i.ToString();
        _currentStageObjects.Add(button.gameObject);

        var value = i;
        var task = _currentTask;

        async void OnClick()
        {
          Debug.Log($"Answered question with: {value}");
          await LogSurveyAnswer(task, questionIndex, value);
          NextQuestion();
        }

        button.onClick.AddListener(OnClick);
      }

      questionCanvas.gameObject.SetActive(true);
    }

    private void ClearStageObjects()
    {
      _revealActions.Clear();
      _currentDisplay = null;
      foreach (var stageObject in _currentStageObjects)
      {
        Destroy(stageObject);
      }

      questionCanvas.gameObject.SetActive(false);
      userRevealButton.SetActive(false);
      userSubmitButton.SetActive(false);
    }

    private async Task WriteSubmissionHeaderRow()
    {
      await using var file = new StreamWriter(Path.Join(_config.outputPath, SubmissionsFile), true);
      await file.WriteLineAsync("Task,Task Time,Video Time,Correct");
    }

    private async Task LogSubmission(int task, float taskTime, float videoTime, bool correct)
    {
      await using var file = new StreamWriter(Path.Join(_config.outputPath, SubmissionsFile), true);
      await file.WriteLineAsync($"{task},{taskTime},{videoTime},{correct}");
    }

    private async Task WriteSurveyHeaderRow()
    {
      await using var file = new StreamWriter(Path.Join(_config.outputPath, SurveyFile), true);
      await file.WriteLineAsync("Task,Question,Answer");
    }

    private async Task LogSurveyAnswer(int task, int question, int answer)
    {
      await using var file = new StreamWriter(Path.Join(_config.outputPath, SurveyFile), true);
      await file.WriteLineAsync($"{task},{question},{answer}");
    }

    private async Task WriteSkippedHeaderRow()
    {
      await using var file = new StreamWriter(Path.Join(_config.outputPath, SkippedFile), true);
      await file.WriteLineAsync("Task,Time");
    }

    private async Task LogSkipped(int task, float time)
    {
      await using var file = new StreamWriter(Path.Join(_config.outputPath, SkippedFile), true);
      await file.WriteLineAsync($"{task},{time}");
    }
  }
}