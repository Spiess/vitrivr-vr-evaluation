using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Video;
using Vitrivr.UnityInterface.CineastApi;
using Vitrivr.UnityInterface.CineastApi.Model.Data;
using VitrivrVR.Config;
using VitrivrVR.Media.Controller;
using VitrivrVR.Util;

namespace VitrivrVR.Media.Display
{
  public class CanvasVideoEvaluationDisplay : CanvasVideoDisplay
  {
    private ObjectData _data;
    private readonly TaskCompletionSource<object> _isInitialized = new();
    private string _mediaUrl;

    public async Task Initialize(ObjectData data)
    {
      _data = data;
      // Change texture to loading texture and reset scale
      previewImage.texture = loadingTexture;
      _imageTransform.sizeDelta = new Vector2(1000, 1000);

      // Resolve media URL
      _mediaUrl = await CineastWrapper.GetMediaUrlOfAsync(data);

      _videoPlayerController =
        new VideoPlayerController(gameObject, _mediaUrl, 0, PrepareCompleted, ErrorEncountered);

      var volume = ConfigManager.Config.defaultMediaVolume;
      SetVolume(volume);
      volumeSlider.value = volume;

      var progressClickHandler = progressBar.gameObject.AddComponent<ClickHandler>();
      progressClickHandler.onClick = OnClickProgressBar;

      await _isInitialized.Task;
    }

    public ObjectData GetObjectData()
    {
      return _data;
    }

    /// <summary>
    /// Returns the current video time in seconds.
    /// </summary>
    /// <returns>Video progress time in seconds.</returns>
    public float GetCurrentTime()
    {
      return (float)_videoPlayerController.Time;
    }

    public new void SkipToSegment(int i)
    {
      base.SkipToSegment(i);
    }

    public async Task<Texture2D> GetFrameAtTime(float time)
    {
      var timeBefore = _videoPlayerController.Time;
      var vp = _videoPlayerController.GetVideoPlayer();

      var tcs = new TaskCompletionSource<object>();

      void SeekCompleted(VideoPlayer _)
      {
        tcs.SetResult(null);
      }
      vp.seekCompleted += SeekCompleted;
      _videoPlayerController.SetTime(time);
      await tcs.Task;
      vp.seekCompleted -= SeekCompleted;
      await Task.Delay(200);
      var frame = _videoPlayerController.GetCurrentFrame();
      _videoPlayerController.SetTime(timeBefore);

      return frame;
    }

    public string GetMediaURL()
    {
      return _mediaUrl;
    }

    protected new async void PrepareCompleted(RenderTexture texture)
    {
      // Get video dimensions and scale preview image to fit video into 1x1 square
      var width = _videoPlayerController.Width;
      var height = _videoPlayerController.Height;
      var factor = Mathf.Max(width, height);
      previewImage.texture = texture;
      _imageTransform.sizeDelta = new Vector2(1000f * width / factor, 1000f * height / factor);

      UpdateProgressIndicator(0);
      // Set progress bar active
      progressBar.gameObject.SetActive(true);

      // Instantiate segment indicators
      _segments = await _data.GetSegments();
      var segmentStarts = (await Task.WhenAll(
          _segments.Select(segment => segment.GetAbsoluteStart())))
        .Where(segStart => segStart != 0);
      StartCoroutine(InstantiateSegmentIndicators(segmentStarts));
      _isInitialized.SetResult(null);
    }
  }
}