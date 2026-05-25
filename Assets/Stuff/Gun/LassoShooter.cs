using UnityEngine;

[CreateAssetMenu(fileName = "LassoShooter", menuName = "Gun/Shooter/InstantiatedLasso")]
public class LassoShooter : Shooter
{
    [Header("Lasso Physics Setup")]
    [SerializeField] private float lassoRange = 35f;
    [SerializeField] private LayerMask lassoMask;

    public override void Shoot(Transform handAnchor, float bloom)
    {
        // 1. Calculate the target destination using the screen center crosshair raycast
        Ray cameraRay = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
        Vector3 targetPoint;

        if (Physics.Raycast(cameraRay, out RaycastHit hit, lassoRange, lassoMask))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = cameraRay.origin + (cameraRay.direction * lassoRange);
        }

        // 2. Instantiate the Lasso Prefab at the hand's position and rotation
        // (This uses the 'bullet' GameObject property inherited from your Shooter base class!)
        if (bullet == null)
        {
            Debug.LogError("No Lasso Prefab assigned to the 'bullet' slot on this LassoShooter asset!");
            return;
        }

        GameObject lassoInstance = Instantiate(bullet, handAnchor.position, handAnchor.rotation);

        // 3. Extract the RopeVerlet component from the freshly spawned object
        RopeVerlet ropeEngine = lassoInstance.GetComponent<RopeVerlet>();

        if (ropeEngine != null)
        {
            // Launch the 3D Verlet simulation directly from the scriptable object trigger
            ropeEngine.LaunchRope(handAnchor, targetPoint);
        }
        else
        {
            Debug.LogError("The instantiated Lasso Prefab is missing the RopeVerlet script component!");
        }
    }
}