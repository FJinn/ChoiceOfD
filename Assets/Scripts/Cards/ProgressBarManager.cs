using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ProgressBarManager : Singleton<ProgressBarManager>
{
    [SerializeField] ProgressBar2D progressBar2DTemplate;

    List<ProgressBar2D> progressBarPool = new();

    public void SetProgressBar(CardController instigator)
    {
        ProgressBar2D progressBar = GetProgressBar();
        progressBar.Activate(instigator);
    }

    ProgressBar2D GetProgressBar()
    {
        ProgressBar2D found = progressBarPool.Find(x => x.IsAvailable());

        if(found != null)
        {
            return found;
        }

        ProgressBar2D newBar = Instantiate(progressBar2DTemplate, transform);
        progressBarPool.Add(newBar);
        return newBar;
    }
}
