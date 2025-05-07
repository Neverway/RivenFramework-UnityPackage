//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose:
// Notes:
//
//=============================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PawnStats : ICloneable
{
    [Header("Health")]
    public float health = 100f;
    public float invulnerabilityTime = 1f;

    [Header("Teaming")] 
    public string team = "";
    public List<string> alliedTeams = new List<string>();
    public List<string> opposedTeams = new List<string>();
    
    public abstract object Clone();
}
