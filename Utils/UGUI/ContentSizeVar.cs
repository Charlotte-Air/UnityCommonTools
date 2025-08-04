using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class ContentSizeVar : MonoBehaviour
{
    public int MaxWidth = 10;
    private ContentSizeFitter filter = null;
    private RectTransform rectTrans = null;
    private Text text = null;

    void Awake()
    {
        filter = GetComponent<ContentSizeFitter>();
        rectTrans = GetComponent<RectTransform>();
        text = GetComponent<Text>();
    }
    
    void Start()
    {
        
    }
    
    // Update is called once per frame
    void Update()
    {
        if (filter == null) filter = GetComponent<ContentSizeFitter>();
        if (rectTrans == null) rectTrans = GetComponent<RectTransform>();
        if (text == null) text = GetComponent<Text>();

        if (text.preferredWidth >= MaxWidth) {
            filter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            filter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            rectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, MaxWidth);
        } else {
            filter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            filter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }
    }
}
