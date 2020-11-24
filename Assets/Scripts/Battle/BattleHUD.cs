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

        if (Pokemon.Gender == Gender.male)
            battleHUD.sprite = maleSprite;
        else if (Pokemon.Gender == Gender.female)
            battleHUD.sprite = femaleSprite;

        nameText.text = Pokemon.Base.Name;
        levelText.text = Pokemon.Level.ToString();
        hpBar.SetHP((float)Pokemon.HP / Pokemon.MaxHp, Pokemon.HP, Pokemon.MaxHp);
    }

    public IEnumerator UpdateHP()
    {
        yield return hpBar.SetHPSmooth((float)Pokemon.HP / Pokemon.MaxHp, Pokemon.HP, Pokemon.MaxHp);
    }

    public void ResetHP()
    {
        hpBar.ResetHP(1f);
    }
}
