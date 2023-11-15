using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DungeonSelectionUIItem : MonoBehaviour
{
    [SerializeField] Button button;
    [SerializeField] TMP_Text displayText;

    [SerializeField, ReadOnly] GameEventData cacheEventData;

    public void Initialize(GameEventData gameEventData, DungeonReception _dungeonReception)
    {
        cacheEventData = gameEventData;
        displayText.text = cacheEventData.eventTitle;

        button.onClick.AddListener(()=>
        {
            GameEvent.Instance.ToDungeonEvent(cacheEventData);
            _dungeonReception.Deactivate();
        });
    }

    public void Deinitialize()
    {
        cacheEventData = null;
        button.onClick.RemoveAllListeners();
    }
}
