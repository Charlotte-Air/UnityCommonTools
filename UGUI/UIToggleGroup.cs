using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class UIToggleGroup : UIBehaviour
{
    private bool m_InteractableAll = true;

    [SerializeField]
    private bool m_AllowSwitchOff = false;
    public bool allowSwitchOff { get { return m_AllowSwitchOff; } set { m_AllowSwitchOff = value; } }

    private List<UIToggle> m_Toggles = new List<UIToggle>();

    protected UIToggleGroup()
    { }

    private void ValidateToggleIsInGroup(UIToggle toggle)
    {
        if (toggle == null || !m_Toggles.Contains(toggle))
            throw new ArgumentException(string.Format("UIToggle {0} is not part of UIToggleGroup {1}", new object[] { toggle, this }));
    }

    public void NotifyToggleOn(UIToggle toggle)
    {
        ValidateToggleIsInGroup(toggle);

        // disable all toggles in the group
        for (var i = 0; i < m_Toggles.Count; i++)
        {
            if (m_Toggles[i] == toggle)
                continue;

            m_Toggles[i].isOn = false;
        }
    }

    public void UnregisterToggle(UIToggle toggle)
    {
        if (m_Toggles.Contains(toggle))
        {
            m_Toggles.Remove(toggle);
            toggle.interactable = true;
        }
            
    }

    public void RegisterToggle(UIToggle toggle)
    {
        if (!m_Toggles.Contains(toggle))
        {
            m_Toggles.Add(toggle);
            toggle.interactable = m_InteractableAll;
        }
            
    }

    public bool AnyTogglesOn()
    {
        return m_Toggles.Find(x => x.isOn) != null;
    }

    public IEnumerable<UIToggle> ActiveToggles()
    {
        return m_Toggles.Where(x => x.isOn);
    }

    public void SetAllTogglesOff()
    {
        bool oldAllowSwitchOff = m_AllowSwitchOff;
        m_AllowSwitchOff = true;

        for (var i = 0; i < m_Toggles.Count; i++)
            m_Toggles[i].isOn = false;

        m_AllowSwitchOff = oldAllowSwitchOff;
    }

    public UIToggle GetToggleOn(int index = 0)
    {
        UIToggle t = m_Toggles.Find(x => x.isOn);
        if(t != null)
        {
            t.onValueChanged.Invoke(true);
            return t;
        }
        else 
        {
            if (m_Toggles.Count > 0)
            {
                if (index >= m_Toggles.Count) return m_Toggles[0];
                else
                {
                    m_Toggles[index].isOn = true;
                    return m_Toggles[index];
                }
            }
            else
            {
                return null;
            }
        }
    }

    public void InteractableAllTog(bool real)
    {
        m_InteractableAll = real;

        if (m_Toggles == null)
            return;
        
         for (var i = 0; i < m_Toggles.Count; i++)
            m_Toggles[i].interactable = real;
    }
}
