using MoreMountains.Feedbacks;
using MoreMountains.Tools;
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

    public GameObject PlayFeedback(int index,Transform position)
    {
        return PlayFeedback(index,position, Quaternion.identity);
    }

    public GameObject PlayFeedback(int index,Transform position,Quaternion rotation)
    {
        GameObject result = null;
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
                feedbacks[index].GetFeedbackOfType<MMF_InstantiateObject>().TargetTransform = position;
                feedbacks[index].GetFeedbackOfType<MMF_InstantiateObject>().ParentTransform = position;
                break;
            case 3:
                feedbacks[index].GetFeedbackOfType<MMF_ParticlesInstantiation>().TargetWorldPosition = position.position;
                break;
            case 4:
                result = Instantiate(feedbacks[index].transform.GetChild(0).gameObject, position);
                result.transform.eulerAngles = Vector3.zero;
                result.transform.position = position.position;
                result.GetComponent<MMFollowTarget>().Target = position;
                break;
            case 5:
                result = Instantiate(feedbacks[index].gameObject);
                result.transform.position = position.position;
                result.transform.rotation = GameManager.Instance.player.transform.rotation;
                break;
            case 6:
                feedbacks[index].GetFeedbackOfType<MMF_ParticlesInstantiation>().TargetWorldPosition = position.position + new Vector3(0,0.5f,0);
                break;
            case 7:
                feedbacks[index].GetFeedbackOfType<MMF_ParticlesInstantiation>().InstantiateParticlesPosition = position;
                feedbacks[index].transform.rotation = rotation;
                break;
            case 8:
                feedbacks[index].GetFeedbackOfType<MMF_ParticlesInstantiation>().InstantiateParticlesPosition = position;
                feedbacks[index].transform.rotation = rotation;
                break;

        }

        feedbacks[index].PlayFeedbacks();
        return result;
    }

    public static VFXManager Instance { get => instance; set => instance = value; }

}
