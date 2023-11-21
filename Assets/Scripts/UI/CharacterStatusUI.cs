using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterStatusUI : MonoBehaviour
{
    CharacterBase character;
    [SerializeField] Image statusIconTemplate;

    List<StatusIconInfo> spawnedStatusIcons = new();

    class StatusIconInfo
    {
        public CharacterBase.EBuffType buffType = CharacterBase.EBuffType.None;
        public CharacterBase.EDebuffType debuffType = CharacterBase.EDebuffType.None;
        public Image image;

        public bool IsActive() => image.gameObject.activeInHierarchy;

        public void Activate(CharacterBase.EBuffType _buffType)
        {
            buffType = _buffType;
            image.sprite = DataCache.Instance.GetSprite(buffType);
            image.gameObject.SetActive(true);
        }
        public void Activate(CharacterBase.EDebuffType _debuffType)
        {
            debuffType = _debuffType;
            image.sprite = DataCache.Instance.GetSprite(debuffType);
            image.gameObject.SetActive(true);
        }
        public void Deactivate()
        {
            image.gameObject.SetActive(false);
            buffType = CharacterBase.EBuffType.None;
            debuffType = CharacterBase.EDebuffType.None;
        }
    }

    void Awake()
    {
        character = GetComponentInParent<CharacterBase>();
        character.onBuffAdded += OnBuffAdded;
        character.onBuffRemoved += OnBuffRemoved;
        character.onDebuffAdded += OnDebuffAdded;
        character.onDebuffRemoved += OnDebuffRemoved;
    }

    void OnBuffAdded(CharacterBase.EBuffType buffType)
    {
        StatusIconInfo target = GetStatusIconInfo();
        target.Activate(buffType);
    }

    void OnBuffRemoved(CharacterBase.EBuffType buffType)
    {
        spawnedStatusIcons.Find(x => x.buffType == buffType)?.Deactivate();
    }
    
    void OnDebuffAdded(CharacterBase.EDebuffType debuffType)
    {
        StatusIconInfo target = GetStatusIconInfo();
        target.Activate(debuffType);
    }

    void OnDebuffRemoved(CharacterBase.EDebuffType debuffType)
    {
        spawnedStatusIcons.Find(x => x.debuffType == debuffType)?.Deactivate();
    }

    StatusIconInfo GetStatusIconInfo()
    {
        var found = spawnedStatusIcons.Find(x => !x.IsActive());
        if(found != null)
        {
            return found;
        }

        Image newImg = Instantiate(statusIconTemplate, transform);
        StatusIconInfo newStatusIcon = new StatusIconInfo()
        {
            image = newImg
        };
        spawnedStatusIcons.Add(newStatusIcon);
        return newStatusIcon;
    }
}
