//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose:
// Notes:
//
//=============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RivenFramework
{
public class GI_MusicManager : MonoBehaviour
{
    //=-----------------=
    // Public Variables
    //=-----------------=
    public AudioSource primaryTrackPlayer;
    public AudioSource secondaryTrackPlayer;


    //=-----------------=
    // Private Variables
    //=-----------------=


    //=-----------------=
    // Reference Variables
    //=-----------------=


    //=-----------------=
    // Mono Functions
    //=-----------------=
    

    //=-----------------=
    // Internal Functions
    //=-----------------=


    //=-----------------=
    // External Functions
    //=-----------------=
    /// <summary>
    /// Smoothly fades from the current track to a target track
    /// </summary>
    /// <param name="_musicTrack">The music track to change to</param>
    /// <param name="_cossfadeDuration">The amount of time it takes to fade to the target track</param>
    public void CrossFadeToTrack(AudioClip _musicTrack, float _cossfadeDuration)
    {
        
    }

    /// <summary>
    /// Immediately changes the current music track
    /// </summary>
    /// <param name="_musicTrack">The music track to change to</param>
    public void SwitchToTrack(AudioClip _musicTrack)
    {
        
    }
}
}
