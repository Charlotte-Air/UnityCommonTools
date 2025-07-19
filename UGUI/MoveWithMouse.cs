using System;
using DG.Tweening;
using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class MoveWithMouse : MonoBehaviour
{
    private enum EnumRotate
    {
        RotateXY = 0,
        RotateX = 1,
        RotateY = 2
    }
    public enum RotateDirection
    {
        ClockWise,
        AntiClockWise,
    }
    public enum EnumRotateAxis
    {
        X = 0,
        Y = 1,
        Z = 2,
    }
    public EnumRotateAxis targetAxis = EnumRotateAxis.Y;
    public float speed = 1000;
    public bool bEnable = true;
    public Func<bool> handleAction = null;
    public Action clickAction = null;

    private float deltaMove = -1;

    private float rotation;

    private Vector2 touchBeginRot = Vector3.zero;
    private bool bStartMove = false;

    public bool AutoRotate = false;
    public RotateDirection Direction = RotateDirection.ClockWise;
    public float Duration;
    public float DelayTime;

    private Coroutine rotateDelayCoroutine;

    public bool enableRot = true;

    void Awake()
    {
        switch (targetAxis)
        {
            case EnumRotateAxis.X:
                rotation = this.transform.eulerAngles.x;
                break;
            case EnumRotateAxis.Y:
                rotation = this.transform.eulerAngles.y;
                break;
            case EnumRotateAxis.Z:
                rotation = this.transform.eulerAngles.z;
                break;
        }
    }

    void OnEnable()
    {
        if (AutoRotate) DoStartRotate(0);
    }
    void OnDisable()
    {
        DoEndRotate();
    }

    void FixedUpdate()
    {
        if(!enableRot)
            return;
        
#if UNITY_EDITOR
        CalculateCameraByMouse();
#else
        CalculateCameraByTouch();
#endif
    }

    void Update()
    {
#if UNITY_EDITOR
        CalculateCameraUpdateByMouse();
#else
        CalculateCameraUpdateByTouch();
#endif
    }

    private void CalculateCameraByMouse()
    {
        if (Input.GetMouseButton(0))
        {
            if (handleAction != null && handleAction() || handleAction == null)
            {
                if (!bStartMove)
                {
                    touchBeginRot = Input.mousePosition;
                    bStartMove = true;
                    return;
                }

                float deltaX = Input.mousePosition.x - touchBeginRot.x;
                touchBeginRot = Input.mousePosition;

                float nextValue = 0;

                rotation -= deltaX * speed / Screen.width * 1280;

                switch (targetAxis)
                {
                    case EnumRotateAxis.X:
                        nextValue = Mathf.LerpAngle(transform.eulerAngles.x, rotation, 0.6f);
                        transform.eulerAngles = new Vector3(nextValue, transform.eulerAngles.y, transform.eulerAngles.z);
                        break;
                    case EnumRotateAxis.Y:
                        nextValue = Mathf.LerpAngle(transform.eulerAngles.y, rotation, 0.6f);
                        transform.eulerAngles = new Vector3(transform.eulerAngles.x, nextValue, transform.eulerAngles.z);
                        break;
                    case EnumRotateAxis.Z:
                        nextValue = Mathf.LerpAngle(transform.eulerAngles.z, rotation, 0.6f);
                        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, nextValue);
                        break;
                }
            }
            else
            {
                bStartMove = false;
            }
        }
        else
        {
            bStartMove = false;
        }
    }

    private void CalculateCameraUpdateByMouse()
    {
        if (deltaMove >= 0)
        {
            deltaMove += Time.deltaTime;
        }

        if (Input.GetMouseButtonDown(0))
        {
            DoEndRotate();
            deltaMove = 0;
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (deltaMove <= 0.2f)
            {
                if (clickAction != null)
                {
                    clickAction();
                }
            }
            deltaMove = -1;
            DoStartRotate(DelayTime);
        }
    }

    private void CalculateCameraByTouch()
    {
        //��ת
        if (Input.touchCount >= 1)
        {
            if (handleAction != null && handleAction() || handleAction == null)
            {
                DoEndRotate();
                RotateCamera(Input.GetTouch(0));
            }
            else
            {
                bStartMove = false;
                DoStartRotate(DelayTime);
            }
        }
        else
        {
            bStartMove = false;
            DoStartRotate(DelayTime);
        }
    }

    private void CalculateCameraUpdateByTouch()
    {
        if (deltaMove >= 0)
        {
            deltaMove += Time.deltaTime;
        }

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            deltaMove = 0;
        }

        if (Input.touchCount > 0 && (Input.GetTouch(0).phase == TouchPhase.Canceled || Input.GetTouch(0).phase == TouchPhase.Ended))
        {
            if (deltaMove <= 0.2f)
            {
                if (clickAction != null)
                {
                    clickAction();
                }
            }
            deltaMove = -1;
        }
    }
    
    private void RotateCamera(Touch touch)
    {
        if (!bStartMove)
        {
            touchBeginRot = touch.position;
            bStartMove = true;
            return;
        }

        EnumRotate enumRotate = GetRotate(touch);

        //����
        if (enumRotate != EnumRotate.RotateY)
        {
            float deltaX = touch.position.x - touchBeginRot.x;
            touchBeginRot = touch.position;

            float nextValue = 0;

            rotation -= deltaX * speed / Screen.width * 1280;

            switch (targetAxis)
            {
                case EnumRotateAxis.X:
                    nextValue = Mathf.LerpAngle(transform.eulerAngles.x, rotation, 0.6f);
                    transform.eulerAngles = new Vector3(nextValue, transform.eulerAngles.y, transform.eulerAngles.z);
                    break;
                case EnumRotateAxis.Y:
                    nextValue = Mathf.LerpAngle(transform.eulerAngles.y, rotation, 0.6f);
                    transform.eulerAngles = new Vector3(transform.eulerAngles.x, nextValue, transform.eulerAngles.z);
                    break;
                case EnumRotateAxis.Z:
                    nextValue = Mathf.LerpAngle(transform.eulerAngles.z, rotation, 0.6f);
                    transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, nextValue);
                    break;
            }
        }
        
    }

    private EnumRotate GetRotate(Touch touch)
    {
        float tempAngle = Mathf.Atan(Mathf.Abs(touch.deltaPosition.x) / Mathf.Abs(touch.deltaPosition.y)) *
                          Mathf.Rad2Deg;
        if (tempAngle < 15)
        {
            return EnumRotate.RotateY;
        }
        else if (90 - tempAngle < 15)
        {
            return EnumRotate.RotateX;
        }
        else
        {
            return EnumRotate.RotateXY;
        }
    }
    
    private int MoveXDir(Touch touch)
    {
        if (touch.deltaPosition.x > 0)
        {
            return 1;
        }
        else if (touch.deltaPosition.x < 0)
        {
            return -1;
        }
        else
        {
            return 0;
        }
    }

    private void DoStartRotate(float delay)
    {
        if(rotateDelayCoroutine == null && AutoRotate)
        {
            rotateDelayCoroutine = StartCoroutine(DelayStartRotate(delay));
        }
    }
    private IEnumerator DelayStartRotate(float delay)
    {
        float t = 0;
        while(t < delay)
        {
            t += Time.deltaTime;
            yield return null;
        }
        Vector3 endV = Direction == RotateDirection.ClockWise ? new Vector3(0, 90, 0) : new Vector3(0, -90, 0);
        gameObject.transform.DORotate(endV, Duration)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Incremental);
    }
    private void DoEndRotate()
    {
        if (rotateDelayCoroutine != null)
        {
            StopCoroutine(rotateDelayCoroutine);
            rotateDelayCoroutine = null;
        }
        DOTween.Kill(gameObject.transform);
    }
}