using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    float offsetY = 6.5f;
    float offsetX = -7.0f;
    float offsetZ = -7.0f;
    public GameObject player;

    private bool isPanning = false;

    void Update()
    {
        if (!isPanning && player != null)
        {
            transform.position = new Vector3(player.transform.position.x + offsetX, player.transform.position.y + offsetY, player.transform.position.z + offsetZ);
        }
    }

    public void ShowUpgradeZone(Transform targetZone)
    {
        StopAllCoroutines();
        StartCoroutine(PanCameraRoutine(targetZone));
    }

    IEnumerator PanCameraRoutine(Transform targetZone)
    {
        isPanning = true;

        Vector3 startPos = transform.position;
        Vector3 targetPos = new Vector3(targetZone.position.x + offsetX, targetZone.position.y + offsetY, targetZone.position.z + offsetZ);

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 1.5f;
            transform.position = Vector3.Lerp(startPos, targetPos, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }

        yield return new WaitForSeconds(1.5f);

        t = 0;
        startPos = transform.position;
        while (t < 1f)
        {
            t += Time.deltaTime * 1.5f;
            Vector3 playerPos = new Vector3(player.transform.position.x + offsetX, player.transform.position.y + offsetY, player.transform.position.z + offsetZ);
            transform.position = Vector3.Lerp(startPos, playerPos, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }

        isPanning = false;
    }
}