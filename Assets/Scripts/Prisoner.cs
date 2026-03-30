using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Prisoner : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 3f;
    public float turnSpeed = 10f;

    [Header("UI 설정")]
    public GameObject speechBubble;
    public TMP_Text handcuffText;

    [HideInInspector] public int requiredHandcuffs = 1;

    private Animator anim;
    private DeskController targetDesk;
    private Transform prisonWaypoint;

    private Vector3 cellTargetPos;
    private Vector3 currentTargetPos;
    private bool hasReachedSpot = false;

    private enum State { WalkingToDesk, WaitingAtDesk, WalkingToPrisonEntrance, WalkingToCell, IdleInCell }
    private State currentState;

    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
    }

    public void InitializeAsNormal(DeskController desk, Transform prison)
    {
        targetDesk = desk;
        prisonWaypoint = prison;
        hasReachedSpot = false;
        currentState = State.WalkingToDesk;

        if (speechBubble != null) speechBubble.SetActive(false);
        if (handcuffText != null) handcuffText.text = requiredHandcuffs.ToString();
    }

    public void ShowSpeechBubble(bool isVisible)
    {
        if (speechBubble != null) speechBubble.SetActive(isVisible);
    }

    public void SetDestination(Vector3 targetPos)
    {
        currentTargetPos = targetPos;
        hasReachedSpot = false;
        currentState = State.WalkingToDesk;
    }

    public void TransformAndGoToPrison(Vector3 finalCellPos)
    {
        GameObject jailbird = ObjectPool.Instance.GetJailbird();

        if (jailbird != null)
        {
            jailbird.transform.position = transform.position;
            jailbird.transform.rotation = transform.rotation;

            Prisoner jailbirdScript = jailbird.GetComponent<Prisoner>();
            if (jailbirdScript != null)
            {
                jailbirdScript.InitializeAsJailbird(prisonWaypoint, finalCellPos);
            }

            jailbird.SetActive(true);
        }

        gameObject.SetActive(false);
    }

    public void InitializeAsJailbird(Transform entrance, Vector3 finalCellPos)
    {
        prisonWaypoint = entrance;
        cellTargetPos = finalCellPos;

        if (prisonWaypoint != null) currentTargetPos = prisonWaypoint.position;
        hasReachedSpot = false;

        currentState = State.WalkingToPrisonEntrance;

        if (speechBubble != null) speechBubble.SetActive(false);
    }

    void Update()
    {
        if (currentState == State.IdleInCell || currentState == State.WaitingAtDesk)
        {
            if (anim != null) anim.SetBool("Walk", false);

            if (currentState == State.WaitingAtDesk && targetDesk != null)
            {
                Vector3 lookDir = targetDesk.transform.position - transform.position;
                lookDir.y = 0;
                if (lookDir != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(lookDir);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
                }
            }
            return;
        }

        float distance = Vector3.Distance(transform.position, currentTargetPos);

        if (distance > 0.05f)
        {
            Vector3 moveDir = (currentTargetPos - transform.position).normalized;
            transform.position = Vector3.MoveTowards(transform.position, currentTargetPos, moveSpeed * Time.deltaTime);

            moveDir.y = 0;
            if (moveDir != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
            }

            if (anim != null) anim.SetBool("Walk", true);
        }
        else
        {
            if (anim != null) anim.SetBool("Walk", false);

            if (!hasReachedSpot)
            {
                hasReachedSpot = true;

                if (currentState == State.WalkingToPrisonEntrance)
                {
                    currentState = State.WalkingToCell;
                    currentTargetPos = cellTargetPos;
                    hasReachedSpot = false; 
                }
                else if (currentState == State.WalkingToCell)
                {
                    currentState = State.IdleInCell;

                    transform.rotation = Quaternion.Euler(0, 180, 0);
                }
                else if (currentState == State.WalkingToDesk)
                {
                    currentState = State.WaitingAtDesk;
                    if (targetDesk != null) targetDesk.OnPrisonerReachedSpot(this);
                }
            }
        }
    }
}