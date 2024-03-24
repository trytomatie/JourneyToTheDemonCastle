using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Highlighters.HighlighterTrigger;

public class AlphaShaderController : MonoBehaviour
{
    public Material material;
    public float cameraDistance;
    public LayerMask layerMask;
    // Start is called before the first frame update
    void Start()
    {
        cameraDistance = Vector3.Distance(Camera.main.transform.position, GameManager.Instance.player.transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        material.SetVector("_PlayerPos", Camera.main.WorldToViewportPoint(GameManager.Instance.player.transform.position));
        float playerDistance = Vector3.Distance(Camera.main.transform.position, GameManager.Instance.player.transform.position);
        // Raycast to player
        RaycastHit hit;
        Vector3 startPostion = Camera.main.transform.position;
        Vector3 direction = (GameManager.Instance.player.transform.position - Camera.main.transform.position).normalized;
        Ray ray = new Ray(startPostion, direction);
        Debug.DrawRay(startPostion, direction * (playerDistance-0.3f), Color.red);
        if (Physics.Raycast(ray, out hit, playerDistance-0.3f, layerMask))
        {
            if (hit.collider.gameObject == GameManager.Instance.player)
            {
                material.SetInt("_PlayerVisible", 1);
            }
            else
            {
                material.SetInt("_PlayerVisible", 0);
            }
        }
        else
        {
            material.SetInt("_PlayerVisible", 1);
        }
    }
}
