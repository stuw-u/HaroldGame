using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectedShadow : MonoBehaviour, IFlammable, IDeathListener
{
    [SerializeField] float length = 3;
    private UnityEngine.Rendering.Universal.DecalProjector decal;
    private bool isDead = false;

    private void Start ()
    {
        decal = GetComponent<UnityEngine.Rendering.Universal.DecalProjector>();
    }

    public void Restore ()
    {
        isDead = false;
    }

    public void OnDeath ()
    {
        isDead = true;
        decal.fadeFactor = 0f;
    }

    void LateUpdate()
    {
        if(!isDead)
        {
            if (Physics.Raycast(transform.parent.position, Vector3.down, out RaycastHit hit, length))
            {
                decal.enabled = true;
                transform.position = hit.point;
                decal.fadeFactor = 1f-(hit.distance / length);
            }
            else
            {
                decal.fadeFactor = 0f;
            }
        }
    }
}
