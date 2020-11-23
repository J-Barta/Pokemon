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

    BattleState state;
    int CurrentAction;
    int CurrentMove;

    private void Start()
    {
        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle()
    {
        playerUnit.Setup();
        enemyUnit.Setup();
        playerHUD.SetData(playerUnit.Pokemon);
        enemyHUD.SetData(enemyUnit.Pokemon);

        dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);

        yield return dialogBox.TypeDialog($"A wild {enemyUnit.Pokemon.Base.Name} appeared!");
        yield return new WaitForSeconds(1f);

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

        yield return new WaitForSeconds(1f);

        bool isFainted = enemyUnit.Pokemon.TakeDamage(move, playerUnit.Pokemon);
        yield return enemyHUD.UpdateHP();

        if (isFainted)
        {
            yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} fainted!");
        }
        else
            StartCoroutine(EnemyMove());
    }

    IEnumerator EnemyMove()
    {
        state = BattleState.EnemyMove;
        var move = enemyUnit.Pokemon.GetRandomMove();

        yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} used {move.Base.Name}!");

        yield return new WaitForSeconds(1f);

        bool isFainted = playerUnit.Pokemon.TakeDamage(move, enemyUnit.Pokemon);
        yield return playerHUD.UpdateHP();

        if (isFainted)
        {
            yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} fainted!");
        }
        else
            PlayerAction();


    }

    private void Update()
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
