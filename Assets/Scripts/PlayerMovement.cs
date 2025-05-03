using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

public class PlayerMovement : MonoBehaviour
{   
    private bool isMoving = false;
    private bool isGrounded = false;
    private Rigidbody rb;
    private Transform t;
    private Vector2 turn;

    [Header("Movement")]
    public float speed = 5.0f;
    public float rotationSpeed = 90;
    public float force = 250f;

    private Animator anim;
    
    [Header("Owner")]
    [SerializeField] private HomeownerAIScript ownerAI;

    [Header("Audio")]
    public AudioSource footstepAudioSource; // Should have loop = true
    public AudioSource jumpAudioSource;     // Should have loop = false
    public AudioClip jumpClip;

    [Header("Noise")]
    public float footstepNoise = 0.1f;

    [Header("Interact")]
    public TMP_Text interactPrompt; //Press E text object
    public float interactRange = 0.5f;

    Camera mainCamera;
    Camera FPCamera;
    bool isFirstPerson = false;

    void Start()
    {
        interactionVisibility(0f);

        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        t = GetComponent<Transform>();
        Cursor.lockState = CursorLockMode.Locked;

        // Ensure walking audio is looping
        if (footstepAudioSource != null)
        {
            footstepAudioSource.loop = true;
        }
        mainCamera = Camera.main;
        FPCamera = GameObject.FindGameObjectWithTag("FPCamera").GetComponent<Camera>();
        FPCamera.gameObject.SetActive(false); //Disable first person camera at start
    }

    void Update()
    {
        Vector3 moveDirection = Vector3.zero;

        isMoving = false;
        if (Input.GetKey(KeyCode.W)) { moveDirection += transform.forward; isMoving = true; }
        if (Input.GetKey(KeyCode.S)) { moveDirection -= transform.forward; isMoving = true; }
        if (Input.GetKey(KeyCode.D)) { moveDirection += transform.right; isMoving = true; }
        if (Input.GetKey(KeyCode.A)) { moveDirection -= transform.right; isMoving = true; }

        if (moveDirection != Vector3.zero)
        {
            moveDirection = moveDirection.normalized; //Normalize to prevent faster diagonal movement
        }

        anim.SetBool("isMoving", isMoving);

        // Move player
        rb.MovePosition(rb.position + moveDirection * speed * Time.deltaTime);

        // Rotate player and camera
        turn.x += Input.GetAxis("Mouse X");
        turn.y += Input.GetAxis("Mouse Y");
        turn.y = Mathf.Clamp(turn.y, -90, 90);
        transform.localRotation = Quaternion.Euler(0, turn.x, 0);
        
        if (isFirstPerson)
        {
            FPCamera.transform.localRotation = Quaternion.Euler(-turn.y, 0, 0);
        }
        else
        {
            mainCamera.transform.localRotation = Quaternion.Euler(-turn.y, 0, 0);
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log("Toggled first person camera.");
            if (isFirstPerson)
            {
                isFirstPerson = false;
                mainCamera.gameObject.SetActive(true);
                FPCamera.gameObject.SetActive(false);
            }
            else
            {
                isFirstPerson = true;
                mainCamera.gameObject.SetActive(false);
                FPCamera.gameObject.SetActive(true);
            }
        }

        // Play/stop walking sound
        if (isMoving && isGrounded)
        {
            if (!footstepAudioSource.isPlaying) {
                footstepAudioSource.Play();
            } 
            Debug.Log("Adding footstep noise.");
            NoiseManager.Instance.AddNoise(footstepNoise * Time.deltaTime);
        }
        else
        {
            if (footstepAudioSource.isPlaying)
                footstepAudioSource.Stop();
        }

        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * force);

            // Play jump sound
            if (jumpAudioSource != null && jumpClip != null)
            {
                jumpAudioSource.PlayOneShot(jumpClip);
            }
        }

        // Trigger persistent chase
        if (Input.GetKeyDown(KeyCode.X))
        {
            Debug.Log("Triggered pathfinding chase.");
            ownerAI.TriggerPersistentChase(12);
        }

        if (isFirstPerson)
        {
            checkInteractableFP();
        }
        else
        {
            checkInteractableTP();
        }
    }

    private void checkInteractableFP()
    {
        Ray ray = new Ray(FPCamera.transform.position, FPCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactRange)) {
            if (hit.collider.CompareTag("Interactable")) {
                interactionVisibility(1f); //Show prompt
            } else {
                interactionVisibility(0f); //Hide prompt
            }
        } else {
            interactionVisibility(0f); //Hide prompt
        }
    }

    private void checkInteractableTP()
    {
        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactRange)) {
            if (hit.collider.CompareTag("Interactable")) {
                interactionVisibility(1f); //Show prompt
            } else {
                interactionVisibility(0f); //Hide prompt
            }
        } else {
            interactionVisibility(0f); //Hide prompt
        }
    }

    private void interactionVisibility(float alpha) {
        Color color = interactPrompt.color;
        color.a = alpha;
        interactPrompt.color = color;
    }

    private void OnCollisionStay(Collision collision)
    {
        isGrounded = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        isGrounded = false;

        // Stop walking sound if player is airborne
        if (footstepAudioSource.isPlaying)
            footstepAudioSource.Stop();
    }
}