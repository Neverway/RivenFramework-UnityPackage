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

using System;
using System.Collections;
using System.Collections.Generic;
using RivenFramework;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RivenFramework
{
    public class VolumeLevelStream : Volume
    {
        #region====================================( Variables )======================================================//
        #region----------------( Inspector )--------------------------------------------------------------------------//
            [Tooltip("This is the offset that will be applied to objects within this volume when the level changes")]
            [SerializeField] private Vector3 exitPositionOffset;
            [SerializeField] private Vector3 exitRotationOffset;
            [SerializeField] private bool debugDrawExitZone;
            [Header("Linking")]
            [Tooltip("Drag the corresponding volume from the other loaded scene here, then use 'Align To Link Target' to auto-solve the offset/rotation!")]
            [SerializeField] private Transform linkTarget;
            [Tooltip("This is the empty game object that streamed actors are stored in, (to save them from being destroyed on map changes)")]
            [SerializeField] private VolumeLevelStream_Container streamContainer;
        #endregion

        #region----------------( Reference )--------------------------------------------------------------------------//
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
        #endregion
        #endregion


        #region====================================( Functions )======================================================//
        #region----------------( Inherited )--------------------------------------------------------------------------//
            protected override void OnTriggerEnter(Collider _other)
            {
                if (_other.CompareTag("Pawn"))
                {
                    var targetEntity = _other.gameObject.GetComponentInParent<Pawn>();
                    targetEntity.transform.SetParent(streamContainer.transform);
                }
                if (_other.CompareTag("PhysProp"))
                {
                    var targetEntity = _other.gameObject.GetComponentInParent<Actor>();
                    targetEntity.transform.SetParent(streamContainer.transform);
                }
            }

            protected override void OnTriggerExit(Collider _other)
            {
                if (_other.CompareTag("Pawn"))
                {
                    var targetEntity = _other.gameObject.GetComponentInParent<Pawn>();
                    if (targetEntity.transform.parent == streamContainer.transform)
                    {
                        var anchor = SceneManager.GetActiveScene().GetRootGameObjects()[0];
                        targetEntity.transform.SetParent(anchor.transform);
                        targetEntity.transform.SetParent(null);
                    }
                }
                if (_other.CompareTag("PhysProp"))
                {
                    var targetEntity = _other.gameObject.GetComponentInParent<Actor>();
                    if (targetEntity.transform.parent == streamContainer.transform)
                    {
                        var anchor = SceneManager.GetActiveScene().GetRootGameObjects()[0];
                        targetEntity.transform.SetParent(anchor.transform);
                        targetEntity.transform.SetParent(null);
                    }
                }
            }

            private void OnDrawGizmos()
            {
                if (!debugDrawExitZone) return;

                // Compose the exit transform relative to this volume's own orientation
                Quaternion exitRotation = transform.rotation * Quaternion.Euler(exitRotationOffset);
                Vector3 exitPosition = transform.position + (transform.rotation * exitPositionOffset);

                Matrix4x4 oldMatrix = Gizmos.matrix;
                Gizmos.matrix = Matrix4x4.TRS(exitPosition, exitRotation, transform.localScale);

                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(Vector3.zero, Vector3.one);

                // Facing direction of the exit rotation
                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(Vector3.zero, Vector3.forward * 1.5f);

                Gizmos.matrix = oldMatrix;
            }
        #endregion

        #region----------------( External )---------------------------------------------------------------------------//
            public void PrepareForLoad()
            {
                streamContainer.exitPositionOffset = exitPositionOffset;
                streamContainer.exitRotationOffset = exitRotationOffset;
                streamContainer.parentStreamVolume = gameObject;
                streamContainer.PrepareForLoad();
            }
        #endregion

        #region----------------( Internal )---------------------------------------------------------------------------//
            [ContextMenu("Align To Link Target")]
            private void AlignToLinkTarget()
            {
                if (linkTarget == null)
                {
                    Debug.LogWarning($"[{gameObject.name}] No linkTarget assigned, can't align exit offsets", this);
                    return;
                }

            #if UNITY_EDITOR
                UnityEditor.Undo.RecordObject(this, "Align Exit Offset To Link Target");
            #endif

                exitPositionOffset = Quaternion.Inverse(transform.rotation) * (linkTarget.position - transform.position);

                Quaternion relativeRotation = Quaternion.Inverse(transform.rotation) * linkTarget.rotation;
                exitRotationOffset = relativeRotation.eulerAngles;

            #if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
            #endif

                Debug.Log($"[{gameObject.name}] Aligned exit offset to '{linkTarget.name}' Position: {exitPositionOffset}, Rotation: {exitRotationOffset}", this);
            }
        #endregion
        #endregion
    }
}
