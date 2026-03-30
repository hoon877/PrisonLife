using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum MiningToolType
{
    Pickaxe,
    Drill,
    Excavator
}

public class PlayerMovement : MonoBehaviour
{
    Animator anim;
    Rigidbody playerRb;

    [Header("Mining Tools (업그레이드 장비들)")]
    public MiningToolType currentTool = MiningToolType.Pickaxe;
    public GameObject pickaxeObject;
    public GameObject drillObject;
    public GameObject excavatorObject;

    public float excavatorYOffset = 1.1f;
    private float defaultYPos; 

    public int currentMaxCapacity = 10;

    [Header("Audio Settings")]
    public AudioSource toolAudioSource;   
    public AudioClip drillSound;          
    public AudioClip excavatorSound;      

    [Header("Player Stats")]
    public int currentGold = 0;
    public TMP_Text goldTextUI;
    public GameObject maxCapacityUI;
    private bool isShowingMaxUI = false;

    [Header("Machine Reference")]
    public MachineController targetMachine;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float turnSpeed = 15f;
    public FloatingJoystick joystick;

    [Header("Iron Stacking Visuals (Player Back)")]
    public Transform stackParentPosition;
    public float yOffsetPerIngot = 0.1f;

    [Header("Zone Stacking Settings (Target Zone)")]
    public float zoneZOffset = 1f;
    public Vector3 zoneCenterOffset = new Vector3(0, 0.05f, 0);

    [Header("Handcuff Stacking (Player)")]
    public Transform handcuffStackParent;
    public float playerHandcuffYOffset = 0.1f;
    List<GameObject> playerHandcuffs = new List<GameObject>();

    [Header("Money Stacking (Player)")]
    public Transform moneyStackParent;
    public float moneyYOffset = 0.1f;
    public float moneyPushBackOffset = 0.5f;
    private Vector3 moneyBaseLocalPos;
    List<GameObject> stackedMoneyObjects = new List<GameObject>();

    [Header("Desk Stacking Settings")]
    public float deskHandcuffZOffset = 0.1f;
    public Transform targetDeskHandcuffs;

    [Header("Upgrade System")]
    public GameObject upgradeZoneObject;   
    public CameraManager cameraManager;    
    private bool hasUnlockedUpgradeZone = false; 

    private bool isInMine = false;
    private bool isPickingUp = false;
    private bool isDigging = false;
    private bool isTransferring = false;
    private bool isTransferringHandcuffs = false;
    private bool isPickingUpMoney = false;
    private int zoneStackIndex = 0;
    private int totalMinedCount = 0;

    List<GameObject> stackedIronObjects = new List<GameObject>();
    List<GameObject> miningOres = new List<GameObject>();
    Vector3 moveVec;
    public float transferInterval = 0.05f;

    [Header("Cached References")]
    private DeskController cachedDeskController;
    private Transform cachedDeskHandcuffsTarget;

    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        ActivateToolVisual(false);
        UpdateCapacityByTool();
        playerRb = GetComponent<Rigidbody>();

        if (moneyStackParent != null)
        {
            moneyBaseLocalPos = moneyStackParent.localPosition;
        }

        defaultYPos = transform.position.y;

        UpdateGoldUI();

        if (upgradeZoneObject != null) upgradeZoneObject.SetActive(false);
    }
    private void Start()
    {
        // 1. DeskController 캐싱
        cachedDeskController = FindObjectOfType<DeskController>();

        // 2. DeskHandCuffs 위치 캐싱
        if (targetDeskHandcuffs != null)
        {
            cachedDeskHandcuffsTarget = targetDeskHandcuffs;
        }
        else
        {
            GameObject foundObj = GameObject.Find("DeskHandCuffs");
            if (foundObj != null) cachedDeskHandcuffsTarget = foundObj.transform;
        }
    }

    void Update()
    {
        UpdateCarryingAnimation();
        UpdateMoneyStackPosition();
        UpdateToolAnimations();
        UpdateToolAudio();
    }

    void FixedUpdate()
    {
        float hAxis = joystick.Horizontal;
        float vAxis = joystick.Vertical;
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;

        float targetY = defaultYPos;

        if (isInMine && currentTool == MiningToolType.Excavator)
        {
            targetY = defaultYPos + excavatorYOffset; 

            if (playerRb != null)
            {
                playerRb.useGravity = false; 
                playerRb.velocity = new Vector3(playerRb.velocity.x, 0, playerRb.velocity.z); 
            }
        }
        else
        {
            if (playerRb != null) playerRb.useGravity = true; 
        }

        Vector3 nextPos = transform.position;

        if (moveVec != Vector3.zero)
        {
            nextPos += moveVec * moveSpeed * Time.deltaTime;

            Quaternion targetRotation = Quaternion.LookRotation(moveVec);

            if (playerRb != null)
            {
                playerRb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime));
            }
            else
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
            }

            if (isInMine && currentTool != MiningToolType.Pickaxe)
            {
                anim.SetBool("Walk", false);
            }
            else
            {
                anim.SetBool("Walk", true);
            }
        }
        else
        {
            anim.SetBool("Walk", false);
        }

        nextPos.y = Mathf.Lerp(transform.position.y, targetY, Time.deltaTime * 10f);

        if (playerRb != null)
        {
            playerRb.MovePosition(nextPos);
        }
        else
        {
            transform.position = nextPos;
        }
    }

    private void UpdateToolAudio()
    {
        if (toolAudioSource == null) return;

        bool shouldPlayMachineSound = isInMine && (currentTool == MiningToolType.Drill || currentTool == MiningToolType.Excavator);

        if (shouldPlayMachineSound)
        {
            if (!toolAudioSource.isPlaying)
            {
                toolAudioSource.clip = (currentTool == MiningToolType.Drill) ? drillSound : excavatorSound;
                toolAudioSource.loop = true;
                toolAudioSource.Play();
            }
        }
        else
        {
            if (toolAudioSource.isPlaying)
            {
                toolAudioSource.Stop(); 
            }
        }
    }

    private void UpdateGoldUI()
    {
        if (goldTextUI != null)
        {
            goldTextUI.text = currentGold.ToString();
        }
    }

    public void SpendGold(int amount)
    {
        currentGold -= amount;
        UpdateGoldUI();

        int objectsToRemove = amount / 10;

        for (int i = 0; i < objectsToRemove; i++)
        {
            if (stackedMoneyObjects.Count > 0)
            {
                int lastIndex = stackedMoneyObjects.Count - 1;
                GameObject moneyObj = stackedMoneyObjects[lastIndex];

                stackedMoneyObjects.RemoveAt(lastIndex);

                moneyObj.transform.SetParent(null);
                moneyObj.SetActive(false);
            }
        }
    }

    private void UpdateMoneyStackPosition()
    {
        if (moneyStackParent == null) return;

        if (stackedIronObjects.Count > 0)
        {
            Vector3 pushedPos = moneyBaseLocalPos + new Vector3(0, 0, -moneyPushBackOffset);
            moneyStackParent.localPosition = pushedPos;
        }
        else
        {
            moneyStackParent.localPosition = moneyBaseLocalPos;
        }
    }

    private void UpdateCarryingAnimation()
    {
        if (anim != null)
        {
            bool isCarrying = playerHandcuffs.Count > 0;
            anim.SetBool("Carrying", isCarrying);
        }
    }

    private void UpdateToolAnimations()
    {
        if (anim != null)
        {
            anim.SetBool("Digging_Drill", isInMine && currentTool == MiningToolType.Drill);
            anim.SetBool("Digging_Excavator", isInMine && currentTool == MiningToolType.Excavator);
        }
    }

    public void UpgradeTool()
    {
        if (currentTool == MiningToolType.Pickaxe)
        {
            currentTool = MiningToolType.Drill;
            UpdateCapacityByTool();
        }
        else if (currentTool == MiningToolType.Drill)
        {
            currentTool = MiningToolType.Excavator;
            UpdateCapacityByTool();
        }
    }

    private void UpdateCapacityByTool()
    {
        switch (currentTool)
        {
            case MiningToolType.Pickaxe: currentMaxCapacity = 10; break;
            case MiningToolType.Drill: currentMaxCapacity = 20; break;
            case MiningToolType.Excavator: currentMaxCapacity = 50; break;
        }
    }

    private void ActivateToolVisual(bool isActive)
    {
        pickaxeObject.SetActive(false);
        drillObject.SetActive(false);
        excavatorObject.SetActive(false);

        if (isActive)
        {
            switch (currentTool)
            {
                case MiningToolType.Pickaxe: pickaxeObject.SetActive(true); break;
                case MiningToolType.Drill: drillObject.SetActive(true); break;
                case MiningToolType.Excavator: excavatorObject.SetActive(true); break;
            }
        }
    }
    IEnumerator ShowMaxUIRoutine()
    {
        isShowingMaxUI = true;

        if (maxCapacityUI != null)
        {
            maxCapacityUI.SetActive(true); 
        }

        yield return new WaitForSeconds(1.5f); 

        if (maxCapacityUI != null)
        {
            maxCapacityUI.SetActive(false); 
        }

        isShowingMaxUI = false;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Mine"))
        {
            isInMine = true;
            ActivateToolVisual(true);
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Ironstone"))
        {
            if (!miningOres.Contains(other.gameObject))
            {
                if (stackedIronObjects.Count >= currentMaxCapacity)
                {
                    if (!isShowingMaxUI)
                    {
                        StartCoroutine(ShowMaxUIRoutine());
                    }
                }

                if (currentTool == MiningToolType.Pickaxe)
                {
                    if (!isDigging)
                    {
                        StartCoroutine(MineOreRoutine(other.gameObject));
                    }
                }
                else
                {
                    MineOreInstant(other.gameObject);
                }
            }
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("IronZone"))
        {
            Renderer zoneRenderer = other.gameObject.GetComponentInParent<SpriteRenderer>();
            if (zoneRenderer != null) zoneRenderer.material.color = Color.green;

            if (!isTransferring && stackedIronObjects.Count > 0)
            {
                GameObject targetZone = other.gameObject;
                if (targetMachine != null && targetMachine.ironDropZone != null)
                {
                    targetZone = targetMachine.ironDropZone.gameObject;
                }

                if (targetMachine != null) targetMachine.StartProduction(stackedIronObjects.Count, targetZone);
                StartCoroutine(TransferIronRoutine(targetZone));
            }
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("HandcuffZone"))
        {
            if (!isPickingUp && other.transform.childCount > 0)
            {
                StartCoroutine(PickUpHandcuffsRoutine(other.transform));
            }
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("MoneyZone"))
        {
            if (!isPickingUpMoney && other.transform.childCount > 0)
            {
                StartCoroutine(PickUpMoneyRoutine(other.transform));
            }
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("DeskZone"))
        {
            Renderer zoneRenderer = other.gameObject.GetComponentInParent<SpriteRenderer>();
            if (zoneRenderer != null) zoneRenderer.material.color = Color.green;

            DeskController desk = other.GetComponentInParent<DeskController>();
            if (desk == null) desk = cachedDeskController; 
            if (desk != null) desk.isPlayerInZone = true;

            if (!isTransferringHandcuffs && playerHandcuffs.Count > 0)
            {
                if (cachedDeskHandcuffsTarget != null) // ✅ 캐싱된 값 사용
                {
                    StartCoroutine(TransferHandcuffsToDeskRoutine(cachedDeskHandcuffsTarget));
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Mine"))
        {
            isInMine = false;
            ActivateToolVisual(false);
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("IronZone") ||
            other.gameObject.layer == LayerMask.NameToLayer("DeskZone"))
        {
            Renderer zoneRenderer = other.gameObject.GetComponentInParent<SpriteRenderer>();
            if (zoneRenderer != null) zoneRenderer.material.color = Color.white;

            if (other.gameObject.layer == LayerMask.NameToLayer("DeskZone"))
            {
                DeskController desk = other.GetComponentInParent<DeskController>();
                if (desk == null) desk = FindObjectOfType<DeskController>();
                if (desk != null) desk.isPlayerInZone = false;
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("HandcuffZone"))
        {
            if (!isPickingUp && other.transform.childCount > 0)
            {
                StartCoroutine(PickUpHandcuffsRoutine(other.transform));
            }
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("MoneyZone"))
        {
            if (!isPickingUpMoney && other.transform.childCount > 0)
            {
                StartCoroutine(PickUpMoneyRoutine(other.transform));
            }
        }
    }

    private void ProcessMining(GameObject targetOre)
    {
        if (currentTool == MiningToolType.Pickaxe)
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX(SoundManager.Instance.pickaxeHitSound);
            }
        }

        foreach (MeshRenderer mr in targetOre.GetComponentsInChildren<MeshRenderer>()) mr.enabled = false;
        foreach (Collider col in targetOre.GetComponentsInChildren<Collider>()) col.enabled = false;
        StartCoroutine(RespawnOreRoutine(targetOre, 5f));

        totalMinedCount++;

        int visualStackIndex = stackedIronObjects.Count;
        if (visualStackIndex < currentMaxCapacity)
        {
            Vector3 spawnOffset = new Vector3(0, visualStackIndex * yOffsetPerIngot, 0);
            Vector3 spawnPosition = stackParentPosition.position + stackParentPosition.transform.TransformDirection(spawnOffset);

            GameObject newOre = ObjectPool.Instance.GetIronOre();

            if (newOre != null)
            {
                newOre.transform.position = spawnPosition;
                newOre.transform.rotation = stackParentPosition.rotation;
                newOre.transform.SetParent(stackParentPosition);
                newOre.SetActive(true);

                stackedIronObjects.Add(newOre);
            }
        }
    }

    IEnumerator MineOreRoutine(GameObject targetOre)
    {
        isDigging = true;
        miningOres.Add(targetOre);

        anim.SetBool("Digging_Pickaxe", true);

        yield return new WaitForSeconds(0.1f);

        if (targetOre != null)
        {
            ProcessMining(targetOre);
        }

        yield return new WaitForSeconds(0.5f);

        anim.SetBool("Digging_Pickaxe", false);

        miningOres.Remove(targetOre);
        isDigging = false;
    }

    private void MineOreInstant(GameObject targetOre)
    {
        miningOres.Add(targetOre);

        if (targetOre != null)
        {
            ProcessMining(targetOre);
        }

        miningOres.Remove(targetOre);
    }

    IEnumerator RespawnOreRoutine(GameObject ore, float respawnTime)
    {
        yield return new WaitForSeconds(respawnTime);
        if (ore != null)
        {
            foreach (MeshRenderer mr in ore.GetComponentsInChildren<MeshRenderer>()) mr.enabled = true;
            foreach (Collider col in ore.GetComponentsInChildren<Collider>()) col.enabled = true;
        }
    }

    IEnumerator TransferIronRoutine(GameObject zoneObject)
    {
        isTransferring = true;
        zoneStackIndex = zoneObject.transform.childCount;

        while (stackedIronObjects.Count > 0)
        {
            if (zoneObject.transform.childCount >= 100) break;

            int lastPlayerIndex = stackedIronObjects.Count - 1;
            GameObject ironToTransfer = stackedIronObjects[lastPlayerIndex];

            yield return new WaitForSeconds(transferInterval);

            ironToTransfer.transform.SetParent(zoneObject.transform);

            Vector3 relativeStackOffset = new Vector3(0, 0, zoneStackIndex * -zoneZOffset);
            ironToTransfer.transform.localPosition = zoneCenterOffset + relativeStackOffset;
            ironToTransfer.transform.localRotation = Quaternion.identity;

            stackedIronObjects.RemoveAt(lastPlayerIndex);
            totalMinedCount--;
            zoneStackIndex++;
        }

        isTransferring = false;
    }

    IEnumerator PickUpHandcuffsRoutine(Transform zoneTransform)
    {
        isPickingUp = true;

        while (zoneTransform.childCount > 0)
        {
            Transform targetHandcuff = zoneTransform.GetChild(zoneTransform.childCount - 1);
            targetHandcuff.SetParent(handcuffStackParent);

            int currentCount = playerHandcuffs.Count;
            Vector3 stackPos = new Vector3(0, currentCount * playerHandcuffYOffset, 0);

            targetHandcuff.localPosition = stackPos;
            targetHandcuff.localRotation = Quaternion.identity;

            playerHandcuffs.Add(targetHandcuff.gameObject);

            yield return new WaitForSeconds(0.1f);
        }

        isPickingUp = false;
    }

    IEnumerator PickUpMoneyRoutine(Transform zoneTransform)
    {
        isPickingUpMoney = true;

        while (zoneTransform.childCount > 0)
        {
            Transform targetMoney = zoneTransform.GetChild(zoneTransform.childCount - 1);
            targetMoney.SetParent(moneyStackParent);

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySFX(SoundManager.Instance.moneyPickupSound, 0.5f);
            }

            int currentCount = stackedMoneyObjects.Count;
            Vector3 stackPos = new Vector3(0, currentCount * moneyYOffset, 0);

            targetMoney.localPosition = stackPos;
            targetMoney.localRotation = Quaternion.identity;

            stackedMoneyObjects.Add(targetMoney.gameObject);

            currentGold += 10;
            UpdateGoldUI();
            if (!hasUnlockedUpgradeZone && currentGold > 0)
            {
                hasUnlockedUpgradeZone = true; 
                if (upgradeZoneObject != null)
                {
                    upgradeZoneObject.SetActive(true);
                    if (cameraManager != null)
                    {
                        cameraManager.ShowUpgradeZone(upgradeZoneObject.transform);
                    }
                }
            }

            yield return new WaitForSeconds(0.05f);
        }

        isPickingUpMoney = false;
    }

    IEnumerator TransferHandcuffsToDeskRoutine(Transform deskHandcuffsTarget)
    {
        isTransferringHandcuffs = true;

        int currentDeskStackIndex = deskHandcuffsTarget.childCount;

        while (playerHandcuffs.Count > 0)
        {

            int lastIndex = playerHandcuffs.Count - 1;
            GameObject handcuffToTransfer = playerHandcuffs[lastIndex];

            yield return new WaitForSeconds(transferInterval);

            handcuffToTransfer.transform.SetParent(deskHandcuffsTarget);

            Rigidbody rb = handcuffToTransfer.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;

            Collider col = handcuffToTransfer.GetComponent<Collider>();
            if (col != null) col.enabled = false;

            Vector3 stackPos = new Vector3(0, 0, currentDeskStackIndex * deskHandcuffZOffset);

            handcuffToTransfer.transform.localPosition = stackPos;

            playerHandcuffs.RemoveAt(lastIndex);
            currentDeskStackIndex++;
        }

        isTransferringHandcuffs = false;
    }
}