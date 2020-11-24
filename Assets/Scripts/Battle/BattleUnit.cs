using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BattleUnit : MonoBehaviour
{
    [SerializeField] PokemonBase Base;
    [SerializeField] int level;
    [SerializeField] bool isShiny;
    [SerializeField] bool isPlayerUnit;
    [SerializeField] Image shinyParticles;
    [SerializeField] Gender gender;

    public Pokemon Pokemon { get; set; }

    Image image;
    Vector3 originalPos;
    Color originalColor;

    private void Awake()
    {
        image = GetComponent<Image>();
        originalPos = image.transform.localPosition;
        originalColor = image.color;
    }

    public void Setup()
    {
        Pokemon = new Pokemon(Base, level, isShiny, gender);

        if (isPlayerUnit)
            image.sprite = Pokemon.BackSprite;
        else
            image.sprite = Pokemon.FrontSprite;

        image.color = originalColor;

        PlayEnterAnimation();
    }

    public void PlayEnterAnimation()
    {
        if (isPlayerUnit)
            image.transform.localPosition = new Vector3(-500f, originalPos.y);
        else
            image.transform.localPosition = new Vector3(500f, originalPos.y);

        image.transform.DOLocalMoveX(originalPos.x, 1f);
    }

    public void PlayAttackAnimation()
    {
        var sequence = DOTween.Sequence();

        if(isPlayerUnit)
            sequence.Append(image.transform.DOLocalMoveX(originalPos.x + 50f, 0.25f));
        else
            sequence.Append(image.transform.DOLocalMoveX(originalPos.x + -50f, 0.25f));

        sequence.Append(image.transform.DOLocalMoveX(originalPos.x, 0.25f));
    }

    public void PlayHitAnimation()
    {
        var Sequence = DOTween.Sequence();

        Sequence.Append(image.DOColor(Color.gray, 0.1f));
        Sequence.Append(image.DOColor(originalColor, 0.1f));
    }

    public void PlayFaintAnimation()
    {
        var Sequence = DOTween.Sequence();
        Sequence.Append(image.transform.DOLocalMoveY(originalPos.y - 150f, 0.5f));
        Sequence.Join(image.DOFade(0f, 0.5f));
    }

    public void PlayShinyAnimation()
    {
        if(isShiny)
        {
            var Sequence = DOTween.Sequence();
            Sequence.Append(shinyParticles.DOFade(1f, 0.75f));
            Sequence.Append(shinyParticles.DOFade(0f, 0.75f));
        }
    }
}
