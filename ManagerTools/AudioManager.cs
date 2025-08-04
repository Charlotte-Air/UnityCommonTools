using UnityEngine;
using Framework.Utils;
using Framework.Manager;
using UnityEngine.Audio;
using System.Collections.Generic;

public class AudioManager : SingletonMonoBehaviour<AudioManager>
{
    private AudioMixer m_AudioMixer;
    private AudioSource m_AudioSourceMusic;
    private AudioSource m_AudioSourceEffect;
    private float m_VolumeMusic = 1;
    private float m_VolumeSound = 1;
    private bool isMusicMute = false;
    private bool isEffectMute = false;

    private List<AudioSource> _battleEffects = new List<AudioSource>();
    

    protected override void OnSingletonInit()
    {
        
    }

    protected override void OnSingletonRelease()
    {

    }

    void Start()
    {
        
    }

    public void SetBattleListener(bool isBattle = true)
    {
        var listener = transform.Find("UICamera")?.GetComponent<AudioListener>();
        if (listener != null)
        {
            listener.enabled = !isBattle;
        }
    }

    public AudioSource GetAudioSource()
    {
        return m_AudioSourceMusic;
    }
    
    public void SetPlayTime(float time)
    {
        if (m_AudioSourceMusic && m_AudioSourceMusic.clip && time < m_AudioSourceMusic.clip.length)
        {
            m_AudioSourceMusic.time = time;
        }
        else
        {
            Debug.LogWarning("time is over length! time is:" + time + " length is: " + m_AudioSourceMusic.clip.length);
        }
    }

    public float GetPlayTime()
    {
        return m_AudioSourceMusic.time;
    }

    public float GetClipLength()
    {
        if (m_AudioSourceMusic && m_AudioSourceMusic.clip)
            return m_AudioSourceMusic.clip.length;
        else
            return -1;
    }

    public void PauseMusic()
    {
        if (m_AudioSourceMusic.clip != null && m_AudioSourceMusic.isPlaying)
        {
            m_AudioSourceMusic.Pause();
        }
    }

    public void UnPauseMusic()
    {
        if (m_AudioSourceMusic.clip != null && !m_AudioSourceMusic.isPlaying)
        {
            m_AudioSourceMusic.UnPause();
        }
    }

    /// <summary>
    /// 播放背景速度
    /// </summary>
    /// <param name="clip"></param>
    public void SetPitch(float speed)
    {
        if (m_AudioSourceMusic.clip != null)
        {
            m_AudioSourceMusic.pitch = speed;
        }
    }

    /// <summary>
    /// 播放背景音乐
    /// </summary>
    /// <param name="clip"></param>
    public void PlayMusic(AudioClip clip)
    {
        if (m_AudioSourceMusic.clip != null)
        {        
            m_AudioSourceMusic.Stop();
            m_AudioSourceMusic.clip = null;
        }
        m_AudioSourceMusic.loop = true;
        m_AudioSourceMusic.clip = clip;
        m_AudioSourceMusic.time = 0;
        m_AudioSourceMusic.Play();   
    }

    /// <summary>
    /// 播放背景音乐
    /// </summary>
    /// <param name="path"></param>
    public void PlayMusic(string path)
    {
        if (path == null || path == string.Empty || m_AudioSourceMusic == null)
            return;
        if (m_AudioSourceMusic.isPlaying)
        {        
            m_AudioSourceMusic.Stop();
            m_AudioSourceMusic.clip = null;
        }
        ResourceManager.Instance.LoadAssetsAsync(path, (req) =>
        {
            if (req.AssetObject == null)
                return;
            var clip = req.AssetObject as AudioClip;
            if (clip == null)
                return;
            m_AudioSourceMusic.loop = true;
            m_AudioSourceMusic.clip = clip;
            m_AudioSourceMusic.time = 0;
            m_AudioSourceMusic.Play();
        });
    }
    

    public void StopMusic()
    {
        if (m_AudioSourceMusic.clip == null)
            return;
        m_AudioSourceMusic.Stop();
        m_AudioSourceMusic.clip = null;
    }

    public void PlayMusicOnce(AudioClip clip)
    {
        if (m_AudioSourceMusic.clip != null)
        {
            m_AudioSourceMusic.Stop();
            m_AudioSourceMusic.clip = null;
        }
        m_AudioSourceMusic.loop = false;
        m_AudioSourceMusic.clip = clip;
        m_AudioSourceMusic.time = 0;
        m_AudioSourceMusic.Play();
    }

    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="clip"></param>
    /// <param name="position"></param>
    public void PlayEffect(AudioClip clip)
    {
        if (clip == null)
        {
            return;
        }
        // 会自动生成一个名为"One shot audio"的物体
        // AudioSource.PlayClipAtPoint(clip, Vector3.zero); 
        m_AudioSourceEffect.PlayOneShot(clip);
    }

    public void PlayEffect(string path)
    {
        if (path == null || path == string.Empty)
        {
            return;
        }
        if (isEffectMute)
        {
            return;
        }
        ResourceManager.Instance.LoadAssetsAsync(path, (req) =>
        {
            if (req.AssetObject == null)
                return;
            var clip = req.AssetObject as AudioClip;
            if (clip == null)
                return;
            m_AudioSourceEffect.PlayOneShot(clip);
        });
    }
    
    public void StopEffect()
    {
        m_AudioSourceEffect?.Stop();
        for (var i = 0; i < _battleEffects.Count; i++)
        {
            _battleEffects[i]?.Stop();
        }
    }
    

    public void ClearEffect()
    {
        _battleEffects.Clear();
    }

    /// <summary>
    /// 设置背景音乐是否静音
    /// </summary>
    /// <param name="isMute"></param>
    public void SetMusicMute(bool isMute)
    {
        isMusicMute = isMute;
        if (m_AudioSourceMusic != null)
            m_AudioSourceMusic.mute = isMute;
    }

    /// <summary>
    /// 设置音效是否静音
    /// </summary>
    /// <param name="isMute"></param>
    public void SetEffectMute(bool isMute)
    {
        isEffectMute = isMute;
        if (m_AudioSourceEffect != null)
            m_AudioSourceEffect.mute = isMute;
        for (var i = 0; i < _battleEffects.Count; i++)
        {
            if (_battleEffects[i] != null)
            {
                _battleEffects[i].mute = isMute;
            }
        }
    }

    public void SetMasterVolume(float volume)    // 控制主音量的函数
    {
        m_AudioMixer.SetFloat("MasterVolume", volume);
        // MasterVolume为我们暴露出来的Master的参数
    }

    public void SetBGMVolume(float volume)    // 控制背景音乐音量的函数
    {
        m_AudioMixer.SetFloat("BGMVolume", volume);
        // MusicVolume为我们暴露出来的Music的参数
    }

    public void SetSoundVolume(float volume)    // 控制音效音量的函数
    {
        m_AudioMixer.SetFloat("SEVolume", volume);
        // EffectVolume为我们暴露出来的SoundEffect的参数
    }
}