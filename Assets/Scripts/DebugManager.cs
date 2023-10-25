using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if DEBUG
public class DebugManager : MonoBehaviour
{
    [Button("FillParty")]
    public bool bFillParty;
    public List<ECharacterClass> fillWithClasses;

    public void FillParty()
    {
        if(fillWithClasses.Count <= 0 || fillWithClasses.Contains(ECharacterClass.None))
        {
            Debug.LogError("DebugManager:: Fill with classes contains 0 or None! Skipping!");
            return;
        }

        PlayerController player = PlayerController.Instance;
        int fillAmount = Mathf.Clamp(fillWithClasses.Count, 1, 4);
        for(int i=0; i<fillAmount; ++i)
        {
            player.ObtainCharacter(fillWithClasses[i]);
        }
    }
}
#endif