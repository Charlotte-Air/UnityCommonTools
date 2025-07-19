using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(LayoutGroup))]
[RequireComponent(typeof(ContentSizeFitter))]
public class ElasticMenuGroup : UIBehaviour
{
    [SerializeField]
    public bool allowSwitchOff;
    [SerializeField]
    public bool allowSwitchRepeat;

    private List<ElasticMenu> elasticMenus = new List<ElasticMenu>();
    private ElasticMenu curElasticMenu;

    private RectTransform rt;

    protected override void Awake()
    {
        base.Awake();
        rt = GetComponent<RectTransform>();
    }

    public void AddElasticMenu(ElasticMenu elasticMenu)
    {
        if (!elasticMenus.Contains(elasticMenu))
        {
            elasticMenus.Add(elasticMenu);
            if (!allowSwitchOff)
            {
                if (curElasticMenu == null)
                {
                    elasticMenu.isElastic = true;
                    curElasticMenu = elasticMenu;
                }
                else
                {
                    elasticMenu.isElastic = false;
                }
            }
            else
            {
                elasticMenu.isElastic = false;
            }
        }
    }
    public void RemoveElasticMenu(ElasticMenu elasticMenu)
    {
        if (elasticMenus.Contains(elasticMenu))
        {
            elasticMenus.Remove(elasticMenu);
        }
    }

    public void SetElastic(ElasticMenu elastic,bool value)
    {
        if (value)
        {
            if(!allowSwitchRepeat)
            {
                if(elastic != curElasticMenu && curElasticMenu != null)
                {
                    elastic.m_isElastic = true;
                    curElasticMenu.isElastic = false;
                    curElasticMenu = elastic;
                }
                else
                {
                    elastic.m_isElastic = true;
                    curElasticMenu = elastic;
                }
            }
            else
            {
                elastic.m_isElastic = true;
            }
        }
        else
        {
            if(!allowSwitchOff)
            {
                if (!AnyElasticsOn(elastic))
                {
                    elastic.m_isElastic = true;
                }
                else
                {
                    elastic.m_isElastic = false;
                }
            }
            else
            {
                elastic.m_isElastic = false;
            }
        }
    }

    public bool AnyElasticsOn(ElasticMenu elastic = null)
    {
        return elasticMenus.Find(x => x.isElastic && (elastic == null || x != elastic)) != null;
    }
    public void RebuildLayout()
    {
        if(rt != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
    }
}