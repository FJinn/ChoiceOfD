using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataCache : Singleton<DataCache>
{
    // Temp
    // ToDo :: better structure

    public List<BuffUIData> buffUIDatas;
    public List<DebuffUIData> debuffUIDatas;

    [Serializable]
    public class BuffUIData
    {
        public CharacterBase.EBuffType buffType;
        public Sprite sprite;
    }

    [Serializable]
    public class DebuffUIData
    {
        public CharacterBase.EDebuffType debuffType;
        public Sprite sprite;
    }

    public Sprite GetSprite(CharacterBase.EBuffType buffType) => buffUIDatas.Find(x => x.buffType == buffType)?.sprite;
    public Sprite GetSprite(CharacterBase.EDebuffType debuffType) => debuffUIDatas.Find(x => x.debuffType == debuffType)?.sprite;
}
