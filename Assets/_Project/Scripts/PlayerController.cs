using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float speed = 5f;
    [SerializeField] float acceleration = 5f;
    [SerializeField] float arealAcceleration = 5f;
    [SerializeField] float breaking = 0.5f;
    [SerializeField] float gravity = 20f;
    [SerializeField] float jumpImpulse = 20f;
    [SerializeField] new Transform camera;
    [SerializeField] LayerMask groundCollisionLayer;

    [Header("Visuals")]
    [SerializeField] WeaponUser weaponUser;
    [SerializeField] SkidmarkRenderer skidmark1;
    [SerializeField] SkidmarkRenderer skidmark2;
    [SerializeField] float skidmarkMultiplier = 0.05f;
    [SerializeField] ParticleSystem poof;
    [SerializeField] AudioSource poofSound;
    [SerializeField] AudioSource wheelSound;
    [SerializeField] Transform visualRoot;
    [SerializeField] Transform wheels;
    [SerializeField] float wheelDiameter;
    [SerializeField] float stretchFactor = 0.01f;
    [SerializeField] float accDampSmooth = 0.1f;
    [SerializeField] float smoothingSpeedDefault;
    [SerializeField] float smoothingSpeedOnAttack;

    private float3 targetDirection;
    private float3 direction;

    Vector3 accDamp;
    Vector3 lasPos;
    Vector3 lastVel;
    Vector3 accDampVel;

    private CharacterController cc;
    private float3 vel;
    private bool isGrounded;
    private bool wasGrounded;
    private float3 groundNormal;
    private bool backupJump;
    private bool wasJumpedOn;

    public bool IsGrounded => isGrounded;

    public void Clear (Vector3 position, Vector3 forward)
    {
        var interp = GetComponent<TransformInterpolator>();

        targetDirection = forward;
        cc.enabled = false;
        interp.enabled = false;
        transform.position = position;
        direction = forward;
        isGrounded = true;
        wasGrounded = true;
        groundNormal = Vector3.up;
        lastVel = Vector3.zero;
        lasPos = position;
        accDampVel = Vector3.zero;
        accDamp = Vector3.zero;
        vel = float3.zero;
        cc.enabled = true;
        interp.enabled = true;
        weaponUser.Clear();

        skidmark1.Clear();
        skidmark2.Clear();
    }

    public void Kill ()
    {
        if (!GameManager.IsPlayerInControl) return;

        GameManager.OnPlayerDie();
    }

    public void ApplyImpulse (float3 impulse)
    {
        vel += impulse;
        isGrounded = false;
        cc.Move(math.normalizesafe(impulse) * 0.2f);
    }

    private void Awake ()
    {
        cc = GetComponent<CharacterController>();
        direction = math.forward();
    }

    void GroundCast ()
    {
        isGrounded = false;
        groundNormal = Vector3.up;
        Vector3 origin = transform.position + cc.center + Vector3.up * (-cc.height * 0.5f + cc.radius);
        if(Physics.SphereCast(origin, cc.radius, Vector3.down, out RaycastHit hitInfo, 0.2f, groundCollisionLayer))
        {
            if(Vector3.Angle(hitInfo.normal, Vector3.up) < cc.slopeLimit)
            {
                isGrounded = true;
                groundNormal = hitInfo.normal;
                cc.Move(Vector3.down);
            }
            else
            {
                cc.Move(hitInfo.normal * 0.01f);
            }
        }

    }

    (bool hitWall, Vector3 wallNormal) ProjectVelocityOnWalls (Vector3 delta)
    {
        if(delta == Vector3.zero)
        {
            return (false, default);
        }

        Vector3 p1 = transform.position + cc.center + Vector3.up * (-cc.height * 0.5f + cc.radius);
        Vector3 p2 = p1 + Vector3.up * (cc.height - cc.radius * 2);
        return (Physics.CapsuleCast(p1, p2, cc.radius, delta.normalized, out RaycastHit hit, delta.magnitude, groundCollisionLayer), hit.normal);
    }

    private void FixedUpdate ()
    {
        // Gathering inputs
        float horizontalMove = Input.GetKey(KeyCode.A) ? -1 : 0;
        horizontalMove += Input.GetKey(KeyCode.D) ? 1 : 0;
        float verticalMove = Input.GetKey(KeyCode.S) ? -1 : 0;
        verticalMove += Input.GetKey(KeyCode.W) ? 1 : 0;

        if(!GameManager.IsPlayerInControl)
        {
            horizontalMove = 0f;
            verticalMove = 0f;
        }

        wheelSound.volume = Mathf.Clamp01(math.length(vel.xz) * 0.2f * (IsGrounded ? 1 : 0)) * 0.2f;

        // Reproject directions using surface normal
        var up = groundNormal;
        var fwd = -new float3(camera.forward.x, 0f, camera.forward.z);
        var right = -math.cross(up, fwd);
        fwd = -math.cross(up, right);
        float3 moveVector = horizontalMove * right + fwd * verticalMove;
        bool isMoving = verticalMove != 0f || horizontalMove != 0f;

        // Checking for is grounded
        GroundCast();

        // Applying motion
        vel += moveVector * (isGrounded ? acceleration : arealAcceleration) * speed * Time.deltaTime;
        vel.xz = Vector2.ClampMagnitude(vel.xz, speed);

        // Wheel visual
        var localVel = visualRoot.InverseTransformDirection(vel);
        var rot = (-localVel.z * Time.deltaTime) / (wheelDiameter * Mathf.PI);
        wheels.Rotate(Vector3.right, rot * 360f);

        // Applying friction
        var breakValue = 1f - Mathf.Pow(1f - breaking, Time.deltaTime * 60);
        if(!isGrounded || isMoving)
        {
            breakValue = 1;
        }
        vel.xz *= Mathf.Clamp01(breakValue);

        targetDirection = isMoving ? new float3(moveVector.x, 0, moveVector.z) : float3.zero;

        if (isGrounded)
        {
            vel.y = 0f;
            if (Input.GetKey(KeyCode.Space) && !wasJumpedOn && GameManager.IsPlayerInControl)
            {
                vel.y = jumpImpulse;
            }
            backupJump = true;
        }
        else
        {
            if (Input.GetKey(KeyCode.Space) && !wasJumpedOn && backupJump && GameManager.IsPlayerInControl)
            {
                backupJump = false;
                vel.y = jumpImpulse * 0.5f;
                poof.Play();
            }
        }
        vel.y -= gravity * Time.deltaTime;


        Vector3 delta = (vel * Time.deltaTime);

        (bool hitWall, Vector3 wallNormal) = ProjectVelocityOnWalls(delta);
        if (hitWall && Vector3.Angle(wallNormal, Vector3.up) > cc.slopeLimit)
        {
            vel = Vector3.ProjectOnPlane(vel, wallNormal);
        }

        delta = vel * Time.deltaTime;
        cc.Move(delta);

        wasJumpedOn = Input.GetKey(KeyCode.Space);
    }

    private bool isShooting;
    public void SetShootingState (bool isShooting)
    {
        this.isShooting = isShooting;
    }


    
    void Update()
    {
        if (!math.all(targetDirection == float3.zero) || isShooting)
        {
            if (isShooting)
            {
                targetDirection = new Vector3(camera.forward.x, 0, camera.forward.z);
            }

            var blend = 1f - math.pow(1f - (isShooting ? smoothingSpeedOnAttack : smoothingSpeedDefault), Time.deltaTime * 60);
            direction = Vector3.Slerp(direction, targetDirection, blend);
        }

        Quaternion rot = Quaternion.LookRotation(direction, math.normalize(math.up() + targetDirection * 0.2f));
        visualRoot.rotation = rot;


        if(transform.position.y < -5)
        {
            Kill();
        }
    }

    private void LateUpdate ()
    {
        if (Time.deltaTime == 0f) return;

        var visualVel = (lasPos - transform.position) / Time.deltaTime;
        var visualAcc = (lastVel - visualVel) / Time.deltaTime;
        if (!isGrounded)
        {
            visualAcc -= gravity * Vector3.down;
        }
        if(isGrounded && !wasGrounded)
        {
            poof.Play();
            poofSound.Play();
        }
        accDamp = Vector3.SmoothDamp(accDamp, visualAcc, ref accDampVel, accDampSmooth);

        float squashStretchScale = 1f;
        squashStretchScale += accDamp.y * stretchFactor;
        squashStretchScale = math.clamp(squashStretchScale, 0.5f, 1.5f);

        visualRoot.localScale = new Vector3(squashStretchScale, 1f/squashStretchScale, squashStretchScale);

        float intensity = math.saturate(math.length(vel) * skidmarkMultiplier) * (1f / squashStretchScale);
        skidmark1.UpdateState(isGrounded, intensity);
        skidmark2.UpdateState(isGrounded, intensity);

        lasPos = transform.position;
        lastVel = visualVel;
        wasGrounded = isGrounded;
    }
}
