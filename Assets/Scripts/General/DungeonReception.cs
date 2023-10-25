using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class DungeonReception : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] GameObject canvas;
    [SerializeField] GameEventData[] gameEventDatas;
    [SerializeField] DungeonSelectionUIItem dungeonSelectionUIItemPrefab;
    [SerializeField] Transform dungeonSelectionUIItemParent;

    List<DungeonSelectionUIItem> dungeonSelectionUIItems = new();

    public UnityEvent<PointerEventData> onClick = new UnityEvent<PointerEventData>();

    public void OnPointerClick(PointerEventData eventData)
    {
        onClick?.Invoke(eventData);
        Activate();
    }

    void Awake()
    {
        Deactivate();
        // temp
        SpawnDungeonSelectionUIItems();
    }

    public void Activate()
    {
        canvas.SetActive(true);
    }

    public void Deactivate()
    {
        canvas.SetActive(false);
    }

    // ToDo:: design which event should be showned and hidden
    public void SpawnDungeonSelectionUIItems()
    {
        foreach(var item in gameEventDatas)
        {
            DungeonSelectionUIItem spawned = Instantiate(dungeonSelectionUIItemPrefab, dungeonSelectionUIItemParent);
            spawned.Initialize(item);
            dungeonSelectionUIItems.Add(spawned);
        }
    }
}
