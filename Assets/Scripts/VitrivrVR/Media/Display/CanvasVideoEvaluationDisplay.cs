using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
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

    public async Task Initialize(ObjectData data)
    {
      _data = data;
      // Change texture to loading texture and reset scale
      previewImage.texture = loadingTexture;
      _imageTransform.sizeDelta = new Vector2(1000, 1000);

      // Resolve media URL
      var mediaUrl = await CineastWrapper.GetMediaUrlOfAsync(data);

      _videoPlayerController =
        new VideoPlayerController(gameObject, mediaUrl, 0, PrepareCompleted, ErrorEncountered);

      var volume = ConfigManager.Config.defaultMediaVolume;
      SetVolume(volume);
      volumeSlider.value = volume;

      var progressClickHandler = progressBar.gameObject.AddComponent<ClickHandler>();
      progressClickHandler.onClick = OnClickProgressBar;
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
      var segments = await _data.GetSegments();
      var segmentStarts = (await Task.WhenAll(
          segments.Select(segment => segment.GetAbsoluteStart())))
        .Where(segStart => segStart != 0);
      StartCoroutine(InstantiateSegmentIndicators(segmentStarts));
    }
  }
}