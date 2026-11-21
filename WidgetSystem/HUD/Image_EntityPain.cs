//==========================================( Neverway 2027 )=========================================================//
// Author
//  Liz M.
//
// Contributors
//
//
// Description
//
//====================================================================================================================//

using System.Collections;
using DG.Tweening;
using RivenFramework;
using UnityEngine;
using UnityEngine.UI;

namespace RivenFramework
{
    public class Image_EntityPain : MonoBehaviour
    {
        #region====================================( Variables )======================================================//
        #region----------------( Inspector )--------------------------------------------------------------------------//
            public Pawn targetPawn;
            public bool findPossessedPawn;
            public float fadeSpeed=1;
            public float startAlpha=1;
        #endregion

        #region----------------( Reference )--------------------------------------------------------------------------//
            private Image image;
            private Animator animator;
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
            private bool isInPain; // Used to keep track of if we are currently fading the image in or out (Fadeout: Underground reference?)
            private bool initialized;
        #endregion
        #endregion


        #region====================================( Functions )======================================================//
        #region----------------( Inherited )--------------------------------------------------------------------------//
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
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
            private Pawn FindPossessedPawn()
            {
                if (GameInstance.Get<GI_PawnManager>().localPlayerCharacter) return GameInstance.Get<GI_PawnManager>().localPlayerCharacter.GetComponent<Pawn>();
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
        #endregion
        #endregion
    }
}
