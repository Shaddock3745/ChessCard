using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static TMPro.TMP_Dropdown;

public class DeckEditMenuManager : MonoBehaviour
{
    [SerializeField] private string startSceneName = "StartScene";
    [SerializeField] private CardListScrollViewController allCardsScrollView, currentCardsScrollView;
    [SerializeField] private Toggle useDefaultDeckToggle;
    [SerializeField] private GameObject currentHoveredCardGroup;
    [SerializeField] TMPro.TextMeshProUGUI cardNameText, cardDescriptionText;
    [SerializeField] private TMPro.TMP_Dropdown defaultDeckListDropDown;

    private static DeckEditMenuManager instance;

    private Dictionary<int, DeckEditItem> allCardList, currentCardList;
    private Language currentLanguage = Language.TRADITIONAL_CHINESE;

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
        currentLanguage = JsonReader.loadLanguage();
        allCardList = new Dictionary<int, DeckEditItem>();
        currentCardList = new Dictionary<int, DeckEditItem>();
        loadDefaultDeckListDropDownOptions();
        loadAllCardList();
        loadCurrentCardListByIds(JsonReader.loadPlayDeck());
    }

    public void onSaveAndExitButtonClick()
    {
        List<int> resultDeckList = new List<int>();

        foreach (var item in currentCardList)
        {
            int cardId = item.Key;
            int count = item.Value.getCurrentCardNumber();

            for (int i = 0; i < count; i++)
                resultDeckList.Add(cardId);
        }

        resultDeckList.Sort();

        JsonReader.savePlayDeck(resultDeckList);

        SceneManager.LoadScene(startSceneName);
    }

    public void onCancelButtonClick()
    {
        SceneManager.LoadScene(startSceneName);
    }

    public void onClearButtonClick()
    {
        clearCurrentList();
    }

    private void clearCurrentList()
    {
        List<int> idList = currentCardList.Keys.ToList();

        foreach (var id in idList)
            innerCandNumberChange(id, 0);
    }

    public void onUseDefaultDeckToggleValueChange(bool isOn)
    {
        if (isOn)
        {
            defaultDeckListDropDown.gameObject.SetActive(true);
            onDefaultDeckListDropDownValueChange(0);
        }
        else
            defaultDeckListDropDown.gameObject.SetActive(false);
    }

    public void onDefaultDeckListDropDownValueChange(int value)
    {
        if (useDefaultDeckToggle.isOn)
        {
            AiPersonality personality = (AiPersonality)defaultDeckListDropDown.value;
            loadCurrentCardListByIds(AiProfile.getDefaultDeckListOfPersonality(personality));
        }
    }

    private void loadAllCardList()
    {
        allCardList.Clear();
        List<int> cardIds = JsonReader.getCardIdList();

        foreach (int cardId in cardIds)
        {
            DeckEditItem item = allCardsScrollView.spawnDeckEditItem(cardId, getCardName(cardId));
            allCardList.Add(cardId, item);
        }
    }

    private void loadCurrentCardListByIds(List<int> cardIds)
    {
        clearCurrentList();

        Dictionary<int, int> idNumberPairs = cardIds.GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());

        foreach (var idNumberPair in idNumberPairs)
            innerCandNumberChange(idNumberPair.Key, idNumberPair.Value);
    }

    private void loadDefaultDeckListDropDownOptions()
    {
        List<OptionData> options = new List<OptionData>();

        options.Add(new OptionData(AiPersonality.Balanced.ToString()));
        options.Add(new OptionData(AiPersonality.Aggressive.ToString()));
        options.Add(new OptionData(AiPersonality.Conservative.ToString()));
        options.Add(new OptionData(AiPersonality.Tricky.ToString()));

        defaultDeckListDropDown.AddOptions(options);
    }

    public static void hoverEnter(int cardId)
    {
        instance.currentHoveredCardGroup.SetActive(true);
        instance.cardNameText.SetText(getCardName(cardId));
        instance.cardDescriptionText.SetText(getCardDescription(cardId));
    }

    public static void hoverExit(int cardId)
    {
        instance.currentHoveredCardGroup.SetActive(false);
    }

    public static void onCardNumberChange(int cardId, int number)
    {
        if (instance == null)
            return;

        if (instance.useDefaultDeckToggle.isOn)
        {
            instance.useDefaultDeckToggle.SetIsOnWithoutNotify(false);
            instance.defaultDeckListDropDown.gameObject.SetActive(false);
        }

        instance.innerCandNumberChange(cardId, number);
    }

    private void innerCandNumberChange(int cardId, int number)
    {
        if (allCardList.TryGetValue(cardId, out DeckEditItem allListItem))
            if (allListItem.getCurrentCardNumber() != number)
                allListItem.setCurrentCardNumber(number);


        if (currentCardList.TryGetValue(cardId, out DeckEditItem currentListItem))
        {
            if (currentListItem.getCurrentCardNumber() != number)
                if (number == 0)
                {
                    currentCardList.Remove(cardId);
                    Destroy(currentListItem.gameObject);
                }
                else
                    currentListItem.setCurrentCardNumber(number);
        }
        else
        {
            DeckEditItem newItem = currentCardsScrollView.spawnDeckEditItem(cardId, getCardName(cardId), number);
            currentCardList.Add(cardId, newItem);
        }
    }

    private static string getCardName(int id)
    {
        return JsonReader.getCardName(id, instance.currentLanguage);
    }

    private static string getCardDescription(int id)
    {
        return JsonReader.getCardDescription(id, instance.currentLanguage);
    }
}
