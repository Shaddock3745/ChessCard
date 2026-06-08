using TMPro;
using UnityEngine;

// 掛在任一 TextMeshProUGUI 上，於 Inspector 填入對照表的 key，
// 物件啟用時與語言切換時會自動套用對應語言文字。
[RequireComponent(typeof(TextMeshProUGUI))]
public class LocalizedText : MonoBehaviour
{
    [SerializeField] private string key;

    private TextMeshProUGUI text;

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        LocalizationManager.OnLanguageChanged += apply;
        apply();
    }

    private void OnDisable()
    {
        LocalizationManager.OnLanguageChanged -= apply;
    }

    public void setKey(string newKey)
    {
        key = newKey;
        apply();
    }

    private void apply()
    {
        if (!string.IsNullOrEmpty(key))
            text.SetText(LocalizationManager.get(key));
    }
}
