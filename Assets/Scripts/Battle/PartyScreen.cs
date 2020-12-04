using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyScreen : MonoBehaviour
{
    
    PartyMemberUI[] memberSlots;
    [SerializeField] Text messageText;
    [SerializeField] PokemonConfirmBox pokemonConfirmBox;

    public PokemonConfirmBox PokemonConfirmBox { get { return pokemonConfirmBox; } }

    public void Init()
    {
        memberSlots = GetComponentsInChildren<PartyMemberUI>();
    }

    public void SetPartyData(List<Pokemon> pokemons)
    {
        for (int i = 0; i < memberSlots.Length; i++)
        {
            if (i < pokemons.Count)
                memberSlots[i].SetData(pokemons[i]);
            else
                memberSlots[i].gameObject.SetActive(false);

            if (i == 0)
                memberSlots[i].setFirstMemberSlot();
        }

        messageText.text = "Choose a Pokemon";
    }

    public void UpdatePokemonSelection(int currentPokemon)
    {
        for (int i = 0; i < memberSlots.Length - 1; i++) {
            if (memberSlots[i].Selected == true)
            {
                memberSlots[i].setSelected(false, i);
            }
        }
        memberSlots[currentPokemon].setSelected(true, currentPokemon);
    }

    public void SetMessageText(string message)
    {
        messageText.text = message;
    }

    public void SetConfirmActive(bool active)
    {
        pokemonConfirmBox.gameObject.SetActive(active);
    }
}
