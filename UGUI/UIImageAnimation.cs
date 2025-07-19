using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Image))]
public class UIImageAnimation : MonoBehaviour
{
    public delegate void AnimationEventHandler(float f, object args);

    private class AnimEvent
    {
        public float time;
        public AnimationEventHandler handler;
        public object args;

        private bool m_isFired = false;

        public AnimEvent()
        {
            m_isFired = false;
        }

        public void Fire(float t)
        {
            if (!m_isFired)
            {
                if (handler != null)
                {
                    handler(t, args);
                }
                m_isFired = true;
            }
        }

        public void Reset()
        {
            m_isFired = false;
        }
    }

    public float speed
    {
        get
        {
            return m_speed;
        }
        set
        {
            m_speed = value;
        }
    }

    public bool isPlaying
    {
        get
        {
            return m_isPlaying;
        }
    }

    public float frameRate
    {
        get
        {
            return m_frameRate;
        }
        set
        {
            m_frameRate = value;
        }
    }

    public bool playOnAwake
    {
        get
        {
            return m_playOnAwake;
        }
        set
        {
            this.m_playOnAwake = value;
        }
    }

    public bool isPingPong
    {
        get
        {
            return m_isPingPong;
        }
        set
        {
            this.m_isPingPong = value;
        }
    }

    public bool isReverse
    {
        get
        {
            return m_isReverse;
        }
        set
        {
            m_isReverse = value;
        }
    }

    public bool loop
    {
        get
        {
            return m_loopCount == -1;
        }
        set
        {
            m_loopCount = ((!value) ? 1 : -1);
        }
    }

    public int loopCount
    {
        get
        {
            return m_loopCount;
        }
        set
        {
            m_loopCount = value;
        }
    }

    public bool isStop
    {
        get
        {
            return m_playCount == m_loopCount;
        }
    }

    //[HideInInspector]
    [SerializeField]
    public List<string> anims = new List<string>();

    [SerializeField]
    private float m_frameRate = 30f;
    [SerializeField]
    private float m_speed = 1.0f;
    [SerializeField]
    private bool m_isReverse = false;
    [SerializeField]
    private bool m_isPingPong = false;
    [SerializeField]
    private int m_loopCount = -1;
    [SerializeField]
    private bool m_playOnAwake = true;

    private Image m_image;
    private int m_curFrame = -1;
    private float m_frameTimer;
    private float m_accumTime = 0;
    private int m_playCount;
    private bool m_isPlaying;
    private List<Sprite> m_animSprites = new List<Sprite>();
    private List<AnimEvent> m_animEvents = new List<AnimEvent>();
	private int m_animEventCount = 0;
    private AnimEvent m_finishAnimEvent = null;
    private bool m_prepared = false;
    private bool m_cleared = false;

    private void Awake()
    {
        Prepare();
        m_cleared = false;
    }

    private void OnDestroy()
    {
        /*
        for (int i = 0; i < m_animSprites.Count; i++)
        {
            if (m_animSprites[i] != null)
            {
                UISpriteManager.FreeSprite(m_animSprites[i]);
            }
        }
        */
        m_animSprites.Clear();
        m_animEvents.Clear();
        m_cleared = true;
    }

    private void Prepare()
    {
        if (!m_prepared)
        {
            Reload();
            m_prepared = true;
        }
    }

    public void Reload()
    {
        m_image = gameObject.GetComponent<Image>();
        m_curFrame = 0;
        m_frameTimer = 0f;
        m_accumTime = 0f;
        m_playCount = 0;
        m_isPlaying = false;
        m_animSprites.Clear();
        for (int i = 0; i < anims.Count; i++)
        {
            m_animSprites.Add(null);
        }

        // preload
        for (int i = 0; i < anims.Count; i++)
        {
            int spriteIndex = i;
            string spriteName = anims[spriteIndex];
            /*
            UISpriteManager.GetSprite(spriteName, (si) =>
            {
                if (!m_cleared)
                    m_animSprites[spriteIndex] = si.sprite;
            });
            */
        }
    }

    private void Start()
    {
        if (playOnAwake)
        {
            Play();
        }
    }

    private void Update()
    {
        OnUpdate(Time.deltaTime, true);
    }

    public void SetFrame(int frame)
    {
        if (frame == m_curFrame)
        {
            return;
        }
        m_curFrame = frame;
    }

    public void Play()
    {
        Prepare();
        if (m_animSprites.Count > 0)
        {
            SetFrame(0);
            m_frameTimer = 0f;
            m_accumTime = 0f;
            m_playCount = 0;
            m_isPlaying = true;
        }
    }

    public void PlayRandom()
    {
        Prepare();
        if (m_animSprites.Count > 0)
        {
            SetFrame(UnityEngine.Random.Range(0, m_animSprites.Count));
            m_frameTimer = 0f;
            m_accumTime = 0f;
            m_playCount = 0;
            m_isPlaying = true;
        }
    }

    public void Resume()
    {
        if (m_playCount != m_loopCount)
        {
            m_isPlaying = true;
        }
    }

    public void Pause()
    {
        m_isPlaying = false;
    }

    public void Stop()
    {
        m_playCount = m_loopCount;
        m_isPlaying = false;
    }

    public int AddAnimEventHandler(float time, AnimationEventHandler handler, object args)
    {
        AnimEvent ae = new AnimEvent();
        ae.time = time;
        ae.handler = handler;
        ae.args = args;
        if (time < 0)
        {
            m_finishAnimEvent = ae;
            return (int)0xaddbeef;
        }
        else
        {
			m_animEventCount++;
			for (int i = 0; i < m_animEvents.Count; i++)
			{
				if (m_animEvents [i] == null)
				{
					m_animEvents [i] = ae;
					return i;
				}
			}
            m_animEvents.Add(ae);
            return m_animEvents.Count - 1;
        }
    }

    public void RemoveAnimEventHandler(int Id)
    {
        if (Id == 0xaddbeef)
        {
            m_finishAnimEvent = null;
        }
        else
        {
			m_animEventCount--;
            if (Id >= 0 && Id < m_animEvents.Count)
            {
                m_animEvents[Id] = null;
            }
        }
    }

    public bool OnUpdate(float deltaTime, bool setSprite)
    {
        bool updateFrame = false;

        if (m_isPlaying)
        {
            m_accumTime += deltaTime * m_speed;

			if (m_animEventCount > 0)
			{
				for (int i = 0; i < m_animEvents.Count; i++)
				{
					AnimEvent ae = m_animEvents [i];
					if (ae != null && m_accumTime >= ae.time)
					{
						ae.Fire (m_accumTime);
					}
				}
			}

            m_frameTimer += deltaTime * m_speed * frameRate;
            if (m_frameTimer >= 1f)
            {
                int frame = (int)m_frameTimer;
                m_frameTimer -= (float)((int)m_frameTimer);
                UpdateFrame(frame, setSprite);
                updateFrame = true;
            }
        }

        return updateFrame;
    }

    private void SetSprite()
    {
        if (m_curFrame >= 0 && m_curFrame < m_animSprites.Count)
        {
            if (m_animSprites[m_curFrame] != null)
            {
                m_image.sprite = m_animSprites[m_curFrame];
            }
        }
    }

    public Sprite GetCurSprite()
    {
        if (m_curFrame >= 0 && m_curFrame < m_animSprites.Count)
        {
            return m_animSprites[m_curFrame];
        }
        return null;
    }

    private void UpdateFrame(int frame, bool setSprite)
    {
        if (m_isReverse)
        {
            m_curFrame--;
            if (m_curFrame < 0)
            {
                ReachEnd();
            }
        }
        else
        {
            m_curFrame += frame;
            if (m_curFrame >= m_animSprites.Count)
            {
                ReachEnd();
            }
        }

        if (setSprite)
        {
            SetSprite();
        }
    }

    private void ReachEnd()
    {
        m_playCount++;
        if (!loop && m_playCount >= m_loopCount)
        {
            Stop();
            if (m_finishAnimEvent != null)
            {
                m_finishAnimEvent.Fire(m_accumTime);
            }
        }
        else
        {
            if (m_isPingPong)
            {
                m_isReverse = !m_isReverse;
            }
            m_curFrame = m_isReverse ? m_animSprites.Count - 1 : 0;
        }

		if (m_animEventCount > 0)
		{
			for (int i = 0; i < m_animEvents.Count; i++)
			{
				AnimEvent ae = m_animEvents [i];
				if (ae != null)
				{
					ae.Fire (m_accumTime);
					ae.Reset ();
				}
			}
		}

        m_accumTime = 0f;
    }
}
