using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PokemonConfirmBox : MonoBehaviour
{
    [SerializeField] Color highlightedColor;

    [SerializeField] Text confirmText;
    [SerializeField] Text cancelText;

    public void UpdateActionSelection(int selectedAction)
    {
        if (selectedAction == 0)
        {
            confirmText.color = highlightedColor;
            cancelText.color = Color.black;
        }
        else
        {
            cancelText.color = highlightedColor;
            confirmText.color = Color.black;
        }
    }

}
