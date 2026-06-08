using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

[System.Serializable]
public class LocalizedTextData
{
    public string key;
    public string context;
    public string tw;
    public string en;
    public string ja;
}

public class LocalizationManager : MonoBehaviour
{
    private const string LOCALIZATION_RESOURCE = "Localization";

    private static Dictionary<string, LocalizedTextData> textTable;
    private static Language currentLanguage = Language.TRADITIONAL_CHINESE;
    private static bool languageLoaded = false;

    // 語言切換時觸發，UI（例如 LocalizedText）可訂閱此事件以即時重新套用翻譯
    public static event Action OnLanguageChanged;

    private void Awake()
    {
        ensureLoaded();
    }

    // 對照表採延遲載入，static 保存，因此在任何場景、任何執行順序下 get() 都可用
    private static void ensureLoaded()
    {
        if (textTable != null)
            return;

        textTable = new Dictionary<string, LocalizedTextData>();

        TextAsset jsonFile = Resources.Load<TextAsset>(LOCALIZATION_RESOURCE);

        if (jsonFile == null)
        {
            Debug.LogError($"Localization resource '{LOCALIZATION_RESOURCE}' not found in Resources.");
            return;
        }

        try
        {
            List<LocalizedTextData> list = JsonConvert.DeserializeObject<List<LocalizedTextData>>(jsonFile.text);

            foreach (LocalizedTextData data in list)
            {
                if (string.IsNullOrEmpty(data.key))
                    continue;

                if (textTable.ContainsKey(data.key))
                {
                    Debug.LogWarning($"Duplicate localization key: {data.key}.");
                    continue;
                }

                textTable.Add(data.key, data);
            }
        }
        catch (JsonException e)
        {
            Debug.LogError($"Failed to parse localization data: {e.Message}");
        }
    }

    public static Language getCurrentLanguage()
    {
        if (!languageLoaded)
        {
            currentLanguage = JsonReader.loadLanguage();
            languageLoaded = true;
        }

        return currentLanguage;
    }

    // 切換語言並寫回玩家設定，同時通知訂閱者重新套用翻譯
    public static void setLanguage(Language language)
    {
        currentLanguage = language;
        languageLoaded = true;
        JsonReader.saveLanguage(language);
        OnLanguageChanged?.Invoke();
    }

    // 以目前語言取得翻譯文字
    public static string get(string key)
    {
        return get(key, getCurrentLanguage());
    }

    // 以指定語言取得翻譯文字
    public static string get(string key, Language language)
    {
        ensureLoaded();

        if (string.IsNullOrEmpty(key) || !textTable.TryGetValue(key, out LocalizedTextData data))
        {
            Debug.LogWarning($"Missing localization key: {key}.");
            return key;
        }

        string text = getFieldByLanguage(data, language);

        // 若該語言缺字，退回繁中再退回 key，避免出現空字串
        if (string.IsNullOrEmpty(text))
            text = string.IsNullOrEmpty(data.tw) ? key : data.tw;

        return text;
    }

    private static string getFieldByLanguage(LocalizedTextData data, Language language)
    {
        switch (language)
        {
            case Language.TRADITIONAL_CHINESE:
                return data.tw;
            case Language.ENGLISH:
                return data.en;
            case Language.JAPANESE:
                return data.ja;
            default:
                Debug.LogWarning($"Unsupported language: {language}.");
                return data.en;
        }
    }
}
