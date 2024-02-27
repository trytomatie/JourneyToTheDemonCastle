using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXManager : MonoBehaviour
{
    public MMF_Player[] feedbacks;

    // Singleton
    private static VFXManager instance;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public void PlayFeedback(int index,Transform position)
    {
        PlayFeedback(index,position, Quaternion.identity);
    }

    public void PlayFeedback(int index,Transform position,Quaternion rotation)
    {
        switch (index)
        {
            case 0:
                feedbacks[index].GetFeedbackOfType<MMF_ParticlesInstantiation>().InstantiateParticlesPosition = position;
                break;
            case 1:
                feedbacks[index].GetFeedbackOfType<MMF_InstantiateObject>().TargetTransform = position;
                feedbacks[index].transform.rotation = rotation;
                break;
            case 2:
                feedbacks[index].GetFeedbackOfType<MMF_ParticlesInstantiation>().InstantiateParticlesPosition = position;
                break;
            case 3:
                feedbacks[index].GetFeedbackOfType<MMF_ParticlesInstantiation>().TargetWorldPosition = position.position;
                break;

        }
        feedbacks[index].PlayFeedbacks();
    }

    public static VFXManager Instance { get => instance; set => instance = value; }

}
