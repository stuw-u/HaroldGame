using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper;
using UnityEngine.AI;
using Unity.Mathematics;

public class Enemy : MonoBehaviour, IFlammable
{
    [Header("Enemy Settings")]
    [SerializeField] int defaultHealth = 100;
    [SerializeField] Renderer[] hitRenderer;
    [SerializeField] InterfaceReference<IFlammable, MonoBehaviour>[] restorables;
    [SerializeField] InterfaceReference<IDeathListener, MonoBehaviour>[] deathListeners;

    [Header("Navigation")]
    public float swerveNoise = 10;
    public float swerveSpeed = 1;
    public float navigSpeed = 10;
    public float linkSpeed = 10;
    public bool offMeshLinkVertical = false;
    public float offMeshLinkDistance = 0.1f;
    [SerializeField] protected NavMeshAgent agent;
    [SerializeField] bool navigation;

    List<Material> materialCopies;
    const float hitAnimSpeed = 3;
    const float hitSlowdownSpeed = 1f;
    const float deathAnimSpeed = 1.25f;
    const float warpToSpawnSpeed = 0.5f;
    const float gravity = -9.81f * 4;

    public float debugDist;

    int navigatorSpawn;
    int navigatorPath;
    int navigatorNextPoint;
    int health;
    float hitValue;
    float hitValueSlowdown;
    int hitId;
    float deathValue;
    int deathId;
    bool isDead;
    int navId = 0;
    float warpToSpawnValue;
    Vector3 warpInit;
    Vector3 warpTarget;
    Vector3 warpNextPoint;
    protected Vector3 lastNextPoint;
    protected float realY;
    protected float realSpeedY;


    static int navIdStatic = 0;
    public void InitNavigator (int spawn, int path, Vector3 initPos)
    {
        navId = navIdStatic++;
        agent.enabled = true;

        agent.speed = navigSpeed;
        navigatorSpawn = spawn;
        navigatorPath = path;
        navigatorNextPoint = 0;
        agent.Warp(initPos);
    }

    public void WarpToSpawn (int spawn, int path, Vector3 initPos, Vector3 nextPoint)
    {
        navigatorSpawn = spawn;
        navigatorPath = path;
        warpToSpawnValue = 1f;
        agent.enabled = false;

        warpInit = transform.position;
        warpTarget = initPos;
        warpNextPoint = nextPoint;
    }

    public static Vector3 GetInterpolatedPosition (Vector3 startPoint, Vector3 endPoint, float height, float t)
    {
        float parabolicT = Mathf.Sin(t * Mathf.PI);

        Vector3 interpolatedPosition = Vector3.Lerp(startPoint, endPoint, t);
        interpolatedPosition.y += parabolicT * height;

        return interpolatedPosition;
    }

    public void SetNextPoint (int nextPoint, Vector3 nextPointPosition) {
        agent.SetDestination(nextPointPosition);
        lastNextPoint = nextPointPosition;
        navigatorNextPoint = nextPoint;
    }

    public (int nextPoint, int spawn, int path) GetState () => (navigatorNextPoint, navigatorSpawn, navigatorPath);

    private void Start ()
    {
        hitId = Shader.PropertyToID("_Hit");
        deathId = Shader.PropertyToID("_Death");
        materialCopies = new List<Material>();
        foreach (var renderer in hitRenderer)
        {
            if (renderer == null || renderer.materials.Length == 0) continue;

            if(renderer.materials.Length == 1)
            {
                var newMat = new Material(renderer.material);
                renderer.material = newMat;
                materialCopies.Add(newMat);
            }
            else
            {
                var materials = new Material[renderer.materials.Length];
                for(int i = 0; i < materials.Length; i++)
                {
                    materials[i] = new Material(renderer.materials[i]);
                    materialCopies.Add(materials[i]);
                }
                renderer.materials = materials;
            }
        }
        OnInit();
        Restore();
    }

    public void Restore ()
    {
        health = defaultHealth;
        SetHitValue(0f);
        SetDeathValue(1f);
        isDead = false;
        OnRestore();

        foreach(var restorable in restorables)
        {
            restorable.Value.Restore();
        }
    }

    public void Kill ()
    {
        health = 0;
        isDead = true;
        deathValue = 0f;
        OnDeath();
        foreach (var listeners in deathListeners)
        {
            listeners.Value.OnDeath();
        }
    }

    public void TriggerHitPlayer ()
    {
        OnHitPlayer();
    }


    public bool IsDead => isDead;
    public bool OffSimulation => isDead && deathValue >= 1f;

    protected virtual void OnInit () { }

    protected virtual void OnHitPlayer () { }

    protected virtual void OnRestore () { }

    protected virtual void OnDeath () { }

    protected virtual void OnUpdate () { }

    protected virtual void OnFixedUpdate () { }

    public void ManualUpdate ()
    {
        if(Time.deltaTime == 0f) { return; }

        if (hitValue > 0)
        {
            hitValue = Mathf.Clamp01(hitValue - Time.deltaTime * hitAnimSpeed);
            SetHitValue(hitValue);
        }
        hitValueSlowdown = Mathf.Clamp01(hitValueSlowdown - Time.deltaTime * hitSlowdownSpeed);
        if (isDead)
        {
            deathValue = Mathf.Clamp01(deathValue + Time.deltaTime * deathAnimSpeed);
            SetDeathValue(deathValue);
        }
        else if(deathValue > 0)
        {
            deathValue = Mathf.Clamp01(deathValue - Time.deltaTime * deathAnimSpeed);
            SetDeathValue(deathValue);
        }

        if(warpToSpawnValue > 0)
        {
            warpToSpawnValue = Mathf.Clamp01(warpToSpawnValue - Time.deltaTime * warpToSpawnSpeed);
            transform.position = GetInterpolatedPosition(warpInit, warpTarget, 30f, 1f-warpToSpawnValue);

            if(warpToSpawnValue == 0f)
            {
                InitNavigator(navigatorSpawn, navigatorPath, warpTarget);
                SetNextPoint(0, warpNextPoint);
            }
        }

        
        if(agent.isOnNavMesh)
        {
            if(agent.isOnOffMeshLink && agent.currentOffMeshLinkData.valid)
            {
                //agent.CompleteOffMeshLink();
                //agent.speed = navigSpeed * offMeshMultiplier;
                var link = agent.currentOffMeshLinkData;
                var target = new Vector3(link.endPos.x, transform.position.y, link.endPos.z);
                if(offMeshLinkVertical)
                {
                    target.y = link.endPos.y;
                }
                var diff = target - transform.position;
                agent.Move(Vector3.MoveTowards(Vector3.zero, diff, linkSpeed * Time.deltaTime));
                if(diff.sqrMagnitude < offMeshLinkDistance * offMeshLinkDistance)
                {
                    transform.position = link.endPos + agent.height * 0.5f * Vector3.up;
                    agent.CompleteOffMeshLink();
                }
            }
            else
            {
                if(isDead)
                {
                    agent.speed = 0f;
                }
                else
                {
                    agent.speed = navigSpeed * (1f-hitValueSlowdown);
                }
            }
        }

        realY = Mathf.Max(transform.position.y, realY + Time.deltaTime * realSpeedY);
        if (realY == transform.position.y)
        {
            realSpeedY = 0f;
        }
        else
        {
            realSpeedY += gravity * Time.deltaTime;
        }

        OnUpdate();

        if (agent.isOnNavMesh && !isDead && !agent.isOnOffMeshLink)
        {
            Vector3 randomOffset = new Vector3(
                noise.snoise(new float2(Time.time * swerveSpeed, navId)),
                0f,
                noise.snoise(new float2(Time.time + 100f, navId)));
            randomOffset = Vector3.Project(randomOffset, transform.right);

            agent.Move(randomOffset * (Time.deltaTime * swerveNoise * (1f-hitValueSlowdown)));

        }
    }

    public void ManualFixedUpdate () => OnFixedUpdate();

    public bool ApplyHit (int damage)
    {
        if (isDead) return false;

        hitValue = 1f;
        hitValueSlowdown = 1f;
        health -= damage;
        
        if(health <= 0)
        {
            Kill();
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool ApplyFireHit ()
    {
        if (isDead) return false;

        return ApplyHit(damage: 25);
    }

    private void SetHitValue (float value)
    {
        foreach (var mat in materialCopies)
        {
            mat.SetFloat(hitId, value);
        }
    }
    private void SetDeathValue (float value)
    {
        deathValue = value;
        foreach (var mat in materialCopies)
        {
            mat.SetFloat(deathId, deathValue);
        }
    }
}
