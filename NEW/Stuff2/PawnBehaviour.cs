//==========================================( Neverway 2026 )=========================================================//
// Author
//  Errynei
//
// Contributors
//  Liz M.
//
//====================================================================================================================//

using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;

/// <summary>
/// A behaviour a pawn can have, like having health, 3d movement, topdown movement, etc.
/// A pawn has a list of pawn behaviours and each behaviour or controller may control other pawn behaviours
/// </summary>
[Serializable]
public abstract class PawnBehaviour : IActorFunctions
{
    #region========================================( Variables )======================================================//
    /*-----[ Inspector Variables ]------------------------------------------------------------------------------------*/
    public PawnV2 pawn;


    /*-----[ External Variables ]-------------------------------------------------------------------------------------*/


    /*-----[ Internal Variables ]-------------------------------------------------------------------------------------*/



    /*-----[ Reference Variables ]------------------------------------------------------------------------------------*/



    #endregion


    #region=======================================( Functions )======================================================= //

    /*-----[ Mono Functions ]-----------------------------------------------------------------------------------------*/
    

    /*-----[ Internal Functions ]-------------------------------------------------------------------------------------*/
    public virtual void OnActorCreated() { }

    public virtual void OnActorDestroyed() { }

    public virtual void OnActorEnabled() { }

    public virtual void OnActorDisabled() { }

    public virtual void OnActorUpdate() { }

    public virtual void OnActorFixedUpdate() { }


    /*-----[ External Functions ]-------------------------------------------------------------------------------------*/


    #endregion
}

public interface IUsePawnBehaviour<T> where T : PawnBehaviour
{
    public void AssignPawnBehaviour(T _behaviour);
}

/// <summary>
/// A util that allows for nesting behaviours within other behaviours
/// </summary>
public static class PawnBehaviourUtils
{
    private static readonly ConcurrentDictionary<(Type Handler, Type UsingType), Action<object, object>?> _cache = new();

    public static bool TryAssignPawnBehaviour(object handler, PawnBehaviour pawnBehaviour)
    {
        var key = (handler.GetType(), pawnBehaviour.GetType());

        var invoker = _cache.GetOrAdd(key, k =>
        {
            var interfaceType = typeof(IUsePawnBehaviour<>).MakeGenericType(k.UsingType);

            if (!interfaceType.IsInstanceOfType(handler))
                return null;

            var method = interfaceType.GetMethod(nameof(IUsePawnBehaviour<PawnBehaviour>.AssignPawnBehaviour))!;

            var handlerParam = Expression.Parameter(typeof(object), "handler");
            var usingTypeParam = Expression.Parameter(typeof(object), "usingType");

            var call = Expression.Call(
                Expression.Convert(handlerParam, k.Handler),
                method,
                Expression.Convert(usingTypeParam, k.UsingType));

            return Expression.Lambda<Action<object, object>>(call, handlerParam, usingTypeParam).Compile();
        });

        if (invoker is null)
            return false;

        invoker(handler, pawnBehaviour);
        return true;
    }

    public static void ControlPawnWithTargetPawnController(PawnV2 _pawn, PawnController _controller)
    {
        RemoveControlFromPawn(_pawn);
        _pawn.CurrentController = _controller;
        _controller.ControlledPawn = _pawn;
        
        foreach (var otherBehaviour in _pawn.Behaviours)
        {
            PawnBehaviourUtils.TryAssignPawnBehaviour(_pawn.CurrentController, otherBehaviour);
        }
        
        _controller.OnStartControl();
    }

    public static void RemoveControlFromPawn(PawnV2 _pawn)
    {
        if (_pawn == null) return;
        if (!_pawn.IsControlled) return;
        _pawn.CurrentController.ControlledPawn = null;
        _pawn.CurrentController.OnStopControl();
        _pawn.CurrentController = null;
    }
}