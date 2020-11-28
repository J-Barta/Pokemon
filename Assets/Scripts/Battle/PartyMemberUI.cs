using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] HPBar hpBar;

    Pokemon Pokemon;

    public void SetData(Pokemon pokemon)
    {
        Pokemon = pokemon;

        nameText.text = Pokemon.Base.Name;
        levelText.text = Pokemon.Level.ToString();
        hpBar.SetHP((float)Pokemon.HP / Pokemon.MaxHp, Pokemon.HP, Pokemon.MaxHp);
    }
}
