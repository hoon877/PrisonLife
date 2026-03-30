using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OreCullingManager : MonoBehaviour
{
    [Header("Target & Distance")]
    public Transform player; 
    public float activeDistance = 20f; 

    [Header("Optimization Settings")]
    public float checkInterval = 0.5f; 

    private List<GameObject> allOres = new List<GameObject>();

    void Start()
    {
        
        GameObject[] foundOres = GameObject.FindGameObjectsWithTag("Ironstone");
        allOres.AddRange(foundOres);

        StartCoroutine(CullingRoutine());
    }

    IEnumerator CullingRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(checkInterval);

            if (player == null) continue;

            Vector3 playerPos = player.position;

            float activeDistanceSqr = activeDistance * activeDistance;

            foreach (GameObject ore in allOres)
            {
                if (ore != null)
                {
                    float distanceSqr = (ore.transform.position - playerPos).sqrMagnitude;

                    if (distanceSqr <= activeDistanceSqr)
                    {
                        if (!ore.activeSelf) ore.SetActive(true);
                    }
                    else
                    {
                        if (ore.activeSelf) ore.SetActive(false);
                    }
                }
            }
        }
    }
}