using UnityEngine;
using TMPro;

public class DeskWorkerZone : MonoBehaviour
{
    [Header("판매원 구매 설정")]
    public int workerCost = 100;
    public TMP_Text costText;

    [Header("소환 설정")]
    public GameObject deskWorkerPrefab;
    public Transform spawnPoint;

    [Header("판매원에게 알려줄 정보 맵핑")]
    public DeskController deskController;
    public Transform sourceZone;
    public Transform waitPos;

    private void Start()
    {
        if (costText != null) costText.text = workerCost + " G";
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerMovement player = other.GetComponent<PlayerMovement>();
        if (player != null && player.currentGold >= workerCost)
        {
            player.SpendGold(workerCost);

            if (deskWorkerPrefab != null && spawnPoint != null)
            {
                GameObject newWorker = Instantiate(deskWorkerPrefab, spawnPoint.position, spawnPoint.rotation);
                DeskWorkerAI ai = newWorker.GetComponent<DeskWorkerAI>();

                if (ai != null)
                {
                    ai.sourceZone = this.sourceZone;
                    ai.waitPos = this.waitPos;

                    if (this.deskController != null)
                    {
                        ai.deskHandcuffDropPoint = this.deskController.deskHandcuffsTarget;
                    }
                }

                if (deskController != null)
                {
                    deskController.hasWorker = true;
                }
            }

            gameObject.SetActive(false);
        }
    }
}