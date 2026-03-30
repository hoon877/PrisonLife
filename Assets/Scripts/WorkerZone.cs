using UnityEngine;
using TMPro;

public class WorkerZone : MonoBehaviour
{
    [Header("노동자 구매 설정")]
    public int workerCost = 50;
    public TMP_Text costText;

    [Header("소환 설정")]
    public GameObject workerPrefab;
    public Transform spawnPoint;

    [Header("노동자에게 알려줄 목적지")]
    public Transform ironDropZone;
    public Transform waypoint;

    [Header("다음 업그레이드 연동")]
    public GameObject deskWorkerZoneObject;

    private void Start()
    {
        if (costText != null) costText.text = workerCost + " G";

        if (deskWorkerZoneObject != null) deskWorkerZoneObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerMovement player = other.GetComponent<PlayerMovement>();
        if (player != null && player.currentGold >= workerCost)
        {
            player.SpendGold(workerCost);

            for (int i = 0; i < 3; i++)
            {
                if (workerPrefab != null && spawnPoint != null)
                {
                    Vector3 offset = new Vector3(i * 1.5f, 0, 0);
                    GameObject newWorker = Instantiate(workerPrefab, spawnPoint.position + offset, Quaternion.identity);

                    WorkerAI ai = newWorker.GetComponent<WorkerAI>();
                    if (ai != null)
                    {
                        ai.ironDropZone = this.ironDropZone;
                        ai.waypoint = this.waypoint;
                    }
                }
            }

            if (deskWorkerZoneObject != null) deskWorkerZoneObject.SetActive(true);

            gameObject.SetActive(false);
        }
    }
}