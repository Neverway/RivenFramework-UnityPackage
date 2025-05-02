//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose:
// Notes:
//
//=============================================================================

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorldSettings : MonoBehaviour
{
    //=-----------------=
    // Public Variables
    //=-----------------=
    [Tooltip("Set this to assign the player pawn that will be spawned when this map loads")]
    public GameObject playerPawnOverride;
    public bool debugShowKillZones;
    [Tooltip("If true, actors will not be destroyed when going further than the world kill volume distance")]
    public bool disableWorldKillVolume;
    [Tooltip("If true, actors will not be destroyed when falling to far under the world kill height distance")]
    public bool disableWorldKillHeight;

    [Tooltip("When an actor goes this distance away from the center of the map, they will be despawned")]
    public int worldKillVolumeDistance;
    [Tooltip("When an actor falls beneath this height, they will be despawned")]
    public int worldKillHeightDistance;


    //=-----------------=
    // Private Variables
    //=-----------------=


    //=-----------------=
    // Reference Variables
    //=-----------------=


    //=-----------------=
    // Mono Functions
    //=-----------------=
    private void Start()
    {
        // Destroy actors with duplicate uuid's
        CheckForDuplicateActors();
    }

    private void Update()
    {
    
    }

    //=-----------------=
    // Internal Functions
    //=-----------------=
    private void CheckForDuplicateActors()
    {
        Actor[] allActors = FindObjectsOfType<Actor>();
        Dictionary<string, Actor> uuidMap = new Dictionary<string, Actor>();

        foreach (Actor actor in allActors)
        {
            string uuid = actor.uniqueId;
            
            // Actor was not given a UUID, skip them
            if (actor.uniqueId == "") continue;
            
            // UUID is already in use, destory this object
            if (uuidMap.ContainsKey(uuid))
            {
                Debug.LogWarning($"Duplicate actor with UUID {actor.uniqueId} was found, destroying duplicates! If you are backtracking through level, you can ignore this, otherwise check {actor.displayName}'s on the map for conflicting UUIDs");
                Destroy(actor.gameObject);
            }
            // UUID is not in list yet, add it
            else uuidMap[uuid] = actor;
        }
    }


    //=-----------------=
    // External Functions
    //=-----------------=
}
