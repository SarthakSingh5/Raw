using UnityEngine;

[CreateAssetMenu(fileName = "FireModeSemi", menuName = "Gun/FireMode/Semi")]
public class FireModeSemi : FireMode
{
    // Semi-auto requires a fresh pull for every shot.
    // It uses 'hasFired' to ensure only one bullet per trigger press.

    public override void OnTriggerPulled(FireModeState state)
    {
        // When the trigger is first pulled, we allow a shot.
        state.isFiring = true;
        state.hasFired = false; 
    }

    public override void OnTriggerReleased(FireModeState state)
    {
        state.isFiring = false;
        // Setting this to true ensures that even if CanFire is called, 
        // it won't fire until the next Pull resets it.
        state.hasFired = true; 
    }

    public override bool CanFire(FireModeState state)
    {
        // ADDITION: Check for ammo and reloading status
        // if (state.currAmmo <= 0 || state.isReloading) 
        //     return false;
        // Must be holding trigger, must NOT have fired yet for THIS pull, 
        // and must be past the cooldown time.
        if (state.isFiring && !state.hasFired && Time.time >= state.nextFireTime)
        {
            return true;
        }

        return false;
    }

    public override void OnFired(FireModeState state)
    {
        // Reduce ammo count
        // state.currAmmo--;
        // Lock the gun for this trigger pull
        state.hasFired = true; 
        state.nextFireTime = Time.time + rate;
    }
}