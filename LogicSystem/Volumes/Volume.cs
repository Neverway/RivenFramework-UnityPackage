//==========================================( Neverway 2027 )=========================================================//
// Author
//  Liz M.
//
// Contributors
//  Errynei
//  Connorses
//  Soulex
//
// Description
//  Handles the filtering, entry, and exiting of pawns and physics props from a Logic Volume
//====================================================================================================================//

using System.Collections.Generic;
using UnityEngine;

namespace RivenFramework
{
    public class Volume : MonoBehaviour
    {
        #region====================================( Variables )======================================================//
        #region----------------( Inspector )--------------------------------------------------------------------------//
            [Header("Object Filtering")]
            [Tooltip("Decides which actors this volume applies to. Actors that don't pass are ignored completely. If the list is empty, the volume applies to everyone. (For example, to keep a pain volume from hurting its own team, add a Team Relation filter set to Opposed and Neutral)")]
            public ActorFilterSet affectedActors = new ActorFilterSet();
            [Tooltip("If enabled, physics props that are being held won't be effected by the volume (This is used for things like wind boxes)")]
            public bool ignoreHeldObjects = true;
            [Tooltip("If enabled, will remove objects from the list of objects in the volume when that object becomes disabled")]
            public bool disabledObjectsExitVolume = true;
            [System.Flags] public enum VolumeTargets { Pawns = 1, Props = 2 }
            [Tooltip("Which kinds of objects this volume reacts to")]
            public VolumeTargets detects = VolumeTargets.Pawns | VolumeTargets.Props;
        #endregion

        #region----------------( Reference )--------------------------------------------------------------------------//
            private GI_PawnManager pawnManager;
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
            [Header("Debugging Stuff")]
            public List<Pawn> pawnsInTrigger = new List<Pawn>();
            public List<GameObject> propsInTrigger = new List<GameObject>();
        #endregion
        #endregion


        #region====================================( Functions )======================================================//
        #region----------------( Inherited )--------------------------------------------------------------------------//
            protected virtual void Update()
            {
                CheckPawnsInTrigger();
                CheckPropsInTrigger();
            }

            protected virtual void OnTriggerEnter(Collider _other)
            {
                var actor = _other.GetComponentInParent<Actor>();
                if (actor == null || !PassesActorFilter(actor)) return;

                if (actor is Pawn pawn)
                {
                    if ((detects & VolumeTargets.Pawns) != 0) AddPawnToVolume(pawn);
                    return;
                }

                if ((detects & VolumeTargets.Props) == 0) return;
                var holdable = _other.GetComponentInParent<IHoldable>();
                if (ignoreHeldObjects && holdable is { IsHeld: true }) return;
                AddPropToVolume(actor.gameObject);
            }

            protected virtual void OnTriggerExit(Collider _other)
            {
                var actor = _other.GetComponentInParent<Actor>();
                if (actor == null) return;
                if (actor is Pawn pawn) RemovePawnFromVolume(pawn);
                else RemovePropFromVolume(actor.gameObject);
            }

            public virtual void OnDisable()
            {
                pawnsInTrigger.Clear();
                propsInTrigger.Clear();
            }
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
            /// <summary>
            /// Checks an actor against this volume's actor filter
            /// </summary>
            /// <param name="_actor">The actor that touched the volume</param>
            /// <returns>True if the volume should apply to this actor</returns>
            protected bool PassesActorFilter(Actor _actor)
            {
                return affectedActors == null || affectedActors.Passes(_actor);
            }

            protected virtual bool AddPawnToVolume(Pawn _pawn)
            {
                // Ignore null
                if (_pawn == null) return false;

                // Add to list if it's not already present
                if (pawnsInTrigger.Contains(_pawn) is false)
                {
                    pawnsInTrigger.Add(_pawn);
                    OnObjectsInVolumeUpdated(VolumeUpdateType.PawnAdded);
                    return true;
                }

                // Default return
                return false;
            }

            protected virtual bool AddPropToVolume(GameObject _prop)
            {
                // Ignore null
                if (_prop == null) return false;

                // Add to list if it's not already present
                if (propsInTrigger.Contains(_prop) is false)
                {
                    propsInTrigger.Add(_prop);
                    OnObjectsInVolumeUpdated(VolumeUpdateType.PropAdded);
                    return true;
                }

                // Default return
                return false;
            }

            /// <summary>
            /// Trys to remove a specified pawn from the volume
            /// </summary>
            /// <param name="_pawn">The target pawn to try to remove from this volume</param>
            /// <returns>Returns true if the pawn was successfully found and removed from the volume</returns>
            protected virtual bool RemovePawnFromVolume(Pawn _pawn)
            {
                // Ignore null
                if (_pawn == null) return false;

                // Remove from list if it's present
                if (pawnsInTrigger.Contains(_pawn) is true)
                {
                    pawnsInTrigger.Remove(_pawn);
                    OnObjectsInVolumeUpdated(VolumeUpdateType.PawnRemoved);
                    return true;
                }

                // Default return
                return false;
            }

            /// <summary>
            /// Trys to remove a specified prop from the volume
            /// </summary>
            /// <param name="_pawn">The target prop to try to remove from this volume</param>
            /// <returns>Returns true if the prop was successfully found and removed from the volume</returns>
            protected virtual bool RemovePropFromVolume(GameObject _prop)
            {
                // Ignore null
                if (_prop == null) return false;

                // Remove from list if it's present
                if (propsInTrigger.Contains(_prop) is true)
                {
                    propsInTrigger.Remove(_prop);
                    OnObjectsInVolumeUpdated(VolumeUpdateType.PropRemoved);
                    return true;
                }

                // Default return
                return false;
            }

            /// <summary>
            /// Cleans up any dead references to pawns that are no longer inside the volume
            /// </summary>
            protected virtual void CheckPawnsInTrigger()
            {
                for (int i = pawnsInTrigger.Count - 1; i >= 0; i--)
                {
                    var pawn = pawnsInTrigger[i];
                    if (pawn == null)
                    {
                        pawnsInTrigger.RemoveAt(i);
                        OnObjectsInVolumeUpdated(VolumeUpdateType.PawnRemoved);
                    }
                    else if (disabledObjectsExitVolume && !pawn.gameObject.activeInHierarchy)
                    {
                        RemovePawnFromVolume(pawn);
                    }
                }
            }

            /// <summary>
            /// Cleans up any dead references to props that are no longer inside the volume
            /// </summary>
            protected virtual void CheckPropsInTrigger()
            {
                for (int i = propsInTrigger.Count - 1; i >= 0; i--)
                {
                    var prop = propsInTrigger[i];
                    if (prop == null)
                    {
                        propsInTrigger.RemoveAt(i);
                        OnObjectsInVolumeUpdated(VolumeUpdateType.PropRemoved);
                    }
                    else if (disabledObjectsExitVolume && !prop.gameObject.activeInHierarchy)
                    {
                        RemovePropFromVolume(prop);
                    }
                }
            }

            /// <summary>
            /// Get the local player that's inside this trigger if they are present
            /// </summary>
            /// <returns>Returns the pawn component of the local player if they are in this volume, otherwise returns null</returns>
            protected Pawn GetPlayerInTrigger()
            {
                if (!GameInstance.TryGet(out GI_PawnManager pawnManager) || pawnManager.localPlayerCharacter == null) return null;
                foreach (var pawn in pawnsInTrigger)
                    if (pawn != null && pawn.gameObject == pawnManager.localPlayerCharacter) return pawn;
                return null;
            }

            /// <summary>
            /// Override this method in extended classes to react when any change to the volume contents occurs
            /// </summary>
            /// <param name="updateType">flags for what kind of update the volume was</param>
            // Example: To check if Props were updated:
            //      (updateType & VolumeUpdateType.PropAdded) != 0
            protected virtual void OnObjectsInVolumeUpdated(VolumeUpdateType updateType)
            {
            }

            /// <summary>
            /// Used to filter different types of updates from OnObjectsInVolumeUpdated callback
            /// </summary>
            [System.Flags]
            public enum VolumeUpdateType
            {
                NoUpdate = 0,
                PawnAdded = 1 << 0,
                PawnRemoved = 1 << 1,
                PropAdded = 1 << 2,
                PropRemoved = 1 << 3,

                AnythingAdded = PawnAdded | PropAdded,
                AnythingRemoved = PawnRemoved | PropRemoved,

                PawnUpdated = PawnAdded | PawnRemoved,
                PropUpdated = PropAdded | PropRemoved,

                AnyUpdate = PawnUpdated | PropUpdated
            }
        #endregion
        #endregion
    }
}
