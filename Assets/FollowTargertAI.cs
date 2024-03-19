using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTargertAI : MonoBehaviour
{
    public Transform target;
    public float speed = 5.0f;
    public float minDistance = 1.0f;

    // Update is called once per frame
    void Update()
    {
        if (target == null)
        {
            return;
        }

        float distance = Vector3.Distance(transform.position, target.position);
        if (distance > minDistance)
        {
            // Interpolate to targetPosition
            transform.position = Vector3.Lerp(transform.position, target.position, speed * Time.deltaTime);

        }
    }
}
