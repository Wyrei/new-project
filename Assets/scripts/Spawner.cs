using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class Spawner : MonoBehaviour
{
    [SerializeField] public List<container> mycon = new List<container>();

    

    void Update()
    {
        foreach (container con in mycon)
        {
            if (con.EnemiesSpawned <= con.MaxEnemiesCanSpawn) 
            {
                con.Timer -= Time.deltaTime;
                if (con.Timer <= 0)
                {
                    Vector3 randomPosition = con.spawnpoint.transform.position + new Vector3(Random.Range(con.reach.x, -con.reach.x), 0, Random.Range(con.reach.y, -con.reach.y));
                    Instantiate(con.enemyPrefab, randomPosition, quaternion.identity);
                    con.Timer = con.ResetTimer;
                    con.EnemiesSpawned += 1;
                } 
            }
            else
            {
                
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        foreach (container con in mycon)
        {
            Gizmos.DrawWireCube(con.spawnpoint.transform.position, con.reach);    
        }
    }
}
