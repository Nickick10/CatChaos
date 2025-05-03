using System.ComponentModel;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Attributes")]
    public float attackRange = 2f;
    public float pushForce = 500f;
    [Tooltip("Size of the attack cone in degrees.")]
    [Range(0f, 180f)]
    public float attackAngle = 45f;
    public float attackCooldown = 1f; // Cooldown in seconds

    private float lastAttackTime = -Mathf.Infinity; // So player can attack immediately

    [Header("Layer")]
    [SerializeField] private LayerMask pushableLayers; // Layers that the attack affects. 
    // (We'll just use the breakable objects layer since that's the only thing that should be affected!)

    [Header("Layer")]
    [SerializeField] private AudioSource attackSound;

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && Time.time >= lastAttackTime + attackCooldown)
        {
            Attack();
        }
    }

    void Attack()
    {
        lastAttackTime = Time.time; // Reset cooldown timer

        attackSound.Play();

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange, pushableLayers);

        foreach (Collider hitCollider in hitColliders)
        {
            Vector3 directionToTarget = (hitCollider.transform.position - transform.position).normalized;

            float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);

            if (angleToTarget <= attackAngle)
            {
                Rigidbody rb = hitCollider.attachedRigidbody;
                if (rb != null && !rb.isKinematic)
                {
                    rb.AddForce(directionToTarget * pushForce);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.blue;
        Vector3 rightLimit = Quaternion.Euler(0, attackAngle, 0) * transform.forward;
        Vector3 leftLimit = Quaternion.Euler(0, -attackAngle, 0) * transform.forward;
        Gizmos.DrawRay(transform.position, rightLimit * attackRange);
        Gizmos.DrawRay(transform.position, leftLimit * attackRange);
    }
}
