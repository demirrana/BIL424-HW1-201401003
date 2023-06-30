using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Collecter : MonoBehaviour
{
    public Transform parentTransform;
    MovementController inputActions;
    Animator animator;

    Transform collectible;

    bool canCollect = false;

    int num = 0;

    void Awake() 
    {
        animator = GetComponent<Animator>();

        inputActions = new MovementController();
        inputActions.Enable();

        inputActions.Movement.Collect.started += OnCollectStarted;
        inputActions.Movement.Collect.canceled += OnCollectCanceled;
    }

    void Update()
    {
        num = 0;
        
        foreach (Transform child in parentTransform)
        {
            if (Vector3.Distance(child.transform.position, transform.position) < 2f)
            {
                num++;
                collectible = child;
            }
        }

        if (num > 0)
        {
            Debug.Log("Etrafta toplanabilecek esyalar var. (E'ye basin.)");
            canCollect = true;
        }
    }

    void OnCollectStarted(InputAction.CallbackContext context)
    {
        if (canCollect)
        {
            Invoke("SendCollectible", 3f);
            animator.SetBool("isCollecting", true);
            Invoke("CancelCollect", 3.2f);
        }
    }

    void SendCollectible()
    {
        collectible.transform.position = new Vector3(9999f, 9999f, 9999f);
    }

    void OnCollectCanceled(InputAction.CallbackContext context)
    {
        animator.SetBool("isCollecting", false);
    }

    void CancelCollect()
    {
        animator.SetBool("isCollecting", false);
    }
}