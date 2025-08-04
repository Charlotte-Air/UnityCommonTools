using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(GraphicRaycaster))]
public class CurvedUISettings : MonoBehaviour
{
    #region SETTINGS

    //Global settings
    [SerializeField] CurvedUIShape shape;

    [SerializeField] float quality = 1f;

    //Cyllinder settings
    [SerializeField] int angle = 90;
    [SerializeField] bool preserveAspect = true;

    //internal system settings
    int baseCircleSegments = 24;


    //support variables
    Vector2 savedRectSize;
    float savedRadius;
    Canvas myCanvas;

    #endregion


    #region LIFECYCLE

    void Start()
    {
        if (myCanvas == null)
            myCanvas = GetComponent<Canvas>();

        savedRadius = GetCyllinderRadiusInCanvasSpace();
    }


    void OnEnable()
    {
        //Redraw canvas object on enable.
        foreach (UnityEngine.UI.Graphic graph in (this).GetComponentsInChildren<UnityEngine.UI.Graphic>())
        {
            graph.SetAllDirty();
        }
    }

    void OnDisable()
    {
        foreach (UnityEngine.UI.Graphic graph in (this).GetComponentsInChildren<UnityEngine.UI.Graphic>())
        {
            graph.SetAllDirty();
        }
    }

    void Update()
    {
        //recreate the geometry if entire canvas has been resized
        if ((transform as RectTransform).rect.size != savedRectSize)
        {
            savedRectSize = (transform as RectTransform).rect.size;
            SetUIAngle(angle);
        }
    }
    #endregion


    #region PRIVATE

    /// <summary>
    /// Changes the horizontal angle of the canvas.
    /// </summary>
    /// <param name="newAngle"></param>
    void SetUIAngle(int newAngle)
    {
        if (myCanvas == null)
            myCanvas = GetComponent<Canvas>();

        //temp fix to make interactions with angle 0 possible
        if (newAngle == 0) newAngle = 1;

        angle = newAngle;

        savedRadius = GetCyllinderRadiusInCanvasSpace();

        foreach (CurvedUIVertexEffect ve in GetComponentsInChildren<CurvedUIVertexEffect>())
            ve.TesselationRequired = true;

        foreach (Graphic graph in GetComponentsInChildren<Graphic>())
            graph.SetVerticesDirty();
    }
    #endregion

    /// <summary>
    /// Returns the radius of curved canvas cyllinder, expressed in Cavas's local space units.
    /// </summary>
    public float GetCyllinderRadiusInCanvasSpace()
    {
        float ret;
        if (PreserveAspect)
        {
            if (shape == CurvedUIShape.Bezier)
            {
                ret = ((transform as RectTransform).rect.size.x / ((2 * Mathf.PI) * (angle / 360.0f)));
            }
            else
            {
                ret = ((transform as RectTransform).rect.size.x / ((2 * Mathf.PI) * (angle / 360.0f)));
            }
        }
        else
            ret = ((transform as RectTransform).rect.size.x * 0.5f) /
                  Mathf.Sin(Mathf.Clamp(angle, -180.0f, 180.0f) * 0.5f * Mathf.Deg2Rad);

        return angle == 0 ? 0 : ret;
    }

    /// <summary>
    /// Tells you how big UI quads can get before they should be tesselate to look good on current canvas settings.
    /// Used by CurvedUIVertexEffect to determine how many quads need to be created for each graphic.
    /// </summary>
    public Vector2 GetTesslationSize(bool UnmodifiedByQuality = false)
    {
        Vector2 canvasSize = GetComponent<RectTransform>().rect.size;
        float ret = canvasSize.x;
        float ret2 = canvasSize.y;

        if (Angle != 0 || (!PreserveAspect))
        {
            switch (shape)
            {
                case CurvedUIShape.Bezier:
                {
                    ret = Mathf.Min(canvasSize.x / 4,
                        canvasSize.x / (Mathf.Abs(angle).Remap(0.0f, 360.0f, 0, 1) * baseCircleSegments));
                    ret2 = Mathf.Min(canvasSize.y / 4,
                        canvasSize.y / (Mathf.Abs(angle).Remap(0.0f, 360.0f, 0, 1) * baseCircleSegments));
                    break;
                }
            }
        }

        return new Vector2(ret, ret2) / (UnmodifiedByQuality ? 1 : Mathf.Clamp(Quality, 0.01f, 10.0f));
    }

    /// <summary>
    /// The measure of the arc of the Canvas.
    /// </summary>
    public int Angle
    {
        get { return angle; }
        set
        {
            if (angle != value)
            {
                angle = value;
                SetUIAngle(value);
            }

        }
    }

    /// <summary>
    /// Multiplier used to deremine how many segments a base curve of a shape has.
    /// Default 1. Lower values greatly increase performance. Higher values give you sharper curve.
    /// </summary>
    public float Quality
    {
        get { return quality; }
        set
        {
            if (quality != value)
            {
                quality = value;
                SetUIAngle(angle);
            }
        }
    }

    /// <summary>
    /// Current Shape of the canvas
    /// </summary>
    public CurvedUIShape Shape
    {
        get { return shape; }
        set
        {
            if (shape != value)
            {
                shape = value;
                SetUIAngle(angle);
            }
        }
    }

    /// <summary>
    /// If enabled, CurvedUI will try to preserve aspect ratio of original canvas.
    /// </summary>
    public bool PreserveAspect
    {
        get { return preserveAspect; }
        set
        {
            if (preserveAspect != value)
            {
                preserveAspect = value;
                SetUIAngle(angle);
            }
        }
    }

    public enum CurvedUIShape
    {
        Bezier = 0
    }
}