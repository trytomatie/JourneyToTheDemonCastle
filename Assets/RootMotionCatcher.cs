using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RootMotionCatcher : MonoBehaviour
{
    public Vector3 rootMotionDelta;
    private Vector3 lastRootPosition;
    private Animator anim;
    public float multiplier = 1;
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frameo
    private void OnAnimatorMove()
    {
        if (anim)
        {
            // Calculate the root motion delta
            rootMotionDelta = anim.deltaPosition;
            rootMotionDelta *= multiplier;
            // Optionally, you can log the root motion delta
        }
        else
        {
            Debug.LogWarning("Animator component not found on " + gameObject.name);
        }
    }
}
