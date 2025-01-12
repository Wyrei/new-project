using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private Color _baseColor, _offSetColor, highLighedColor;

    [SerializeField] private SpriteRenderer _renderer;

    [SerializeField] private GameObject _highLight;

    [SerializeField] private GameObject InRange;

    public float gravity;
    public float groundcheckDistance = 0.1f;

    public Vector3 velocity;
    public bool isGrounded;

    public LayerMask groundLayerMask;

    public CharacterController ch;
    private void Update()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundcheckDistance, groundLayerMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = 0f;
        }
        
        velocity.y -= gravity;
        ch.Move(velocity);
    }

    public void Init(bool isOffset)
    {
        _renderer.color = isOffset ? _offSetColor : _baseColor;
    }

    public void HighLight(bool isInRangeOff)
    {
        InRange.SetActive(isInRangeOff);
    }
    void OnMouseEnter()
    {
        _highLight.SetActive(true);
    }

    void OnMouseExit()
    {
        _highLight.SetActive(false);
    }
}
