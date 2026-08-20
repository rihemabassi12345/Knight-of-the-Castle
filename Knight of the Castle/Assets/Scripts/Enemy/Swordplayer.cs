using UnityEngine;

public class SwordPlayer : MonoBehaviour
{
    private Animator anim;

    [Header("Attack Settings")]
    [Tooltip("Global speed multiplier for attack animations.")]
    public float attackSpeed = 1.35f; 
    [Tooltip("Blend duration between attacks in seconds.")]
    public float crossFadeDuration = 0.08f; 
    [Tooltip("Time window after an attack starts to chain the next one.")]
    public float comboWindow = 0.8f; 

    [Header("Animation State Names")]
    [Tooltip("Animator state names for the first hit variations.")]
    public string[] hit1StateNames = new string[] { "hit1", "hit1_B", "hit1_C" };
    [Tooltip("Animator state names for the second hit variations.")]
    public string[] hit2StateNames = new string[] { "hit2", "hit2_B", "hit2_C" };

    private int comboStep = 0;
    private float lastAttackTime = 0f;
    private const int MAX_COMBO = 2;

    // Converted Animator Hashes
    private int[] hit1Hashes;
    private int[] hit2Hashes;

    // Track last played indices to prevent immediate repeats
    private int lastHit1Index = -1;
    private int lastHit2Index = -1;

    void Start()
    {
        anim = GetComponent<Animator>();
        anim.speed = attackSpeed;

        // Convert string state names to hashes for maximum performance
        hit1Hashes = ConvertToHashes(hit1StateNames);
        hit2Hashes = ConvertToHashes(hit2StateNames);
    }

    void Update()
    {
        // Reset combo if the player waits too long
        if (comboStep > 0 && Time.time - lastAttackTime > comboWindow)
        {
            comboStep = 0;
        }

        // Handle Input
        if (Input.GetMouseButtonDown(0))
        {
            ExecuteComboAttack();
        }
    }

    void ExecuteComboAttack()
    {
        comboStep++;

        if (comboStep > MAX_COMBO)
        {
            comboStep = 1; 
        }

        lastAttackTime = Time.time;

        if (comboStep == 1)
        {
            int targetHash = GetRandomAnimationHash(hit1Hashes, ref lastHit1Index);
            anim.CrossFadeInFixedTime(targetHash, crossFadeDuration);
        }
        else if (comboStep == 2)
        {
            int targetHash = GetRandomAnimationHash(hit2Hashes, ref lastHit2Index);
            anim.CrossFadeInFixedTime(targetHash, crossFadeDuration);
        }
    }

    // Picks a random animation hash without repeating the previous index if multiple choices exist
    private int GetRandomAnimationHash(int[] hashArray, ref int lastIndex)
    {
        if (hashArray == null || hashArray.Length == 0)
        {
            Debug.LogError("No animation states defined in array!");
            return 0;
        }

        if (hashArray.Length == 1)
        {
            return hashArray[0];
        }

        int randomIndex;
        do
        {
            randomIndex = Random.Range(0, hashArray.Length);
        } 
        while (randomIndex == lastIndex);

        lastIndex = randomIndex;
        return hashArray[randomIndex];
    }

    private int[] ConvertToHashes(string[] names)
    {
        int[] hashes = new int[names.Length];
        for (int i = 0; i < names.Length; i++)
        {
            hashes[i] = Animator.StringToHash(names[i]);
        }
        return hashes;
    }
}