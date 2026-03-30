using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeZone : MonoBehaviour
{
    [Header("업그레이드 비용")]
    public int drillCost = 20;
    public int excavatorCost = 50;

    [Header("UI 설정")]
    public TMP_Text costText;

    public Image toolIconImage;
    public Sprite drillSprite;
    public Sprite excavatorSprite;
    public Sprite maxLevelSprite;

    [Header("노동자 구매 존 연동")]
    public GameObject workerZoneObject;

    private void Start()
    {
        UpdateUI(MiningToolType.Pickaxe);
        if (workerZoneObject != null) workerZoneObject.SetActive(false);
    }

    public void UpdateUI(MiningToolType currentTool)
    {
        if (currentTool == MiningToolType.Pickaxe)
        {
            if (costText != null) costText.text = drillCost + " G";
            if (toolIconImage != null && drillSprite != null) toolIconImage.sprite = drillSprite;
        }
        else if (currentTool == MiningToolType.Drill)
        {
            if (costText != null) costText.text = excavatorCost + " G";
            if (toolIconImage != null && excavatorSprite != null) toolIconImage.sprite = excavatorSprite;
        }
        else
        {
            if (costText != null) costText.text = "MAX";
            if (toolIconImage != null && maxLevelSprite != null) toolIconImage.sprite = maxLevelSprite;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerMovement player = other.GetComponent<PlayerMovement>();
        if (player != null)
        {
            if (player.currentTool == MiningToolType.Pickaxe && player.currentGold >= drillCost)
            {
                player.SpendGold(drillCost);
                player.UpgradeTool();
                UpdateUI(player.currentTool);

                if (workerZoneObject != null) workerZoneObject.SetActive(true);
            }
            else if (player.currentTool == MiningToolType.Drill && player.currentGold >= excavatorCost)
            {
                player.SpendGold(excavatorCost);
                player.UpgradeTool();
                gameObject.SetActive(false);
            }
        }
    }
}