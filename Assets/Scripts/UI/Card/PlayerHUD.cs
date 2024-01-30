using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerHUD : MonoBehaviour
{
    [SerializeField] UIDocument uiDocument;

    Label dayLabel;
    Label timeLabel;
    Label weatherLabel;

    void Start()
    {
        VisualElement root = uiDocument.rootVisualElement;
        dayLabel = root.Q<Label>("DayLabel");
        timeLabel = root.Q<Label>("TimeLabel");
        weatherLabel = root.Q<Label>("WeatherLabel");

        dayLabel.style.fontSize = UIUtils.GetBestLabelFontSize(dayLabel, 50f, 50f);
        timeLabel.style.fontSize = UIUtils.GetBestLabelFontSize(dayLabel, 50f, 50f);
        weatherLabel.style.fontSize = UIUtils.GetBestLabelFontSize(dayLabel, 50f, 50f);

        TimeManager.onDayEnded += UpdateDay;
        TimeManager.onTimeOfDayChanged += UpdateTimeOfDay;

        timeLabel.text = TimeManager.currentTimeOfDay.ToString();
    }

    void OnDestroy()
    {
        TimeManager.onDayEnded -= UpdateDay;
    }

    void UpdateDay()
    {
        dayLabel.text = "Day: " + TimeManager.currentDay.ToString();
    }

    void UpdateTimeOfDay()
    {
        timeLabel.text = TimeManager.currentTimeOfDay.ToString();
    }
}
