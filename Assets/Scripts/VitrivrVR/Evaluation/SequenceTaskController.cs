using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using VitrivrVR.Config;
using VitrivrVR.Media.Controller;

namespace VitrivrVR.Evaluation
{
  public class SequenceTaskController : MonoBehaviour
  {
    public RawImage previewImage;
    public Slider volumeSlider;
    public Texture2D errorTexture;

    private VideoPlayerController _videoPlayerController;
    private readonly TaskCompletionSource<object> _isInitialized = new();

    private bool _initialized;
    private float _sequenceStart, _sequenceEnd;

    private void Awake()
    {
      GetComponent<Canvas>().worldCamera = Camera.main;
    }

    private void Update()
    {
      if (!_initialized || _videoPlayerController.IsPaused || (!(_videoPlayerController.Time > _sequenceEnd) &&
                                                               !(_videoPlayerController.Time < _sequenceStart - 0.1)))
        return;
      _videoPlayerController.Pause();
      _videoPlayerController.SetTime(_sequenceStart);
    }

    public async Task Initialize(string mediaUrl, float sequenceStart, float sequenceEnd)
    {
      _videoPlayerController =
        new VideoPlayerController(gameObject, mediaUrl, 0, PrepareCompleted, ErrorEncountered);
      _sequenceStart = sequenceStart;
      _sequenceEnd = sequenceEnd;

      var volume = ConfigManager.Config.defaultMediaVolume;
      SetVolume(volume);
      volumeSlider.value = volume;

      await _isInitialized.Task;
    }

    public void SetVolume(float volume)
    {
      _videoPlayerController.SetVolume(volume);
    }

    private void ErrorEncountered(VideoPlayer videoPlayer, string error)
    {
      Debug.LogError(error);
      previewImage.texture = errorTexture;
    }

    private void PrepareCompleted(RenderTexture texture)
    {
      // Get video dimensions and scale preview image to fit video into 1x1 square
      var width = _videoPlayerController.Width;
      var height = _videoPlayerController.Height;
      var factor = Mathf.Max(width, height);
      previewImage.texture = texture;
      previewImage.GetComponent<RectTransform>().sizeDelta =
        new Vector2(1000f * width / factor, 1000f * height / factor);

      _videoPlayerController.Pause();
      _videoPlayerController.SetOnSeekComplete(SeekCompleteAction);
      _videoPlayerController.SetTime(_sequenceStart);
      _initialized = true;
      _isInitialized.SetResult(null);
    }

    private void SeekCompleteAction(VideoPlayer _)
    {
      if (_videoPlayerController.IsPlaying)
      {
        return;
      }

      _videoPlayerController.Play();
    }
  }
}