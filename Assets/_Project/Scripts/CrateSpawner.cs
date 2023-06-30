using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CrateSpawner : MonoBehaviour
{
    public ItemBox cratePrefab;
    public float initCreateHeight = 9.28f;
    public Vector3 offset;
    public Vector3 extents;
    public Transform[] avoidRegions;
    public float avoidRegionRadius = 5f;

    private Vector3 lastPos;
    private ItemBox lastCrate;

    public void SpawnCrate ()
    {
        for(int i = 0; i < 100; i++)
        {
            var randomPosition = new Vector3(
                Mathf.Lerp(offset.x - extents.x, offset.x + extents.x, Random.value),
                offset.y + extents.y,
                Mathf.Lerp(offset.z - extents.z, offset.z + extents.z, Random.value)
            );
            lastPos = randomPosition;
            if (NavMesh.SamplePosition(randomPosition, out var hit, 1000f, ~0))
            {
                foreach(var region in avoidRegions)
                {
                    if(Vector3.Distance(hit.position, region.position) < avoidRegionRadius)
                    {
                        goto breakout;
                    }
                }

                lastCrate = Instantiate(cratePrefab, hit.position, Quaternion.identity);
                return;
            }
        breakout: continue;
        }
        Debug.LogError("Could not find any position somehow!");
    }

    public void Clear ()
    {
        if (lastCrate != null)
        {
            Destroy(lastCrate.gameObject);
        }

        lastCrate = Instantiate(cratePrefab, Vector3.up * initCreateHeight, Quaternion.identity);
    }

    private void OnDrawGizmos ()
    {
        Gizmos.DrawSphere(lastPos, 2f);
    }

    private void OnDrawGizmosSelected ()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(offset, extents*2f);
    }
}
