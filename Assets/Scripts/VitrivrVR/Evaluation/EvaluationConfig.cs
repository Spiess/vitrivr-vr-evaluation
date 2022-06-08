using System;
using System.Collections.Generic;

namespace VitrivrVR.Evaluation
{
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
  }
  
  [Serializable]
  public class EvaluationConfig
  {
    public List<EvaluationStage> stages;
  }
}