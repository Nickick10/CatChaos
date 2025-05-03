using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public enum PowerUpType { Speed, Jump }

public class PowerUpScript : MonoBehaviour
{
    public PowerUpType powerUpType; //Set object as Speed or Jump power-up
    public float duration = 15f; //Duration of the power-up
    public float multiplier = 2f; //Multiply power-up effect by this value

    private bool activated = false;

    public GameObject sparkleEffectPrefab;

    public Image speedIcon;
    public Image jumpIcon;

    private AudioSource audioSource;
    public AudioClip speedPickupSound;
    public AudioClip jumpPickupSound;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        StartCoroutine(HideIconsAtStart());
    }

    IEnumerator HideIconsAtStart()
    {
        yield return null; //Wait 1 frame to ensure icons are initialized
        speedIcon.enabled = false;
        jumpIcon.enabled = false;
    }

    private void OnTriggerStay(Collider other)
    {
        //Check if player presses E key on item
        if (!activated && other.CompareTag("Player") && Input.GetKeyDown(KeyCode.E))
        {
            activated = true;
            //Get the player's movement script
            PlayerMovement player = other.GetComponent<PlayerMovement>();
            if (player != null)
            {
                //Start the boost on the player script
                if (powerUpType == PowerUpType.Speed && !speedIcon.enabled)
                {
                    //Instantiate sparkle effect at the power-up position
                    GameObject sparkle = Instantiate(sparkleEffectPrefab, transform.position, Quaternion.identity);
                    Destroy(sparkle, 2f);

                    //Plays speed powerup sound
                    AudioSource.PlayClipAtPoint(speedPickupSound, transform.position);

                    player.StartCoroutine(ApplySpeedBoost(player));
                }
                else if (powerUpType == PowerUpType.Jump && !jumpIcon.enabled)
                {
                    //Instantiate sparkle effect at the power-up position
                    GameObject sparkle = Instantiate(sparkleEffectPrefab, transform.position, Quaternion.identity);
                    Destroy(sparkle, 2f);

                    //Plays jump powerup sound and lowers its volume to 0.2 of the original
                    GameObject tempGO = new GameObject("TempJumpSound");
                    AudioSource tempSource = tempGO.AddComponent<AudioSource>();
                    tempSource.clip = jumpPickupSound; tempSource.volume = 0.2f; tempSource.spatialBlend = 1f; tempSource.transform.position = transform.position; tempSource.Play();
                    Destroy(tempGO, jumpPickupSound.length);

                    player.StartCoroutine(ApplyJumpBoost(player));
                }
            }
            StartCoroutine(RespawnPowerUp());
        }
    }

    private IEnumerator RespawnPowerUp()
    {
        // Disable renderer and collider, not the whole object
        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<Collider>().enabled = false;
        
        // Wait
        yield return new WaitForSeconds(30f);

        // Re-enable renderer and collider
        GetComponent<MeshRenderer>().enabled = true;
        GetComponent<Collider>().enabled = true;
        activated = false;
    }


    private IEnumerator ApplySpeedBoost(PlayerMovement player)
    {   
        speedIcon.enabled = true; //Enable speed icon
        float originalSpeed = player.speed; //Save original speed
        player.speed *= multiplier; //Increase speed
        yield return new WaitForSeconds(duration); //Wait for the boost duration
        player.speed = originalSpeed; //Reset speed to original value
        speedIcon.enabled = false; //Disable speed icon
    }

    private IEnumerator ApplyJumpBoost(PlayerMovement player)
    {   
        jumpIcon.enabled = true; //Enable jump icon
        float originalForce = player.force; //Save original jump force
        player.force *= multiplier; //Increase jump force
        yield return new WaitForSeconds(duration); // ait for the boost duration
        player.force = originalForce; //Reset jump force to original value
        jumpIcon.enabled = false; //Disable jump icon
    }
}