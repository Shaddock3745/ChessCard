using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static TMPro.TMP_Dropdown;

public class StartMenuManager : MonoBehaviour
{
    private static StartMenuManager instance;

    [SerializeField] private TMPro.TMP_Dropdown enemyAiPersonalityDropDown, enemyAiDifficultyDropDown, languageDropDown;
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private string deckEditingSceneName = "DeckSettingScene";

    private static readonly AiPersonality[] personalityByIndex =
   {
        AiPersonality.Balanced,
        AiPersonality.Aggressive,
        AiPersonality.Conservative,
        AiPersonality.Tricky,
    };

    private static readonly AiDifficulty[] difficultyByIndex =
    {
        AiDifficulty.Easy,
        AiDifficulty.Normal,
        AiDifficulty.Hard,
    };

    private static readonly Language[] languageByIndex =
    {
        Language.TRADITIONAL_CHINESE,
        Language.ENGLISH,
        Language.JAPANESE,
    };

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        loadDropDownOptions();
    }

    public void startGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void deckSetting()
    {
        SceneManager.LoadScene(deckEditingSceneName);
    }

    public void quitGame()
    {
        Application.Quit();
    }

    private void loadDropDownOptions()
    {
        enemyAiPersonalityDropDown.ClearOptions();
        enemyAiPersonalityDropDown.AddOptions(toOptionList(personalityByIndex));
        enemyAiPersonalityDropDown.SetValueWithoutNotify(
            indexOf(personalityByIndex, JsonReader.loadAiPersonality()));

        enemyAiDifficultyDropDown.ClearOptions();
        enemyAiDifficultyDropDown.AddOptions(toOptionList(difficultyByIndex));
        enemyAiDifficultyDropDown.SetValueWithoutNotify(
            indexOf(difficultyByIndex, JsonReader.loadAiDifficulty()));

        languageDropDown.ClearOptions();
        languageDropDown.AddOptions(new List<OptionData>
        {
            new OptionData("繁體中文"),
            new OptionData("English"),
            new OptionData("日本語"),
        });
        languageDropDown.SetValueWithoutNotify(
            indexOf(languageByIndex, JsonReader.loadLanguage()));
    }

    public void onEnemyAiPersonalityDropDownValueChange(int value)
    {
        JsonReader.saveAiPersonality(personalityByIndex[value]);
    }

    public void onEnemyAiDifficultyDropDownValueChange(int value)
    {
        JsonReader.saveAiDifficulty(difficultyByIndex[value]);
    }

    public void onLanguageDropDownValueChange(int value)
    {
        JsonReader.saveLanguage(languageByIndex[value]);
    }

    private static List<OptionData> toOptionList<T>(T[] values)
    {
        List<OptionData> options = new List<OptionData>();
        foreach (T v in values)
            options.Add(new OptionData(v.ToString()));
        return options;
    }

    private static int indexOf<T>(T[] values, T target)
    {
        for (int i = 0; i < values.Length; i++)
            if (EqualityComparer<T>.Default.Equals(values[i], target))
                return i;
        return 0;
    }
}
