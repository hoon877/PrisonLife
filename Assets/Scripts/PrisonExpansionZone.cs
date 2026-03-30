using UnityEngine;
using TMPro;

public class PrisonExpansionZone : MonoBehaviour
{
    [Header("감옥 증설 설정")]
    public int expansionCost = 50;    
    public int additionalCapacity = 80;
    public TMP_Text costText;

    [Header("연결")]
    public DeskController deskController;

    [Header("시각적 확장 (Visuals)")]
    public GameObject newPrisonVisual;
    public GameObject oldPrisonWalls;

    public Transform prisonFocusPoint;

    private void Start()
    {
        if (costText != null) costText.text = expansionCost + " G";

        if (newPrisonVisual != null) newPrisonVisual.SetActive(false);
    }

    private void OnEnable()
    {
        if (Time.timeSinceLevelLoad > 0.5f && CameraManager.Instance != null)
        {
            CameraManager.Instance.ShowUpgradeZone(this.transform);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerMovement player = other.GetComponent<PlayerMovement>();

        if (player != null && player.currentGold >= expansionCost)
        {
            player.SpendGold(expansionCost);

            if (deskController != null) deskController.ExpandPrison(additionalCapacity);

            if (newPrisonVisual != null) newPrisonVisual.SetActive(true);
            if (oldPrisonWalls != null) oldPrisonWalls.SetActive(false);

            if (CameraManager.Instance != null && newPrisonVisual != null)
            {
                Transform target = prisonFocusPoint != null ? prisonFocusPoint : newPrisonVisual.transform;

                CameraManager.Instance.ShowUpgradeZone(target);
            }

            gameObject.SetActive(false);
        }
    }
}