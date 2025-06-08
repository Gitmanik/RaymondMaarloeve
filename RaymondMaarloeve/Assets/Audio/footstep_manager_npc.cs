using UnityEngine;

public class footstep_manager_npc : StateMachineBehaviour
{
    private bool first_step_triggered = false;
    private bool second_step_triggered = false;

    private AudioSource audioSource;
    private AudioClip[] footstepClips;
    int last_index = 0;
    private string footstep_folder = "Footsteps_DirtyGround_Walk";
    public bool running = false;
    private float previousLoopPosition = 0f;



    // Footstep timings as percentages of animation duration
    public float firstFootstepPercent_float = 0.30f;
    public float secondFootstepPercent_float = 0.81f;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        first_step_triggered = false;
        second_step_triggered = false;
        previousLoopPosition = 0f;

        audioSource = animator.gameObject.GetComponent<AudioSource>();

        if (audioSource == null)
        {
            Debug.LogWarning("No AudioSource found on the GameObject.");
        }

        if (running)
        {
            footstep_folder = "Footsteps_DirtyGround_Run";
        }
        
        footstepClips = Resources.LoadAll<AudioClip>(footstep_folder);

        if (footstepClips == null || footstepClips.Length == 0)
        {
            Debug.LogWarning("No footstep sounds found in Resources/" + footstep_folder);
        }
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

        if (audioSource == null || footstepClips == null || footstepClips.Length == 0)
            return;


        float loopPosition = stateInfo.normalizedTime % 1f;

        // Reset when animation loops
        if (loopPosition < previousLoopPosition)
        {
            first_step_triggered = false;
            second_step_triggered = false;
        }

        previousLoopPosition = loopPosition;

        if (loopPosition >= firstFootstepPercent_float && !first_step_triggered)
        {
            PlayRandomFootstep();
            first_step_triggered = true;
        }

        if (loopPosition >= secondFootstepPercent_float && !second_step_triggered)
        {
            PlayRandomFootstep();
            second_step_triggered = true;
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        first_step_triggered = false;
        second_step_triggered = false;
        previousLoopPosition = 0f;
    }

    private void PlayRandomFootstep()
    {
        if (footstepClips.Length == 0) return;

        int index = Random.Range(0, footstepClips.Length);

        if (index == last_index && footstepClips.Length != 1)
        {
            do
            {
                index = Random.Range(0, footstepClips.Length);
            } while (index == last_index);
        }

        audioSource.PlayOneShot(footstepClips[index]);

        last_index = index;
    }
}
