using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleUnit : MonoBehaviour
{
    [SerializeField] PokemonBase Base;
    [SerializeField] int level;
    [SerializeField] bool isShiny;
    [SerializeField] bool isPlayerUnit;

    public Pokemon Pokemon { get; set; }

    public void Setup()
    {
        Pokemon = new Pokemon(Base, level, isShiny);

        if (isPlayerUnit)
            GetComponent<Image>().sprite = Pokemon.BackSprite;
        else
            GetComponent<Image>().sprite = Pokemon.FrontSprite;
    }
}
