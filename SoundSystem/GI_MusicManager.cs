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


    //=-----------------=
    // Private Variables
    //=-----------------=
    private bool inProgress;


    //=-----------------=
    // Reference Variables
    //=-----------------=


    //=-----------------=
    // Mono Functions
    //=-----------------=
    public AudioSource currentTrackPlayer;
    public AudioSource queuedTrackPlayer;
    public AudioClip currentTrack;
    public AudioClip queuedTrack;
    

    //=-----------------=
    // Internal Functions
    //=-----------------=
    private IEnumerator Crossfade(float _duration)
    {
        
        // Cross-fade the tracks
        var timer = 0f;
        while (timer < _duration)
        {
            timer += Time.deltaTime;
            var t = timer / _duration;

            currentTrackPlayer.volume = Mathf.Lerp(1f, 0f, t);
            queuedTrackPlayer.volume = Mathf.Lerp(0f, 1f, t);

            yield return null;
        }
        // Clean-up possible floating point issues
        currentTrackPlayer.volume = 0f;
        queuedTrackPlayer.volume = 1f;
            
        // Cross-fade is complete, now it's time to shift and clear the queue
        // Store the queue playback point
        var storedPlaybackPoint = queuedTrackPlayer.time;
        // Shift the queue to the current track
        currentTrackPlayer.clip = queuedTrackPlayer.clip;
        // Make sure the track swap doesn't affect where the audio left off
        currentTrackPlayer.Play();
        currentTrackPlayer.time = storedPlaybackPoint;
        // Clear the queue track
        queuedTrackPlayer.clip = null;
        
        // Flip-flop the audio track volumes
        currentTrackPlayer.volume = 1f;
        queuedTrackPlayer.volume = 0f;

        inProgress = false;
    }


    //=-----------------=
    // External Functions
    //=-----------------=
    /// <summary>
    /// Smoothly fades from the current track to a target track
    /// </summary>
    /// <param name="_musicTrack">The music track to change to</param>
    /// <param name="_cossfadeDuration">The amount of time it takes to fade to the target track</param>
    public void CrossFadeToTrack(AudioClip _musicTrack, float _cossfadeDuration, bool _playTrackFromStart=false)
    {
        if (inProgress) return;
        inProgress = true;
        // Set next track
        queuedTrack = _musicTrack;
        queuedTrackPlayer.clip = queuedTrack;
        queuedTrackPlayer.Play();
        if (_playTrackFromStart is false) queuedTrackPlayer.time = currentTrackPlayer.time;
        
        // Lerp volumes
        StartCoroutine(Crossfade(_cossfadeDuration));
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
