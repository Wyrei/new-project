using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using TMPro;
using UnityEngine.EventSystems;

public class playerController : MonoBehaviour
{
    [SerializeField] private float movementspeed;
    private CharacterController ch;
    private Vector3 movedirection;

    public float gravity;
    public float groundcheckDistance = 0.1f;

    private Vector3 velocity;
    private bool isGrounded;
    
    public Animator anime;

    public bool mods, gengrid = true;
    
    
    [Tooltip("How fast you move over the tiles.")]
    public float movementSpeed = 1f;
    [Tooltip("How big the steps are the player takes over the tiles.")]
    public float tileSize = 1f; 
    [Tooltip("How many steps you can take.")]
    public int currentMovementRange;
    [Tooltip("Resets Current Movement Range when turn starts.")]
    public int initialMovementRange = 5; 
    
    [Tooltip("The layer the character moves over and raycast looks for when playing.")]
    public LayerMask groundLayerMask; 
    [Tooltip("layer of the enemy when attacking.")]
    public LayerMask enemyLayerMask;
    [Tooltip("particles for attacking.")]
    public GameObject attackParticlesPrefab;
    
    [HideInInspector] public bool attack = true;
    [HideInInspector] public bool MoveToPosBool;
    
    public TMP_Text damageTextPrefab;
    
    private Vector3 targetPosition; 
    private bool isAbleToMove = true;
    private GameObject selectedUnit;
    private PlayerStats selectedPlayerStats;
    private GridManager gridManager;
    private TurnManager _GM;
    
    private bool isWalking;

    public Camera cam;

    public GridManager grid;
    
    void Start()
    {
        ch = GetComponent<CharacterController>();
        
        selectedPlayerStats = GetComponent<PlayerStats>();
        currentMovementRange = initialMovementRange; //resetting movement
        targetPosition = transform.position;
        gridManager = FindObjectOfType<GridManager>();
        _GM = FindObjectOfType<TurnManager>();
    }
    void Update()
    {
        
        if (mods == false)
        {
            isGrounded = Physics.Raycast(transform.position, Vector3.down, groundcheckDistance, groundLayerMask);

            if (isGrounded && velocity.y < 0)
            {
                velocity.y = 0f;
            }
            
            anime.SetFloat("sidemovement", movedirection.x);
        
            movedirection = new Vector3(Input.GetAxis("Horizontal"),0, Input.GetAxis("Vertical"));

            ch.Move(movedirection * movementspeed * Time.deltaTime);

            velocity.y -= gravity * Time.deltaTime;
            ch.Move(velocity * Time.deltaTime);
            
            new Vector3(0,gravity * Time.deltaTime,0);
        }
        else if (mods == true)
        {
            if (gengrid == true)
            {
                grid.GenerateGrid();
                gengrid = false;

                targetPosition = transform.position;
            }
            
              // Checks if there if UI in raycast ray
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        
        //handles left mouse button click for selecting unit and moving
        if (Input.GetMouseButtonDown(0) && isAbleToMove && _GM.TurnSystem == true)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit))
            {
                GameObject clickedObject = hit.collider.gameObject;

                // Select a unit if clicked
                if (clickedObject.CompareTag("Player"))
                {
                    SelectUnit(clickedObject);
                }
                // move to selected unit if valid position is clicked
                else if (selectedUnit != null && currentMovementRange > 0 && MoveToPosBool == false)
                {
                    MoveToPosition(hit.point);
                }
            }
        }
        // right mouse click handles attacking
        else if (Input.GetMouseButtonDown(1) && selectedUnit != null)
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, enemyLayerMask))
            {
                Vector3 enemyposition = hit.collider.transform.position;
                float distance = Vector3.Distance(transform.position, enemyposition) / tileSize;
                
                if (distance <= 1 && attack == true)
                {
                    attack = false;
                    
                    EnemyStats enemyHealth = hit.collider.GetComponent<EnemyStats>();
                    enemyHealth.TakeDamage(selectedPlayerStats.damage);
                    
                    GameObject particle = Instantiate(attackParticlesPrefab, hit.transform.position, quaternion.identity);
                    StartCoroutine(DestroyParticlesAfterDelay(particle, 5f));
                    
                    TMP_Text _text = Instantiate(damageTextPrefab, hit.transform.position + Vector3.up, quaternion.identity);
                    _text.text = selectedPlayerStats.damage.ToString();
                }
            }
        }
        
        // move towards target position
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, movementSpeed * Time.deltaTime); 
        }
    }

    // select and deselect units
    public void SelectUnit(GameObject unit)
    {  
        if (selectedUnit == unit)
        {
            selectedPlayerStats.Deselect();
            selectedUnit = null;
            selectedPlayerStats = null;
            gridManager.clearHighlight();
            return;
        }
        
        if (selectedUnit != null)
        {
            selectedPlayerStats.Deselect(); 
            gridManager.clearHighlight();
        }
        
        selectedUnit = unit;
        selectedPlayerStats = unit.GetComponent<PlayerStats>();
        selectedPlayerStats.Select();
        
        gridManager.highLightTilesInRange(selectedUnit.transform.position, currentMovementRange);
    }

    // clear highlighted tiles
    public void clearHighlight()
    {
        gridManager.clearHighlight();
    }

    // move to the specified position if within range
    void MoveToPosition(Vector3 destination)
    {
        Ray ray = new Ray(transform.position, destination - transform.position);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, currentMovementRange * tileSize, groundLayerMask))
        {
            Vector3 gridPosition = RoundToNearestTile(hit.point);
            float distance = Vector3.Distance(transform.position, gridPosition) / tileSize;

            if (distance <= currentMovementRange && !IsTileOccupied(gridPosition) && selectedUnit != null && selectedUnit.transform == transform)
            {
                isWalking = true;
                isAbleToMove = false;
                StartCoroutine(MoveToDestination(gridPosition));
            }
        }
    }
    
    // destroys particles after a delay float
    IEnumerator DestroyParticlesAfterDelay(GameObject particle, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(particle);
    }

    // moves unit to testination tile by tile
    IEnumerator MoveToDestination(Vector3 destination)
    {
        gridManager.clearHighlight();
        
        while (transform.position != destination)
        {
            Vector3 currentPos = transform.position;
            Vector3 diff = destination - transform.position;

            if (Mathf.Abs(diff.x) > 0.000001f)
            {
                float targetX = Mathf.MoveTowards(currentPos.x, destination.x, tileSize);
                targetPosition = new Vector3(targetX, currentPos.y, currentPos.z);

                if (isWalking == true)
                {
                    currentMovementRange -= Mathf.CeilToInt(Mathf.Abs(destination.x - currentPos.x));
                    isWalking = false;
                }
               
            }
            else if (Mathf.Abs(diff.z) > 0.1f)
            {
                float targetZ = Mathf.MoveTowards(currentPos.z, destination.z, tileSize);
                targetPosition = new Vector3(currentPos.x,currentPos.y,targetZ);

                if (isWalking == true)
                {
                    currentMovementRange -= Mathf.CeilToInt(Mathf.Abs(destination.z - currentPos.z));
                    isWalking = false;
                }
                
            }
            
            transform.position = Vector3.MoveTowards(currentPos, targetPosition, tileSize * Time.deltaTime);
            
            yield return null;
        }
        if (Vector3.Distance(transform.position, destination) < 0.1f)
        {
            gridManager.highLightTilesInRange(selectedUnit.transform.position, currentMovementRange);
            isAbleToMove = true;
        }
    }

    //round a position the nearest tile grid
    Vector3 RoundToNearestTile(Vector3 position)
    {
        float x = Mathf.Round(position.x / tileSize) * tileSize;
        float z = Mathf.Round(position.z / tileSize) * tileSize;

        return new Vector3(x,transform.position.y, z);
    }

    //check if a tile is occupied
    bool IsTileOccupied(Vector3 position)
    {
        Collider[] colliders = Physics.OverlapSphere(position, 0.1f, groundLayerMask);

        foreach (Collider col in colliders)
        {
            if (col.gameObject != gameObject)
            {
                return true;
            }
        }
        return false;
    }
}
