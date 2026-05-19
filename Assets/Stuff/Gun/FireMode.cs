using UnityEngine;

public abstract class FireMode : ScriptableObject
{
    public float rate = 0.3f;

    public virtual void OnTriggerPulled(FireModeState state) { }
    public virtual void OnTriggerReleased(FireModeState state) { }
    public abstract bool CanFire(FireModeState state);
    public virtual void OnFired(FireModeState state) { }
}