using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState { Start, ActionSelection, MoveSelection, PerformMove, partyScreen, Busy}

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleHUD playerHUD;
    [SerializeField] BattleHUD enemyHUD;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScreen partyScreen;

    public event Action<bool> OnBattleOver;

    BattleState state;
    int CurrentAction;
    int CurrentMove;
    int CurrentPokemon;
    int CurrentConfirm;
    int escapeAttempts;

    bool pokemonMoveConfirm = false;

    PokemonParty playerParty;
    Pokemon wildPokemon;
    Pokemon SelectedMember;

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

        partyScreen.Init();

        dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);

        yield return dialogBox.TypeDialog($"A wild {enemyUnit.Pokemon.Base.Name} appeared!");

        PlayerAction();
    }

    void PlayerAction()
    { 
        StartCoroutine(dialogBox.TypeDialog("Choose an action:"));
        state = BattleState.ActionSelection;
        dialogBox.EnableActionSelector(true);
    }

    void OpenPartyScreen()
    {
        state = BattleState.partyScreen;
        partyScreen.SetPartyData(playerParty.Pokemons);
        partyScreen.gameObject.SetActive(true);
    }

    void MoveSelection()
    {
        state = BattleState.PerformMove;
        dialogBox.EnableDialogText(false);
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableMoveSelector(true);
    }

    IEnumerator PlayerMove()
    {
        state = BattleState.Busy;

        var move = playerUnit.Pokemon.Moves[CurrentMove];
        playerUnit.Pokemon.Moves[CurrentMove].PP--;

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
        state = BattleState.PerformMove;

        int moveNum = enemyUnit.Pokemon.GetRandomMoveNumber();
        enemyUnit.Pokemon.Moves[moveNum].PP--;
        var move = enemyUnit.Pokemon.Moves[moveNum];

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

                yield return dialogBox.TypeDialog($"Go, {nextPokemon.Base.Name}!");


                PlayerAction();
            }
            else
                OnBattleOver(false);
        }
        else
            PlayerAction();
    }

    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, int moveNum)
    {

        //Set the proper move and reduce the PP of the move by 1
        Move move = sourceUnit.Pokemon.Moves[moveNum];
        sourceUnit.Pokemon.Moves[moveNum].PP--;

        //Tell the player which move is being used
        yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name} used {sourceUnit.Pokemon.Moves[CurrentMove].Base.Name}!");

        //Play the hit and attack animations.
        sourceUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(0.5f);
        targetUnit.PlayHitAnimation();

        //Apply the damage and update the HP bar
        var damageDetails = targetUnit.Pokemon.TakeDamage(move, sourceUnit.Pokemon);
        yield return enemyHUD.UpdateHP();
        yield return ShowDamageDetails(damageDetails);

        if (damageDetails.Fainted)
        {
            //Inform the player that the target unit fainted
            targetUnit.PlayFaintAnimation();
            yield return dialogBox.TypeDialog($"{targetUnit.Pokemon.Base.Name} fainted!");
            yield return new WaitForSeconds(2f);
        }
    }

    void CheckForBattleOver(BattleUnit faintedUnit)
    {
        if (faintedUnit.IsPlayerUnit)
        {

        }
        else
        {
            OnBattleOver(true);
        }
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
        if (state == BattleState.ActionSelection)
        {
            HandleActionSelection();
        }
        else if (state == BattleState.MoveSelection)
        {
            HandleMoveSelection();
        }
        else if (state == BattleState.partyScreen)
        {
            HandlePartySelection();
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
                MoveSelection();
            }
            else if (CurrentAction == 1)
            {
                //Bag
                
            }
            else if (CurrentAction == 2)
            {
                //Pokemon
                OpenPartyScreen();
                state = BattleState.partyScreen;
                dialogBox.EnableMoveSelector(false);
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
            state = BattleState.MoveSelection;
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

        CurrentMove = Mathf.Clamp(CurrentMove, 0, playerUnit.Pokemon.Moves.Count - 1);

        dialogBox.UpdateMoveSelection(CurrentMove, playerUnit.Pokemon.Moves[CurrentMove]);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(PlayerMove());
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            PlayerAction();
        }
    }

    void HandlePartySelection()
    {
        if (pokemonMoveConfirm == false)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
                ++CurrentPokemon;
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
                --CurrentPokemon;
            else if (Input.GetKeyDown(KeyCode.DownArrow))
                CurrentPokemon += 2;
            else if (Input.GetKeyDown(KeyCode.UpArrow))
                CurrentPokemon -= 2;

            CurrentPokemon = Mathf.Clamp(CurrentPokemon, 0, playerParty.Pokemons.Count - 1);

            partyScreen.UpdatePokemonSelection(CurrentPokemon);

            if (Input.GetKeyDown(KeyCode.Z))
            {
                var selectedMember = playerParty.Pokemons[CurrentPokemon];
                if (selectedMember.HP <= 0)
                {
                    partyScreen.SetMessageText("You can't send out a fainted Pokemon!");
                    return;
                }
                else if (selectedMember == playerUnit.Pokemon)
                {
                    partyScreen.SetMessageText($"{playerUnit.Pokemon.Base.Name} is already in battle!");
                    return;
                }
                else
                {
                    SelectedMember = selectedMember;
                    partyScreen.SetConfirmActive(true);

                    pokemonMoveConfirm = true;
                }

            }
            else if (Input.GetKeyDown(KeyCode.X))
            {
                partyScreen.gameObject.SetActive(false);
                PlayerAction();
            }
        }
        else
        {
            partyScreen.SetMessageText($"Are you sure you want to send {SelectedMember.Base.Name} into batte?");

            if (Input.GetKeyDown(KeyCode.RightArrow))
                ++CurrentConfirm;
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
                --CurrentConfirm;
            else if (Input.GetKeyDown(KeyCode.DownArrow))
                ++CurrentConfirm;
            else if (Input.GetKeyDown(KeyCode.UpArrow))
                --CurrentConfirm;

            CurrentPokemon = Mathf.Clamp(CurrentPokemon, 0, 1);

            partyScreen.PokemonConfirmBox.UpdateActionSelection(CurrentConfirm);

            if (Input.GetKeyDown(KeyCode.Z)) 
            {
               if(CurrentConfirm == 0)
               {  
                    partyScreen.gameObject.SetActive(false);
                    state = BattleState.Busy;
                    pokemonMoveConfirm = false;
                    StartCoroutine(SwitchPokemon(SelectedMember));
               }
               else
               {
                    partyScreen.SetMessageText("Choose a Pokemon.");
                    partyScreen.SetConfirmActive(false);
                    pokemonMoveConfirm = false;
               }
            }
            else if (Input.GetKeyDown(KeyCode.X))
            {
                partyScreen.SetMessageText("Choose a Pokemon.");
                partyScreen.SetConfirmActive(false);
                pokemonMoveConfirm = false;
            }
        }
        
    }

    IEnumerator SwitchPokemon (Pokemon newPokemon)
    {
        yield return dialogBox.TypeDialog($"Come back, {playerUnit.Pokemon.Base.Name}!");
        playerUnit.PlayFaintAnimation();
        yield return new WaitForSeconds(2f);

        playerUnit.Setup(newPokemon);
        yield return new WaitForSeconds(1f);
        playerUnit.PlayShinyAnimation();
        playerHUD.SetData(playerUnit.Pokemon);
        dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);
        yield return dialogBox.TypeDialog($"Go, {newPokemon.Base.Name}!");

        StartCoroutine(EnemyMove());
    }
}