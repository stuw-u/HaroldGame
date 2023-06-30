using UnityEngine;

[DefaultExecutionOrder(1)]
public class CameraThirdPerson : MonoBehaviour
{
    [SerializeField] float sensitivity = 0.3f;
    [SerializeField] float maxDistance = 5f;
    [SerializeField] float minAngle = -85f;
    [SerializeField] float maxAngle = 85f;
    [SerializeField] float radius = 0.5f;
    [SerializeField] float distSmooth = 0.5f;
    [SerializeField] float yDampSmooth = 0.5f;
    [SerializeField] float laptopYSmooth = 0.5f;
    [SerializeField] float laptopYAngles = 35f;
    [SerializeField] Transform anchor;
    [SerializeField] LayerMask collidingLayerMask;

    private float dist;
    private Vector2 rot;
    private float yPos;
    private float yVel;
    private Vector3 lastTarget;
    private float sensitivityMul = 1f;
    bool laptopMode = false;
    float laptopY;
    float laptopYVel;

    private void Start ()
    {
        yPos = anchor.position.y;
    }

    private void OnEnable () {
        Settings.onSettingsUpdate += Settings_onSettingsUpdate; }
    private void OnDisable () => Settings.onSettingsUpdate -= Settings_onSettingsUpdate;

    private void Settings_onSettingsUpdate (SettingsData data)
    {
        sensitivityMul = data.sensibility;
        laptopMode = data.laptopMode;
    }

    void LateUpdate ()
    {
        if(GameManager.IsPlayerInControl)
        {
            if (laptopMode)
            {
                float delta = (Input.GetKey(KeyCode.LeftArrow) ? -1 : 0) + (Input.GetKey(KeyCode.RightArrow) ? 1 : 0);
                float target = (Input.GetKey(KeyCode.DownArrow) ? 1 : 0) + (Input.GetKey(KeyCode.UpArrow) ? -1 : 0);
                rot.x += delta * sensitivity * sensitivityMul;
                laptopY = Mathf.SmoothDamp(laptopY, target * laptopYAngles + 15f, ref laptopYVel, laptopYSmooth);
                rot.y = laptopY;
            }
            else
            {
                rot.x += Input.GetAxisRaw("Mouse X") * sensitivity * sensitivityMul;
                rot.y -= Input.GetAxisRaw("Mouse Y") * sensitivity * sensitivityMul;
                rot.y = Mathf.Clamp(rot.y, minAngle, maxAngle);
            }
        }

        transform.rotation = Quaternion.Euler(rot.y, rot.x, 0);
        if (anchor != null)
        {
            Vector3 targetPos = new Vector3(anchor.position.x, yPos, anchor.position.z);
            if(targetPos.y < -5f)
            {
                targetPos = lastTarget;
            }
            else
            {
                lastTarget = targetPos;
            }


            float clampedDist = maxDistance;
            if(Physics.SphereCast(targetPos, radius, -transform.forward, out RaycastHit hit, maxDistance, collidingLayerMask))
            {
                clampedDist = hit.distance;
            }

            var blend = 1f - Mathf.Pow(1f - distSmooth, Time.unscaledDeltaTime * 60);
            dist = Mathf.Lerp(dist, clampedDist, blend);

            yPos = Mathf.SmoothDamp(yPos, anchor.position.y, ref yVel, yDampSmooth);
            transform.position = targetPos + transform.forward * -dist;
        }
    }

    public void Clear (Vector3 direction)
    {
        rot = new Vector2(Quaternion.LookRotation(direction, Vector3.up).eulerAngles.y, 0f);
        yPos = anchor.position.y;
        yVel = 0f;
    }
}
