using UnityEngine;

/// <summary>
/// Handles a spatial "King of the Hill" style hacking mechanic.
/// The player must stay inside the Trigger Zone for X continuous seconds.
/// </summary>
public class NodeHacking : MonoBehaviour
{
    [Header("Hacking Settings")]
    [Tooltip("Consecutive seconds required to stay inside the node to successfully hack it")]
    [SerializeField] private float requiredHackTime = 3.0f;
    
    [Tooltip("Amount of Data Fragments awarded upon successful hack")]
    [SerializeField] private int rewardAmount = 50;

    [Header("Visual Feedback")]
    [Tooltip("SpriteRenderer representing the node's visual state")]
    [SerializeField] private SpriteRenderer nodeGraphic;
    
    [Tooltip("The color when the node is idle")]
    [SerializeField] private Color idleColor = new Color(0f, 0.4f, 1f, 0.6f); // Cyan
    
    [Tooltip("The color right before it completes")]
    [SerializeField] private Color hackFinishedColor = new Color(0f, 1f, 0.2f, 1f); // Neon Green

    [Header("FX")]
    [Tooltip("Prefab instantiated when the hack is successful")]
    public GameObject successParticles;

    private float currentHackTimer = 0f;
    private bool isPlayerInZone = false;

    private void Start()
    {
        currentHackTimer = 0f;
        UpdateVisuals();
    }

    private void Update()
    {
        if (isPlayerInZone)
        {
            currentHackTimer += Time.deltaTime;
            UpdateVisuals();

            if (currentHackTimer >= requiredHackTime)
            {
                CompleteHack();
            }
        }
        else if (currentHackTimer > 0)
        {
            // The prompt requests: "If the player leaves the zone early reset the hacking timer"
            currentHackTimer = 0f;
            UpdateVisuals();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // The attached object MUST be tagged "Player"
        if (collision.CompareTag("Player"))
        {
            isPlayerInZone = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInZone = false;
        }
    }

    private void UpdateVisuals()
    {
        if (nodeGraphic != null)
        {
            float progressPercentage = Mathf.Clamp01(currentHackTimer / requiredHackTime);
            
            // Fade color from Cyan -> Bright Green based on time
            nodeGraphic.color = Color.Lerp(idleColor, hackFinishedColor, progressPercentage);
            
            // Slightly expand the node's graphic as a physical progress indicator
            float scale = Mathf.Lerp(1.0f, 1.4f, progressPercentage);
            nodeGraphic.transform.localScale = new Vector3(scale, scale, 1f);
        }
    }

    private void CompleteHack()
    {
        // Prevent multiple triggers in the same frame
        isPlayerInZone = false;

        Debug.Log($"[NodeHacking] Successfully decrypted node! Rewarding {rewardAmount} Data Fragments.");

        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.AddFragments(rewardAmount);
        }

        // Spawn a burst effect
        if (successParticles != null)
        {
            Instantiate(successParticles, transform.position, Quaternion.identity);
        }

        // Optional bonus points for leaderboard
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(50);
        }

        // Erase the hack node from the world
        Destroy(gameObject);
    }
}
