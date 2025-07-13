using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DamageObject : MonoBehaviour
{
    public float lifeTime = 0.5f;
    public GameObject hitVFXPrefab;
    public UnityEvent HitEvent;
    public StatusManager source;
    public float speed = 0;

    private List<GameObject> hitObjects = new List<GameObject>();
    private void OnTriggerEnter(Collider other)
    {
        if (hitObjects.Contains(other.gameObject) || other.gameObject == source.gameObject ) return;

        ResourceStatusManager resourceStatusManager = other.GetComponent<ResourceStatusManager>() ?? null;
        StatusManager statusManager = other.GetComponent<StatusManager>() ?? null;
        if(resourceStatusManager != null)
        {
            HitEvent.Invoke();  
            hitObjects.Add(other.gameObject);
            resourceStatusManager.ApplyDamage(source.AttackDamage);
            resourceStatusManager.OnHit.Invoke();
            Vector3 hitPosition = other.ClosestPointOnBounds(transform.position+ new Vector3(0,0.5f,0));
            Quaternion hitrotation = Quaternion.LookRotation(transform.forward);
            GameObject hitVFX = Instantiate(hitVFXPrefab, hitPosition, hitrotation);
            Destroy(hitVFX, 2f);
        }
        else if (statusManager != null && statusManager.faction != source.faction)
        {
            HitEvent.Invoke();  
            hitObjects.Add(other.gameObject);
            statusManager.OnHit.Invoke();
            Vector3 hitPosition = other.ClosestPointOnBounds(transform.position+ new Vector3(0,0.5f,0));
            Quaternion hitrotation = Quaternion.LookRotation(transform.forward);
            GameObject hitVFX = Instantiate(hitVFXPrefab, hitPosition, hitrotation);
            Destroy(hitVFX, 2f);
        }

    }

    private IEnumerator ReleaseSnapShotEntityAction()
    {

        yield return new WaitForSeconds(0.1f);
            foreach (GameObject hitObject in hitObjects)
            {
                ResourceStatusManager resourceStatusManager = hitObject.GetComponent<ResourceStatusManager>();
                if (resourceStatusManager != null)
                {
                    resourceStatusManager.ApplyDamage(source.AttackDamage);
                }
                else
                {
                    StatusManager statusManager = hitObject.GetComponent<StatusManager>();
                    if (statusManager != null)
                    {
                        statusManager.ApplyDamage(source.AttackDamage);
                    }
                }
            }

    }

    private IEnumerator DisableAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        GetComponent<Collider>().enabled = false;
        Destroy(gameObject,5);
    }

    private void Start()
    {
        StartCoroutine(DisableAfterSeconds(lifeTime));
        StartCoroutine(ReleaseSnapShotEntityAction());
    }

    private void Update()
    {
        if (speed > 0)
        {
            transform.position += transform.forward * speed * Time.deltaTime;
        }
    }

}
