using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MachineController : MonoBehaviour
{
    [Header("수갑 결과물 설정")]
    public Transform handcuffZone;  
    public float handcuffZoneYOffset = 5f; 

    [Header("식어가는 애니메이션 설정")]
    public float coolingDuration = 0.5f;
    public Color hotColor = Color.red;

    [Header("기계 설정")]
    public Transform inputPoint;
    public Transform ironDropZone;
    [Header("레일 경로 설정")]
    public Transform[] railWaypoints; 

    public Transform transformationWaypoint;

    [Header("애니메이션 설정")]
    public float produceInterval = 0.5f; 
    public float moveSpeed = 3f;      

    private bool isWorking = false;
    private int currentZoneIronCount = 0;
    private Transform currentZoneTransform; 

    public void StartProduction(int ironCount, GameObject zone)
    {
        currentZoneIronCount += ironCount;

        currentZoneTransform = (ironDropZone != null) ? ironDropZone : zone.transform;

        if (!isWorking && currentZoneIronCount > 0)
        {
            StartCoroutine(ProduceRoutine());
        }
    }

    IEnumerator ProduceRoutine()
    {
        isWorking = true;

        while (currentZoneIronCount > 0)
        {
            while (handcuffZone != null && handcuffZone.childCount >= 100)
            {
                yield return new WaitForSeconds(0.5f);
            }

            yield return new WaitForSeconds(produceInterval);

            if (currentZoneTransform != null && currentZoneTransform.childCount > 0)
            {
                GameObject ironInZone = null;

                for (int i = currentZoneTransform.childCount - 1; i >= 0; i--)
                {
                    GameObject child = currentZoneTransform.GetChild(i).gameObject;

                    Collider col = child.GetComponent<Collider>();
                    if (col != null && col.isTrigger) continue;
                    if (child.name.Contains("Zone")) continue;

                    ironInZone = child;
                    break;
                }

                if (ironInZone != null)
                {
                    ironInZone.SetActive(false);
                    ironInZone.transform.SetParent(null);
                }
            }

            currentZoneIronCount--;

            GameObject ironToAnimate = ObjectPool.Instance.GetIronIngot();
            if (ironToAnimate != null)
            {
                ironToAnimate.transform.position = inputPoint.position;
                ironToAnimate.SetActive(true);
                StartCoroutine(CoolDownIron(ironToAnimate));
                StartCoroutine(AnimateIronOnRail(ironToAnimate));
            }
            else
            {
                break;
            }
        }

        isWorking = false;
    }

    IEnumerator CoolDownIron(GameObject ironObject)
    {
        MeshRenderer[] meshRenderers = ironObject.GetComponentsInChildren<MeshRenderer>();
        if (meshRenderers.Length == 0)
        {
            MeshRenderer mr = ironObject.GetComponent<MeshRenderer>();
            if (mr != null) meshRenderers = new MeshRenderer[] { mr };
        }

        if (meshRenderers.Length == 0)
        {
            yield break;
        }

        Color coolColor = meshRenderers[0].material.color;

        foreach (MeshRenderer meshRenderer in meshRenderers)
        {
            meshRenderer.material.color = hotColor;
        }

        float elapsedTime = 0f;
        while (elapsedTime < coolingDuration)
        {
            float normalizedTime = elapsedTime / coolingDuration;
            Color currentColor = Color.Lerp(hotColor, coolColor, normalizedTime);

            foreach (MeshRenderer meshRenderer in meshRenderers)
            {
                meshRenderer.material.color = currentColor;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        foreach (MeshRenderer meshRenderer in meshRenderers)
        {
            meshRenderer.material.color = coolColor;
        }
    }

    IEnumerator AnimateIronOnRail(GameObject ironObject)
    {
        int waypointIndex = 0;
        bool hasTransformed = false;

        GameObject activeObject = ironObject;

        while (waypointIndex < railWaypoints.Length)
        {
            Transform targetWaypoint = railWaypoints[waypointIndex];

            float step = moveSpeed * Time.deltaTime;

            activeObject.transform.position = Vector3.MoveTowards(activeObject.transform.position, targetWaypoint.position, step);

            if (!hasTransformed && transformationWaypoint != null && targetWaypoint == transformationWaypoint &&
                Vector3.Distance(activeObject.transform.position, targetWaypoint.position) < 0.1f)
            {
                hasTransformed = true;

                GameObject handcuff = ObjectPool.Instance.GetHandcuff();
                if (handcuff != null)
                {
                    handcuff.transform.position = activeObject.transform.position;
                    handcuff.SetActive(true);

                    activeObject.SetActive(false);

                    activeObject = handcuff;

                }
            }

            if (Vector3.Distance(activeObject.transform.position, targetWaypoint.position) < 0.1f)
            {
                waypointIndex++;
            }

            yield return null;
        }

        if (activeObject != null)
        {
            if (hasTransformed) 
            {
                activeObject.transform.SetParent(handcuffZone);


                int currentCount = handcuffZone.childCount - 1;
                Vector3 stackPos = new Vector3(0, currentCount * handcuffZoneYOffset, 0);

                activeObject.transform.localPosition = stackPos;
                activeObject.transform.localRotation = Quaternion.identity;

            }
            else
            {
                activeObject.SetActive(false);
            }
        }
    }
}