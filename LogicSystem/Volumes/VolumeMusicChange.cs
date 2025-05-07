//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose:
// Notes:
//
//=============================================================================

using System.Collections;
using System.Collections.Generic;
using RivenFramework;
using UnityEngine;

public class VolumeMusicChange : Volume
{
    //=-----------------=
    // Public Variables
    //=-----------------=
    public AudioClip musicTrack;
    public float transitionTime = 1;


    //=-----------------=
    // Private Variables
    //=-----------------=


    //=-----------------=
    // Reference Variables
    //=-----------------=
    private GI_MusicManager musicManager;


    //=-----------------=
    // Mono Functions
    //=-----------------=
    private new void OnTriggerEnter(Collider _other)
    {
        base.OnTriggerEnter(_other); // Call the base class method
        if (!GetPlayerInTrigger())
        {
            return;
        }
        if (_other.gameObject == GetPlayerInTrigger().gameObject)
        {
                if (!musicManager) musicManager = FindObjectOfType<GI_MusicManager>();
                musicManager.CrossFadeToTrack(musicTrack, transitionTime);
        }
    }

    //=-----------------=
    // Internal Functions
    //=-----------------=


    //=-----------------=
    // External Functions
    //=-----------------=
}
