using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokemonBase : ScriptableObject
{
    [SerializedField] string name;


    [TextArea]
    [SerializedField] string description;

    [SerializedField] Sprite frontSprite;
    [SerializedField] Sprite backSprite;

    [SerializedField] PokemonType type1;
    [SerializedField] PokemonType type2;



}

public enum PokemonType
{
    None,
    Normal,
    Fire,
    Water,
    Electric,
    Grass,
    Ice,
    Fighting,
    Poison,
    Ground,
    Flying,
    Psychic,
    Bug,
    Rock,
    Ghost,
    Dragon
}
