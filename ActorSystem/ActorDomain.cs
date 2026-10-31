using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains actors, and passes any ActorFunction call on it down to 
/// </summary>
public class ActorDomain : Actor
{
    public HashSet<Actor> actors = new HashSet<Actor>();

    public static ActorDomain CreateNewGlobalDomain()
    {
        ActorDomain domain = CreateNewInScene();
        DontDestroyOnLoad(domain.gameObject);
        return domain;
    }
    public static ActorDomain CreateNewInScene()
    {
        ActorDomain domain = new GameObject("Actor Domain").AddComponent<ActorDomain>();
        domain.NewGUID();

        return domain;
    }

    public void RegisterActor(Actor actor) => actors.Add(actor);
    public void UnregisterActor(Actor actor) => actors.Remove(actor);
}
