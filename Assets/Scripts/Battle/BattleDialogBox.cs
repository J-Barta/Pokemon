﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleDialogBox : MonoBehaviour
{
    [SerializeField] Text dialogText;
    [SerializeField] Color highlightedColor;

    [SerializeField] GameObject actionSelector;
    [SerializeField] GameObject moveSelector;
    [SerializeField] GameObject moveDetails;

    [SerializeField] List<Text> actionTexts;
    [SerializeField] List<Text> moveTexts;

    [SerializeField] Text ppText;
    [SerializeField] Text typeText;

    [SerializeField] int lettersPerSecond;

    public IEnumerator TypeDialog(string dialog)
    {
        dialogText.text = "";
        foreach (var letter in dialog.ToCharArray())
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(1f / lettersPerSecond);
        }
    }

    public void EnableDialogText(bool enabled)
    {
        dialogText.enabled = enabled;
    }

    public void EnableActionSelector(bool enabled)
    {
        actionSelector.SetActive(enabled);
    }

    public void EnableMoveSelector(bool enabled)
    {
        moveSelector.SetActive(enabled);
        moveDetails.SetActive(enabled);
    }

    public void UpdateActionSelection(int selectedAction)
    {
        for (int i=0; i<actionTexts.Count; i++)
        {
            if (i == selectedAction)
                actionTexts[i].color = highlightedColor;
            else
                actionTexts[i].color = Color.black;
            
        }
    }

    public void SetMoveNames(List<Move> moves) {
        for (int i=0; i<moveTexts.Count; ++i)
        {
            if (i < moves.Count)

                moveTexts[i].text = moves[i].Base.Name;
            else
                moveTexts[i].text = "-";
        }
    }
}