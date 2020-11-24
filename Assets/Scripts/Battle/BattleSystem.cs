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

    public void StartBattle()
    {
        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle()
    {
        playerHUD.ResetHP();
        enemyHUD.ResetHP();

        playerUnit.Setup();
        enemyUnit.Setup();

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
    }

    void HandleActionSelection()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (CurrentAction < 1)
            {
                ++CurrentAction;
            }
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (CurrentAction > 0)
            {
                --CurrentAction;
            }
        }

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
                //Run
            }
        }
    }

    void HandleMoveSelection()
    {
        
        if (Input.GetKeyDown(KeyCode.DownArrow) && CurrentMove < 2)
        {
            Debug.Log("Down");
            if (CurrentMove == 0)
                CurrentMove = 2;
            else
                CurrentMove = 3;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) && CurrentMove > 1)
        {
            Debug.Log("Up");
            if (CurrentMove == 2)
                CurrentMove = 0;
            else
                CurrentMove = 1;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) && CurrentMove < 3)
        {
            Debug.Log("Right");
            ++CurrentMove;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) && CurrentMove > 0)
        {
            Debug.Log("Left");
            --CurrentMove;
        }

        if (playerUnit.Pokemon.Moves.Count < 4 && CurrentMove > playerUnit.Pokemon.Moves.Count - 1)
            CurrentMove = playerUnit.Pokemon.Moves.Count - 1;

        dialogBox.UpdateMoveSelection(CurrentMove, playerUnit.Pokemon.Moves[CurrentMove]);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(PerformPlayerMove());
        }
    }
}