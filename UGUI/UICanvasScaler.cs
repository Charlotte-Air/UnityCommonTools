using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UnityEngine.UI
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(RectTransform))]
    public class UICanvasScaler : UIBehaviour
    {
        public enum ReferenceResolutionMode
        {
            Inherit,
            Custom,
        }

        [SerializeField] private ReferenceResolutionMode m_ReferenceResolutionMode = ReferenceResolutionMode.Inherit;

        public ReferenceResolutionMode uiReferenceResolutionMode
        {
            get { return m_ReferenceResolutionMode; }
            set { m_ReferenceResolutionMode = value; }
        }

        [SerializeField] private Vector2 m_ReferenceResolution;

        public Vector2 referenceResolution
        {
            get { return m_ReferenceResolution; }
            set { m_ReferenceResolution = value; }
        }

        [SerializeField] private CanvasScaler m_CanvasScaler;

        public CanvasScaler uiCanvasScaler
        {
            get { return m_CanvasScaler; }
            set { m_CanvasScaler = value; }
        }

        private RectTransform m_RectTransform;

        protected override void Awake()
        {
            m_RectTransform = this.transform.GetComponent<RectTransform>();
            ScaleMatchWidthOrHeight();
        }

        private void ScaleMatchWidthOrHeight()
        {
            if (m_CanvasScaler == null || m_RectTransform == null)
                return;

            Vector2 screenSize = new Vector2(Screen.width, Screen.height);
            Debug.LogFormat("{0},{1}", Screen.width, Screen.height);

            Vector2 referenceResolution = Vector2.zero;
            switch (m_ReferenceResolutionMode)
            {
                case ReferenceResolutionMode.Inherit:
                    referenceResolution = m_CanvasScaler.referenceResolution;
                    break;
                case ReferenceResolutionMode.Custom:
                    referenceResolution = m_ReferenceResolution;
                    break;
            }

            m_CanvasScaler.matchWidthOrHeight = 1;

            m_RectTransform.anchorMax = 0.5f * Vector2.one;
            m_RectTransform.anchorMin = 0.5f * Vector2.one;
            m_RectTransform.anchoredPosition = Vector2.zero;
            m_RectTransform.pivot = 0.5f * Vector2.one;

            float newWidth = referenceResolution.x / referenceResolution.y * screenSize.y;
            if (newWidth > screenSize.x)
            {
                float rate = screenSize.x / newWidth;
                m_RectTransform.localScale = new Vector3(rate, rate, 1);
            }
            else
            {
                m_RectTransform.localScale = Vector3.one;
            }
        }
    }
}

