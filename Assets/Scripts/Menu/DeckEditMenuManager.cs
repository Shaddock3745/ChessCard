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
        allCardList = new Dictionary<int, DeckEditItem>();
        currentCardList = new Dictionary<int, DeckEditItem>();
        loadDefaultDeckListDropDownOptions();
        loadAllCardList();
    }

    public void onSaveAndExitButtonClick()
    {
        // To Do

        SceneManager.LoadScene(startSceneName);
    }

    public void onCancelButtonClick()
    {
        SceneManager.LoadScene(startSceneName);
    }

    public void onClearButtonClick()
    {
        currentCardList.Clear();
        loadCurrentCardListByIds(new List<int>());
    }

    public void onUseDefaultDeckToggleValueChange(bool isOn)
    {

    }

    public void onDefaultDeckListDropDownValueChange(int value)
    {

    }

    private void loadAllCardList()
    {
        allCardList.Clear();
        List<int> cardIds = JsonReader.getCardIdList();

        foreach (int cardId in cardIds)
        {
            DeckEditItem item = allCardsScrollView.spawnDeckEditItem(cardId, JsonReader.getCardName(cardId));
            allCardList.Add(cardId, item);
        }
    }

    private void loadCurrentCardListByIds(List<int> cardIds)
    {
        currentCardList.Clear();

        Dictionary<int, int> idNumberPairs = cardIds.GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());

        foreach (var idNumberPair in idNumberPairs)
        {
            int cardId = idNumberPair.Key;
            DeckEditItem item = currentCardsScrollView.spawnDeckEditItem(cardId, JsonReader.getCardName(cardId));
            item.setCurrentCardNumber(idNumberPair.Value);
            currentCardList.Add(cardId, item);
        }
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

    }

    public static void hoverExit(int cardId)
    {

    }
}
