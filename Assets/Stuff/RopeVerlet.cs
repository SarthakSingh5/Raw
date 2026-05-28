using UnityEngine;
using System.Collections.Generic;

public class RopeVerlet : MonoBehaviour
{
    [Header("Rope Structure")]
    [SerializeField] private int numOfRopeSegments = 40;
    [SerializeField] private float ropeSegmentLength = 0.25f;

    [Header("3D Physics Simulation")]
    [SerializeField] private Vector3 gravityForce = new Vector3(0f, -9.81f, 0f);
    [SerializeField] private float dampingFactor = 0.96f;
    [SerializeField] private LayerMask collisionMask;
    [SerializeField] private float collisionRadius = 0.05f;

    [Header("Throw Speed Mechanics")]
    [SerializeField] private float travelSpeed = 45f;
    [SerializeField] private float loopWaveSwell = 0.4f;

    [Header("Constraints Solvers")]
    [SerializeField] private int numOFConstraintRuns = 15;

    [Header("Visual Attachments")]
    [SerializeField] private Transform nooseMeshInstance;

    [Header("Dragging Mechanics")]
    [SerializeField] private float pullForce = 150f; 
    
    private Rigidbody draggedSpineRb = null;
    private Rigidbody draggedHipsRb = null; 

    [Header("Grab Settings")]
    [SerializeField] private LayerMask grabTargetMask;
    [SerializeField] private float grabDetectionRadius = 1.5f; 
    [SerializeField] private float spineHeightOffset = 1.2f;

    private LineRenderer lineRenderer;
    private List<RopeSegment3D> ropeSegments = new List<RopeSegment3D>();

    private Transform trackingHand;
    private Vector3 pointOfImpact = Vector3.zero;
    private bool ropeFired = false;
    private float flyProgress = 0f;
    private bool isFlying = false;

    private Transform grabbedTargetRoot = null;
    private Vector3 manualTargetLocalOffset;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = numOfRopeSegments;
        gameObject.SetActive(false);
    }

    public void LaunchRope(Transform handAnchor, Vector3 targetWorldPosition)
    {
        if (targetWorldPosition == Vector3.zero)
        {
            targetWorldPosition = handAnchor.position + (handAnchor.forward * 20f);
        }

        trackingHand = handAnchor;
        pointOfImpact = targetWorldPosition;
        flyProgress = 0f;
        ropeFired = true;
        isFlying = true;
        grabbedTargetRoot = null;
        draggedSpineRb = null;
        draggedHipsRb = null;

        ropeSegments.Clear();
        Vector3 spawnOrigin = handAnchor.position;

        for (int i = 0; i < numOfRopeSegments; i++)
        {
            RopeSegment3D segment = new RopeSegment3D(spawnOrigin);
            ropeSegments.Add(segment);
        }

        gameObject.SetActive(true);
    }

    private void Update()
    {
        if (!ropeFired) return;
        DrawRope();
        PositionNooseHead();
    }

    private void FixedUpdate()
    {
        if (!ropeFired) return;

        Simulate3D();

        for (int i = 0; i < numOFConstraintRuns; i++)
        {
            Apply3DConstraints();
            
            if (!isFlying)
            {
                Resolve3DCollisions();
            }
        }

        // ==========================================
        // SAFE CENTER-OF-MASS DRAGGING LOGIC ONLY
        // ==========================================
        if (!isFlying && draggedSpineRb != null && trackingHand != null)
        {
            float maxRopeLength = numOfRopeSegments * ropeSegmentLength;
            float currentDistance = Vector3.Distance(trackingHand.position, draggedSpineRb.position);

            if (currentDistance > maxRopeLength)
            {
                Vector3 pullDirection = trackingHand.position - draggedSpineRb.position;
                pullDirection.y = 0f; // Keep it perfectly flat to the ground
                
                if (pullDirection.magnitude > 0.01f)
                {
                    pullDirection.Normalize();
                    
                    float overstretchAmount = Mathf.Clamp(currentDistance - maxRopeLength, 0f, 3f);
                    
                    Rigidbody targetDragRb = (draggedHipsRb != null) ? draggedHipsRb : draggedSpineRb;
                    
                    Vector3 appliedForce = pullDirection * (overstretchAmount * pullForce * targetDragRb.mass);
                    targetDragRb.AddForce(appliedForce, ForceMode.Force);
                }
            }
        }
    }

    private void DrawRope()
    {
        Vector3[] ropePositions = new Vector3[numOfRopeSegments];
        for (int i = 0; i < ropeSegments.Count; i++)
        {
            ropePositions[i] = ropeSegments[i].CurrentPosition;
        }
        lineRenderer.SetPositions(ropePositions);
    }

    private void Simulate3D()
    {
        if (trackingHand == null) return;

        bool justHit = false;

        if (isFlying)
        {
            float rangeGap = Vector3.Distance(trackingHand.position, pointOfImpact);
            float timeFrame = rangeGap > 0.1f ? (rangeGap / travelSpeed) : 0.1f;
            
            float nextFlyProgress = Mathf.Clamp01(flyProgress + (Time.fixedDeltaTime / timeFrame));
            Vector3 oldTipPos = Vector3.Lerp(trackingHand.position, pointOfImpact, flyProgress);
            Vector3 newTipPos = Vector3.Lerp(trackingHand.position, pointOfImpact, nextFlyProgress);

            float travelDist = Vector3.Distance(oldTipPos, newTipPos);
            Vector3 travelDir = travelDist > 0.001f ? (newTipPos - oldTipPos).normalized : (pointOfImpact - trackingHand.position).normalized;

            Collider[] instantHits = Physics.OverlapSphere(newTipPos, grabDetectionRadius, grabTargetMask);
            
            if (instantHits.Length > 0)
            {
                AttachToTarget(instantHits[0]);
                justHit = true;
            }
            else if (travelDist > 0.001f && Physics.SphereCast(oldTipPos, grabDetectionRadius, travelDir, out RaycastHit hit, travelDist, grabTargetMask))
            {
                AttachToTarget(hit.collider);
                justHit = true;
            }

            if (justHit)
            {
                isFlying = false;
            }
            else
            {
                flyProgress = nextFlyProgress;

                for (int i = 0; i < ropeSegments.Count; i++)
                {
                    float ratio = (float)i / (float)(ropeSegments.Count - 1);
                    Vector3 targetPathPos = Vector3.Lerp(trackingHand.position, newTipPos, ratio);

                    if (i > 0 && i < ropeSegments.Count - 1)
                    {
                        Vector3 headingDir = travelDir;
                        Vector3 curveNormal = Vector3.Cross(headingDir, Vector3.up).normalized;
                        targetPathPos += curveNormal * Mathf.Sin(i * 0.4f + flyProgress * 10f) * (loopWaveSwell * ratio);
                    }

                    RopeSegment3D segment = ropeSegments[i];
                    segment.CurrentPosition = targetPathPos;
                    segment.OldPosition = targetPathPos;
                    ropeSegments[i] = segment;
                }

                if (flyProgress >= 1f)
                {
                    isFlying = false;
                }
            }
        }

        if (!isFlying)
        {
            if (justHit && grabbedTargetRoot != null)
            {
                Vector3 finalAttachPoint = grabbedTargetRoot.TransformPoint(manualTargetLocalOffset);
                for (int i = 0; i < ropeSegments.Count; i++)
                {
                    float ratio = (float)i / (float)(ropeSegments.Count - 1);
                    RopeSegment3D seg = ropeSegments[i];
                    seg.CurrentPosition = Vector3.Lerp(trackingHand.position, finalAttachPoint, ratio);
                    seg.OldPosition = seg.CurrentPosition; 
                    ropeSegments[i] = seg;
                }
            }
            else
            {
                for (int i = 0; i < ropeSegments.Count; i++)
                {
                    RopeSegment3D segment = ropeSegments[i];
                    Vector3 travelDistance = (segment.CurrentPosition - segment.OldPosition) * dampingFactor;

                    segment.OldPosition = segment.CurrentPosition;
                    segment.CurrentPosition += travelDistance;
                    segment.CurrentPosition += gravityForce * Time.fixedDeltaTime;

                    ropeSegments[i] = segment;
                }

                RopeSegment3D leadSegment = ropeSegments[0];
                leadSegment.CurrentPosition = trackingHand.position;
                ropeSegments[0] = leadSegment;

                if (grabbedTargetRoot != null)
                {
                    RopeSegment3D tailTip = ropeSegments[ropeSegments.Count - 1];
                    tailTip.CurrentPosition = grabbedTargetRoot.TransformPoint(manualTargetLocalOffset);
                    ropeSegments[ropeSegments.Count - 1] = tailTip;
                }
            }
        }
    }

    private void AttachToTarget(Collider targetCollider)
    {
        Transform hitRoot = targetCollider.transform;
        Transform explicitSpine = FindChildByNameRecursively(hitRoot.root, "mixamorig:Spine1");
        Transform explicitHips = FindChildByNameRecursively(hitRoot.root, "mixamorig:Hips"); 
        
        if (explicitSpine != null)
        {
            grabbedTargetRoot = explicitSpine;
            manualTargetLocalOffset = Vector3.zero; 
            TriggerRagdollAndHook(hitRoot.root, explicitSpine, explicitHips);
        }
        else
        {
            grabbedTargetRoot = hitRoot;
            Vector3 estimatedSpineWorldPos = grabbedTargetRoot.position + (Vector3.up * spineHeightOffset);
            manualTargetLocalOffset = grabbedTargetRoot.InverseTransformPoint(estimatedSpineWorldPos);
        }
    }

    private void TriggerRagdollAndHook(Transform root, Transform spineBone, Transform hipsBone)
    {
        Collider mainCollider = root.GetComponent<Collider>();
        if (mainCollider != null) mainCollider.enabled = false;

        Animator anim = root.GetComponentInChildren<Animator>();
        if (anim != null) anim.enabled = false;

        Rigidbody[] boneBodies = root.GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in boneBodies)
        {
            rb.isKinematic = false; 
            rb.useGravity = true;
        }

        draggedSpineRb = spineBone.GetComponent<Rigidbody>();
        if (hipsBone != null) draggedHipsRb = hipsBone.GetComponent<Rigidbody>(); 
    }

    private Transform FindChildByNameRecursively(Transform currentParent, string targetName)
    {
        if (currentParent.name == targetName) return currentParent;
        for (int i = 0; i < currentParent.childCount; i++)
        {
            Transform child = FindChildByNameRecursively(currentParent.GetChild(i), targetName);
            if (child != null) return child;
        }
        return null;
    }

    private void Apply3DConstraints()
    {
        if (ropeSegments.Count == 0 || trackingHand == null) return;

        RopeSegment3D leadSegment = ropeSegments[0];
        leadSegment.CurrentPosition = trackingHand.position;
        ropeSegments[0] = leadSegment;

        bool isPinned = (grabbedTargetRoot != null && !isFlying);
        if (isPinned)
        {
            RopeSegment3D tailSegment = ropeSegments[ropeSegments.Count - 1];
            tailSegment.CurrentPosition = grabbedTargetRoot.TransformPoint(manualTargetLocalOffset);
            ropeSegments[ropeSegments.Count - 1] = tailSegment;
        }

        for (int i = 0; i < ropeSegments.Count - 1; i++)
        {
            RopeSegment3D currentSeg = ropeSegments[i];
            RopeSegment3D nextSeg = ropeSegments[i + 1];

            float distance = Vector3.Distance(currentSeg.CurrentPosition, nextSeg.CurrentPosition);
            float lengthError = distance - ropeSegmentLength;
            Vector3 alignmentDir = (currentSeg.CurrentPosition - nextSeg.CurrentPosition).normalized;

            if (alignmentDir == Vector3.zero) alignmentDir = Vector3.forward;
            Vector3 resolutionVector = alignmentDir * lengthError;

            if (i == 0)
            {
                nextSeg.CurrentPosition += resolutionVector; 
            }
            else if (isPinned && i == ropeSegments.Count - 2)
            {
                currentSeg.CurrentPosition -= resolutionVector; 
            }
            else
            {
                currentSeg.CurrentPosition -= resolutionVector * 0.5f; 
                nextSeg.CurrentPosition += resolutionVector * 0.5f;
            }

            ropeSegments[i] = currentSeg;
            ropeSegments[i + 1] = nextSeg;
        }
    }

    private void Resolve3DCollisions()
    {
        for (int i = 2; i < ropeSegments.Count; i++)
        {
            RopeSegment3D segment = ropeSegments[i];
            Vector3 lateralVelocity = segment.CurrentPosition - segment.OldPosition;

            Collider[] targetsHit = Physics.OverlapSphere(segment.CurrentPosition, collisionRadius, collisionMask);

            foreach (Collider hitObj in targetsHit)
            {
                if (hitObj.transform.IsChildOf(trackingHand.root)) continue;

                Vector3 boundaryPoint = hitObj.ClosestPoint(segment.CurrentPosition);
                float gapDistance = Vector3.Distance(segment.CurrentPosition, boundaryPoint);

                if (gapDistance < collisionRadius)
                {
                    Vector3 surfaceNormal = (segment.CurrentPosition - boundaryPoint).normalized;
                    if (surfaceNormal == Vector3.zero)
                    {
                        surfaceNormal = Vector3.up;
                    }

                    float penetrationDepth = collisionRadius - gapDistance;
                    segment.CurrentPosition += surfaceNormal * penetrationDepth;

                    if (surfaceNormal.y > 0.5f && lateralVelocity.y > 0f)
                    {
                        lateralVelocity.y = 0f;
                    }

                    lateralVelocity = Vector3.Reflect(lateralVelocity, surfaceNormal) * 0.02f;
                }
            }

            lateralVelocity = Vector3.ClampMagnitude(lateralVelocity, ropeSegmentLength);
            segment.OldPosition = segment.CurrentPosition - lateralVelocity;
            ropeSegments[i] = segment;
        }
    }

    private void PositionNooseHead()
    {
        if (nooseMeshInstance == null || ropeSegments.Count < 2) return;

        if (!isFlying && grabbedTargetRoot != null)
        {
            nooseMeshInstance.position = grabbedTargetRoot.TransformPoint(manualTargetLocalOffset);
            nooseMeshInstance.forward = grabbedTargetRoot.forward;
            return;
        }

        Vector3 tipNodePos = ropeSegments[ropeSegments.Count - 1].CurrentPosition;
        Vector3 penultNodePos = ropeSegments[ropeSegments.Count - 2].CurrentPosition;

        nooseMeshInstance.position = tipNodePos;

        Vector3 flightDirection = (tipNodePos - penultNodePos).normalized;
        if (flightDirection != Vector3.zero)
        {
            nooseMeshInstance.forward = flightDirection;
        }
    }

    public struct RopeSegment3D
    {
        public Vector3 CurrentPosition;
        public Vector3 OldPosition;

        public RopeSegment3D(Vector3 initialPos)
        {
            CurrentPosition = initialPos;
            OldPosition = initialPos;
        }
    }
}