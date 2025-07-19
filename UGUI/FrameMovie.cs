using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

public class FrameMovie : MonoBehaviour
{
    public string path;

    public string movieName;
    public List<Material> m_materials;
    public float fSep = 0.05f;
    public bool IsLoop = true;
    public bool hide = true;

    public Action action = null;
    public float showerWidth
    {
        get
        {
            if (shower == null)
            {
                return 0;
            }
            return shower.rectTransform.rect.width;
        }
    }
    public float showerHeight
    {
        get
        {
            if (shower == null)
            {
                return 0;
            }
            return shower.rectTransform.rect.height;
        }
    }

    void Awake()
    {
        shower = GetComponent<Image>();

        if (string.IsNullOrEmpty(movieName))
        {
            movieName = "movieName";
        }
    }
    //void Start()
    //{
    //    Play(curFrame);
    //}

    void OnEnable()
    {
        Play(curFrame);
    }

    public void Play(int iFrame)
    {
        if (iFrame >= FrameCount)
        {
            if (IsLoop)
            {
                iFrame = 0;
            }
            else
            {
                if (action != null)
                    action();
                iFrame = FrameCount - 1;
                this.gameObject.SetActive(!hide);
                this.enabled = false;
            }
        }
        shower.material = m_materials[iFrame];
        curFrame = iFrame;
        shower.SetNativeSize();

        if (dMovieEvents.ContainsKey(iFrame))
        {
            foreach (delegateMovieEvent del in dMovieEvents[iFrame])
            {
                del();
            }
        }
    }

    private Image shower;

    public int curFrame = 0;
    public int FrameCount
    {
        get
        {
            return m_materials.Count;
        }
    }

    float fDelta = 0;
    void Update()
    {
        fDelta += Time.deltaTime;
        if (fDelta > fSep)
        {
            fDelta = 0;
            curFrame++;
            Play(curFrame);
        }
    }

    public void Finish()
    {
        gameObject.SetActive(false);
        shower.material = null;

        //for (int i = 0; i < m_materials.Count; i++)
        //{
        //    ResourceManager.UnloadAsset(m_materials[i]);
        //}
        m_materials.Clear();
        m_materials = null;
    }

    public delegate void delegateMovieEvent();
    private Dictionary<int, List<delegateMovieEvent>> dMovieEvents = new Dictionary<int, List<delegateMovieEvent>>();
    public void RegistMovieEvent(int frame, delegateMovieEvent delEvent)
    {
        if (!dMovieEvents.ContainsKey(frame))
        {
            dMovieEvents.Add(frame, new List<delegateMovieEvent>());
        }
        dMovieEvents[frame].Add(delEvent);
    }
    public void UnregistMovieEvent(int frame, delegateMovieEvent delEvent)
    {
        if (!dMovieEvents.ContainsKey(frame))
        {
            return;
        }
        if (dMovieEvents[frame].Contains(delEvent))
        {
            dMovieEvents[frame].Remove(delEvent);
        }
    }
}
