using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SelectionScreenManager : MonoBehaviour
{

    [Header("UI Panels")]
    public GameObject loadoutScreen;
    public GameObject battleScreen;

    [Header("Unit Selection")]
    public Transform unitSelectionGrid;
    public GameObject unitButtonPrefab;
    public List<GameObject> availableUnits;

    [Header("Loadout Display")]
    public TMP_Text frontlineCountText;
    public TMP_Text backlineCountText;

    [Header("Upgrade Screen")]
    public UpgradeScreenUI UpgradeScreenUI;


    private void Start()
    {
        CreateUnitButtons();
        LoadoutData.selectedFrontline.Clear();
        LoadoutData.selectedBackline.Clear();
        // PopulateUnitButtons();
        UpdateCountText();

        for (int i = 0; i < 6; i++)
        {
            DropSlot drop = loadoutSlotsParent.GetChild(i).gameObject.AddComponent<DropSlot>();
            drop.slotIndex = i;
        }
    }

    [Header("Loadout Slots")]
    public Transform loadoutSlotsParent;
    void CreateUnitButtons()
    {
        foreach (GameObject unit in availableUnits)
        {
            GameObject btn = Instantiate(unitButtonPrefab, unitSelectionGrid);

            Image btnImage = btn.GetComponent<Image>();
            Sprite unitSprite = unit.GetComponent<SpriteRenderer>()?.sprite;
            if (btnImage != null && unitSprite != null)
            {
                btnImage.sprite = unitSprite;
            }

            DraggableUnit drag = btn.GetComponent<DraggableUnit>();
            if (drag != null)
            {
                drag.SetPrefab(unit);
            }

            UnitClickHandler clickHandler = btn.GetComponent<UnitClickHandler>();
            if (clickHandler != null)
            {
                clickHandler.unitStats = unit.GetComponent<UnitStats>();
                clickHandler.unitSprite = unitSprite;
                clickHandler.UpgradeScreenUI = UpgradeScreenUI;
            }
        }
    }
    void OnUnitSelected(GameObject unitPrefab)
    {
        Debug.Log($"Selected Unit: {unitPrefab.name}");
        // UpdateCountText();
    }

    void UpdateCountText()
    {
        if (frontlineCountText)
            frontlineCountText.text = $"Frontline: {LoadoutData.selectedFrontline.Count}/3";
        if (backlineCountText)
            backlineCountText.text = $"Backline: {LoadoutData.selectedBackline.Count}/3";
    }

    public void OnStartBattleClicked()
    {
        int frontCount = 0, backCount = 0;

        for (int i = 0; i < 3; i++)
            if (LoadoutData.selectedUnits[i] != null) frontCount++;

        for (int i = 3; i < 6; i++)
            if (LoadoutData.selectedUnits[i] != null) backCount++;

        if (frontCount != 3 || backCount != 3)
        {
            Debug.LogWarning("Please assign 3 front and 3 back units before starting the battle.");
            return;
        }

        // Hide Loadout UI, Show Battle Screen
        loadoutScreen.SetActive(false);
        battleScreen.SetActive(true);

        // Pass control to BattleSystem
        BattleSystem bs = FindFirstObjectByType<BattleSystem>();
        bs.InitializeBattle();
    }

    public void ReAddUnitToGrid(GameObject unit)
    {
        Debug.Log($"Re-adding unit to grid: {unit.name}");
        GameObject btn = Instantiate(unitButtonPrefab, unitSelectionGrid);

        Image btnImage = btn.GetComponent<Image>();
        Sprite unitSprite = unit.GetComponent<SpriteRenderer>()?.sprite;
        if (btnImage != null && unitSprite != null)
        {
            btnImage.sprite = unitSprite;
        }

        DraggableUnit drag = btn.GetComponent<DraggableUnit>();
        if (drag != null)
        {
            drag.SetPrefab(unit);
        }
    }

}
