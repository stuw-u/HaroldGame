using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] Enemy[] prefabs;
    [SerializeField] float[] randomWeights;
    [SerializeField] Transform[] spawnPoints;

    [SerializeField] float minSpawnInterval = 2f;
    [SerializeField] float maxSpawnInterval = 0.4f;
    [SerializeField] float spawnMaxIntervalTime = 8f;
    [SerializeField] float pathPointRadius = 0;

    private List<Enemy> managedEnemies;
    private float spawnTimer = 0f;
    private Vector3[][][] paths;
    private float spawnValue;

    private static EnemyManager inst;

    public void Clear ()
    {
        for (int i = managedEnemies.Count - 1; i >= 0; i--)
        {
            Destroy(managedEnemies[i].gameObject);
        }
        managedEnemies.Clear();
    }

    public int GetRandomWeightedIndex (float[] weights)
    {
        if (weights == null || weights.Length == 0) return -1;

        float total = 0;
        foreach(float weight in weights)
        {
            total += weight;
        }

        float w, t = 0f;
        int i;
        for (i = 0; i < weights.Length; i++)
        {
            w = weights[i];
            if (float.IsPositiveInfinity(w)) return i;
            else if (w >= 0f && !float.IsNaN(w)) t += weights[i];
        }

        float r = Random.value;
        float s = 0f;

        for (i = 0; i < weights.Length; i++)
        {
            w = weights[i];
            if (float.IsNaN(w) || w <= 0f) continue;

            s += w / total;
            if (s >= r) return i;
        }

        return -1;
    }

    private void Awake ()
    {
        inst = this;
        managedEnemies = new List<Enemy>();

        paths = new Vector3[spawnPoints.Length][][];
        for(int i = 0; i < paths.Length; i++)
        {
            paths[i] = new Vector3[spawnPoints[i].childCount][];
            for(int j = 0; j < paths[i].Length; j++)
            {
                paths[i][j] = new Vector3[spawnPoints[i].GetChild(j).childCount];
                for (int k = 0; k < paths[i][j].Length; k++)
                {
                    paths[i][j][k] = spawnPoints[i].GetChild(j).GetChild(k).position;
                }
            }
        }
    }

    public static bool InDanger ()
    {
        return inst.spawnValue > 0.75f && GameManager.IsPlayerInControl;
    }
    public void OnCollectCrate ()
    {
        spawnValue = 0f;
    }

    private void Update ()
    {
        foreach (var enemy in managedEnemies)
        {
            enemy.ManualUpdate();
        }

        if (!GameManager.IsEnemySpawningAllowed) {
            spawnValue = 0f;
            return;
        }

        spawnValue = Mathf.Clamp01(spawnValue + Time.deltaTime / spawnMaxIntervalTime);
        float spawnInterval = Mathf.Lerp(minSpawnInterval, maxSpawnInterval, spawnValue);
        if(spawnTimer > spawnInterval)
        {
            int randomSpawnPoint = Random.Range(0, spawnPoints.Length);
            int randomEnemy = GetRandomWeightedIndex(randomWeights); //Random.Range(0, prefabs.Length);
            int randomPath = Random.Range(0, paths[randomSpawnPoint].Length);

            var enemy = Instantiate(prefabs[randomEnemy], spawnPoints[randomSpawnPoint].position, Quaternion.identity);
            enemy.InitNavigator(randomSpawnPoint, randomPath, enemy.transform.position);
            enemy.SetNextPoint(0, paths[randomSpawnPoint][randomPath][0]);
            managedEnemies.Add(enemy);

            spawnTimer = 0;
        }
        spawnTimer += Time.deltaTime;

        for(int i = managedEnemies.Count - 1; i >= 0; i--)
        {
            var enemy = managedEnemies[i];
            if (enemy.transform.position.y < -5)
            {
                enemy.Kill();
            }
            if(enemy.OffSimulation)
            {
                Destroy(enemy.gameObject);
                managedEnemies.RemoveAt(i);
            }
            else
            {
                (int point, int spawn, int path) = enemy.GetState();
                float distSq = (enemy.transform.position - paths[spawn][path][point]).sqrMagnitude;
                enemy.debugDist = Mathf.Sqrt(distSq);
                if (distSq > pathPointRadius * pathPointRadius) continue;

                if (point + 1 >= paths[spawn][path].Length)
                {
                    enemy.WarpToSpawn(spawn, path, spawnPoints[spawn].position, paths[spawn][path][0]);
                }
                else
                {
                    enemy.SetNextPoint(point + 1, paths[spawn][path][point + 1]);
                }
            }
        }
    }

    private void FixedUpdate ()
    {
        foreach (var enemy in managedEnemies)
        {
            enemy.ManualFixedUpdate();
        }
    }
}
