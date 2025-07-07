
using UnityEngine;

public class RootMotionCatcher : MonoBehaviour
{
    public Vector3 rootMotionDelta;
    private Vector3 lastRootPosition;
    private Animator anim;
    public float multiplier = 1;
    public float rootMotionDeltaThreshold = 0.01f;
    private string lastStateName;
    private AnimatorStateInfo si;
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        si = anim.GetCurrentAnimatorStateInfo(0);
    }

    // Update is called once per frameo
    private void OnAnimatorMove()
    {
        if (anim)
        {
            AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
            if(stateInfo.shortNameHash == si.shortNameHash)
            {

                rootMotionDelta = anim.deltaPosition;
                rootMotionDelta.y = 0;
                rootMotionDelta *= multiplier;
            }
            else
            {
                si = stateInfo;
            }

        }
        else
        {
            Debug.LogWarning("Animator component not found on " + gameObject.name);
        }
    }
}
