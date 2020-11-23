using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleHUD : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] HPBar hpBar;

    Pokemon Pokemon;

    public void SetData(Pokemon pokemon)
    {
        Pokemon = pokemon;

        nameText.text = pokemon.Base.Name;
        Debug.Log(pokemon.Base.Name);
        levelText.text = "Lvl " + pokemon.level;
        hpBar.SetHP((float)pokemon.HP / pokemon.MaxHp);
    }

    public IEnumerator UpdateHP()
    {
        yield return hpBar.SetHPSmooth((float)Pokemon.HP / Pokemon.MaxHp);
    }
}
