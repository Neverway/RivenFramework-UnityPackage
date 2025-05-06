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
    public float health = 100f;
    public float invulnerabilityTime = 1f;
    public abstract object Clone();
}
