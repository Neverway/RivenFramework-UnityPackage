//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose:
// Notes:
//
//=============================================================================

using System.Collections;
using DG.Tweening;
using RivenFramework;
using UnityEngine;
using UnityEngine.UI;

public class Image_EntityPain : MonoBehaviour
{
    //=-----------------=
    // Public Variables
    //=-----------------=
    public Pawn targetPawn;
    public bool findPossessedPawn;
    public float fadeSpeed=1;
    public float startAlpha=1;


    //=-----------------=
    // Private Variables
    //=-----------------=
    private bool isInPain; // Used to keep track of if we are currently fading the image in or out (Fadeout: Underground reference?)
    private bool initialized;


    //=-----------------=
    // Reference Variables
    //=-----------------=
    private Image image;
    private Animator animator;


    //=-----------------=
    // Mono Functions
    //=-----------------=
    private void Start()
    {
        image = GetComponent<Image>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (findPossessedPawn && !targetPawn)
        {
            targetPawn = FindPossessedPawn();
        }
        if (targetPawn && !initialized)
        {
            initialized = true;
            targetPawn.OnPawnHurt -= OnHurt;
            targetPawn.OnPawnHurt += OnHurt;
        }
        else if (!targetPawn)
        {
            var color = image.color;
            image.color = new Color(color.r, color.g, color.b, 0);
        }
    }

    private void OnDestroy()
    {
        if (!targetPawn) return;
        targetPawn.OnPawnHurt -= OnHurt;
    }

    //=-----------------=
    // Internal Functions
    //=-----------------=
    private Pawn FindPossessedPawn()
    {
        if (FindObjectOfType<GI_PawnManager>().localPlayerCharacter) return FindObjectOfType<GI_PawnManager>().localPlayerCharacter.GetComponent<Pawn>();
        return null;
    }

    private IEnumerator FadeInPain()
    {
        isInPain = true;
        DOVirtual.Color(
            new Color(image.color.r, image.color.g, image.color.b, startAlpha), 
            new Color(image.color.r, image.color.g, image.color.b, 0),
            fadeSpeed,
            (value) =>
            {
                image.color = value;
            });
        yield return new WaitForSeconds(targetPawn.currentStats.invulnerabilityTime);
        isInPain = false;
    }

    private void OnHurt()
    {
        if (!isInPain)
        {
            StartCoroutine(FadeInPain());
        }
    }
}
