using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Teleporter : MonoBehaviour
{
    MovementController inputActions;
    Animator animator;

    float waitToTeleport = 0f;

    bool teleported = false;
    bool isWaiting = false;

    // Start is called before the first frame update
    void Awake()
    {
        animator = GetComponent<Animator>(); 

        inputActions = new MovementController();
        inputActions.Enable();

        inputActions.Movement.Teleport.started += OnTeleportStarted;
        inputActions.Movement.Teleport.canceled += OnTeleportCanceled;
    }

    void Start()
    {
        Debug.Log("Isinlanmak icin B'ye basin.");
    }

    void FixedUpdate()
    {
        if (teleported && waitToTeleport < 0.01f) //Isinlanmaya yeni basilmis ve beklenecek sure baslamamissa
        {
            waitToTeleport = 15f;
            teleported = false;
            isWaiting = true;
        }
        else if (!teleported && waitToTeleport >= 0.01f) //Isinlanmak icin beklenecek hala sure varsa bitene kadar azalir
        {
            waitToTeleport -= Time.fixedDeltaTime;
        }
        else if (!teleported && waitToTeleport < 0.01f) //Isinlanma kullanilabilir halde ama kullanilmamissa isWaiting guncellenir
        {
            isWaiting = false; //Ki OnTeleportStarted istendigi zaman cagrilamasin
        }
    }

    void OnTeleportStarted(InputAction.CallbackContext context)
    {
        if (!isWaiting)
        {
            animator.SetTrigger("TeleportTrigger");
            teleported = true;
            Invoke("Teleport", 3f);
        }
        else
        {
            Debug.Log("Isinlanma henuz kullanilamaz.");
        }
    }

    void OnTeleportCanceled(InputAction.CallbackContext context)
    {
        animator.SetBool("isTeleportFinished", true);
    }

    void Teleport()
    {
        animator.SetBool("isTeleportFinished", true);
        transform.position += Vector3.forward*5 + Vector3.right*5;
        Debug.Log("Tekrar isinlanabilmek icin 15 saniye bekleme islemi suruyor.");
    }
}
