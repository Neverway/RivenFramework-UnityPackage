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
public class PlayerStart : MonoBehaviour
{
    //=-----------------=
    // Public Variables
    //=-----------------=
    [Tooltip("")]
    public string playerStartFilter;
    [Tooltip("If enabled, draw the dummy model to get a sense of scale of the player")]
    public bool debugShowMesh;
    [Tooltip("Colorizes the debug player mesh, mostly used for identifying team spawns")]
    public Color debugPlayerStartColor = new Color(0, 0.5f, 1, 0.5f);


    //=-----------------=
    // Private Variables
    //=-----------------=


    //=-----------------=
    // Reference Variables
    //=-----------------=


    //=-----------------=
    // Mono Functions
    //=-----------------=
    void OnDrawGizmos()
    {
        Gizmos.color = debugPlayerStartColor;
        var fixedGizmoRotation = transform.rotation * Quaternion.AngleAxis(180, Vector3.up) * Quaternion.AngleAxis(-90, Vector3.right);
        if (debugShowMesh)
        {
            Gizmos.DrawMesh(
                Resources.Load<Mesh>("Models/DevCharacter"), 
                transform.position + (transform.up * -0.1f), 
                fixedGizmoRotation, 
                transform.localScale * 100);
        }

        Gizmos.DrawIcon(gameObject.transform.position, "player_start");
    }
    
    
    //=-----------------=
    // Internal Functions
    //=-----------------=


    //=-----------------=
    // External Functions
    //=-----------------=
}
}
