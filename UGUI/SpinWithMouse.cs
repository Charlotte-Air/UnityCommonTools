using UnityEngine;
using UnityEngine.EventSystems;

public class SpinWithMouse : MonoBehaviour,IDragHandler,IPointerDownHandler,IPointerUpHandler
{
    /// <summary>
    /// ��ת��Ŀ��
    /// </summary>
	private Transform target;
    public Transform Target
    {
        get { return target; }
        set { target = value; }
    }

    /// <summary>
    /// ��ת�ٶ�
    /// </summary>
	private float speed = 0.7f;
    public float Speed
    {
        get { return speed; }
        set { speed = value; } 
    }

    /// <summary>
    /// ��ת�ص� 
    /// </summary>
    /// <param name="delta"></param>
    public delegate void OnDragHandle(Vector2 delta);
    public OnDragHandle onDrag;

    /// <summary>
    /// ����ص�
    /// </summary>
    public delegate void OnPressHandle();
    public OnPressHandle onPress;

    private Animator _animator;
    /// <summary>
    /// �ܷ����ص�0,0,0��λ��
    /// </summary>
    private bool bCreateRole = false;
    public bool BCreateRole
    {
        get { return bCreateRole; }
        set { bCreateRole = value; }
    }

    /// <summary>
    /// ʱ����
    /// </summary>
    private float delaytime = 0.0f;


	void Start ()
	{
        if (target != null)
        {
            _animator = target.GetComponent<Animator>();
        }
	}

    void OnDestroy()
    {
        onDrag = null;
        onPress = null;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (target != null)
        {
            if (_animator != null)
            {
                if (_animator.GetCurrentAnimatorStateInfo(0).IsTag("SpeAttackTag"))
                    return;
            }
            target.RotateAround(target.position, Vector3.up, -eventData.delta.x * speed);

            if (onDrag != null)
            {
                onDrag(eventData.delta);
            }
        }
        //else
        //{
        //    mTrans.RotateAround(mTrans.position, Vector3.up, Mathf.Clamp(-0.5f * eventData.delta.x * speed, -10, 10));
        //}
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        delaytime = Time.realtimeSinceStartup;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (Time.realtimeSinceStartup - delaytime < 0.5f)
        {
            if (onPress != null)
            {
                onPress();
            }
        }
    }

    float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360)
            angle += 360;
        if (angle > 360)
            angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }

    private float GetAngle(float angle)
    {
        if (angle < 0)
            return angle + 360;
        else
            return angle;
    }
}