using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class Chapter01TunnelEntranceBuilder
{
    private const string MenuPath = "Tools/Cu Chi/Build Chapter 01 Tunnel Entrance";

    [MenuItem(MenuPath)]
    public static void Build()
    {
        Material earth = EnsureMaterial("Assets/_Project/Art/Materials/MAT_Tunnel_Earth.mat", new Color(0.29f, 0.20f, 0.12f, 1f));
        Material dark = EnsureMaterial("Assets/_Project/Art/Materials/MAT_Tunnel_DarkOpening.mat", new Color(0.015f, 0.013f, 0.011f, 1f));
        Material wood = EnsureMaterial("Assets/_Project/Art/Materials/MAT_Tunnel_Wood.mat", new Color(0.34f, 0.22f, 0.12f, 1f));
        Material floor = EnsureMaterial("Assets/_Project/Art/Materials/MAT_Tunnel_Floor.mat", new Color(0.20f, 0.16f, 0.10f, 1f));

        Transform entranceRoot = EnsureRoot("Chapter01_TunnelEntrance_Setpiece");
        entranceRoot.position = Vector3.zero;

        CreateCube("Entrance_EarthMound", entranceRoot, new Vector3(-28.15f, 0.18f, -3.55f), new Vector3(4.0f, 0.34f, 3.2f), earth);
        CreateCube("Entrance_LeftShoulder", entranceRoot, new Vector3(-29.45f, 0.42f, -4.15f), new Vector3(0.8f, 0.7f, 1.55f), earth, new Vector3(0f, 0f, -8f));
        CreateCube("Entrance_RightShoulder", entranceRoot, new Vector3(-26.85f, 0.42f, -4.15f), new Vector3(0.8f, 0.7f, 1.55f), earth, new Vector3(0f, 0f, 8f));
        CreateCube("Entrance_Ramp", entranceRoot, new Vector3(-28.15f, 0.08f, -4.05f), new Vector3(1.75f, 0.12f, 1.85f), earth, new Vector3(-10f, 0f, 0f));
        CreateCube("Entrance_DarkOpening", entranceRoot, new Vector3(-28.15f, 0.52f, -4.72f), new Vector3(1.45f, 0.95f, 0.12f), dark);
        CreateCube("Entrance_BackShadow", entranceRoot, new Vector3(-28.15f, 0.12f, -3.55f), new Vector3(1.35f, 0.42f, 1.55f), dark, new Vector3(-12f, 0f, 0f));
        CreateCube("Entrance_WoodBeam_Left", entranceRoot, new Vector3(-29.0f, 0.55f, -4.66f), new Vector3(0.18f, 1.06f, 0.2f), wood);
        CreateCube("Entrance_WoodBeam_Right", entranceRoot, new Vector3(-27.3f, 0.55f, -4.66f), new Vector3(0.18f, 1.06f, 0.2f), wood);
        CreateCube("Entrance_WoodBeam_Top", entranceRoot, new Vector3(-28.15f, 1.12f, -4.66f), new Vector3(1.95f, 0.18f, 0.22f), wood);
        EnsurePointLight("Entrance_GuidingLight", entranceRoot, new Vector3(-28.15f, 1.55f, -4.4f), new Color(1f, 0.73f, 0.42f, 1f), 1.4f, 5.2f);

        GameObject zone = EnsureGameObject("TunnelEntrance_InteractZone", entranceRoot);
        zone.transform.SetPositionAndRotation(new Vector3(-28.15f, 0.65f, -4.1f), Quaternion.identity);
        BoxCollider trigger = zone.GetComponent<BoxCollider>();
        if (trigger == null)
        {
            trigger = zone.AddComponent<BoxCollider>();
        }
        trigger.isTrigger = true;
        trigger.size = new Vector3(2.8f, 1.6f, 2.1f);
        trigger.center = Vector3.zero;

        Transform undergroundRoot = EnsureRoot("Chapter01_Underground_Blockout");
        undergroundRoot.position = Vector3.zero;
        BuildUndergroundBlockout(undergroundRoot, floor, earth, wood);

        Transform destination = EnsureChild("UndergroundSpawnPoint", undergroundRoot);
        destination.SetPositionAndRotation(new Vector3(-28.15f, -6.55f, -13.8f), Quaternion.Euler(0f, 180f, 0f));

        SceneTransitionController transition = EnsureTransitionUI();
        TunnelEntranceInteractable interactable = zone.GetComponent<TunnelEntranceInteractable>();
        if (interactable == null)
        {
            interactable = zone.AddComponent<TunnelEntranceInteractable>();
        }

        SerializedObject serializedEntrance = new SerializedObject(interactable);
        serializedEntrance.FindProperty("prompt").stringValue = "Nhan E de vao dia dao";
        serializedEntrance.FindProperty("transitionController").objectReferenceValue = transition;
        serializedEntrance.FindProperty("teleportDestination").objectReferenceValue = destination;
        serializedEntrance.FindProperty("transitionMessage").stringValue = "Dang vao dia dao...";
        serializedEntrance.FindProperty("sceneName").stringValue = string.Empty;
        serializedEntrance.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(zone);
        EditorUtility.SetDirty(transition);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        Debug.Log("Chapter 01 tunnel entrance, fade transition, and underground blockout built.");
    }

    private static void BuildUndergroundBlockout(Transform root, Material floor, Material earth, Material wood)
    {
        CreateCube("Tunnel_Floor", root, new Vector3(-28.15f, -7.05f, -16.2f), new Vector3(3.0f, 0.18f, 7.2f), floor);
        CreateCube("Tunnel_Ceiling", root, new Vector3(-28.15f, -5.48f, -16.2f), new Vector3(3.0f, 0.18f, 7.2f), earth);
        CreateCube("Tunnel_LeftWall", root, new Vector3(-29.7f, -6.25f, -16.2f), new Vector3(0.22f, 1.65f, 7.2f), earth);
        CreateCube("Tunnel_RightWall", root, new Vector3(-26.6f, -6.25f, -16.2f), new Vector3(0.22f, 1.65f, 7.2f), earth);
        CreateCube("Tunnel_EndShadow", root, new Vector3(-28.15f, -6.25f, -19.85f), new Vector3(3.0f, 1.65f, 0.2f), earth);
        CreateCube("Tunnel_WoodSupport_A", root, new Vector3(-28.15f, -6.23f, -14.45f), new Vector3(3.25f, 1.75f, 0.16f), wood);
        CreateCube("Tunnel_WoodSupport_B", root, new Vector3(-28.15f, -6.23f, -17.25f), new Vector3(3.25f, 1.75f, 0.16f), wood);
        EnsurePointLight("Tunnel_WarmGlow", root, new Vector3(-28.15f, -5.95f, -15.1f), new Color(1f, 0.62f, 0.34f, 1f), 0.9f, 4f);
    }

    private static SceneTransitionController EnsureTransitionUI()
    {
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
        }

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        RectTransform root = EnsureRectTransform("Transition_Root", canvasRect);
        StretchToParent(root);
        root.SetAsLastSibling();

        Image fadeImage = root.GetComponent<Image>();
        if (fadeImage == null)
        {
            fadeImage = root.gameObject.AddComponent<Image>();
        }
        fadeImage.color = Color.black;
        fadeImage.raycastTarget = true;

        CanvasGroup fadeGroup = root.GetComponent<CanvasGroup>();
        if (fadeGroup == null)
        {
            fadeGroup = root.gameObject.AddComponent<CanvasGroup>();
        }
        fadeGroup.alpha = 0f;
        fadeGroup.blocksRaycasts = false;
        fadeGroup.interactable = false;

        TMP_Text statusText = EnsureText("TransitionStatusText", root, string.Empty, 30f, FontStyles.Normal, TextAlignmentOptions.Center);
        RectTransform statusRect = statusText.rectTransform;
        statusRect.anchorMin = new Vector2(0.5f, 0.5f);
        statusRect.anchorMax = new Vector2(0.5f, 0.5f);
        statusRect.pivot = new Vector2(0.5f, 0.5f);
        statusRect.anchoredPosition = new Vector2(0f, -96f);
        statusRect.sizeDelta = new Vector2(720f, 80f);
        statusText.color = new Color(0.88f, 0.78f, 0.55f, 1f);

        SceneTransitionController transition = root.GetComponent<SceneTransitionController>();
        if (transition == null)
        {
            transition = root.gameObject.AddComponent<SceneTransitionController>();
        }

        SerializedObject serializedTransition = new SerializedObject(transition);
        serializedTransition.FindProperty("fadeGroup").objectReferenceValue = fadeGroup;
        serializedTransition.FindProperty("statusText").objectReferenceValue = statusText;
        serializedTransition.FindProperty("fadeDuration").floatValue = 0.65f;
        serializedTransition.FindProperty("holdDuration").floatValue = 0.35f;
        serializedTransition.ApplyModifiedPropertiesWithoutUndo();

        return transition;
    }

    private static Material EnsureMaterial(string path, Color color)
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            EnsureFolder("Assets/_Project/Art");
            EnsureFolder("Assets/_Project/Art/Materials");
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
        }

        material.color = color;
        EditorUtility.SetDirty(material);
        return material;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        string parent = System.IO.Path.GetDirectoryName(path)?.Replace("\\", "/");
        string name = System.IO.Path.GetFileName(path);
        if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
        {
            EnsureFolder(parent);
        }

        AssetDatabase.CreateFolder(parent, name);
    }

    private static Transform EnsureRoot(string name)
    {
        GameObject existing = GameObject.Find(name);
        if (existing != null)
        {
            return existing.transform;
        }

        return new GameObject(name).transform;
    }

    private static GameObject EnsureGameObject(string name, Transform parent)
    {
        Transform existing = parent.Find(name);
        if (existing != null)
        {
            return existing.gameObject;
        }

        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go;
    }

    private static Transform EnsureChild(string name, Transform parent)
    {
        return EnsureGameObject(name, parent).transform;
    }

    private static GameObject CreateCube(string name, Transform parent, Vector3 position, Vector3 scale, Material material, Vector3? rotation = null)
    {
        GameObject go = EnsureGameObject(name, parent);
        if (go.GetComponent<MeshFilter>() == null)
        {
            GameObject primitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
            MeshFilter mesh = primitive.GetComponent<MeshFilter>();
            MeshRenderer renderer = primitive.GetComponent<MeshRenderer>();
            BoxCollider collider = primitive.GetComponent<BoxCollider>();

            go.AddComponent<MeshFilter>().sharedMesh = mesh.sharedMesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = renderer.sharedMaterial;
            if (go.GetComponent<BoxCollider>() == null)
            {
                go.AddComponent<BoxCollider>();
            }

            Object.DestroyImmediate(primitive);
            if (collider == null)
            {
                go.AddComponent<BoxCollider>();
            }
        }

        go.transform.SetPositionAndRotation(position, Quaternion.Euler(rotation ?? Vector3.zero));
        go.transform.localScale = scale;

        MeshRenderer meshRenderer = go.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.sharedMaterial = material;
        }

        return go;
    }

    private static Light EnsurePointLight(string name, Transform parent, Vector3 position, Color color, float intensity, float range)
    {
        GameObject go = EnsureGameObject(name, parent);
        go.transform.position = position;

        Light light = go.GetComponent<Light>();
        if (light == null)
        {
            light = go.AddComponent<Light>();
        }

        light.type = LightType.Point;
        light.color = color;
        light.intensity = intensity;
        light.range = range;
        return light;
    }

    private static RectTransform EnsureRectTransform(string name, RectTransform parent)
    {
        Transform existing = parent.Find(name);
        if (existing != null)
        {
            return existing as RectTransform;
        }

        GameObject go = new GameObject(name, typeof(RectTransform));
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        return rect;
    }

    private static TMP_Text EnsureText(string name, RectTransform parent, string text, float fontSize, FontStyles style, TextAlignmentOptions alignment)
    {
        Transform existing = parent.Find(name);
        TextMeshProUGUI tmp = existing != null ? existing.GetComponent<TextMeshProUGUI>() : null;
        if (tmp == null)
        {
            GameObject go = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform));
            if (existing == null)
            {
                go.transform.SetParent(parent, false);
            }
            tmp = go.AddComponent<TextMeshProUGUI>();
        }

        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.alignment = alignment;
        tmp.raycastTarget = false;
        return tmp;
    }

    private static void StretchToParent(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
