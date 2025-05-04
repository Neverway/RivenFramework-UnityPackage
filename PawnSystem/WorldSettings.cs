//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose:
// Notes:
//
//=============================================================================

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RivenFramework;
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
    public int worldKillVolumeDistance = 32767;
    [Tooltip("When an actor falls beneath this height, they will be despawned")]
    public int worldKillHeightDistance = -100;


    //=-----------------=
    // Private Variables
    //=-----------------=


    //=-----------------=
    // Reference Variables
    //=-----------------=
    private GI_PawnManager pawnManager;


    //=-----------------=
    // Mono Functions
    //=-----------------=
    private void Start()
    {
        // Destroy actors with duplicate uuid's
        CheckForDuplicateActors();
        
        // Begin kill volume checker
        InvokeRepeating(nameof(CheckKillVolume), 0, 2);
    }

    private void Update()
    {
        if (GetPawnManager() is false)
        {
            return;
        }

        if (!pawnManager.localPlayerCharacter)
        {
            SpawnPlayerCharacter();
        }
    }

    private void OnDrawGizmos()
    {
        if (debugShowKillZones is false) return;
        Gizmos.color = Color.red;
        if (disableWorldKillVolume is false) Gizmos.DrawCube(new Vector3(0, 0, 0), new Vector3(worldKillVolumeDistance * 0.5f, worldKillVolumeDistance * 0.5f, worldKillVolumeDistance * 0.5f));
        if (disableWorldKillHeight is false) Gizmos.DrawCube(new Vector3(0, worldKillHeightDistance, 0), new Vector3(1000, 1, 1000));
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

    private void CheckKillVolume()
    {
        if (disableWorldKillVolume && disableWorldKillHeight) return;
        foreach (var actor in FindObjectsOfType<Actor>())
        {
            if (disableWorldKillVolume is false)
            {
                var distanceToEntity = Vector3.Distance(actor.transform.position, new Vector3(0, 0, 0));
                if (distanceToEntity >= worldKillVolumeDistance || distanceToEntity <= (worldKillVolumeDistance * -1))
                {
                    Destroy(actor.gameObject);
                }
            }

            if (disableWorldKillHeight is false)
            {
                if (actor.transform.position.y <= worldKillHeightDistance)
                {
                    Destroy(actor.gameObject);
                }
            }
        }
    }
    
    public static Transform GetPlayerStartPoint(bool _usePlayerStart = true)
    {
        if (_usePlayerStart is false) return null;
        var allPossibleStartPoints = FindObjectsOfType<PlayerStart>();
        var allValidStartPoints = allPossibleStartPoints.Where(_possibleStartPoint => _possibleStartPoint.playerStartFilter == "").ToList();

        if (allValidStartPoints.Count == 0) return null;
        var random = Random.Range(0, allValidStartPoints.Count);
        return allValidStartPoints[random].transform;
    }
    public void SpawnPlayerCharacter()
    {
        var startPoint = GetPlayerStartPoint();

        var classToInstantiate = pawnManager.defaultPawn;
        if (playerPawnOverride) classToInstantiate = playerPawnOverride;

        // Determine the game mode to use - either the override or the default.
        //var gameMode = pawnManager.defaultGamemode;
        //if (pawnManager is not null) gameMode = pawnManager;
        // Choose the class type based on whether starting as a spectator.
        //var classToInstantiate = gameMode.gamemodePawns[startingGameMode];
        // Determine the spawn position and rotation - use default if startPoint is null.
        Vector3 spawnPosition = startPoint ? startPoint.position : Vector3.zero;
        Quaternion spawnRotation = startPoint ? startPoint.rotation : Quaternion.identity;
        // Instantiate and assign the local player character
        pawnManager.localPlayerCharacter = Instantiate(classToInstantiate, spawnPosition, spawnRotation);
    }

    private bool GetPawnManager()
    {
        if (pawnManager is null)
        {
            pawnManager = FindObjectOfType<GI_PawnManager>();
            if (pawnManager is null)
            {
                print("ERROR");
                return false;
            }
        }

        return true;
    }

    //=-----------------=
    // External Functions
    //=-----------------=
}
