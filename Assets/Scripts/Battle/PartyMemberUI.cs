using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] Image memberSlot;
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] HPBar hpBar;
    [SerializeField] Image gender;
    [SerializeField] Sprite maleSprite;
    [SerializeField] Sprite femaleSprite;
    [SerializeField] Sprite firstSprite;
    [SerializeField] Image miniSprite;

    Pokemon Pokemon;

    public void SetData(Pokemon pokemon)
    {
        Pokemon = pokemon;

        nameText.text = Pokemon.Base.Name;
        levelText.text = Pokemon.Level.ToString();

        if (Pokemon.Gender == Gender.male)
            gender.sprite = maleSprite;
        else if (Pokemon.Gender == Gender.female)
            gender.sprite = femaleSprite;
        else
            gender.sprite = null;

        hpBar.SetHP((float)Pokemon.HP / Pokemon.MaxHp, Pokemon.HP, Pokemon.MaxHp);

        miniSprite.sprite = Pokemon.Base.MiniSprite;
    }

    public void setFirstMemberSlot()
    {
        memberSlot.sprite = firstSprite;
    }
}
