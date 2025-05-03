//lets add some target
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class Target : MonoBehaviour
{
    public Score scoreManager;
    public float destructionForceThreshold = 100f;

    public float breakNoise = 30f;

    [SerializeField] private AudioSource breakSound;
 
    //this method is called whenever a collision is detected
    private void OnCollisionEnter(Collision collision) {
        //on collision adding point to the score
        if(collision.gameObject.layer == 6){
            // printing if collision is detected on the console
            Debug.Log("Collision Detected");

            // Get the total impulse from the collision
            Vector3 impulse = collision.impulse;
            float impactForce = impulse.magnitude / Time.fixedDeltaTime;

            Debug.Log($"Impact Force: {impactForce}");

            // Check if the force is over the threshold
            if (impactForce >= destructionForceThreshold)
            {
                DestroyObject();
            }
        }
    }

    void DestroyObject()
    {
        Debug.Log($"{gameObject.name} was destroyed due to high impact! Point added.");
        scoreManager.AddScoreGain();
        breakSound.Play();
        NoiseManager.Instance.AddNoise(breakNoise);
        Destroy(gameObject); // Could also spawn particles, sounds, etc.
    }
}