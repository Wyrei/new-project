using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class GridManager : MonoBehaviour
{
    [SerializeField] private int _Width,_Deept;

    [SerializeField] private Tile _TilePrefab;
    
    [SerializeField] private GameObject _PlayerPrefab;
    
    public Vector3 tileZeroZero;

    private float distanceground = 0.2f;

    [SerializeField] private GameObject ParentTiles;

    [SerializeField] private Vector3 spawnpointEnemie;
    [SerializeField] private GameObject Enemie;

    public List<GameObject> spawnedEnemies;

    public bool enemyspawn;

    public float hight;
    
    public bool MapIsMade;

    public int randomspawn;

    public List<Tile> tiles;

    public Vector3 playerpos;

    public GameObject player;

    public Vector3 sps;
    
    private void Start()
    {
        if (MapIsMade == false)
        {
            GenerateGrid();
            InstantiatePlayerOnTileZeroZero();
            MapIsMade = true;
        }
    }
    
    public void highLightTilesInRange(Vector3 center, int range)
    {
        
            foreach (var tile in tiles)
            {
                float distance = Vector3.Distance(center, tile.transform.position);
                bool isInrange = distance <= range;
                tile.HighLight(isInrange);
            }     
        
    }

    public void clearHighlight()
    {
        foreach (var tile in tiles)
        {
            tile.HighLight(false);
        }
    }
    public void GenerateGrid()
    {
        playerpos = player.transform.position;
        tiles = new List<Tile>();

        float startX = playerpos.x - (_Width / 2);
        float startZ = playerpos.z - (_Width / 2);
        
        
        for (int x = 0; x < _Width; x++)
        {
            for (int z = 0; z < _Deept; z++)
            {
                
                sps  = new Vector3(x, playerpos.y, z);

                Vector3 tilepos = new Vector3(startX + x, playerpos.y, startZ + z);
                
                if (!isPositionOccupied(playerpos))
                {
                    var spawnedTile = Instantiate(_TilePrefab, tilepos, Quaternion.Euler(90, 0, 0));
                    tiles.Add(spawnedTile);
                    
                    
                    float xR = Random.Range(10, 20);
                    float zR = Random.Range(0, 10);

                    Vector3 sp = new Vector3(xR, hight + 0.5f, zR);
                    
                    spawnedTile.name = $"Tile {x} {z}";
                    spawnedTile.transform.SetParent(ParentTiles.transform);

                    var isOffset = (x % 2 == 0 && z % 2 != 0) || (x % 2 != 0 && z % 2 == 0);
                    spawnedTile.Init(isOffset);
                    
                }
            }
        }
    }

    bool isPositionOccupied(Vector3 position)
    {
        Collider[] colliders = Physics.OverlapSphere(position, 0.5f);

        foreach(Collider col in colliders)
        {
            if(col.CompareTag("obs"))
            {
                return true;     
            }
           
        }

        return false;
    }
    void InstantiatePlayerOnTileZeroZero()
    {
        Instantiate(_PlayerPrefab, tileZeroZero, Quaternion.identity);

        //Instantiate(Enemie, spawnpointEnemie, quaternion.identity);
    }

    public void regererateGrid()
    {
        tiles.Clear();
        removeOldGrid();
        GenerateGrid();
        InstantiatePlayerOnTileZeroZero();
        MapIsMade = true;
    }

    public void removeOldGrid()
    {
        
        
            spawnedEnemies.Clear();
            tiles.Clear();
            
            DestroyImmediate(GameObject.FindGameObjectWithTag("Player"));
            DestroyImmediate(GameObject.FindGameObjectWithTag("Enemy"));
            
            foreach (var obj in ParentTiles.GetComponentsInChildren<Transform>())
            {
                if (obj != ParentTiles.transform)
                {
                    DestroyImmediate(obj.gameObject);
                    DestroyImmediate(GameObject.FindGameObjectWithTag("Enemy"));
                }
            }     
        
       
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(GridManager))] public class GridManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GridManager gridManager = (GridManager)target;

        if (GUILayout.Button("Regenerate Grid"))
        {
            gridManager.regererateGrid();
        }
        
        if (GUILayout.Button("Remove Grid"))
        {
            gridManager.removeOldGrid();
        }
    }
}
#endif
