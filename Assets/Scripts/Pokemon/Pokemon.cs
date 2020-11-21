using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pokemon
{
    public PokemonBase Base { get; set; }
    public int level { get; set; }

    public List<Move> Moves { get; set; }

    public int HP { get; set; }

    public Pokemon(PokemonBase pBase, int pLevel)
    {
        Base = pBase;
        level = pLevel;
        HP = MaxHp;

        Moves = new List<Move>();
        foreach (var move in Base.LearnableMoves) 
        {
            if (move.Level <= level)
                Moves.Add(new Move(move.Base));

            if (Moves.Count >= 4)
                break;
        }
    }

    public int Attack
    {
        get { return Mathf.FloorToInt((Base.Attack * level) / 100f) + 5; }
    }

    public int Defense
    {
        get { return Mathf.FloorToInt((Base.Defense * level) / 100f) + 5; }
    }

    public int SpAttack
    {
        get { return Mathf.FloorToInt((Base.SpAttack * level) / 100f) + 5; }
    }

    public int SpDefense
    {
        get { return Mathf.FloorToInt((Base.SpDefense * level) / 100f) + 5; }
    }

    public int Speed
    {
        get { return Mathf.FloorToInt((Base.Speed * level) / 100f) + 5; }
    }

    public int MaxHp
    {
        get { return Mathf.FloorToInt((Base.MaxHp * level) / 100f) + 10; }
    }
}
