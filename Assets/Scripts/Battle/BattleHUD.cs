using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleHUD : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] HPBar hpBar;

    public void SetData(Pokemon pokemon)
    {
        nameText.text = pokemon.Base.Name;
        Debug.Log(pokemon.Base.Name);
        levelText.text = "Lvl " + pokemon.level;
        hpBar.setHP((float)pokemon.HP / pokemon.MaxHp);
    }
}
