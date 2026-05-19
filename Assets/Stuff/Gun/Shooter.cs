using UnityEngine;


public class Shooter : ScriptableObject
{
    [Header("Firing")]
    [SerializeField]
    protected float MaxRange = 30f;

    [Header("Physics")]
    [SerializeField]
    protected float HitImpulse = 200f;

    [Header("Damage")]
    [SerializeField]
    public float Damage = 10.0f;

    [SerializeField]
    public float DamageRandom = 5f;

    [Header("Accuracy")]
    protected float MaxSpreadAngle = 5f;


    [Header("Components")]
    [SerializeField]
    public Transform muzzle;

    [SerializeField]
    protected ParticleSystem MuzzleFlashFX;

    [SerializeField]
    protected GameObject bullet;

    [SerializeField]
    protected float bulletVelocity;

    [SerializeField]
    protected int bulletsPerShot;

    [SerializeField]
    protected LayerMask HitMask = Physics.DefaultRaycastLayers;


    #region Tracer

    [SerializeField]
    protected LineRenderer tracerPrefab;

    [SerializeField]
    protected float tracerSpeed = 100f;

    [SerializeField]
    protected float tracerLifetime = 0.1f;

    #endregion

    [Header("Audio")]
    [SerializeField] public AudioClip shotSound;
    [SerializeField] public AudioClip magOutSound;
    [SerializeField] public AudioClip magInSound;
    [SerializeField] public AudioClip releaseSlideSound;
    [SerializeField, Range(0, 1)] public float volume = 1f;
    [SerializeField, Range(0, 1)] public float pitchRandomness = 0.05f;


    // High multiplier (e.g., 20) means 2.0 bloom = 40 degrees of error
    [SerializeField] protected float spreadMultiplier = 2f;

    [Header("Recoil Bloom Stats")]
    public float bloomPerShot = 0.5f;
    public float bloomRecoveryRate = 8f;

    public float reloadDuration = 2f;
    public int magSize = 30;

    
    public virtual void Shoot(Transform muzzle, float bloom)
    {
        

    }

    void PlayMuzzleFlashFX()
    {
        if (MuzzleFlashFX == null)
        {
            return;
        }

        if (MuzzleFlashFX.isPlaying)
        {
            MuzzleFlashFX.Stop();
        }

        MuzzleFlashFX.Play();
    }
}
