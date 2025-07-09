using UnityEngine;
using UnityEngine.EventSystems;

public class UnitClickHandler : MonoBehaviour, IPointerClickHandler
{
    public UnitStats unitStats;

    public Sprite unitSprite;
    public UpgradeScreenUI UpgradeScreenUI;

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"Unit clicked: {unitStats?.name}");

        if(UpgradeScreenUI == null)
        {
            UpgradeScreenUI = FindFirstObjectByType<UpgradeScreenUI>();
            if (UpgradeScreenUI == null)
            {
                Debug.LogError("UpgradeScreenUI not found in the scene!");
                return;
            }
        }

        if (unitStats != null && UpgradeScreenUI != null)
        {
            UpgradeScreenUI.Open(unitStats, unitSprite);
        }
    }
}
