using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleHUD : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] HPBar hpBar;
    [SerializeField] Image battleHUD;
    [SerializeField] Sprite maleSprite;
    [SerializeField] Sprite femaleSprite;

    Pokemon Pokemon;

    public void SetData(Pokemon pokemon)
    {
        Pokemon = pokemon;

        if (pokemon.Gender == Gender.male)
            battleHUD.sprite = maleSprite;
        else if (pokemon.Gender == Gender.female)
            battleHUD.sprite = femaleSprite;

        nameText.text = pokemon.Base.Name;
        levelText.text = pokemon.level.ToString();
        hpBar.SetHP((float)pokemon.HP / pokemon.MaxHp);
    }

    public IEnumerator UpdateHP()
    {
        yield return hpBar.SetHPSmooth((float)Pokemon.HP / Pokemon.MaxHp);
    }

    public void ResetHP()
    {
        hpBar.SetHP(1f);
    }
}
