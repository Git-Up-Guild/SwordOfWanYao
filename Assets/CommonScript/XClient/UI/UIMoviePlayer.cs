using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

[RequireComponent(typeof(UnityEngine.Video.VideoPlayer))]
public class UIMoviePlayer : MonoBehaviour
{
    public bool isDoPlay = false;
    public VideoPlayer videoPlayer;
    public RawImage rawImage;
    public Button btnEnd;
    public Action<bool> onMovieDone;        // 完成回调，参数是是否播放完整

    //播放影片
    public VideoClip videoClip;

    public void Start()
    {
        if (rawImage == null || videoPlayer == null || videoPlayer.targetTexture == null || rawImage.mainTexture == null)
        {
            Debug.LogError("UIMoviePlayer  找不到相机 或 渲染图片 或 视频播放器");
            return;
        }

        /*
        var renderTexture = (RenderTexture)rawImage.mainTexture;
        renderTexture?.Release();
        */

    }

    private void OnEnable()
    {
        if (videoPlayer)
            videoPlayer.loopPointReached += OnLoopPointReached;
        if (btnEnd)
            btnEnd.onClick.AddListener(OnClickEnd);
    }

    private void OnDisable()
    {
        (rawImage.mainTexture as RenderTexture)?.Release();
        if (videoPlayer)
            videoPlayer.loopPointReached -= OnLoopPointReached;
        if (btnEnd)
            btnEnd.onClick.RemoveListener(OnClickEnd);
    }

    public void SetVideoClip(VideoClip clip)
    {
        if (videoPlayer)
            videoPlayer.clip = clip;
    }

    public void Update()
    {
        if (isDoPlay)
        {
            isDoPlay = false;
            if(videoPlayer.isPlaying==false)
            {
                videoPlayer.Play();
            }
            

        }else
        {
            /*
            if(false== videoPlayer.isPlaying)
            {
               // Debug.LogError(" if(false== videoPlayer.isPlaying)");
                videoPlayer.Stop();
                onMovieDone?.Invoke(true);
            }
            */
            
        }
    }

    public void Play()
    {
       // Debug.LogError(" UIMoviePlayer:Play())");
        Stop();


        if(null!= videoClip)
        {
           // Debug.LogError(" videoPlayer.clip = videoClip;");
            videoPlayer.clip = videoClip;
            isDoPlay = true;
            videoPlayer.Play();
        }
        else
        {
           // Debug.LogError(" null== videoClip;");
            onMovieDone?.Invoke(true);
        }

     
    }

    public void Stop()
    {
       
        if (videoPlayer.isPlaying)
        {
            videoPlayer.Stop();
            onMovieDone?.Invoke(false);
        }
    }

    private void OnClickEnd()
    {
        onMovieDone?.Invoke(false);
    }
    
    private void OnLoopPointReached(UnityEngine.Video.VideoPlayer source)
    {
        onMovieDone?.Invoke(true);
    }

    private void OnDestroy()
    {
        if (rawImage)
        {
            var renderTexture = (RenderTexture)rawImage.mainTexture;
            renderTexture?.Release();

            rawImage = null;
        }
    }
}