#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class LoginUISetupBuilder : EditorWindow
{
    [MenuItem("Tools/Build Login Panel UI")]
    public static void BuildUI()
    {
        // 1. Ensure AuthManager exists in the scene
        AuthManager authManager = FindObjectOfType<AuthManager>();
        if (authManager == null)
        {
            GameObject authObj = new GameObject("AuthManager");
            authManager = authObj.AddComponent<AuthManager>();
            Debug.Log("[Login Setup] Created AuthManager in scene.");
        }

        // 2. Find Canvas
        GameObject canvasObj = GameObject.Find("MainMenuCanvas") ?? GameObject.Find("Canvas");
        if (canvasObj == null)
        {
            Debug.LogError("Could not find MainMenuCanvas or Canvas.");
            return;
        }

        // 3. Create LoginContainer
        GameObject loginContainer = CreateUIObject("LoginContainer", canvasObj.transform);
        StretchToFill(loginContainer.GetComponent<RectTransform>());

        // Add Dark Background
        Image bgImg = loginContainer.AddComponent<Image>();
        bgImg.color = new Color(0.05f, 0.05f, 0.1f, 1f); // Dark blue/grey

        // Title
        GameObject titleObj = CreateUIObject("Title", loginContainer.transform);
        TextMeshProUGUI titleTmp = titleObj.AddComponent<TextMeshProUGUI>();
        titleTmp.text = "TERMINAL LOGIN\nREQUESTED";
        titleTmp.fontSize = 72;
        titleTmp.color = Color.green;
        titleTmp.alignment = TextAlignmentOptions.Center;
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.anchoredPosition = new Vector2(0, -150);
        titleRect.sizeDelta = new Vector2(0, 200);

        // Input Field Background logic
        GameObject inputObj = CreateUIObject("UsernameInput", loginContainer.transform);
        Image inputBg = inputObj.AddComponent<Image>();
        inputBg.color = Color.black;
        RectTransform inputRect = inputObj.GetComponent<RectTransform>();
        inputRect.anchorMin = new Vector2(0.5f, 0.5f);
        inputRect.anchorMax = new Vector2(0.5f, 0.5f);
        inputRect.sizeDelta = new Vector2(600, 100);
        inputRect.anchoredPosition = new Vector2(0, 50);

        // Input Field Text Area
        GameObject textArea = CreateUIObject("Text Area", inputObj.transform);
        RectTransform textRect = textArea.GetComponent<RectTransform>();
        StretchToFill(textRect);
        textRect.offsetMin = new Vector2(20, 10);
        textRect.offsetMax = new Vector2(-20, -10);
        textArea.AddComponent<RectMask2D>();

        // Text Content
        GameObject textContent = CreateUIObject("Text", textArea.transform);
        TextMeshProUGUI textTmp = textContent.AddComponent<TextMeshProUGUI>();
        textTmp.text = "";
        textTmp.fontSize = 48;
        textTmp.color = Color.green;
        textTmp.alignment = TextAlignmentOptions.Center;
        StretchToFill(textContent.GetComponent<RectTransform>());

        // Placeholder
        GameObject placeholder = CreateUIObject("Placeholder", textArea.transform);
        TextMeshProUGUI pTmp = placeholder.AddComponent<TextMeshProUGUI>();
        pTmp.text = "Enter Username...";
        pTmp.fontSize = 48;
        pTmp.color = Color.gray;
        pTmp.alignment = TextAlignmentOptions.Center;
        StretchToFill(placeholder.GetComponent<RectTransform>());

        // TMP_InputField Component
        TMP_InputField inputComp = inputObj.AddComponent<TMP_InputField>();
        inputComp.textComponent = textTmp;
        inputComp.placeholder = pTmp;
        inputComp.textViewport = textRect;

        // Login Button
        GameObject loginBtnObj = CreateUIObject("LoginButton", loginContainer.transform);
        Image btnImg = loginBtnObj.AddComponent<Image>();
        btnImg.color = new Color(0.1f, 0.6f, 0.1f);
        Button btnComp = loginBtnObj.AddComponent<Button>();
        RectTransform btnRect = loginBtnObj.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0.5f);
        btnRect.anchorMax = new Vector2(0.5f, 0.5f);
        btnRect.sizeDelta = new Vector2(300, 80);
        btnRect.anchoredPosition = new Vector2(0, -100);

        GameObject btnTxtObj = CreateUIObject("Text", loginBtnObj.transform);
        TextMeshProUGUI btnTmp = btnTxtObj.AddComponent<TextMeshProUGUI>();
        btnTmp.text = "ACCESS";
        btnTmp.fontSize = 36;
        btnTmp.color = Color.white;
        btnTmp.alignment = TextAlignmentOptions.Center;
        StretchToFill(btnTxtObj.GetComponent<RectTransform>());

        // Error Text
        GameObject errObj = CreateUIObject("ErrorText", loginContainer.transform);
        TextMeshProUGUI errTmp = errObj.AddComponent<TextMeshProUGUI>();
        errTmp.text = "";
        errTmp.fontSize = 32;
        errTmp.color = Color.red;
        errTmp.alignment = TextAlignmentOptions.Center;
        RectTransform errRect = errObj.GetComponent<RectTransform>();
        errRect.anchorMin = new Vector2(0, 0.5f);
        errRect.anchorMax = new Vector2(1, 0.5f);
        errRect.anchoredPosition = new Vector2(0, -200);

        // 4. Attach and wire LoginUI
        LoginUI loginUI = loginContainer.AddComponent<LoginUI>();
        SerializedObject soLogin = new SerializedObject(loginUI);
        soLogin.FindProperty("usernameInput").objectReferenceValue = inputComp;
        soLogin.FindProperty("loginButton").objectReferenceValue = btnComp;
        soLogin.FindProperty("errorText").objectReferenceValue = errTmp;
        soLogin.ApplyModifiedProperties();

        // 5. Wire into MainMenuController
        MainMenuController menuController = FindObjectOfType<MainMenuController>();
        if (menuController != null)
        {
            SerializedObject soMenu = new SerializedObject(menuController);
            soMenu.FindProperty("loginPanel").objectReferenceValue = loginContainer;
            soMenu.ApplyModifiedProperties();
        }

        // 6. Push LoginPanel to be the last sibling (render on top of everything)
        loginContainer.transform.SetAsLastSibling();

        Selection.activeGameObject = loginContainer;
        Debug.Log("[Login Setup] Generated Login Screen and injected into MainMenuController!");
    }

    private static GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.AddComponent<RectTransform>();
        return obj;
    }

    private static void StretchToFill(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
#endif
