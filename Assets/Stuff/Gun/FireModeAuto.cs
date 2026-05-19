using UnityEngine;

[CreateAssetMenu(fileName = "FireMode", menuName = "Gun/FireMode/Auto")]
public class FireModeAuto : FireMode
{
    public override void OnTriggerPulled(FireModeState state) => state.isFiring = true;
    public override void OnTriggerReleased(FireModeState state) => state.isFiring = false;

    public override bool CanFire(FireModeState state)
    {
        // ADDITION: Check for ammo and reloading status
        // if (state.currAmmo <= 0 || state.isReloading) 
        //     return false;
        return state.isFiring && Time.time >= state.nextFireTime;
    }

    public override void OnFired(FireModeState state)
    {
        // Reduce ammo count
        // state.currAmmo--;
        state.nextFireTime = Time.time + rate;
    }
}