using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerController : MonoBehaviour
{
    [SerializeField] private float movementspeed;
    private Rigidbody rb;
    private Vector3 movedirection;

    public Animator anime;
    
    
    [SerializeField] private LayerMask groundLayer;
    
    public Vector3 targetPosition;

    private bool move;
    
    
    [SerializeField] private float reach;

    [SerializeField] private LayerMask interactionlayer;

    public Vector3 groundPos;
    
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    
    void Update()
    {
        anime.SetFloat("sidemovement", movedirection.x);
        
        movedirection = new Vector3(Input.GetAxis("Horizontal"),0, Input.GetAxis("Vertical"));
        
        RaycastHit hit;
        
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out hit, Mathf.Infinity))
        {
            transform.position = hit.point;
        }
        
        if (Input.GetKeyDown(KeyCode.F))
        { 
            if (Physics.Raycast(transform.position, Vector3.forward, out hit, reach, interactionlayer)) 
            {
                
            }
        }
    }
    void FixedUpdate()
    {
        rb.velocity = movedirection * movementspeed;
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawRay(transform.position, Vector3.forward * reach);
        
        Gizmos.DrawWireSphere(transform.position + transform.forward * reach, 0.2f);
    }
}
