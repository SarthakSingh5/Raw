using UnityEngine;
using UnityEngine.Animations.Rigging;


public class NpcLook : NpcComponent
{
    [SerializeField] public float turnSpeed = 15f;
    [SerializeField] float expireTime = 0.2f;
    float lookTimer = 0f;
    Vector3 targetPosition = Vector3.zero;
    float animatorTurnParameter = 0f;

    [Header("Animator Settings")]
    [SerializeField] float animatorTurnSmoothing = 5f;

    [Header("Rigs")]
    [SerializeField] Rig SpineRig;
    [SerializeField] Rig RArmRig;
    [SerializeField] Rig LArmRig;
    [SerializeField] Transform SpineAimTarget;
    [SerializeField] Transform AimTarget;



    [Header("Gizmos")]
    [SerializeField] bool drawGizmos = false;
    [SerializeField] Color gizmosColor = Color.cyan;


    public void Start()
    {
        npc.LookAt += OnLookAt;
        npc.IsLookingAt += OnIsLookingAt;
        npc.LookForward += NPCLookForward;
    }

    #region Rig Manipulation Methods
    void UpdateArmRigs()
    {
        if (npc.Aiming && npc.carryingGun)
        {
            RArmRig.weight = 1f;
            LArmRig.weight = 1f;
        }
        else
        {
            RArmRig.weight = 0f;
            LArmRig.weight = 0f;
        }
    }

    void SetSpineRigWeight(float weight)
    {
        float smoothing = 5f;
        SpineRig.weight = Mathf.Lerp(SpineRig.weight, weight, smoothing * Time.deltaTime);
    }

    void SetSpineAimTargetPosition(Vector3 position)
    {
        SpineAimTarget.position = Vector3.Lerp(SpineAimTarget.position, position, 5f * Time.deltaTime);
    }

    void SetAimTargetPosition(Vector3 position)
    {
        AimTarget.position = Vector3.Lerp(AimTarget.position, position, 5f * Time.deltaTime);
    }
    #endregion

    void OnLookAt(Vector3 position)
    {
        lookTimer = expireTime;
        targetPosition = position;

        SetSpineRigWeight(1.0f);
        AimTarget.position = position;
        ClampAimTargetAngle();

        if (npc.Aiming && npc.carryingGun)
        {
            SetSpineAimTargetPosition(transform.TransformPoint(Quaternion.Euler(0f, 45f, 0f) * AimTarget.localPosition));
        }
        else
        {
            SetSpineAimTargetPosition(AimTarget.position);
        }
    }

    private void OnDrawGizmos()
    {
        if (npc == null || !drawGizmos)
            return;

        Gizmos.color = gizmosColor;
        Gizmos.DrawLine(npc.SensorPosition, AimTarget.position);
        Gizmos.DrawWireSphere(AimTarget.position, 0.3f);
    }

    void ClampAimTargetAngle()
    {
        float HorizontalAngleLimit = 15f;
        Vector3 forward = npc.transform.forward;
        Vector3 direction = (AimTarget.position - npc.SensorPosition).normalized;
        Vector3 horizontalDirection = direction;
        horizontalDirection.y = 0f;
        horizontalDirection.Normalize();
        float angle = Vector3.Angle(forward, horizontalDirection);
        if (angle > HorizontalAngleLimit)
        {
            Vector3 clampedDirection = Vector3.RotateTowards(forward, horizontalDirection, Mathf.Deg2Rad * HorizontalAngleLimit, 0f);
            clampedDirection.y = direction.y;
            clampedDirection.Normalize();
            AimTarget.position = npc.SensorPosition + clampedDirection * Vector3.Distance(npc.SensorPosition, AimTarget.position);
        }
    }

    bool OnIsLookingAt(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - npc.transform.position);
        direction.y = 0f;
        direction.Normalize();

        Vector3 fwd = npc.transform.forward;
        fwd.y = 0f;
        fwd.Normalize();

        return Vector3.Dot(fwd, direction) > 0.95f;
    }


    private void LateUpdate()
    {
        lookTimer -= Time.deltaTime;

        UpdateArmRigs();

        if (lookTimer < 0f)
        {
            lookTimer = -1f;
            npc.canTurn = true;

            animatorTurnParameter = Mathf.Lerp(animatorTurnParameter, 0f, animatorTurnSmoothing * Time.deltaTime);

            SpineRig.weight = 0f;
        }
        else
        {
            npc.canTurn = false;
            NPCLookAtPosition(targetPosition);
        }
        npc.anim.SetFloat("Turn", animatorTurnParameter);
    }

    void NPCLookAtPosition(Vector3 position)
    {
        Vector3 directionToTarget = position - npc.transform.position;
        directionToTarget.y = 0;

        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
        npc.transform.rotation = Quaternion.RotateTowards(
            npc.transform.rotation,
            targetRotation,
            turnSpeed * Time.deltaTime
        );

        UpdateAnimatorTurnParameter(position);
    }

    void UpdateAnimatorTurnParameter(Vector3 targetPosition)
    {
        Vector3 normalizedDirection = targetPosition - npc.transform.position;
        normalizedDirection.y = 0f;
        normalizedDirection.Normalize();

        Vector3 forward = npc.transform.forward;
        float angle = Vector3.SignedAngle(forward, normalizedDirection, Vector3.up);

        float maxAngle = 30f;
        angle = Mathf.Clamp(angle, -maxAngle, maxAngle) / maxAngle;

        animatorTurnParameter = Mathf.Lerp(animatorTurnParameter, angle, animatorTurnSmoothing * Time.deltaTime);
    }

    void NPCLookForward()
    {
        Vector3 pos = npc.transform.position;
        pos += npc.transform.forward * 1000.0f;
        pos += Vector3.up * 1.5f;

        NPCLookAtPosition(pos);
    }
}
