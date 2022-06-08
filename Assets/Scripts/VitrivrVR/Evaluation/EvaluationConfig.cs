using System;
using System.Collections.Generic;

namespace VitrivrVR.Evaluation
{
  [Serializable]
  public class EvaluationQuestion
  {
    public string questionText;
    public int max;
    public string maxLabel;
    public string minLabel;
  }

  [Serializable]
  public class EvaluationStage
  {
    public string videoId;

    /// <summary>
    /// Start of correct target sequence in seconds.
    /// </summary>
    public float targetStart;

    /// <summary>
    /// End of correct target sequence in seconds.
    /// </summary>
    public float targetEnd;

    public string queryText;

    /// <summary>
    /// If the task from this stage includes the multimedia drawer.
    /// </summary>
    public bool multimediaDrawer;

    public List<EvaluationQuestion> questions;
  }

  [Serializable]
  public class EvaluationConfig
  {
    /// <summary>
    /// Path to the file into which to record all result data.
    /// </summary>
    public string outputPath;

    public List<EvaluationStage> stages;
  }
}