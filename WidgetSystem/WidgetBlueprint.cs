using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WidgetBlueprint : MonoBehaviour
{
    //Define whether or not the widget pauses the pawns in the scene
    public abstract bool PausesPawns();

    protected void OnEnable()
    {
        if (PausesPawns())
            foreach (var pawn in FindObjectsOfType<Pawn>())
                pawn.Pause(this);
    }

    protected void OnDisable()
    {
        if (PausesPawns())
            foreach (var pawn in FindObjectsOfType<Pawn>())
                pawn.Unpause(this);
    }
}
