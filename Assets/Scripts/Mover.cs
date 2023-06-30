using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Mover : MonoBehaviour
{
    Rigidbody rb;
    MovementController inputActions;
    Animator animator;

    float walkingSpeed = 2.4f;
    float runningSpeed = 5f;
    float coolDown = 0;
    [SerializeField] float rotationSpeed = 170f;
    [SerializeField] float jumpForce = 9f;
    [SerializeField] float doubleJumpForce = 7f;
    [SerializeField] float moveForce = 1.5f;

    bool isWalking = false;
    bool isRunning = false;
    bool isJumping = false;
    bool isGrounded = true;
    bool shiftIsPressed = false;
    bool firstJump = false;
    bool finishedFirstJump = false;
    bool secondJump = false;
    bool finishedSecondJump = false;
    bool coolDownStarted = false; //5 saniye kostuktan sonra cooldown baslamasini kontrol eder

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        inputActions = new MovementController();
        inputActions.Enable();

        //Hareket
        inputActions.Movement.Move.performed += OnMovePerformed;
        inputActions.Movement.Move.canceled += OnMoveCanceled;
        inputActions.Movement.MoveFast.started += OnMoveFastStarted;
        inputActions.Movement.MoveFast.performed += OnMoveFastPerformed;
        inputActions.Movement.MoveFast.canceled += OnMoveFastCanceled;
        inputActions.Movement.Jump.started += OnJumpStarted;
    }

    void Update()
    {
        if (isJumping) //Ziplamak icin space'e basilmis
        {
            if (firstJump && !finishedFirstJump) //Yerdeyse ilk ziplamasi
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
                //rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
                finishedFirstJump = true;
            }
            else if (secondJump && finishedFirstJump && !finishedSecondJump) //Ya da daha 1 kez zipladiysa 2.yi yapacak
            {
                rb.AddForce(Vector3.up * doubleJumpForce, ForceMode.VelocityChange);
                //rb.velocity = new Vector3(rb.velocity.x, doubleJumpForce, rb.velocity.z);
                isJumping = false;
                animator.SetBool("isJumpingFinished", true);
                finishedSecondJump = true;
            }
        }
        else //Ziplamiyorken hareket ederiz
        {
            Vector2 moveInput = inputActions.Movement.Move.ReadValue<Vector2>();

            if (isGrounded) //Yere degene kadar hareket edemez
            {
                animator.SetBool("isJumpingFinished", true); //Yere degdiginde ziplama durur

                if (isRunning)
                {
                    Vector3 movement = new Vector3(moveInput.x, 0.0f, moveInput.y) * runningSpeed * Time.deltaTime;
                    rb.AddForce(movement * moveForce, ForceMode.VelocityChange);
                }
                else if (isWalking)
                {
                    Vector3 movement = new Vector3(moveInput.x, 0.0f, moveInput.y) * walkingSpeed * Time.deltaTime;
                    rb.AddForce(movement * moveForce * 2f, ForceMode.VelocityChange);
                }
            }

            if (moveInput.magnitude > 0) //Hareket etme inputu varsa gidecegi yone donmesi
            {
                Quaternion targetRotation = Quaternion.LookRotation(new Vector3(moveInput.x, 0.0f, moveInput.y));
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

                isWalking = true;
            }
            else //Hareket etmez
            {
                isWalking = false;
            }
        }

        animator.SetBool("isWalking", isWalking);
        animator.SetBool("isRunning", isRunning);
    }

    void FixedUpdate() //Kosma yeteneginin suresi dolmus mu bakar
    {
        if (coolDown <= 0.01f) //Kosabilecek durumdaysa (yani en basta)
        {
            if (shiftIsPressed)
            {
                coolDown += 2 * Time.fixedDeltaTime;
                coolDownStarted = false;
                isRunning = true;
            }
        }
        else //Kosamayacak bir durumdaysa veya kosmaya basladi ve daha suresi varsa
        {
            if (shiftIsPressed) //Shifte hala basiliyor
            {
                if (coolDown < 10f && coolDownStarted) //5 saniyeyi gecmis ve azaliyor
                {
                    coolDown -= Time.fixedDeltaTime;
                    Debug.Log("Hizli kosma su anda kullanilamaz.");
                }
                else if (coolDown < 10f && !coolDownStarted) //Hala kosabilir 5 saniye dolana kadar
                {
                    coolDown += 2 * Time.fixedDeltaTime;
                    isRunning = true;
                }
                else if (coolDown >= 10f) //Cooldown olana kadar kosamaz
                {
                    coolDown -= Time.fixedDeltaTime;
                    coolDownStarted = true;
                    isRunning = false;
                    Debug.Log("Tekrar kosana kadar 10 saniye bekleme islemi suruyor.");
                }
            }
            else if (coolDownStarted) //Shifte basmasa bile ve cooldown suresi beklenecekse o sure 0'a inene kadar devam eder
            {
                coolDown -= Time.fixedDeltaTime;
                Debug.Log("Tekrar kosana kadar 10 saniye bekleme islemi suruyor.");
            }
            else //Cooldown henuz baslamadi ve shifte de basmiyorsam bekleme suresi azalmali (cooldown azalmali)
            {
                if (coolDown > 0.01f)
                {
                    coolDown -= Time.fixedDeltaTime;
                }
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground")) //Yere degdigi an ziplamayla ilgili tum booleanlar sifirlanir
        {
            isGrounded = true;
            firstJump = false;
            secondJump = false;
            isJumping = false;
            finishedFirstJump = false;
            finishedSecondJump = false;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

    void OnMovePerformed(InputAction.CallbackContext context)
    {
        Vector2 movement = context.ReadValue<Vector2>();
        isWalking = true;
    }

    void OnMoveFastPerformed(InputAction.CallbackContext context) //isRunning, FixedUpdate'te guncelleniyor
    {
        animator.SetBool("isRunning", isRunning);
        shiftIsPressed = true;
    }

    void OnMoveFastStarted(InputAction.CallbackContext context) //Her sekilde ilk shifte basildiginda kosar duruma getirir
    {
        animator.SetBool("isRunning", isRunning);
        shiftIsPressed = true;
    }

    void OnJumpStarted(InputAction.CallbackContext context) //Ziplamanin kacinci ziplama oldugunu kontrol edip trigger isini yapar.
    {
        isJumping = true;

        if (finishedSecondJump)
        {
            Debug.Log("3. kez ziplanamaz.");
        }

        if (isGrounded)
        {
            animator.SetTrigger("JumpTrigger");
            firstJump = true;
        }
        else if (firstJump && !secondJump) //Ilk atlayisini yapmissa ikinciyi yapiyordur
        {
            firstJump = false;
            secondJump = true;
        }
        else if (!firstJump && secondJump)
        {
            firstJump = false;
            secondJump = false;
        }
    }

    void OnMoveCanceled(InputAction.CallbackContext context)
    {
        isWalking = false;
    }

    void OnMoveFastCanceled(InputAction.CallbackContext context)
    {
        isRunning = false;
        animator.SetBool("isRunning", false);
        shiftIsPressed = false;
    }

    void OnDestroy()
    {
        inputActions.Disable();
        inputActions.Movement.Move.performed -= OnMovePerformed;
        inputActions.Movement.Move.canceled -= OnMoveCanceled;
        inputActions.Movement.MoveFast.started -= OnMoveFastStarted;
        inputActions.Movement.MoveFast.performed -= OnMoveFastPerformed;
        inputActions.Movement.MoveFast.canceled -= OnMoveFastCanceled;
        inputActions.Movement.Jump.started -= OnJumpStarted;
    }
}