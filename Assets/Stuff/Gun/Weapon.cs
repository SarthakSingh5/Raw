using UnityEngine;
using System.Collections;

public class Weapon : NpcComponent
{
    [SerializeField]
    public
    Shooter shooter;
    [SerializeField]
    public
    FireMode fireMode;

    [SerializeField]
    Transform muzzle;

    // Each AK-47 in the scene has its own private memory now.
    public FireModeState state = new FireModeState();

    private AudioSource audioSource;

    // private Coroutine reloadCoroutine;

    protected override void Awake()
    {
        base.Awake();
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1.0f; // 1.0 makes it fully 3D
    }


    public override void SetNpc(Npc npc)
    {
        if (this.npc != null)
        {
            this.npc.TryShoot -= TryShoot;
            this.npc.NotShoot -= NotShoot;
            this.npc.LookAt -= OnLookAt;
            // this.npc.TryReload -= OnReload;
        }

        base.SetNpc(npc);

        if (npc != null)
        {
            npc.TryShoot += TryShoot;
            npc.NotShoot += NotShoot;
            npc.LookAt += OnLookAt;
            // npc.TryReload += OnReload;
        }
    }

    void TryShoot()
    {
        PullTrigger();
    }

    void NotShoot()
    {
        ReleaseTrigger();
    }



    void OnLookAt(Vector3 target)
    {
        // we orient the muzzle towards the target, like they did in games made in the 90s.
        // if you want realistic aiming, you should use animation rigging
        // Code Monkey has a great tutorial on that on Youtube.
        muzzle.forward = target - muzzle.position;
    }

    // void OnReload()
    // {
    //     if (state.isReloading || state.currAmmo == shooter.magSize) return;

    //     // Lock the gun logic
    //     state.isReloading = true;
    //     npc.SetReload(true);

    //     // Play the animation
    //     npc.anim.SetTrigger("Reload");
    // }

    public void PullTrigger()
    {
        fireMode.OnTriggerPulled(state);
    }

    public void ReleaseTrigger()
    {
        fireMode.OnTriggerReleased(state);
    }

    void Start()
    {
        // Ensure the gun starts with a full magazine
        // if (shooter != null)
        // {
        //     state.currAmmo = shooter.magSize;
        // }
    }

    public void Update()
    {
        // if (state.isReloading) return;
        // if (state.currAmmo <= 0)
        // {
        //     OnReload();
        // }
        if (fireMode.CanFire(state))
        {
            // npc.AddRecoilBloom();
            shooter.Shoot(muzzle, npc.currentBloom);
            fireMode.OnFired(state);
            PlayWeaponSound(shooter.shotSound);
        }
    }

    // public void OnMagOut()
    // {
    //     PlayWeaponSound(shooter.magOutSound);
    // }

    // public void OnMagIn()
    // {
    //     PlayWeaponSound(shooter.magInSound);
    // }

    // public void OnReleaseSlide()
    // {
    //     PlayWeaponSound(shooter.releaseSlideSound);
    // }

    // public void OnWeaponReloaded()
    // {
    //     state.currAmmo = shooter.magSize;
    //     state.isReloading = false;
    //     npc.SetReload(false);

    //     Debug.Log("Reload logic finished via Animation Event!");
    // }


    public void LassoThrow()
    {
        // Highlighted: Route the animation trigger framework straight out to your modular scriptable shooter configuration
        if (shooter != null && muzzle != null)
        {
            shooter.Shoot(muzzle, npc.currentBloom);
            PlayWeaponSound(shooter.shotSound);
        }
        else
        {
            Debug.LogWarning("LassoThrow method triggered on Weapon instance, but its modular Shooter configuration properties are blank!");
        }
    }

    private void PlayWeaponSound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            // Keep the variety by using the same pitch randomness as your shots
            audioSource.pitch = 1.0f + Random.Range(-shooter.pitchRandomness, shooter.pitchRandomness);
            audioSource.PlayOneShot(clip, shooter.volume);
        }
    }

    private void OnDrawGizmos()
    {
        if (muzzle == null || npc == null) return;

        // 1. Set the color (Red for inaccurate, Green for perfect)
        float colorAlpha = Mathf.InverseLerp(0f, 2.0f, npc.currentBloom);
        Gizmos.color = Color.Lerp(Color.green, Color.red, colorAlpha);

        // 2. Project a point forward to show the center of aim
        Vector3 centerPoint = muzzle.position + muzzle.forward * 5f; // 5 meters ahead
        Gizmos.DrawLine(muzzle.position, centerPoint);

        // 3. Draw the "Circle of Error" at that distance
        // The radius is proportional to our currentBloom
        float radius = npc.currentBloom * 1.2f;

        // Draw a wire circle using a helper loop
        DrawGizmoCircle(centerPoint, muzzle.forward, radius);
    }

    private void DrawGizmoCircle(Vector3 center, Vector3 normal, float radius)
    {
        Vector3 up = Vector3.up;
        if (Mathf.Abs(Vector3.Dot(normal, up)) > 0.99f) up = Vector3.right;

        Vector3 right = Vector3.Cross(normal, up).normalized;
        Vector3 circleUp = Vector3.Cross(right, normal).normalized;

        Vector3 lastPoint = center + right * radius;
        for (int i = 1; i <= 32; i++)
        {
            float angle = i / 32f * Mathf.PI * 2f;
            Vector3 nextPoint = center + (right * Mathf.Cos(angle) + circleUp * Mathf.Sin(angle)) * radius;
            Gizmos.DrawLine(lastPoint, nextPoint);
            lastPoint = nextPoint;
        }
    }
}
