using UnityEngine;

public class ZoneCapacityUI : MonoBehaviour
{
    [Header("모니터링할 구역 설정")]
    public Transform targetZone;  
    public int maxCapacity = 100; 

    [Header("UI 설정")]
    public GameObject maxUIObject; 

    private bool isShowing = false;

    void Start()
    {
        if (maxUIObject != null)
        {
            maxUIObject.SetActive(false);
            isShowing = false;
        }
    }

    void Update()
    {
        if (targetZone != null && maxUIObject != null)
        {
            bool shouldShow = targetZone.childCount >= maxCapacity;

            if (shouldShow != isShowing)
            {
                isShowing = shouldShow;
                maxUIObject.SetActive(isShowing);
            }
        }
    }
}