using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(ContentSizeFitter))]
public class ElasticMenu : Selectable, IPointerClickHandler, ISubmitHandler, ICanvasElement
{
    
    //[Serializable]
    //public class ElasticBaseParam
    //{
    //    public string content;
    //    public Graphic[] checkMarks;
    //    public Graphic[] baseMarks;
    //}
    //protected internal class ElasticItem : MonoBehaviour, IPointerEnterHandler, ICancelHandler
    //{
    //    public Text content;
    //    public Graphic[] checkMarks;
    //    public Graphic[] baseMarks;

    //    public ElasticBaseParam param;

    //    public delegate void OnDrawElasticItem(ElasticBaseParam elasticParam);
    //    public OnDrawElasticItem onDrawElasticItem;

    //    public virtual void OnShow()
    //    {
    //        onDrawElasticItem(param);
    //    }
    //    public virtual void OnHide()
    //    {

    //    }

    //    public virtual void OnPointerEnter(PointerEventData eventData)
    //    {
    //        EventSystem.current.SetSelectedGameObject(gameObject);
    //    }

    //    public virtual void OnCancel(BaseEventData eventData)
    //    {
    //        ElasticMenu dropdown = GetComponentInParent<ElasticMenu>();
    //        if (dropdown)
    //            dropdown.HideElasticItems();
    //    }
    //}

    public class ElasticData
    {
        public RectTransform rectTransform;
        public Vector2 rect;

        public ElasticData(RectTransform item)
        {
            rectTransform = item;
            rect = item.sizeDelta;
        }
    }

    public virtual void Rebuild(CanvasUpdate executing)
    {
    }
    public virtual void LayoutComplete()
    { }
    public virtual void GraphicUpdateComplete()
    { }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        isElastic = !isElastic;
    }

    public virtual void OnSubmit(BaseEventData eventData)
    {
        isElastic = !isElastic;
    }
    [SerializeField]
    public Graphic[] checkMarks;
    [SerializeField]
    public Graphic[] baseMarks;

    [SerializeField]
    private ElasticMenuGroup m_group;
    public ElasticMenuGroup group
    {
        get { return m_group; }
        set
        {
            if(m_group != null)
            {
                m_group.RemoveElasticMenu(this);
            }
            m_group = value;
            if(m_group != null)
            {
                m_group.AddElasticMenu(this);
            }
        }
    }

    [SerializeField]
    public bool m_isElastic;
    public bool isElastic
    {
        get { return m_isElastic; }
        set
        {
            bool oldState = m_isElastic;
            if (m_group != null)
                m_group.SetElastic(this, value);
            else
                m_isElastic = value;

            if (m_isElastic != oldState) onValueChanged.Invoke(m_isElastic);

            if (m_isElastic)
            {
                ShowElasticItems();
            }
            else
            {
                HideElasticItems();
            }

            if(group != null)
                group.RebuildLayout();
        }
    }

    public class ElaseticEvent : UnityEvent<bool>{ }
    public ElaseticEvent onValueChanged = new ElaseticEvent();

    [SerializeField]
    public RectTransform content;
    [SerializeField]
    public UIText txt_Title;
    public Dictionary<RectTransform ,ElasticData> elasticItems;

    protected override void Awake()
    {
        //if (m_group != null)
        //{
        //    Init(m_group);
        //}
    }

    public virtual void Init(ElasticMenuGroup g,string title = null)
    {
        elasticItems = new Dictionary<RectTransform, ElasticData>();

        GetElasticItems();
        group = g;

        if (txt_Title != null && !string.IsNullOrEmpty(title)) txt_Title.text = title;
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    }

    private void GetElasticItems()
    {
        if (content)
        {
            foreach (RectTransform rt in content)
            {
                elasticItems.Add(rt, new ElasticData(rt));
            }
        }
    }

    private void ShowElasticItems()
    {
        foreach (ElasticData data in elasticItems.Values)
        {
            //data.rectTransform.sizeDelta = data.rect;
            data.rectTransform.gameObject.SetActive(true);
        }
        SetCheckMarks();
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
    }
    private void HideElasticItems()
    {
        foreach (ElasticData data in elasticItems.Values)
        {
            //data.rectTransform.sizeDelta = Vector2.zero;
            data.rectTransform.gameObject.SetActive(false);
        }
        SetCheckMarks();
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
    }
    public void ReSetElasticItems()
    {
        foreach (ElasticData data in elasticItems.Values)
        {
            //data.rectTransform.sizeDelta = data.rect;
            data.rectTransform.gameObject.SetActive(true);
        }
    }

    private void SetCheckMarks()
    {
        if (checkMarks != null)
        {
            for (int i = 0; i < checkMarks.Length; i++)
            {
                if (checkMarks[i])
                {
                    checkMarks[i].canvasRenderer.SetAlpha(m_isElastic ? 1f : 0f);
                }
            }
        }
        if (baseMarks != null)
        {
            for (int i = 0; i < baseMarks.Length; i++)
            {
                if (baseMarks[i])
                {
                    baseMarks[i].canvasRenderer.SetAlpha(m_isElastic ? 0f : 1f);
                }
            }
        }
    }

    public void AddElasticItems(RectTransform item)
    {
        if (!elasticItems.ContainsKey(item))
        {
            elasticItems.Add(item, new ElasticData(item));
        }
    }
    public void RemoveElasticItems(RectTransform item)
    {
        if (elasticItems.ContainsKey(item))
        {
            elasticItems.Remove(item);
        }
    }
}