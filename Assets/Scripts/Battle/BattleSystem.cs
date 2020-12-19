using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState { Start, ActionSelection, MoveSelection, PerformMove, PartyScreen, Busy, BattleOver}

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
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
        playerUnit.Setup(playerParty.GetHealthyPokemon());
        enemyUnit.Setup(wildPokemon);

        yield return new WaitForSeconds(1f);

        playerUnit.PlayShinyAnimation();
        enemyUnit.PlayShinyAnimation();

        partyScreen.Init();

        dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);

        yield return dialogBox.TypeDialog($"A wild {enemyUnit.Pokemon.Base.Name} appeared!");

        ActionSelection();
    }

    void ActionSelection()
    { 
        StartCoroutine(dialogBox.TypeDialog("Choose an action:"));
        state = BattleState.ActionSelection;
        dialogBox.EnableActionSelector(true);
    }

    void OpenPartyScreen()
    {
        state = BattleState.PartyScreen;
        partyScreen.SetPartyData(playerParty.Pokemons);
        partyScreen.gameObject.SetActive(true);
    }

    void MoveSelection()
    {
        state = BattleState.MoveSelection;
        dialogBox.EnableDialogText(false);
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableMoveSelector(true);
    }

    IEnumerator PlayerMove()
    {
        state = BattleState.PerformMove;

        yield return RunMove(playerUnit, enemyUnit, CurrentMove);

        //If the battle state was not chnaged by RunMove, then go to the next step
        if(state == BattleState.PerformMove)
            StartCoroutine(EnemyMove());
    }

    IEnumerator EnemyMove()
    {
        state = BattleState.PerformMove;

        int moveNum = enemyUnit.Pokemon.GetRandomMoveNumber();

        yield return RunMove(enemyUnit, playerUnit, moveNum);

        //If the battle stae was not changed by RunMove, then go to next step
        if (state == BattleState.PerformMove)
            ActionSelection();
    }

    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, int moveNum)
    {

        //Set the proper move and reduce the PP of the move by 1
        var move = sourceUnit.Pokemon.Moves[moveNum];
        sourceUnit.Pokemon.Moves[moveNum].PP--;

        //Tell the player which move is being used
        yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name} used {move.Base.Name}!");

        //Play the hit and attack animations.

        int i = 1;
        sourceUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(0.5f);

        if(move.Base.Category == MoveCategory.Status)
        {
            var effects = move.Base.Effects;
            if (effects.Boosts != null)
            {
                if (move.Base.Target == MoveTarget.Self)
                    sourceUnit.Pokemon.ApplyBoosts(effects.Boosts);
                else
                    targetUnit.Pokemon.ApplyBoosts(effects.Boosts);
            }
        }
        else
        {
            targetUnit.PlayHitAnimation();

            //Apply the damage and update the HP bar
            var damageDetails = targetUnit.Pokemon.TakeDamage(move, sourceUnit.Pokemon);
            yield return targetUnit.HUD.UpdateHP();
            yield return ShowDamageDetails(damageDetails);
        }

        if (targetUnit.Pokemon.HP <= 0)
        {
            //Inform the player that the target unit fainted
            targetUnit.PlayFaintAnimation();
            yield return dialogBox.TypeDialog($"{targetUnit.Pokemon.Base.Name} fainted!");
            yield return new WaitForSeconds(2f);

            CheckForBattleOver(targetUnit);
        }
    }

    void BattleOver(bool won)
    {
        state = BattleState.BattleOver;
        OnBattleOver(won);
    }

    void CheckForBattleOver(BattleUnit faintedUnit)
    {
        if (faintedUnit.IsPlayerUnit)
        {
            var nextPokemon = playerParty.GetHealthyPokemon();
            if (nextPokemon != null)
            {
                OpenPartyScreen();
            }
            else
                BattleOver(false);
        }
        else
            BattleOver(true);
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
        else if (state == BattleState.PartyScreen)
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
                state = BattleState.PartyScreen;
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
            ActionSelection();
        }
    }

    void HandlePartySelection()
    {
        dialogBox.EnableActionSelector(false);
        if (!pokemonMoveConfirm)
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
                ActionSelection();
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
        dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);
        yield return dialogBox.TypeDialog($"Go, {newPokemon.Base.Name}!");

        StartCoroutine(EnemyMove());
    }
}