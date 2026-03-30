using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeskController : MonoBehaviour
{
    [Header("감옥 수용 인원 UI & 증설 설정")]
    public TMPro.TMP_Text capacityText;      
    public GameObject prisonExpansionZone;   
    public Transform zoneFocusPoint;
    [Header("수갑 쌓이는 위치")]
    public Transform deskHandcuffsTarget;

    [Header("돈 생성 설정 (Money Zone)")]
    public Transform moneyZone;
    public float moneyZoneYOffset = 0.1f;

    [Header("웨이포인트")]
    public Transform spawnPoint;
    public Transform deskWaitPoint;
    public Transform prisonPoint;

    [Header("감옥 내부 설정 (Cell)")]
    public Transform cellPoint;      
    public int prisonersPerRow = 5;  
    public Vector3 columnOffset = new Vector3(1f, 0, 0); 
    public Vector3 rowOffset = new Vector3(0, 0, 1f);    
    public int maxCellCapacity = 20; 
    private int jailedCount = 0;

    [Header("대기열(Queue) 설정")]
    public float spawnInterval = 3f;
    public int maxQueueSize = 5;
    public Vector3 queueOffset = new Vector3(-1.5f, 0, 0);

    private List<Prisoner> prisonerQueue = new List<Prisoner>();
    private bool isProcessing = false;

    [HideInInspector] public bool isPlayerInZone = false;
    [HideInInspector] public bool hasWorker = false;

    void Start()
    {
        UpdateCapacityUI();

        if (prisonExpansionZone != null) prisonExpansionZone.SetActive(false);
        StartCoroutine(SpawnPrisonerRoutine());
    }

    IEnumerator SpawnPrisonerRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (prisonerQueue.Count < maxQueueSize)
            {
                GameObject obj = ObjectPool.Instance.GetNormalPrisoner();
                if (obj != null)
                {
                    obj.transform.position = spawnPoint.position;
                    obj.SetActive(true);

                    Prisoner prisoner = obj.GetComponent<Prisoner>();
                    prisoner.requiredHandcuffs = Random.Range(1, 4);

                    prisoner.InitializeAsNormal(this, prisonPoint);

                    prisonerQueue.Add(prisoner);
                    UpdateQueuePositions();
                }
            }
        }
    }

    private void UpdateQueuePositions()
    {
        for (int i = 0; i < prisonerQueue.Count; i++)
        {
            Vector3 targetPosition = deskWaitPoint.position + (queueOffset * i);
            prisonerQueue[i].SetDestination(targetPosition);
        }
    }

    public void OnPrisonerReachedSpot(Prisoner prisoner)
    {
        if (prisonerQueue.Count > 0 && prisonerQueue[0] == prisoner && !isProcessing)
        {
            StartCoroutine(ProcessTransactionRoutine(prisoner));
        }
    }

    IEnumerator ProcessTransactionRoutine(Prisoner currentPrisoner)
    {
        isProcessing = true;
        currentPrisoner.ShowSpeechBubble(true);

        while (deskHandcuffsTarget.childCount < currentPrisoner.requiredHandcuffs ||
              (!isPlayerInZone && !hasWorker) ||
              jailedCount >= maxCellCapacity)
        {
            if (jailedCount >= maxCellCapacity && prisonExpansionZone != null && !prisonExpansionZone.activeSelf)
            {
                prisonExpansionZone.SetActive(true);

                if (CameraManager.Instance != null)
                {
                    Transform target = zoneFocusPoint != null ? zoneFocusPoint : prisonExpansionZone.transform;
                    CameraManager.Instance.ShowUpgradeZone(prisonExpansionZone.transform);
                }
            }
            yield return null;
        }

        for (int i = 0; i < currentPrisoner.requiredHandcuffs; i++)
        {
            Transform handcuff = deskHandcuffsTarget.GetChild(deskHandcuffsTarget.childCount - 1);
            handcuff.gameObject.SetActive(false);
            handcuff.SetParent(null);
        }

        for (int i = 0; i < currentPrisoner.requiredHandcuffs; i++)
        {
            GameObject money = ObjectPool.Instance.GetMoney();
            if (money != null)
            {
                money.transform.SetParent(moneyZone);

                int currentCount = moneyZone.childCount - 1;
                Vector3 stackPos = new Vector3(0, currentCount * moneyZoneYOffset, 0);

                money.transform.localPosition = stackPos;
                money.transform.localRotation = Quaternion.identity;
                money.SetActive(true);
            }
        }

        int rowIndex = jailedCount / prisonersPerRow; 
        int colIndex = jailedCount % prisonersPerRow; 

        Vector3 targetCellPos = cellPoint.position + (columnOffset * colIndex) + (rowOffset * rowIndex);
        
        currentPrisoner.TransformAndGoToPrison(targetCellPos);
        
        jailedCount++;

        UpdateCapacityUI();

        if (jailedCount >= maxCellCapacity && prisonExpansionZone != null)
        {
            prisonExpansionZone.SetActive(true);
        }

        prisonerQueue.RemoveAt(0);
        UpdateQueuePositions();
        isProcessing = false;
    }

    public void UpdateCapacityUI()
    {
        if (capacityText != null)
        {
            capacityText.text = "Prisoners: " + jailedCount + " / " + maxCellCapacity;

            if (jailedCount >= maxCellCapacity) capacityText.color = Color.red;
            else capacityText.color = Color.black;
        }
    }

    public void ExpandPrison(int additionalCapacity)
    {
        maxCellCapacity += additionalCapacity;
        UpdateCapacityUI(); 
    }
}