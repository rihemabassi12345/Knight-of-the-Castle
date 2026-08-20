using UnityEngine;

public class SwordPlayer : MonoBehaviour
{
    private Animator anim;

    [Header("Attack Speed & Timing")]
    public float attackSpeed = 1.2f;
    public float crossFadeDuration = 0.12f;

    [Header("Free-Flow Controls")]
    [Range(0f, 1f)]
    [Tooltip("How much movement speed is retained while attacking (0 = full stop, 1 = full speed).")]
    public float movementMultiplierDuringAttack = 0.45f;

    [Header("Animation State Names (UpperBody Layer)")]
    public string[] hit1StateNames = new string[] { "hit1", "hit1_B", "hit1_C" };
    public string[] hit2StateNames = new string[] { "hit2", "hit2_B", "hit2_C" };

    private int upperBodyLayerIndex;
    private int emptyStateHash;
    private int[] hit1Hashes;
    private int[] hit2Hashes;

    private int comboStep = 0;
    private const int MAX_COMBO = 2;
    private int lastHit1Index = -1;
    private int lastHit2Index = -1;

    private bool stateResetDone = false;

    public bool IsAttacking { get; private set; }

    private void Start()
    {
        anim = GetComponent<Animator>();

        // Find UpperBody layer index (fallback to layer 0 if missing)
        upperBodyLayerIndex = anim.GetLayerIndex("UpperBody");
        if (upperBodyLayerIndex == -1) upperBodyLayerIndex = 0;

        // Target default state on UpperBody layer
        emptyStateHash = Animator.StringToHash("UpperBody.Empty");

        hit1Hashes = ConvertToHashes(hit1StateNames);
        hit2Hashes = ConvertToHashes(hit2StateNames);
    }

    private void Update()
    {
        CheckAttackState();

        if (Input.GetMouseButtonDown(0))
        {
            ExecuteComboAttack();
        }
    }

    private void CheckAttackState()
    {
        if (anim == null) return;

        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(upperBodyLayerIndex);

        bool isPlayingAttack = IsStateInArray(stateInfo.fullPathHash, hit1Hashes) ||
                               IsStateInArray(stateInfo.fullPathHash, hit2Hashes);

        if (isPlayingAttack)
        {
            IsAttacking = true;
            anim.speed = attackSpeed;

            // Transition back to UpperBody.Empty near the end of the swing (85%)
            if (stateInfo.normalizedTime >= 0.85f && !stateResetDone)
            {
                stateResetDone = true;
                anim.CrossFadeInFixedTime(emptyStateHash, crossFadeDuration, upperBodyLayerIndex, 0f);
            }
        }
        else
        {
            IsAttacking = false;
            anim.speed = 1.0f;

            if (stateInfo.fullPathHash == emptyStateHash)
            {
                comboStep = 0;
            }
        }
    }

    private void ExecuteComboAttack()
    {
        comboStep++;
        if (comboStep > MAX_COMBO) comboStep = 1;

        stateResetDone = false;

        int targetHash = (comboStep == 1)
            ? GetRandomAnimationHash(hit1Hashes, ref lastHit1Index)
            : GetRandomAnimationHash(hit2Hashes, ref lastHit2Index);

        if (targetHash != 0)
        {
            anim.CrossFadeInFixedTime(targetHash, crossFadeDuration, upperBodyLayerIndex, 0f);
        }
    }

    private bool IsStateInArray(int hash, int[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] == hash) return true;
        }
        return false;
    }

    private int GetRandomAnimationHash(int[] hashArray, ref int lastIndex)
    {
        if (hashArray == null || hashArray.Length == 0) return 0;
        if (hashArray.Length == 1) return hashArray[0];

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
            hashes[i] = Animator.StringToHash("UpperBody." + names[i]);
        }
        return hashes;
    }
}