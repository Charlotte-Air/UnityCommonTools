using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;

static internal class UIMenuOptions
{
    private const string kUILayerName = "UI";
    private const string kStandardSpritePath = "UI/Skin/UISprite.psd";
    private const string kBackgroundSpritePath = "UI/Skin/Background.psd";
    private const string kInputFieldBackgroundPath = "UI/Skin/InputFieldBackground.psd";
    private const string kKnobPath = "UI/Skin/Knob.psd";
    private const string kCheckmarkPath = "UI/Skin/Checkmark.psd";
    private const string kDropdownArrowPath = "UI/Skin/DropdownArrow.psd";
    private const string kMaskPath = "UI/Skin/UIMask.psd";


    static private DefaultControls.Resources s_StandardResources;

    static private DefaultControls.Resources GetStandardResources()
    {
        if (s_StandardResources.standard == null)
        {
            s_StandardResources.standard = AssetDatabase.GetBuiltinExtraResource<Sprite>(kStandardSpritePath);
            s_StandardResources.background = AssetDatabase.GetBuiltinExtraResource<Sprite>(kBackgroundSpritePath);
            s_StandardResources.inputField = AssetDatabase.GetBuiltinExtraResource<Sprite>(kInputFieldBackgroundPath);
            s_StandardResources.knob = AssetDatabase.GetBuiltinExtraResource<Sprite>(kKnobPath);
            s_StandardResources.checkmark = AssetDatabase.GetBuiltinExtraResource<Sprite>(kCheckmarkPath);
            s_StandardResources.dropdown = AssetDatabase.GetBuiltinExtraResource<Sprite>(kDropdownArrowPath);
            s_StandardResources.mask = AssetDatabase.GetBuiltinExtraResource<Sprite>(kMaskPath);
        }
        return s_StandardResources;
    }

    private static void SetPositionVisibleinSceneView(RectTransform canvasRTransform, RectTransform itemTransform)
    {
        // Find the best scene view
        SceneView sceneView = SceneView.lastActiveSceneView;
        if (sceneView == null && SceneView.sceneViews.Count > 0)
            sceneView = SceneView.sceneViews[0] as SceneView;

        // Couldn't find a SceneView. Don't set position.
        if (sceneView == null || sceneView.camera == null)
            return;

        // Create world space Plane from canvas position.
        Vector2 localPlanePosition;
        Camera camera = sceneView.camera;
        Vector3 position = Vector3.zero;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRTransform, new Vector2(camera.pixelWidth / 2, camera.pixelHeight / 2), camera, out localPlanePosition))
        {
            // Adjust for canvas pivot
            localPlanePosition.x = localPlanePosition.x + canvasRTransform.sizeDelta.x * canvasRTransform.pivot.x;
            localPlanePosition.y = localPlanePosition.y + canvasRTransform.sizeDelta.y * canvasRTransform.pivot.y;

            localPlanePosition.x = Mathf.Clamp(localPlanePosition.x, 0, canvasRTransform.sizeDelta.x);
            localPlanePosition.y = Mathf.Clamp(localPlanePosition.y, 0, canvasRTransform.sizeDelta.y);

            // Adjust for anchoring
            position.x = localPlanePosition.x - canvasRTransform.sizeDelta.x * itemTransform.anchorMin.x;
            position.y = localPlanePosition.y - canvasRTransform.sizeDelta.y * itemTransform.anchorMin.y;

            Vector3 minLocalPosition;
            minLocalPosition.x = canvasRTransform.sizeDelta.x * (0 - canvasRTransform.pivot.x) + itemTransform.sizeDelta.x * itemTransform.pivot.x;
            minLocalPosition.y = canvasRTransform.sizeDelta.y * (0 - canvasRTransform.pivot.y) + itemTransform.sizeDelta.y * itemTransform.pivot.y;

            Vector3 maxLocalPosition;
            maxLocalPosition.x = canvasRTransform.sizeDelta.x * (1 - canvasRTransform.pivot.x) - itemTransform.sizeDelta.x * itemTransform.pivot.x;
            maxLocalPosition.y = canvasRTransform.sizeDelta.y * (1 - canvasRTransform.pivot.y) - itemTransform.sizeDelta.y * itemTransform.pivot.y;

            position.x = Mathf.Clamp(position.x, minLocalPosition.x, maxLocalPosition.x);
            position.y = Mathf.Clamp(position.y, minLocalPosition.y, maxLocalPosition.y);
        }

        itemTransform.anchoredPosition = position;
        itemTransform.localRotation = Quaternion.identity;
        itemTransform.localScale = Vector3.one;
    }

    private static void PlaceUIElementRoot(GameObject element, MenuCommand menuCommand)
    {
        GameObject parent = menuCommand.context as GameObject;
        if (parent == null || parent.GetComponentInParent<Canvas>() == null)
        {
            parent = GetOrCreateCanvasGameObject();
        }

        string uniqueName = GameObjectUtility.GetUniqueNameForSibling(parent.transform, element.name);
        element.name = uniqueName;
        Undo.RegisterCreatedObjectUndo(element, "Create " + element.name);
        Undo.SetTransformParent(element.transform, parent.transform, "Parent " + element.name);
        GameObjectUtility.SetParentAndAlign(element, parent);
        if (parent != menuCommand.context) // not a context click, so center in sceneview
            SetPositionVisibleinSceneView(parent.GetComponent<RectTransform>(), element.GetComponent<RectTransform>());

        Selection.activeGameObject = element;
    }

    // Graphic elements

    [MenuItem("GameObject/UI/UIText", false, 1998)]
    static public void AddText(MenuCommand menuCommand)
    {
        GameObject go = CreateText(GetStandardResources());
        PlaceUIElementRoot(go, menuCommand);
    }

    [MenuItem("GameObject/UI/UIImage", false, 1999)]
    static public void AddImage(MenuCommand menuCommand)
    {
        GameObject go = CreateImage(GetStandardResources());
        PlaceUIElementRoot(go, menuCommand);
    }

    [MenuItem("GameObject/UI/UIRawImage", false, 1999)]
    static public void AddRawImage(MenuCommand menuCommand)
    {
        GameObject go = CreateRawImage(GetStandardResources());
        PlaceUIElementRoot(go, menuCommand);
    }

    [MenuItem("GameObject/UI/UIButton", false, 2000)]
    static public void AddButton(MenuCommand menuCommand)
    {
        GameObject go = CreateButton(GetStandardResources());
        PlaceUIElementRoot(go, menuCommand);
    }

    [MenuItem("GameObject/UI/UISlider", false, 2000)]
    static public void AddSlider(MenuCommand menuCommand)
    {
        GameObject go = CreateSlider(GetStandardResources());
        PlaceUIElementRoot(go, menuCommand);
    }

    [MenuItem("GameObject/UI/UIInputField", false, 2000)]
    static public void AddInputField(MenuCommand menuCommand)
    {
        GameObject go = CreateInputField(GetStandardResources());
        PlaceUIElementRoot(go, menuCommand);
    }

    [MenuItem("GameObject/UI/UIToggle", false, 2000)]
    static public void AddToggle(MenuCommand menuCommand)
    {
        GameObject go = CreateToggle(GetStandardResources());
        PlaceUIElementRoot(go, menuCommand);
    }

    [MenuItem("GameObject/UI/UIToggleGroup", false, 2000)]
    static public void AddToggleGroup(MenuCommand menuCommand)
    {
        GameObject go = CreateToggleGroup(GetStandardResources());
        PlaceUIElementRoot(go, menuCommand);
    }

    [MenuItem("GameObject/UI/UIScroll", false, 2000)]
    static public void AddScroll(MenuCommand menuCommand)
    {
        GameObject go = CreateScroll(GetStandardResources());
        PlaceUIElementRoot(go, menuCommand);
    }

    [MenuItem("GameObject/UI/UILoopVerticalScroll", false, 2000)]
    static public void AddLoopVerticalScroll(MenuCommand menuCommand)
    {
        GameObject go = CreateLoopVerticalScroll(GetStandardResources());
        PlaceUIElementRoot(go, menuCommand);
    }

    [MenuItem("GameObject/UI/UILoopHorizontalScroll", false, 2000)]
    static public void AddLoopHorizontalScroll(MenuCommand menuCommand)
    {
        GameObject go = CreateLoopHorizontalScroll(GetStandardResources());
        PlaceUIElementRoot(go, menuCommand);
    }

    [MenuItem("GameObject/UI/UIDropdown", false, 2000)]
    static public void AddDropdown(MenuCommand menuCommand)
    {
        GameObject go = CreateDropdown(GetStandardResources());
        PlaceUIElementRoot(go, menuCommand);
    }

    [MenuItem("GameObject/UI/ElasticMenu", false, 2000)]
    static public void AddElasticMenu(MenuCommand menuCommand)
    {
        GameObject go = CreatElasticMenu(GetStandardResources());
        PlaceUIElementRoot(go, menuCommand);
    }

    [MenuItem("GameObject/UI/UIBloodSlider", false, 2000)]
    static public void AddUIBloodSlider(MenuCommand menuCommand)
    {
        GameObject go = CreatUIBloodSlider(GetStandardResources());
        PlaceUIElementRoot(go, menuCommand);
    }
    // Helper methods

    static public GameObject CreateNewUI()
    {
        // Root for the UI
        var root = new GameObject("Canvas");
        root.layer = LayerMask.NameToLayer(kUILayerName);
        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1;
        root.AddComponent<CanvasScaler>();
        root.AddComponent<GraphicRaycaster>();
        Undo.RegisterCreatedObjectUndo(root, "Create " + root.name);

        // if there is no event system add one...
        CreateEventSystem(false);
        return root;
    }

    private static void CreateEventSystem(bool select)
    {
        CreateEventSystem(select, null);
    }

    private static void CreateEventSystem(bool select, GameObject parent)
    {
        var esys = Object.FindObjectOfType<EventSystem>();
        if (esys == null)
        {
            var eventSystem = new GameObject("EventSystem");
            GameObjectUtility.SetParentAndAlign(eventSystem, parent);
            esys = eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
            //eventSystem.AddComponent<TouchInputModule>();

            Undo.RegisterCreatedObjectUndo(eventSystem, "Create " + eventSystem.name);
        }

        if (select && esys != null)
        {
            Selection.activeGameObject = esys.gameObject;
        }
    }

    static public GameObject GetOrCreateCanvasGameObject()
    {
        GameObject selectedGo = Selection.activeGameObject;

        // Try to find a gameobject that is the selected GO or one if its parents.
        Canvas canvas = (selectedGo != null) ? selectedGo.GetComponentInParent<Canvas>() : null;
        if (canvas != null && canvas.gameObject.activeInHierarchy)
            return canvas.gameObject;

        // No canvas in selection or its parents? Then use just any canvas..
        canvas = Object.FindObjectOfType(typeof(Canvas)) as Canvas;
        if (canvas != null && canvas.gameObject.activeInHierarchy)
            return canvas.gameObject;

        // No canvas in the scene at all? Then create a new one.
        return UIMenuOptions.CreateNewUI();
    }

    // DefaultControls create

    private const float kWidth = 160f;
    private const float kThickHeight = 30f;
    private const float kThinHeight = 20f;
    private static Vector2 s_ThickElementSize = new Vector2(kWidth, kThickHeight);
    private static Vector2 s_ThinElementSize = new Vector2(kWidth, kThinHeight);
    private static Vector2 s_ImageElementSize = new Vector2(100f, 100f);
    private static Color s_DefaultSelectableColor = new Color(1f, 1f, 1f, 1f);
    private static Color s_PanelColor = new Color(1f, 1f, 1f, 0.392f);
    private static Color s_TextColor = new Color(50f / 255f, 50f / 255f, 50f / 255f, 1f);

    private const string kFontPath = "Assets/DataRes/UI/Font/FZWBJW.TTF";
    private static Font s_DefaultFont;

    private static void SetDefaultTextValues(Text lbl)
    {
        // Set text values we want across UI elements in default controls.
        // Don't set values which are the same as the default values for the Text component,
        // since there's no point in that, and it's good to keep them as consistent as possible.
        if(s_DefaultFont == null)
        {
            s_DefaultFont = AssetDatabase.LoadAssetAtPath<Font>(kFontPath);
        }

        lbl.color = s_TextColor;
        lbl.font = s_DefaultFont;
    }

    private static void SetDefaultColorTransitionValues(Selectable slider)
    {
        ColorBlock colors = slider.colors;
        colors.highlightedColor = new Color(0.882f, 0.882f, 0.882f);
        colors.pressedColor = new Color(0.698f, 0.698f, 0.698f);
        colors.disabledColor = new Color(0.521f, 0.521f, 0.521f);
    }

    private static void SetParentAndAlign(GameObject child, GameObject parent)
    {
        if (parent == null)
            return;

        child.transform.SetParent(parent.transform, false);
        SetLayerRecursively(child, parent.layer);
    }

    private static void SetLayerRecursively(GameObject go, int layer)
    {
        go.layer = layer;
        Transform t = go.transform;
        for (int i = 0; i < t.childCount; i++)
            SetLayerRecursively(t.GetChild(i).gameObject, layer);
    }

    private static GameObject CreateUIElementRoot(string name, Vector2 size)
    {
        GameObject child = new GameObject(name);
        RectTransform rectTransform = child.AddComponent<RectTransform>();
        rectTransform.sizeDelta = size;
        return child;
    }

    static GameObject CreateUIObject(string name, GameObject parent)
    {
        GameObject go = new GameObject(name);
        go.AddComponent<RectTransform>();
        SetParentAndAlign(go, parent);
        return go;
    }

    public static GameObject CreateText(DefaultControls.Resources resources)
    {
        GameObject go = CreateUIElementRoot("Text", s_ThickElementSize);

        Text lbl = go.AddComponent<UIText>();
        lbl.text = "New Text";
        SetDefaultTextValues(lbl);
        //lbl.color = s_TextColor;
        //lbl.font = Font.CreateDynamicFontFromOSFont("FZWBJW",14);

        return go;
    }

    public static GameObject CreateImage(DefaultControls.Resources resources)
    {
        GameObject go = CreateUIElementRoot("Image", s_ImageElementSize);
        go.AddComponent<UIImage>();
        return go;
    }

    public static GameObject CreateRawImage(DefaultControls.Resources resources)
    {
        GameObject go = CreateUIElementRoot("RawImage", s_ImageElementSize);
        go.AddComponent<UIRawImage>();
        return go;
    }

    public static GameObject CreateButton(DefaultControls.Resources resources)
    {
        GameObject buttonRoot = CreateUIElementRoot("Button", s_ThickElementSize);

        GameObject childText = new GameObject("Text");
        SetParentAndAlign(childText, buttonRoot);

        Image image = buttonRoot.AddComponent<UIImage>();
        image.sprite = resources.standard;
        image.type = Image.Type.Sliced;
        image.color = s_DefaultSelectableColor;

        UIButton bt = buttonRoot.AddComponent<UIButton>();
        SetDefaultColorTransitionValues(bt);

        Text text = childText.AddComponent<UIText>();
        text.text = "Button";
        text.alignment = TextAnchor.MiddleCenter;
        SetDefaultTextValues(text);

        RectTransform textRectTransform = childText.GetComponent<RectTransform>();
        textRectTransform.anchorMin = Vector2.zero;
        textRectTransform.anchorMax = Vector2.one;
        textRectTransform.sizeDelta = Vector2.zero;

        return buttonRoot;
    }

    public static GameObject CreateSlider(DefaultControls.Resources resources)
    {
        // Create GOs Hierarchy
        GameObject root = CreateUIElementRoot("Slider", s_ThinElementSize);

        GameObject background = CreateUIObject("Background", root);
        GameObject fillArea = CreateUIObject("Fill Area", root);
        GameObject fill = CreateUIObject("Fill", fillArea);
        GameObject handleArea = CreateUIObject("Handle Slide Area", root);
        GameObject handle = CreateUIObject("Handle", handleArea);

        // Background
        Image backgroundImage = background.AddComponent<UIImage>();
        backgroundImage.sprite = resources.background;
        backgroundImage.type = Image.Type.Sliced;
        backgroundImage.color = s_DefaultSelectableColor;
        RectTransform backgroundRect = background.GetComponent<RectTransform>();
        backgroundRect.anchorMin = new Vector2(0, 0.25f);
        backgroundRect.anchorMax = new Vector2(1, 0.75f);
        backgroundRect.sizeDelta = new Vector2(0, 0);

        // Fill Area
        RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0, 0.25f);
        fillAreaRect.anchorMax = new Vector2(1, 0.75f);
        fillAreaRect.anchoredPosition = new Vector2(-5, 0);
        fillAreaRect.sizeDelta = new Vector2(-20, 0);

        // Fill
        Image fillImage = fill.AddComponent<UIImage>();
        fillImage.sprite = resources.standard;
        fillImage.type = Image.Type.Sliced;
        fillImage.color = s_DefaultSelectableColor;

        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.sizeDelta = new Vector2(10, 0);

        // Handle Area
        RectTransform handleAreaRect = handleArea.GetComponent<RectTransform>();
        handleAreaRect.sizeDelta = new Vector2(-20, 0);
        handleAreaRect.anchorMin = new Vector2(0, 0);
        handleAreaRect.anchorMax = new Vector2(1, 1);

        // Handle
        Image handleImage = handle.AddComponent<UIImage>();
        handleImage.sprite = resources.knob;
        handleImage.color = s_DefaultSelectableColor;

        RectTransform handleRect = handle.GetComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(20, 0);

        // Setup slider component
        UISlider slider = root.AddComponent<UISlider>();
        slider.fillRect = fill.GetComponent<RectTransform>();
        slider.handleRect = handle.GetComponent<RectTransform>();
        slider.targetGraphic = handleImage;
        slider.direction = UISlider.Direction.LeftToRight;
        SetDefaultColorTransitionValues(slider);

        return root;
    }

    public static GameObject CreateInputField(DefaultControls.Resources resources)
    {
        // Create GOs Hierarchy
        GameObject root = CreateUIElementRoot("UIInputField", s_ThinElementSize);

        GameObject placeholder = new GameObject("Placeholder");
        SetParentAndAlign(placeholder, root);
        GameObject Text = new GameObject("Text");
        SetParentAndAlign(Text, root);

        UIText utext = Text.AddComponent<UIText>();
        utext.text = "";
        SetDefaultTextValues(utext);
        //utext.color = s_TextColor;
        //utext.font = Font.CreateDynamicFontFromOSFont("FZWBJW", 14);

        UIText ptext = placeholder.AddComponent<UIText>();
        ptext.text = "Enter text...";
        SetDefaultTextValues(ptext);
        //ptext.color = s_TextColor;
        //ptext.font = Font.CreateDynamicFontFromOSFont("FZWBJW", 14);

        UIImage img = root.AddComponent<UIImage>();
        img.sprite = resources.standard;
        img.type = Image.Type.Sliced;
        img.color = s_DefaultSelectableColor;

        UIInputField input = root.AddComponent<UIInputField>();
        input.textComponent = utext;
        input.placeholder = ptext;

        RectTransform iRectTransform = root.GetComponent<RectTransform>();
        iRectTransform.sizeDelta = new Vector2(160, 30);

        RectTransform tRectTransform = Text.GetComponent<RectTransform>();
        tRectTransform.anchorMin = Vector2.zero;
        tRectTransform.anchorMax = Vector2.one;
        tRectTransform.sizeDelta = new Vector2(-20,-13);

        RectTransform pRectTransform = placeholder.GetComponent<RectTransform>();
        pRectTransform.anchorMin = Vector2.zero;
        pRectTransform.anchorMax = Vector2.one;
        pRectTransform.sizeDelta = new Vector2(-20, -13);

        return root;
    }

    public static GameObject CreateToggle(DefaultControls.Resources resources)
    {
        GameObject root = CreateUIElementRoot("UIToggle", s_ThinElementSize);

        GameObject bkg = new GameObject("Background");
        SetParentAndAlign(bkg, root);

        GameObject mask = new GameObject("Checkmark");
        SetParentAndAlign(mask, bkg);

        UIImage mimg = mask.AddComponent<UIImage>();
        mimg.sprite = resources.standard;
        mimg.type = Image.Type.Sliced;
        mimg.color = s_DefaultSelectableColor;

        UIImage img = bkg.AddComponent<UIImage>();
        img.sprite = resources.standard;
        img.type = Image.Type.Sliced;
        img.color = s_DefaultSelectableColor;

        UIToggle toggle = root.AddComponent<UIToggle>();
        toggle.targetGraphic = img;
        toggle.graphic = new Graphic[] { mimg };

        RectTransform iRectTransform = bkg.GetComponent<RectTransform>();
        iRectTransform.anchorMin = Vector2.zero;
        iRectTransform.anchorMax = Vector2.one;
        iRectTransform.sizeDelta = Vector2.zero;

        return root;
    }

    public static GameObject CreateToggleGroup(DefaultControls.Resources resources)
    {
        GameObject root = CreateUIElementRoot("UIToggleGroup", s_ThinElementSize);
        root.AddComponent<UIToggleGroup>();

        return root;
    }

    public static GameObject CreateScroll(DefaultControls.Resources resources)
    {
        GameObject root = CreateUIElementRoot("UIScroll", s_ThinElementSize);
        GameObject Viewport = CreateUIObject("Viewport", root);
        GameObject Content = CreateUIObject("Content", Viewport);

        RectTransform rRectTransform = root.GetComponent<RectTransform>();
        rRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rRectTransform.sizeDelta = new Vector2(200, 200);

        RectTransform vRectTransform = Viewport.GetComponent<RectTransform>();
        vRectTransform.anchorMin = new Vector2(0, 0);
        vRectTransform.anchorMax = new Vector2(1, 1);
        vRectTransform.sizeDelta = new Vector2(0, 0);
        vRectTransform.pivot = new Vector2(0, 1);

        RectTransform cRectTransform = Content.GetComponent<RectTransform>();
        cRectTransform.anchorMin = new Vector2(0, 1);
        cRectTransform.anchorMax = new Vector2(1, 1);
        cRectTransform.sizeDelta = new Vector2(0, 300);
        cRectTransform.pivot = new Vector2(0, 1);

        UIImage img = Viewport.AddComponent<UIImage>();
        img.sprite = resources.background;
        img.type = Image.Type.Sliced;
        img.color = s_DefaultSelectableColor;

        Mask mask = Viewport.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        ScrollRect scroll = root.AddComponent<ScrollRect>();
        scroll.viewport = vRectTransform;
        scroll.content = cRectTransform;

        UIImage bg = root.AddComponent<UIImage>();
        bg.sprite = resources.background;
        bg.type = Image.Type.Sliced;
        bg.color = s_PanelColor;

        return root;
    }

    public static GameObject CreateLoopVerticalScroll(DefaultControls.Resources resources)
    {
        GameObject root = CreateUIElementRoot("UILoopVerticalScroll", s_ThinElementSize);
        GameObject Viewport = CreateUIObject("Viewport", root);
        GameObject Content = CreateUIObject("Content", Viewport);

        RectTransform rRectTransform = root.GetComponent<RectTransform>();
        rRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rRectTransform.sizeDelta = new Vector2(200, 200);

        RectTransform vRectTransform = Viewport.GetComponent<RectTransform>();
        vRectTransform.anchorMin = new Vector2(0, 0);
        vRectTransform.anchorMax = new Vector2(1, 1);
        vRectTransform.sizeDelta = new Vector2(0, 0);
        vRectTransform.pivot = new Vector2(0, 1);

        RectTransform cRectTransform = Content.GetComponent<RectTransform>();
        cRectTransform.anchorMin = new Vector2(0, 1);
        cRectTransform.anchorMax = new Vector2(1, 1);
        cRectTransform.sizeDelta = new Vector2(0, 300);
        cRectTransform.pivot = new Vector2(0, 1);

        GridLayoutGroup glayout = Content.AddComponent<GridLayoutGroup>();
        glayout.startAxis = GridLayoutGroup.Axis.Horizontal;
        glayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        glayout.constraintCount = 1;

        ContentSizeFitter sf = Content.AddComponent<ContentSizeFitter>();
        sf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        UIImage img = Viewport.AddComponent<UIImage>();
        img.sprite = resources.background;
        img.type = Image.Type.Sliced;
        img.color = s_PanelColor;

        /*Mask mask = */Viewport.AddComponent<Mask>();

        LoopVerticalScrollRect scroll = root.AddComponent<LoopVerticalScrollRect>();
        scroll.viewport = vRectTransform;
        scroll.content = cRectTransform;
        scroll.horizontal = false;

        return root;
    }

    public static GameObject CreateLoopHorizontalScroll(DefaultControls.Resources resources)
    {
        GameObject root = CreateUIElementRoot("UILoopHorizontalScroll", s_ThinElementSize);
        GameObject Viewport = CreateUIObject("Viewport", root);
        GameObject Content = CreateUIObject("Content", Viewport);

        RectTransform rRectTransform = root.GetComponent<RectTransform>();
        rRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rRectTransform.sizeDelta = new Vector2(200, 200);

        RectTransform vRectTransform = Viewport.GetComponent<RectTransform>();
        vRectTransform.anchorMin = new Vector2(0, 0);
        vRectTransform.anchorMax = new Vector2(1, 1);
        vRectTransform.sizeDelta = new Vector2(0, 0);
        vRectTransform.pivot = new Vector2(0, 1);

        RectTransform cRectTransform = Content.GetComponent<RectTransform>();
        cRectTransform.anchorMin = new Vector2(0, 0);
        cRectTransform.anchorMax = new Vector2(0, 1);
        cRectTransform.sizeDelta = new Vector2(300, 0);
        cRectTransform.pivot = new Vector2(0, 1);

        GridLayoutGroup glayout = Content.AddComponent<GridLayoutGroup>();
        glayout.startAxis = GridLayoutGroup.Axis.Vertical;
        glayout.constraint = GridLayoutGroup.Constraint.FixedRowCount;
        glayout.constraintCount = 1;

        ContentSizeFitter sf = Content.AddComponent<ContentSizeFitter>();
        sf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

        UIImage img = Viewport.AddComponent<UIImage>();
        img.sprite = resources.background;
        img.type = Image.Type.Sliced;
        img.color = s_PanelColor;

        /*Mask mask = */Viewport.AddComponent<Mask>();

        LoopHorizontalScrollRect scroll = root.AddComponent<LoopHorizontalScrollRect>();
        scroll.viewport = vRectTransform;
        scroll.content = cRectTransform;
        scroll.vertical = false;

        return root;
    }

    public static GameObject CreateDropdown(DefaultControls.Resources resources)
    {
        GameObject root = CreateUIElementRoot("UIDropdown", s_ThinElementSize);
        GameObject bg = CreateUIObject("Background", root);
        GameObject lable = CreateUIObject("Lable", root);
        GameObject arrow = CreateUIObject("Arrow", root);
        GameObject temp = CreateScroll(resources);
        temp.name = "Template";
        temp.transform.SetParent(root.transform);
        ScrollRect scroll = temp.GetComponent<ScrollRect>();

        GameObject item = CreateUIObject("Item", scroll.content.gameObject);
        GameObject itemBG = CreateUIObject("Item Background", item);
        GameObject itemMark = CreateUIObject("Item Checkmark", item);
        GameObject itemLable = CreateUIObject("Item Label", item);

        RectTransform rRectTransform = root.GetComponent<RectTransform>();
        rRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rRectTransform.sizeDelta = new Vector2(160, 30);
        rRectTransform.pivot = new Vector2(0.5f, 1);

        RectTransform bgRectTransform = bg.GetComponent<RectTransform>();
        bgRectTransform.anchorMin = new Vector2(0, 1);
        bgRectTransform.anchorMax = new Vector2(1, 1);
        bgRectTransform.sizeDelta = new Vector2(0, 30);
        bgRectTransform.pivot = new Vector2(0.5f,1);

        RectTransform lRectTransform = lable.GetComponent<RectTransform>();
        lRectTransform.anchorMin = new Vector2(0, 1);
        lRectTransform.anchorMax = new Vector2(1, 1);
        lRectTransform.sizeDelta = new Vector2(-35, 17);
        lRectTransform.anchoredPosition = new Vector2(-7.5f, -7f);
        lRectTransform.pivot = new Vector2(0.5f, 1);

        RectTransform aRectTransform = arrow.GetComponent<RectTransform>();
        aRectTransform.anchorMin = new Vector2(0, 1);
        aRectTransform.anchorMax = new Vector2(1, 1);
        aRectTransform.sizeDelta = new Vector2(-140, 20);
        aRectTransform.anchoredPosition = new Vector2(65, -5);
        aRectTransform.pivot = new Vector2(0.5f, 1);

        RectTransform tRectTransform = temp.GetComponent<RectTransform>();
        tRectTransform.anchorMin = new Vector2(0, 1);
        tRectTransform.anchorMax = new Vector2(1, 1);
        tRectTransform.sizeDelta = new Vector2(0, 150);
        tRectTransform.anchoredPosition = new Vector2(0, -30);
        tRectTransform.pivot = new Vector2(0.5f, 1);

        RectTransform cRectTransform = scroll.content.GetComponent<RectTransform>();
        cRectTransform.anchorMin = new Vector2(0f, 1);
        cRectTransform.anchorMax = new Vector2(1f, 1);
        cRectTransform.pivot = new Vector2(0.5f, 1);
        cRectTransform.anchoredPosition = new Vector2(0, 0);
        cRectTransform.sizeDelta = new Vector2(0, 28);

        RectTransform iRectTransform = item.GetComponent<RectTransform>();
        iRectTransform.anchorMin = new Vector2(0, 0.5f);
        iRectTransform.anchorMax = new Vector2(1, 0.5f);
        iRectTransform.sizeDelta = new Vector2(0, 20);
        iRectTransform.anchoredPosition = new Vector2(0, 0);
        iRectTransform.pivot = new Vector2(0.5f, 0.5f);

        RectTransform bRectTransform = itemBG.GetComponent<RectTransform>();
        bRectTransform.anchorMin = new Vector2(0, 0);
        bRectTransform.anchorMax = new Vector2(1, 1);
        bRectTransform.sizeDelta = new Vector2(0, 0);
        bRectTransform.pivot = new Vector2(0.5f, 0.5f);

        RectTransform mRectTransform = itemMark.GetComponent<RectTransform>();
        mRectTransform.anchorMin = new Vector2(0, 0.5f);
        mRectTransform.anchorMax = new Vector2(0, 0.5f);
        mRectTransform.sizeDelta = new Vector2(20, 20);
        mRectTransform.anchoredPosition = new Vector2(10, 0);
        mRectTransform.pivot = new Vector2(0.5f, 0.5f);

        RectTransform ilRectTransform = itemLable.GetComponent<RectTransform>();
        ilRectTransform.anchorMin = new Vector2(0, 0);
        ilRectTransform.anchorMax = new Vector2(1, 1);
        ilRectTransform.sizeDelta = new Vector2(-30, -3);
        ilRectTransform.anchoredPosition = new Vector2(5, -0.5f);
        ilRectTransform.pivot = new Vector2(0.5f, 0.5f);

        UIText ilText = itemLable.AddComponent<UIText>();
        ilText.text = "Option A";
        SetDefaultTextValues(ilText);

        UIImage mImg = itemMark.AddComponent<UIImage>();
        mImg.sprite = resources.standard;
        mImg.type = Image.Type.Sliced;
        mImg.color = s_DefaultSelectableColor;

        UIImage bImg = itemBG.AddComponent<UIImage>();
        mImg.sprite = resources.standard;
        mImg.type = Image.Type.Sliced;
        mImg.color = s_DefaultSelectableColor;

        UIToggle iTog = item.AddComponent<UIToggle>();
        iTog.targetGraphic = bImg;
        iTog.graphic = new Graphic[] { mImg };

        UIImage aImg = arrow.AddComponent<UIImage>();
        aImg.sprite = resources.standard;
        aImg.type = Image.Type.Sliced;
        aImg.color = s_DefaultSelectableColor;

        UIText lText = lable.AddComponent<UIText>();
        lText.text = "";
        SetDefaultTextValues(lText);

        UIImage tGraphic = bg.AddComponent<UIImage>();
        tGraphic.sprite = resources.standard;
        tGraphic.type = Image.Type.Sliced;
        tGraphic.color = s_DefaultSelectableColor;

        UIDropdown drop = root.AddComponent<UIDropdown>();
        drop.targetGraphic = tGraphic;
        drop.template = temp.GetComponent<RectTransform>();
        drop.captionText = lText;
        drop.itemText = ilText;
        drop.options.Add(new UIDropdown.OptionData { text = "Option A" });
        drop.options.Add(new UIDropdown.OptionData { text = "Option B" });
        drop.options.Add(new UIDropdown.OptionData { text = "Option C" });
        drop.RefreshShownValue();

        temp.SetActive(false);

        return root;
    }

    public static GameObject CreatElasticMenu(DefaultControls.Resources resources)
    {
        GameObject elasticRoot = CreateUIElementRoot("ElasticMenu", s_ThickElementSize);

        GameObject baseItem = CreateUIObject("BaseItem", elasticRoot);
        GameObject checkMark = CreateUIObject("CheckMark", baseItem);
        GameObject childText = CreateUIObject("Text", baseItem);
        GameObject content = CreateUIObject("Content", elasticRoot);

        UIImage bg = baseItem.AddComponent<UIImage>();
        bg.sprite = resources.standard;
        bg.type = UIImage.Type.Sliced;
        bg.color = s_DefaultSelectableColor;

        UIImage mark = checkMark.AddComponent<UIImage>();
        mark.sprite = resources.standard;
        mark.type = UIImage.Type.Sliced;
        mark.color = s_DefaultSelectableColor;

        UIText title = childText.AddComponent<UIText>();
        title.text = "ElasticMenu1";
        title.alignment = TextAnchor.MiddleCenter;
        SetDefaultTextValues(title);

        RectTransform rtRoot = elasticRoot.GetComponent<RectTransform>();
        rtRoot.anchorMin = new Vector2(0.5f, 1);
        rtRoot.anchorMax = new Vector2(0.5f, 1);
        rtRoot.pivot = new Vector2(0.5f, 1);
        rtRoot.sizeDelta = new Vector2(230, 50);

        RectTransform rtBaseItem = baseItem.GetComponent<RectTransform>();
        rtBaseItem.pivot = new Vector2(0.5f, 1);
        rtBaseItem.sizeDelta = new Vector2(230, 50);
        rtBaseItem.anchoredPosition = new Vector2(0, 0);

        RectTransform rtText = childText.GetComponent<RectTransform>();
        rtText.anchorMin = new Vector2(0.5f, 0.5f);
        rtText.anchorMax = new Vector2(0.5f, 0.5f);
        rtText.pivot = new Vector2(0.5f, 0.5f);
        rtText.sizeDelta = new Vector2(180, 50);

        RectTransform rtBG = bg.GetComponent<RectTransform>();
        rtBG.anchorMin = new Vector2(0.5f, 0.5f);
        rtBG.anchorMax = new Vector2(0.5f, 0.5f);
        rtBG.pivot = new Vector2(0.5f, 0.5f);
        rtBG.sizeDelta = new Vector2(230, 50);

        RectTransform rtMark = checkMark.GetComponent<RectTransform>();
        rtMark.anchorMin = new Vector2(0.5f, 0.5f);
        rtMark.anchorMax = new Vector2(0.5f, 0.5f);
        rtMark.pivot = new Vector2(0.5f, 0.5f);
        rtMark.sizeDelta = new Vector2(230, 50);

        RectTransform rtContent = content.GetComponent<RectTransform>();
        rtContent.pivot = new Vector2(0.5f, 1);
        rtContent.sizeDelta = new Vector2(230, 50);
        rtContent.anchoredPosition = new Vector2(0, 0);

        VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.UpperCenter;

        ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        VerticalLayoutGroup layout1 = elasticRoot.AddComponent<VerticalLayoutGroup>();
        layout1.childAlignment = TextAnchor.UpperCenter;

        ContentSizeFitter fitter1 = elasticRoot.AddComponent<ContentSizeFitter>();
        fitter1.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ElasticMenu menu = elasticRoot.AddComponent<ElasticMenu>();
        menu.targetGraphic = bg;
        menu.checkMarks = new Graphic[] { mark };
        menu.baseMarks = new Graphic[] { bg };
        menu.content = rtContent;
        menu.txt_Title = title;

        return elasticRoot;
    }

    public static GameObject CreatUIBloodSlider(DefaultControls.Resources resources)
    {
        GameObject sliderRoot = CreateUIElementRoot("UIBloodSlider", new Vector2(410,20));

        GameObject backGround = CreateUIObject("BackGround", sliderRoot);
        GameObject top = CreateUIObject("HPImage_Top", backGround);
        GameObject top_shadow = CreateUIObject("HPImage_Top_Shadow", backGround);
        GameObject mid = CreateUIObject("HPImage_Mid", backGround);
        GameObject mid_shadow = CreateUIObject("HPImage_Mid_Shadow", backGround);
        GameObject bot = CreateUIObject("HPImage_Bot", backGround);
        GameObject bot_shadow = CreateUIObject("HPImage_Bot_Shadow", backGround);

        GameObject hpCount = CreateUIObject("TextHPCount", sliderRoot);
        GameObject hpValue = CreateUIObject("TextHPValue", sliderRoot);

        UIImage bg = backGround.AddComponent<UIImage>();
        bg.sprite = resources.standard;
        bg.type = UIImage.Type.Sliced;
        bg.color = s_DefaultSelectableColor;
        RectTransform bgTs = backGround.GetComponent<RectTransform>();
        bgTs.sizeDelta = new Vector2(410, 20);

        UIImage topImg = top.AddComponent<UIImage>();
        topImg.sprite = resources.standard;
        topImg.type = UIImage.Type.Filled;
        topImg.fillMethod = Image.FillMethod.Horizontal;
        topImg.fillOrigin = 0;
        topImg.color = s_DefaultSelectableColor;
        RectTransform topTs = top.GetComponent<RectTransform>();
        topTs.sizeDelta = new Vector2(410, 20);

        UIImage topShadowImg = top_shadow.AddComponent<UIImage>();
        topShadowImg.sprite = resources.standard;
        topShadowImg.type = UIImage.Type.Filled;
        topShadowImg.fillMethod = Image.FillMethod.Horizontal;
        topShadowImg.fillOrigin = 0;
        topShadowImg.color = s_DefaultSelectableColor;
        RectTransform topSTs = top_shadow.GetComponent<RectTransform>();
        topSTs.sizeDelta = new Vector2(410, 20);

        UIImage midImg = mid.AddComponent<UIImage>();
        midImg.sprite = resources.standard;
        midImg.type = UIImage.Type.Filled;
        midImg.fillMethod = Image.FillMethod.Horizontal;
        midImg.fillOrigin = 0;
        midImg.color = s_DefaultSelectableColor;
        RectTransform midTs = mid.GetComponent<RectTransform>();
        midTs.sizeDelta = new Vector2(410, 20);

        UIImage midShadowImg = mid_shadow.AddComponent<UIImage>();
        midShadowImg.sprite = resources.standard;
        midShadowImg.type = UIImage.Type.Filled;
        midShadowImg.fillMethod = Image.FillMethod.Horizontal;
        midShadowImg.fillOrigin = 0;
        midShadowImg.color = s_DefaultSelectableColor;
        RectTransform midSTs = mid_shadow.GetComponent<RectTransform>();
        midSTs.sizeDelta = new Vector2(410, 20);

        UIImage botImg = bot.AddComponent<UIImage>();
        botImg.sprite = resources.standard;
        botImg.type = UIImage.Type.Filled;
        botImg.fillMethod = Image.FillMethod.Horizontal;
        botImg.fillOrigin = 0;
        botImg.color = s_DefaultSelectableColor;
        RectTransform botTs = bot.GetComponent<RectTransform>();
        botTs.sizeDelta = new Vector2(410, 20);

        UIImage botShadowImg = bot_shadow.AddComponent<UIImage>();
        botShadowImg.sprite = resources.standard;
        botShadowImg.type = UIImage.Type.Filled;
        botShadowImg.fillMethod = Image.FillMethod.Horizontal;
        botShadowImg.fillOrigin = 0;
        botShadowImg.color = s_DefaultSelectableColor;
        RectTransform botSTs = bot_shadow.GetComponent<RectTransform>();
        botSTs.sizeDelta = new Vector2(410, 20);

        UIText txtHPCount = hpCount.AddComponent<UIText>();
        txtHPCount.text = "x 0";
        SetDefaultTextValues(txtHPCount);

        UIText txtHPValue = hpValue.AddComponent<UIText>();
        txtHPValue.text = "0/0";
        SetDefaultTextValues(txtHPValue);

        UIBloodSlider hpSlider = sliderRoot.AddComponent<UIBloodSlider>();
        hpSlider.HPImageTop = topImg;
        hpSlider.HPImageTop_Shadow = topShadowImg;
        hpSlider.HPImageMid = midImg;
        hpSlider.HPImageMid_Shadow = midShadowImg;
        hpSlider.HPImageBot = botImg;
        hpSlider.HPImageBot_Shadow = botShadowImg;

        hpSlider.TextHp = txtHPValue;
        hpSlider.TextValue = txtHPCount;

        return sliderRoot;
    }

}