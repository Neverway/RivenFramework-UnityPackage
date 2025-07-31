using System;
using UnityEngine;

public class TestComponent : MonoBehaviour
{
    public LogicInput<bool> someBoolInput;

    [FetchComponent] public Actor[] actorReference;
    [FetchComponent] public Rigidbody rigidBodyReference;
    [FetchComponent] public Collider colliderReference;
}