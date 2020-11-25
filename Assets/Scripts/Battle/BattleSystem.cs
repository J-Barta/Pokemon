using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState { Start, PlayerAction, PlayerMove, EnemyMove, Busy}

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleHUD playerHUD;
    [SerializeField] BattleHUD enemyHUD;
    [SerializeField] BattleDialogBox dialogBox;

    public event Action<bool> OnBattleOver;

    BattleState state;
    int CurrentAction;
    int CurrentMove;
    int escapeAttempts;

    PokemonParty playerParty;
    Pokemon wildPokemon;

    public void StartBattle(PokemonParty playerParty, Pokemon wildPokemon)
    {
        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon;

        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle()
    {

        playerHUD.ResetHP();
        enemyHUD.ResetHP();

        playerUnit.Setup(playerParty.GetHealthyPokemon());
        enemyUnit.Setup(wildPokemon);

        yield return new WaitForSeconds(1f);

        playerUnit.PlayShinyAnimation();
        enemyUnit.PlayShinyAnimation();

        playerHUD.SetData(playerUnit.Pokemon);
        enemyHUD.SetData(enemyUnit.Pokemon);

        dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);

        yield return dialogBox.TypeDialog($"A wild {enemyUnit.Pokemon.Base.Name} appeared!");

        PlayerAction();
    }

    void PlayerAction()
    {
        state = BattleState.PlayerAction;
        StartCoroutine(dialogBox.TypeDialog("Choose an action:"));
        dialogBox.EnableActionSelector(true);
    }

    void PlayerMove()
    {
        state = BattleState.PlayerMove;
        dialogBox.EnableDialogText(false);
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableMoveSelector(true);
    }

    IEnumerator PerformPlayerMove()
    {
        state = BattleState.Busy;
        var move = playerUnit.Pokemon.Moves[CurrentMove];

        yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} used {playerUnit.Pokemon.Moves[CurrentMove].Base.Name}!");

        playerUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(0.5f);
        enemyUnit.PlayHitAnimation();

        var damageDetails = enemyUnit.Pokemon.TakeDamage(move, playerUnit.Pokemon);
        yield return enemyHUD.UpdateHP();

        yield return ShowDamageDetails(damageDetails);

        if (damageDetails.Fainted)
        {
            enemyUnit.PlayFaintAnimation();
            yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} fainted!");
            dialogBox.SetDialogText("");
            yield return new WaitForSeconds(2f);
            OnBattleOver(true);
        }
        else
            StartCoroutine(EnemyMove());
    }

    IEnumerator EnemyMove()
    {
        state = BattleState.EnemyMove;
        var move = enemyUnit.Pokemon.GetRandomMove();

        yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} used {move.Base.Name}!");

        enemyUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(0.5f);
        playerUnit.PlayHitAnimation();

        var damageDetails = playerUnit.Pokemon.TakeDamage(move, enemyUnit.Pokemon);
        yield return playerHUD.UpdateHP();

        yield return ShowDamageDetails(damageDetails);

        if (damageDetails.Fainted)
        {
            playerUnit.PlayFaintAnimation();
            yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} fainted!");

            yield return new WaitForSeconds(2f);
            dialogBox.SetDialogText("");

            var nextPokemon = playerParty.GetHealthyPokemon();
            if (nextPokemon != null)
            {
                playerHUD.ResetHP();

                playerUnit.Setup(playerParty.GetHealthyPokemon());

                yield return new WaitForSeconds(1f);

                playerUnit.PlayShinyAnimation();

                playerHUD.SetData(playerUnit.Pokemon);

                dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);

                yield return dialogBox.TypeDialog($"Go {nextPokemon.Base.Name}!");


                PlayerAction();
            }
            else
                OnBattleOver(false);
        }
        else
            PlayerAction();
    }

    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        if (damageDetails.Critical > 1f)
            yield return dialogBox.TypeDialog("It's a critical hit!");

        if (damageDetails.Type > 1f)
            yield return dialogBox.TypeDialog("It's super effective!");
        else if (damageDetails.Type < 1f)
            yield return dialogBox.TypeDialog("It's not very effective!");
    }

    public void HandleUpdate()
    {
        if (state == BattleState.PlayerAction)
        {
            HandleActionSelection();
        }
        else if (state == BattleState.PlayerMove)
        {
            HandleMoveSelection();
        }
        else if (state == BattleState.EnemyMove)
        {

        }
    }

    void HandleActionSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
            ++CurrentAction;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            --CurrentAction;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            CurrentAction += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            CurrentAction -= 2;

        CurrentAction = Mathf.Clamp(CurrentAction, 0, 3);

        dialogBox.UpdateActionSelection(CurrentAction);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (CurrentAction == 0)
            {
                //Fight
                PlayerMove();
            }
            else if (CurrentAction == 1)
            {
                //Bag
                
            }
            else if (CurrentAction == 2)
            {
                //Pokemon
            }
            else if (CurrentAction == 3)
            {
                //Run
                StartCoroutine(TryPlayerRun());
            }
        }
    }

    public IEnumerator TryPlayerRun()
    {
        state = BattleState.Busy;
        escapeAttempts += 1;
        bool escape;
        if (playerUnit.Pokemon.Speed > enemyUnit.Pokemon.Speed)
            escape = true;
        else
        {
            int escapeNum = ((playerUnit.Pokemon.Speed * 128) / enemyUnit.Pokemon.Speed) + (30 * escapeAttempts) % 256;
            System.Random r = new System.Random();
            if (escapeNum > r.Next(0, 256))
                escape = true;
            else
                escape = false;
        }

        if(escape)
        {
            yield return dialogBox.TypeDialog("You escaped successfully!");

            yield return new WaitForSeconds(2f);
            dialogBox.SetDialogText("");
            OnBattleOver(false);
        }
        else
        {
            yield return dialogBox.TypeDialog("You couldn't get away!");
            yield return new WaitForSeconds(2f);
            dialogBox.SetDialogText("");
            state = BattleState.EnemyMove;
        }
    }

    void HandleMoveSelection()
    {

        if (Input.GetKeyDown(KeyCode.RightArrow))
            ++CurrentMove;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            --CurrentMove;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            CurrentMove += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            CurrentMove -= 2;

        CurrentMove = Mathf.Clamp(CurrentAction, 0, playerUnit.Pokemon.Moves.Count - 1);

        dialogBox.UpdateMoveSelection(CurrentMove, playerUnit.Pokemon.Moves[CurrentMove]);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(PerformPlayerMove());
        }
    }
}