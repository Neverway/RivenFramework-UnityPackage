//==========================================( Neverway 2027 )=========================================================//
// Author
//  Liz M.
//
// Contributors
//
//
// Description
//  Plays audio clips for button events like hovering or selecting
//====================================================================================================================//

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RivenFramework
{
    public class Button_Sounds : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
    {
        #region====================================( Variables )======================================================//
        #region----------------( Inspector )--------------------------------------------------------------------------//
            [SerializeField] private AudioClip hover;
            [SerializeField] private AudioClip select;
        #endregion

        #region----------------( Reference )--------------------------------------------------------------------------//
            private Button button;
            private AudioSource audioSource;
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
        #endregion
        #endregion


        #region====================================( Functions )======================================================//
        #region----------------( Inherited )--------------------------------------------------------------------------//
            private void Start()
            {
                button = GetComponent<Button>();
                audioSource = GetComponent<AudioSource>();
            }

            public void OnPointerEnter(PointerEventData _pointerEventData)
            {
                //Audio_FMODAudioManager.PlayOneShot(Audio_FMODEvents.Instance.hover);
                audioSource.clip = hover;
                audioSource.Play();
            }

            public void OnPointerDown(PointerEventData _pointerEventData)
            {
                //Audio_FMODAudioManager.PlayOneShot(Audio_FMODEvents.Instance.select);
                audioSource.clip = select;
                audioSource.Play();
            }
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
        #endregion
        #endregion
    }
}
