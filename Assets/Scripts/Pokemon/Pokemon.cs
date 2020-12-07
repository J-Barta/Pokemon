using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

public class Pokemon
{
    [SerializeField] PokemonBase _base;
    [SerializeField] int level;
    [SerializeField] bool shiny;
    [SerializeField] Gender gender;



    public PokemonBase Base { get{return _base;}}

    public int Level { get { return level; } }
    public bool Shiny { get { return shiny; } }
    public Gender Gender { get { return gender; } }

    Sprite frontSprite;
    Sprite backSprite;

    public List<Move> Moves { get; set; }

    public int HP { get; set; }


    public void Init()
    {
        HP = MaxHp;

        Moves = new List<Move>();
        foreach (var move in Base.LearnableMoves) 
        {
            if (move.Level <= level)
                Moves.Add(new Move(move.Base));

            if (Moves.Count >= 4)
                break;
        }

        if(shiny)
        {
            frontSprite = Base.ShinyFrontSprite;
            backSprite = Base.ShinyBackSprite;
        } else
        {
            frontSprite = Base.FrontSprite;
            backSprite = Base.BackSprite;
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

    public Sprite FrontSprite
    {
        get { return frontSprite; }
    }

    public Sprite BackSprite
    {
        get { return backSprite; }
    }

    public DamageDetails TakeDamage(Move move, Pokemon attacker)
    {
        float type = TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type1) * TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type2);

        float critical = 1f;
        if (Random.value * 100f <= 6.25)
            critical = 2f;

        var DamageDetails = new DamageDetails()
        {
            Type = type,
            Critical = critical,
            Fainted = false
        };

        float attack = (move.Base.IsSpecial) ? attacker.SpAttack : attacker.Attack;
        float defense = (move.Base.IsSpecial) ? SpDefense : Defense; 

        float modifiers = Random.Range(0.85f, 1f) * type * critical;
        float a = (2 * attacker.level + 10) / 250f;
        float d = a * move.Base.Power * ((float)attack / defense) + 2;
        int damage = Mathf.FloorToInt(d * modifiers);



        HP -= damage;
        if (HP <= 0)
        {
            HP = 0;
            DamageDetails.Fainted = true;
        }

        return DamageDetails;
           
    }

    public int GetRandomMoveNumber()
    {
        int r = Random.Range(0, Moves.Count);
        return r;
    }
}

public enum Gender {
    none,
    male,
    female
}

public class DamageDetails
{
    public bool Fainted { get; set; }

    public float Critical { get; set; }
    public float Type { get; set; }
}
