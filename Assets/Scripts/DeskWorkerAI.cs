using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeskWorkerAI : MonoBehaviour
{
    [Header("설정")]
    public float moveSpeed = 4f;
    public float turnSpeed = 15f;
    public int maxCapacity = 5;
    public float handcuffYOffset = 0.1f;
    public float deskHandcuffZOffset = 0.0001f;
    [HideInInspector] public Transform sourceZone; 

    [HideInInspector] public Transform deskHandcuffDropPoint;
    [HideInInspector] public Transform waitPos;

    [Header("수갑 들고 있을 위치 (손/등)")]
    public Transform handcuffStackParent;

    private Animator anim;
    private List<GameObject> carriedHandcuffs = new List<GameObject>();
    private bool isWorking = false;

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (isWorking) return;

        if (carriedHandcuffs.Count > 0)
        {
            if (waitPos != null)
            {
                MoveTowards(waitPos.position);

                if (Vector3.Distance(transform.position, waitPos.position) < 1.0f)
                {
                    if (deskHandcuffDropPoint != null)
                    {
                        StartCoroutine(DropHandcuffsRoutine());
                    }
                }
            }
        }
        else
        {
            if (sourceZone != null && sourceZone.childCount > 0)
            {
                MoveTowards(sourceZone.position);
                if (Vector3.Distance(transform.position, sourceZone.position) < 1.0f)
                {
                    StartCoroutine(PickUpHandcuffsRoutine());
                }
            }
            else if (waitPos != null)
            {
                MoveTowards(waitPos.position);

                if (Vector3.Distance(transform.position, waitPos.position) < 0.5f)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, waitPos.rotation, turnSpeed * Time.deltaTime);
                    if (anim != null) anim.SetBool("Walk", false);
                }
            }
        }

        if (anim != null) anim.SetBool("Carrying", carriedHandcuffs.Count > 0);
    }

    void MoveTowards(Vector3 targetPos)
    {
        Vector3 moveDir = targetPos - transform.position;
        moveDir.y = 0;

        if (moveDir.magnitude > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(targetPos.x, transform.position.y, targetPos.z), moveSpeed * Time.deltaTime);
            Quaternion targetRotation = Quaternion.LookRotation(moveDir.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);

            if (anim != null) anim.SetBool("Walk", true);
        }
    }

    IEnumerator PickUpHandcuffsRoutine()
    {
        isWorking = true;
        if (anim != null) anim.SetBool("Walk", false);

        while (sourceZone != null && sourceZone.childCount > 0 && carriedHandcuffs.Count < maxCapacity)
        {
            Transform targetHandcuff = sourceZone.GetChild(sourceZone.childCount - 1);

            Rigidbody rb = targetHandcuff.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;

            Collider col = targetHandcuff.GetComponent<Collider>();
            if (col != null) col.enabled = false;

            targetHandcuff.SetParent(handcuffStackParent);

            int currentCount = carriedHandcuffs.Count;
            Vector3 stackPos = new Vector3(0, currentCount * handcuffYOffset, 0);

            targetHandcuff.localPosition = stackPos;
            targetHandcuff.localRotation = Quaternion.identity;

            carriedHandcuffs.Add(targetHandcuff.gameObject);

            yield return new WaitForSeconds(0.1f);
        }

        isWorking = false;
    }

    IEnumerator DropHandcuffsRoutine()
    {
        isWorking = true;
        if (anim != null) anim.SetBool("Walk", false);

        if (deskHandcuffDropPoint != null)
        {
            int currentDeskStackIndex = deskHandcuffDropPoint.childCount;

            while (carriedHandcuffs.Count > 0)
            {

                int lastIndex = carriedHandcuffs.Count - 1;
                GameObject handcuff = carriedHandcuffs[lastIndex];

                yield return new WaitForSeconds(0.1f);

                handcuff.transform.SetParent(deskHandcuffDropPoint);

                Rigidbody rb = handcuff.GetComponent<Rigidbody>();
                if (rb != null) rb.isKinematic = true;

                Collider col = handcuff.GetComponent<Collider>();
                if (col != null) col.enabled = false;

                Vector3 stackPos = new Vector3(0, 0, currentDeskStackIndex * deskHandcuffZOffset);

                handcuff.transform.localPosition = stackPos;

                carriedHandcuffs.RemoveAt(lastIndex);
                currentDeskStackIndex++;
            }
        }

        isWorking = false;
    }
}