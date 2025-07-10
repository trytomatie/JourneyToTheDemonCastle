using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXController : MonoBehaviour
{
    public GameObject[] attackVFX;
    public GameObject attackHitBox;
    public GameObject dashEffect;
    public ParticleSystem mainParticleSystem;
    public ParticleSystem spearParticleSystem;
    private Transform savedPosition;
    private float spearTimeArival;
    private float startTime;
    private void Start()
    {
        startTime = Time.time;
        StartCoroutine(CalculateSpearArivalTime());
    }

    private IEnumerator CalculateSpearArivalTime()
    {
        yield return new WaitForSeconds(2.4f);
        float timeToReachDestination;
        float speed = spearParticleSystem.main.startSpeed.constant;
        if (savedPosition != null)
        {
            timeToReachDestination = Vector3.Distance(spearParticleSystem.transform.position, savedPosition.position) / speed;
            spearTimeArival = startTime + timeToReachDestination;
            yield return new WaitForSeconds(timeToReachDestination*0.9f);
            VFXManager.Instance.PlayFeedback(17, savedPosition);
            yield return new WaitForSeconds(timeToReachDestination * 0.1f);
            spearParticleSystem.gameObject.SetActive(false);
        }
        else
        {
            spearTimeArival = startTime + 2.4f; // Default time if no position is set
        }

    }
    public void HandleVFX(int i)
    {
        GameObject go = Instantiate(attackVFX[i], attackVFX[i].transform.position, attackVFX[i].transform.rotation);
        go.SetActive(true);
        Destroy(go, 20f);

        //GameObject go2 = Instantiate(attackHitBox, attackHitBox.transform.position, attackHitBox.transform.rotation);
        //go2.SetActive(true);
    }

    public void PlayParticleSystem()
    {
        if (mainParticleSystem != null)
        {
            mainParticleSystem.Play();
        }
    }

    private void Update()
    {
        if(savedPosition != null)
        {
            spearParticleSystem.transform.LookAt(savedPosition);
        }
    }

    public void SpearDestination(Transform position)
    {
        savedPosition = new GameObject("SpearDestination").transform;
        savedPosition.position = position.position;
        spearParticleSystem.transform.LookAt(savedPosition);
    }
}
