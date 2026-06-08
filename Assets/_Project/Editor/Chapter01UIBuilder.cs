using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class Chapter01UIBuilder
{
    private const string MenuPath = "Tools/Cu Chi/Build Chapter 01 Interaction UI";

    [MenuItem(MenuPath)]
    public static void Build()
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        ConfigureCanvas(canvas);

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        RectTransform hudRoot = EnsureRectTransform("HUD_Root", canvasRect);
        StretchToParent(hudRoot);

        RectTransform dialogueRoot = EnsureRectTransform("Dialogue_Root", canvasRect);
        StretchToParent(dialogueRoot);

        MoveExistingOxygenBar(hudRoot);
        BuildInteractionPrompt(hudRoot);
        DialogueUI dialogueUI = BuildDialogueUI(dialogueRoot);
        ConnectDialogueManager(dialogueUI);
        EnsureNpcInteractionCollider();

        EditorUtility.SetDirty(canvas.gameObject);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
        Debug.Log("Chapter 01 interaction UI built and wired.");
    }

    private static void ConfigureCanvas(Canvas canvas)
    {
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = canvas.gameObject.AddComponent<CanvasScaler>();
        }

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
    }

    private static void MoveExistingOxygenBar(RectTransform hudRoot)
    {
        Transform oxygenBar = hudRoot.Find("OxygenBar");
        if (oxygenBar == null)
        {
            GameObject existing = GameObject.Find("OxygenBar");
            if (existing != null)
            {
                oxygenBar = existing.transform;
                oxygenBar.SetParent(hudRoot, false);
            }
        }

        if (oxygenBar == null)
        {
            return;
        }

        RectTransform rect = oxygenBar as RectTransform;
        if (rect == null)
        {
            return;
        }

        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(36f, -36f);
        rect.sizeDelta = new Vector2(320f, 42f);
    }

    private static void BuildInteractionPrompt(RectTransform parent)
    {
        RectTransform root = EnsureRectTransform("InteractionPrompt", parent);
        root.anchorMin = new Vector2(0.5f, 0f);
        root.anchorMax = new Vector2(0.5f, 0f);
        root.pivot = new Vector2(0.5f, 0f);
        root.anchoredPosition = new Vector2(0f, 96f);
        root.sizeDelta = new Vector2(620f, 64f);

        CanvasGroup group = root.GetComponent<CanvasGroup>();
        if (group == null)
        {
            group = root.gameObject.AddComponent<CanvasGroup>();
        }

        Image background = root.GetComponent<Image>();
        if (background == null)
        {
            background = root.gameObject.AddComponent<Image>();
        }

        background.color = new Color(0.05f, 0.055f, 0.045f, 0.86f);
        background.raycastTarget = false;

        RectTransform keyBadge = EnsureRectTransform("KeyBadge", root);
        keyBadge.anchorMin = new Vector2(0f, 0.5f);
        keyBadge.anchorMax = new Vector2(0f, 0.5f);
        keyBadge.pivot = new Vector2(0f, 0.5f);
        keyBadge.anchoredPosition = new Vector2(20f, 0f);
        keyBadge.sizeDelta = new Vector2(52f, 42f);

        Image keyImage = keyBadge.GetComponent<Image>();
        if (keyImage == null)
        {
            keyImage = keyBadge.gameObject.AddComponent<Image>();
        }
        keyImage.color = new Color(0.82f, 0.72f, 0.45f, 1f);
        keyImage.raycastTarget = false;

        TMP_Text keyText = EnsureText("KeyText", keyBadge, "E", 28f, FontStyles.Bold, TextAlignmentOptions.Center);
        StretchToParent(keyText.rectTransform);
        keyText.color = new Color(0.08f, 0.07f, 0.045f, 1f);

        TMP_Text promptText = EnsureText("PromptText", root, "Nhan E de tuong tac", 25f, FontStyles.Normal, TextAlignmentOptions.MidlineLeft);
        RectTransform promptRect = promptText.rectTransform;
        promptRect.anchorMin = new Vector2(0f, 0f);
        promptRect.anchorMax = new Vector2(1f, 1f);
        promptRect.pivot = new Vector2(0.5f, 0.5f);
        promptRect.offsetMin = new Vector2(92f, 8f);
        promptRect.offsetMax = new Vector2(-24f, -8f);
        promptText.color = new Color(0.96f, 0.93f, 0.82f, 1f);

        InteractionPromptUI promptUI = root.GetComponent<InteractionPromptUI>();
        if (promptUI == null)
        {
            promptUI = root.gameObject.AddComponent<InteractionPromptUI>();
        }

        SerializedObject serializedPrompt = new SerializedObject(promptUI);
        serializedPrompt.FindProperty("promptText").objectReferenceValue = promptText;
        serializedPrompt.FindProperty("canvasGroup").objectReferenceValue = group;
        serializedPrompt.ApplyModifiedPropertiesWithoutUndo();
        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;
    }

    private static DialogueUI BuildDialogueUI(RectTransform parent)
    {
        RectTransform root = EnsureRectTransform("DialogueUI", parent);
        StretchToParent(root);

        DialogueUI dialogueUI = root.GetComponent<DialogueUI>();
        if (dialogueUI == null)
        {
            dialogueUI = root.gameObject.AddComponent<DialogueUI>();
        }

        RectTransform panel = EnsureRectTransform("DialoguePanel", root);
        panel.anchorMin = new Vector2(0.5f, 0f);
        panel.anchorMax = new Vector2(0.5f, 0f);
        panel.pivot = new Vector2(0.5f, 0f);
        panel.anchoredPosition = new Vector2(0f, 28f);
        panel.sizeDelta = new Vector2(1280f, 210f);

        Image panelImage = panel.GetComponent<Image>();
        if (panelImage == null)
        {
            panelImage = panel.gameObject.AddComponent<Image>();
        }
        panelImage.color = new Color(0.035f, 0.04f, 0.035f, 0.92f);
        panelImage.raycastTarget = true;

        TMP_Text speakerText = EnsureText("SpeakerNameText", panel, "Giao lien ky cuu", 26f, FontStyles.Bold, TextAlignmentOptions.MidlineLeft);
        RectTransform speakerRect = speakerText.rectTransform;
        speakerRect.anchorMin = new Vector2(0f, 1f);
        speakerRect.anchorMax = new Vector2(1f, 1f);
        speakerRect.pivot = new Vector2(0.5f, 1f);
        speakerRect.offsetMin = new Vector2(36f, -62f);
        speakerRect.offsetMax = new Vector2(-36f, -24f);
        speakerText.color = new Color(0.88f, 0.74f, 0.42f, 1f);

        TMP_Text contentText = EnsureText("ContentText", panel, "Noi dung hoi thoai se hien o day.", 30f, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        RectTransform contentRect = contentText.rectTransform;
        contentRect.anchorMin = new Vector2(0f, 0f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 0.5f);
        contentRect.offsetMin = new Vector2(36f, 58f);
        contentRect.offsetMax = new Vector2(-36f, -72f);
        contentText.color = new Color(0.96f, 0.94f, 0.86f, 1f);
        contentText.enableWordWrapping = true;

        TMP_Text hintText = EnsureText("ContinueHintText", panel, "Space / E", 20f, FontStyles.Normal, TextAlignmentOptions.MidlineRight);
        RectTransform hintRect = hintText.rectTransform;
        hintRect.anchorMin = new Vector2(0f, 0f);
        hintRect.anchorMax = new Vector2(1f, 0f);
        hintRect.pivot = new Vector2(0.5f, 0f);
        hintRect.offsetMin = new Vector2(36f, 20f);
        hintRect.offsetMax = new Vector2(-36f, 48f);
        hintText.color = new Color(0.73f, 0.69f, 0.58f, 1f);

        SerializedObject serializedDialogue = new SerializedObject(dialogueUI);
        serializedDialogue.FindProperty("dialoguePanel").objectReferenceValue = panel.gameObject;
        serializedDialogue.FindProperty("speakerNameText").objectReferenceValue = speakerText;
        serializedDialogue.FindProperty("contentText").objectReferenceValue = contentText;
        serializedDialogue.FindProperty("continueHintText").objectReferenceValue = hintText;
        serializedDialogue.ApplyModifiedPropertiesWithoutUndo();

        panel.gameObject.SetActive(false);
        return dialogueUI;
    }

    private static void ConnectDialogueManager(DialogueUI dialogueUI)
    {
        DialogueManager manager = Object.FindFirstObjectByType<DialogueManager>();
        if (manager == null || dialogueUI == null)
        {
            return;
        }

        SerializedObject serializedManager = new SerializedObject(manager);
        serializedManager.FindProperty("dialogueUI").objectReferenceValue = dialogueUI;
        serializedManager.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(manager);
    }

    private static void EnsureNpcInteractionCollider()
    {
        NPCDialogueTrigger npc = Object.FindFirstObjectByType<NPCDialogueTrigger>();
        if (npc == null)
        {
            return;
        }

        Collider collider = npc.GetComponent<Collider>();
        if (collider == null)
        {
            CapsuleCollider capsule = npc.gameObject.AddComponent<CapsuleCollider>();
            capsule.isTrigger = true;
            capsule.radius = 0.75f;
            capsule.height = 1.8f;
            capsule.center = new Vector3(0f, 0.9f, 0f);
        }
        else
        {
            collider.isTrigger = true;
        }

        EditorUtility.SetDirty(npc.gameObject);
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
