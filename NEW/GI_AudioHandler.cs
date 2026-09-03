//==========================================( Neverway 2026 )=========================================================//
// Author
//
//
// Contributors
//
//
//====================================================================================================================//

using System;
using RivenFramework;
using UnityEngine;

[Serializable]
[GIModuleColor(_color: GIModuleColors.Blue)]
public class GI_AudioHandler : GameInstanceModule
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/
    private bool transitionInProgress;
    

    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/
    [Tooltip("The audio source that plays the current music track")]
    public AudioSource currentTrackPlayer;
    [Tooltip("The audio source used to queue and crossfade to another music track")]
    public AudioSource queuedTrackPlayer;
    [Tooltip("The current music track that is playing on the currentTrackPlayer audio source")]
    public AudioClip currentTrack;
    [Tooltip("The queued music track that is playing on the queuedTrackPlayer audio source")]
    public AudioClip queuedTrack;


    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/


    #endregion
}
