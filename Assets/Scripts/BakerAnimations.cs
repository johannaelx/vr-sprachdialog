using System.Collections;
using UnityEngine;

public class BakerAnimations : MonoBehaviour
{
    [SerializeField] private Animator animator;

    [Header("Wave (on start)")]
    [SerializeField] private bool waveOnStart = true;
    [SerializeField] private int waveTimes = 3;
    [SerializeField] private string waveTriggerName = "wave";
    [SerializeField] private string waveStateName = "Wave";

    [Header("Dance")]
    [SerializeField] private string danceTriggerName = "dance";
    [SerializeField] private string danceStateName = "Dancing"; // <-- wichtig

    private Coroutine routine;

    private void Reset() => animator = GetComponent<Animator>();

    private void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (waveOnStart) routine = StartCoroutine(TriggerRoutine(waveTimes, waveTriggerName, waveStateName));
    }

    public void DanceTimes(int times)
    {
        if (animator == null) animator = GetComponent<Animator>();

        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }

        routine = StartCoroutine(TriggerRoutine(times, danceTriggerName, danceStateName));
    }

    public void DanceFiveTimes()
    {
        DanceTimes(5);
    }

    private IEnumerator TriggerRoutine(int times, string triggerName, string stateName)
    {
        for (int i = 0; i < times; i++)
        {
            animator.ResetTrigger(triggerName);
            animator.SetTrigger(triggerName);

            yield return null;
            while (!animator.GetCurrentAnimatorStateInfo(0).IsName(stateName))
                yield return null;

            while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
                yield return null;
        }

        routine = null;
    }
}