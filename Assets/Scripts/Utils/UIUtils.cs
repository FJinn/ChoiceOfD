using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIUtils
{
    public static float GetBestLabelFontSize(Label textElement, float maxWidth, float maxHeight)
    {
        // Some initial font size
        float fontSize = 16f;

        // Use a loop to find the best fit
        while (textElement.resolvedStyle.width <= maxWidth && textElement.resolvedStyle.height <= maxHeight)
        {
            fontSize += 1f;
            textElement.style.fontSize = fontSize;
        }

        // Return the best fit font size
        return fontSize - 1f;
    }
}
