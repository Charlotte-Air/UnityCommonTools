using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using System.Linq;
using Sirenix.Utilities;
using Soda;
using UnityEngine.EventSystems;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(GridLayoutGroup), typeof(ContentSizeFitter)), DisallowMultipleComponent, ExecuteInEditMode]
[AddComponentMenu("UI/UIPopupGrid")]
public class UIPopupGrid : MonoBehaviour
{
    private RectTransform rect;
    private GridLayoutGroup grid;

    private SubUIPopupBtn subUI;

    public int maxColumnCount;
    public Button btnMain;
    public bool bInitShow = false;
    public GraphicRaycaster matchCanvas;

    
    void Awake()
    {
        rect = this.GetComponent<RectTransform>();
        grid = this.GetComponent<GridLayoutGroup>();

        ContentSizeFitter fitter = this.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 0;
        //rect.pivot = new Vector2(0.5f, 1);

        if (btnMain != null)
        {
            btnMain.onClick.RemoveAllListeners();
            btnMain.onClick.AddListener(() =>
            {
                this.gameObject.SetActive(!this.gameObject.activeSelf);
            });
        }

        if (Application.isPlaying)
        {
            subUI = btnMain.gameObject.GetOrAddComponent<SubUIPopupBtn>();
            subUI.subTrans = this.transform;
            this.gameObject.SetActive(bInitShow);
            SetGridSize();
        }
    }

    void OnEnable()
    {
        SetGridSize();
    }

    void Update()
    {
        SetGridSize();

        if (EventSystem.current != null)
        {
#if UNITY_EDITOR
            if (Input.GetMouseButtonUp(0))
#else
            if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Ended)
#endif
            {
                bool isShow = false;

                if (EventSystem.current.IsPointerOverGameObject())
                {
                    PointerEventData pointerEventData = new PointerEventData(EventSystem.current)
                    {
#if UNITY_EDITOR
                        position = Input.mousePosition
#else
                        position = Input.GetTouch(0).position
#endif
                    };

                    List<RaycastResult> results = new List<RaycastResult>();

                    matchCanvas.Raycast(pointerEventData, results);

                    //EventSystem.current.RaycastAll(pointerEventData, results);

                    if (results.Count > 0)
                    {
                        if (results[0].gameObject.transform.IsChildOf(rect) && results[0].gameObject != rect.gameObject)
                        {
                            isShow = true;
                        }
                    }
                    else
                    {
                        isShow = true;
                    }
                }

                this.gameObject.SetActive(isShow);
            }
        }
    }

    private void SetGridSize()
    {
        int num = Mathf.Min(GetActiveChildCount(transform), maxColumnCount);

        grid.constraintCount = num;
        rect.sizeDelta = new Vector2(num * grid.cellSize.x + grid.padding.left + grid.padding.right + grid.spacing.x * (num - 1), rect.sizeDelta.y);
    }

    public int GetActiveChildCount(Transform root)
    {
        int childCount = 0;
        for (int i = 0; i < root.childCount; i++)
        {
            GameObject child = root.GetChild(i).gameObject;
            if (child.activeSelf)
            {
                LayoutElement childLE = child.transform.GetComponent<LayoutElement>();
                if (childLE == null || !childLE.ignoreLayout)
                {
                    childCount++;
                }
            }
        }
        return childCount;
    }
}

public class SubUIPopupBtn: MonoBehaviour
{
    private LayoutElement le;
    private Image btnImg;
    private Button btnMain;
    public Transform subTrans;

    void Awake()
    {
        le = this.gameObject.GetOrAddComponent<LayoutElement>();
        btnImg = this.gameObject.GetComponent<UIImage>();
        btnMain = this.gameObject.GetComponent<Button>();
    }

    
    void Update()
    {
        bool bHide = !IsActiveChid();

        if (le.ignoreLayout != bHide)
        {
            le.ignoreLayout = bHide;
            btnMain.interactable = !bHide;
            btnImg.color = bHide ? new Color(255, 255, 255, 0) : Color.white;
            btnImg.raycastTarget = bHide ? false : true;
        }
    }

    private bool IsActiveChid()
    {
        bool bActive = false;

        if (subTrans != null)
        {
            for (int i = 0; i <subTrans.childCount ; i++)
            {
                GameObject child = subTrans.GetChild(i).gameObject;
                if (child.activeSelf)
                {
                    LayoutElement childLE = child.transform.GetComponent<LayoutElement>();
                    if (childLE == null || !childLE.ignoreLayout)
                    {
                        bActive = true;
                        break;
                    }
                }
            }
        }
        return bActive;
    }
}