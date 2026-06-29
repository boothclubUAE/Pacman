using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ColliderEventTrigger : MonoBehaviour
{
    [Title("Events")]
    [SerializeField][BeginGroup] public UnityEvent<TriggerState, Collider> onTrigger = new UnityEvent<TriggerState, Collider>();
    [SerializeField][EndGroup] public UnityEvent<TriggerState, Collision> onCollision = new UnityEvent<TriggerState, Collision>();
    [Line(5), SpaceArea]

    [Title("Trigger Events")]
    [SerializeField][BeginGroup] public UnityEvent<Collider> onTriggerEnter = new UnityEvent<Collider>();
    [SerializeField] public UnityEvent<Collider> onTriggerStay = new UnityEvent<Collider>();
    [SerializeField][EndGroup] public UnityEvent<Collider> onTriggerExit = new UnityEvent<Collider>();
    [Line(5), SpaceArea]

    [Title("Collision Events")]
    [SerializeField][BeginGroup] public UnityEvent<Collision> onCollisionEnter = new UnityEvent<Collision>();
    [SerializeField] public UnityEvent<Collision> onCollisionStay = new UnityEvent<Collision>();
    [SerializeField][EndGroup] public UnityEvent<Collision> onCollisionExit = new UnityEvent<Collision>();

    private void OnTriggerEnter(Collider other)
    {
        onTrigger?.Invoke(TriggerState.Enter, other);
        onTriggerEnter?.Invoke(other);
    }

    private void OnTriggerStay(Collider other)
    {
        onTrigger?.Invoke(TriggerState.Stay, other);
        onTriggerStay?.Invoke(other);
    }

    private void OnTriggerExit(Collider other)
    {
        onTrigger?.Invoke(TriggerState.Exit, other);
        onTriggerExit?.Invoke(other);
    }

    private void OnCollisionEnter(Collision collision)
    {
        onCollision?.Invoke(TriggerState.Enter, collision);
        onCollisionEnter?.Invoke(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        onCollision?.Invoke(TriggerState.Stay, collision);
        onCollisionStay?.Invoke(collision);
    }

    private void OnCollisionExit(Collision collision)
    {
        onCollision?.Invoke(TriggerState.Exit, collision);
        onCollisionExit?.Invoke(collision);
    }

    public enum TriggerState
    {
        Enter,
        Stay,
        Exit
    }
}
