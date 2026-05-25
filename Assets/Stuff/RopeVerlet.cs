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
    [SerializeField] private float collisionRadius = 0.12f;

    [Header("Throw Speed Mechanics")]
    [SerializeField] private float travelSpeed = 45f;
    [SerializeField] private float loopWaveSwell = 0.4f;

    [Header("Constraints Solvers")]
    [SerializeField] private int numOFConstraintRuns = 15;

    private LineRenderer lineRenderer;
    private List<RopeSegment3D> ropeSegments = new List<RopeSegment3D>();
    
    private Transform trackingHand;
    private Vector3 pointOfImpact;
    private bool ropeFired = false;
    private float flyProgress = 0f;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = numOfRopeSegments;
        
        // Hide visual line renderer completely on initialization awake frame
        gameObject.SetActive(false);
    }

    public void LaunchRope(Transform handAnchor, Vector3 targetWorldPosition)
    {
        trackingHand = handAnchor;
        pointOfImpact = targetWorldPosition;
        flyProgress = 0f;
        ropeFired = true;

        ropeSegments.Clear();
        Vector3 spawnOrigin = handAnchor.position;
        Vector3 headingDir = (targetWorldPosition - spawnOrigin).normalized;

        // Initialize segments huddled together, adding a dynamic forward velocity pop
        for (int i = 0; i < numOfRopeSegments; i++)
        {
            RopeSegment3D segment = new RopeSegment3D(spawnOrigin);
            
            // Build progressive trailing segment weight bias metrics
            float forwardFactor = (float)i / (float)numOfRopeSegments;
            Vector3 burstVelocity = headingDir * travelSpeed * forwardFactor;
            
            // Recreate loop opening curves using structural sinus offsets
            if (i > 0 && i < numOfRopeSegments - 1)
            {
                Vector3 curveNormal = Vector3.Cross(headingDir, Vector3.up).normalized;
                burstVelocity += curveNormal * Mathf.Sin(i * 0.4f) * loopWaveSwell;
            }

            segment.OldPosition = spawnOrigin - (burstVelocity * Time.fixedDeltaTime);
            ropeSegments.Add(segment);
        }

        gameObject.SetActive(true);
    }

    private void Update()
    {
        if (!ropeFired) return;
        DrawRope();
    }

    private void FixedUpdate()
    {
        if (!ropeFired) return;

        Simulate3D();

        for (int i = 0; i < numOFConstraintRuns; i++)
        {
            Apply3DConstraints();
            Resolve3DCollisions();
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
        float rangeGap = Vector3.Distance(trackingHand.position, pointOfImpact);
        float timeFrame = rangeGap / travelSpeed;
        flyProgress = Mathf.Clamp01(flyProgress + (Time.fixedDeltaTime / timeFrame));

        for (int i = 0; i < ropeSegments.Count; i++)
        {
            RopeSegment3D segment = ropeSegments[i];
            Vector3 travelDistance = (segment.CurrentPosition - segment.OldPosition) * dampingFactor;

            segment.OldPosition = segment.CurrentPosition;
            segment.CurrentPosition += travelDistance;
            segment.CurrentPosition += gravityForce * Time.fixedDeltaTime;

            ropeSegments[i] = segment;
        }

        // Lock tip segment movement updates smoothly to the trajectory arc endpoint over frames
        RopeSegment3D tailTip = ropeSegments[ropeSegments.Count - 1];
        tailTip.CurrentPosition = Vector3.Lerp(trackingHand.position, pointOfImpact, flyProgress);
        ropeSegments[ropeSegments.Count - 1] = tailTip;
    }

    private void Apply3DConstraints()
    {
        // Anchor point 1: Clamp your rope root transformation array block directly to the moving bone node
        RopeSegment3D leadSegment = ropeSegments[0];
        leadSegment.CurrentPosition = trackingHand.position;
        ropeSegments[0] = leadSegment;

        // Anchor point 2: Apply 3D distance spacing checks across structural nodes
        for (int i = 0; i < numOfRopeSegments - 1; i++)
        {
            RopeSegment3D currentSeg = ropeSegments[i];
            RopeSegment3D nextSeg = ropeSegments[i + 1];

            float distance = Vector3.Distance(currentSeg.CurrentPosition, nextSeg.CurrentPosition);
            float lengthError = distance - ropeSegmentLength;
            Vector3 alignmentDir = (currentSeg.CurrentPosition - nextSeg.CurrentPosition).normalized;
            Vector3 resolutionVector = alignmentDir * lengthError;

            if (i != 0)
            {
                currentSeg.CurrentPosition -= resolutionVector * 0.5f;
                nextSeg.CurrentPosition += resolutionVector * 0.5f;
            }
            else
            {
                nextSeg.CurrentPosition += resolutionVector;
            }

            ropeSegments[i] = currentSeg;
            ropeSegments[i + 1] = nextSeg;
        }
    }

    private void Resolve3DCollisions()
    {
        for (int i = 1; i < ropeSegments.Count; i++)
        {
            RopeSegment3D segment = ropeSegments[i];
            Vector3 lateralVelocity = segment.CurrentPosition - segment.OldPosition;

            Collider[] targetsHit = Physics.OverlapSphere(segment.CurrentPosition, collisionRadius, collisionMask);

            foreach (Collider hitObj in targetsHit)
            {
                Vector3 boundaryPoint = hitObj.ClosestPoint(segment.CurrentPosition);
                float gapDistance = Vector3.Distance(segment.CurrentPosition, boundaryPoint);

                if (gapDistance < collisionRadius)
                {
                    Vector3 surfaceNormal = (segment.CurrentPosition - boundaryPoint).normalized;
                    if (surfaceNormal == Vector3.zero)
                    {
                        surfaceNormal = (segment.CurrentPosition - hitObj.transform.position).normalized;
                    }

                    float penetrationDepth = collisionRadius - gapDistance;
                    segment.CurrentPosition += surfaceNormal * penetrationDepth;

                    lateralVelocity = Vector3.Reflect(lateralVelocity, surfaceNormal) * 0.05f;
                }
            }

            segment.OldPosition = segment.CurrentPosition - lateralVelocity;
            ropeSegments[i] = segment;
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