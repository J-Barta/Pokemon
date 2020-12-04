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

    [SerializeField] Sprite generalSprite;
    [SerializeField] Sprite firstSprite;
    [SerializeField] Sprite selectedGeneralSprite;
    [SerializeField] Sprite selectedFirstSprite;

    [SerializeField] Image miniSprite;

    Pokemon Pokemon;

    public bool Selected { get; set; }

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

    public void setSelected(bool selected, int slot)
    {
        if(slot == 0)
        {
            if (selected == true)
            {
                memberSlot.sprite = selectedFirstSprite;
                Selected = true;
            }
            else
                memberSlot.sprite = firstSprite;
                
        } 
        else
        {
            if (selected == true)
            {
                memberSlot.sprite = selectedGeneralSprite;
                Selected = true;
            }  
            else
                memberSlot.sprite = generalSprite;
                 
        }
    }
}