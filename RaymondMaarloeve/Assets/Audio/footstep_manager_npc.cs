using UnityEngine;

/// <summary>
/// Handles footstep sounds for NPCs during animation states using StateMachineBehaviour.
/// Plays two footstep sounds at specified points in the animation loop.
/// </summary>
public class footstep_manager_npc : StateMachineBehaviour
{
    /// <summary>
    /// Tracks whether the first footstep has been played in the current animation loop.
    /// </summary>
    private bool first_step_triggered = false;

    /// <summary>
    /// Tracks whether the second footstep has been played in the current animation loop.
    /// </summary>
    private bool second_step_triggered = false;

    /// <summary>
    /// AudioSource component used to play footstep sounds.
    /// </summary>
    private AudioSource audioSource;

    /// <summary>
    /// Array of footstep audio clips loaded from the Resources folder.
    /// </summary>
    private AudioClip[] footstepClips;

    /// <summary>
    /// Index of the last footstep clip played to avoid repetition.
    /// </summary>
    int last_index = 0;

    /// <summary>
    /// Path to the folder in the Resources directory containing footstep sounds.
    /// </summary>
    private string footstep_folder = "Footsteps_DirtyGround_Walk";

    /// <summary>
    /// If true, run footstep sounds are used instead of walk sounds.
    /// </summary>
    public bool running = false;

    /// <summary>
    /// Tracks the last animation loop position to detect when a new loop begins.
    /// </summary>
    private float previousLoopPosition = 0f;

    /// <summary>
    /// Percentage of animation duration when the first footstep should occur.
    /// </summary>
    public float firstFootstepPercent_float = 0.30f;

    /// <summary>
    /// Percentage of animation duration when the second footstep should occur.
    /// </summary>
    public float secondFootstepPercent_float = 0.81f;

    /// <summary>
    /// Called when the animation state is entered.
    /// Resets internal state and loads footstep audio clips.
    /// </summary>
    /// <param name="animator">The Animator playing the state.</param>
    /// <param name="stateInfo">Information about the current state.</param>
    /// <param name="layerIndex">The layer index the state is playing on.</param>
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

    /// <summary>
    /// Called every frame the animation state is active.
    /// Plays footstep sounds based on animation progress.
    /// </summary>
    /// <param name="animator">The Animator playing the state.</param>
    /// <param name="stateInfo">Information about the current state.</param>
    /// <param name="layerIndex">The layer index the state is playing on.</param>
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (audioSource == null || footstepClips == null || footstepClips.Length == 0)
            return;

        float loopPosition = stateInfo.normalizedTime % 1f;

        // Reset flags when the animation loops
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

    /// <summary>
    /// Called when the animation state is exited.
    /// Resets internal state.
    /// </summary>
    /// <param name="animator">The Animator playing the state.</param>
    /// <param name="stateInfo">Information about the current state.</param>
    /// <param name="layerIndex">The layer index the state is playing on.</param>
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        first_step_triggered = false;
        second_step_triggered = false;
        previousLoopPosition = 0f;
    }

    /// <summary>
    /// Plays a random footstep sound, ensuring the same clip is not repeated consecutively.
    /// </summary>
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
