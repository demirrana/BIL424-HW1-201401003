using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public Transform target;
    Vector3 offset; //Karakterle aradaki mesafe

    float rotationSpeed = 0.03f;
    float minRotationY = -30f;
    float maxRotationY = 30f;
    float minRotationX = -60f;
    float maxRotationX = 60f;

    Vector2 rotationInput;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        
        offset = transform.position - target.position;
    }

    void Update()
    {
        RotateCamera();
    }

    void LateUpdate()
    {
        transform.position = target.position + offset; //Karakterden ayni uzaklikta olur hep
    }

    void RotateCamera()
    {
        Vector2 rotationInput = InputSystem.GetDevice<Mouse>().delta.ReadValue();

        float rotationX = Mathf.Clamp(rotationInput.x * rotationSpeed, minRotationX, maxRotationX);
        float rotationY = Mathf.Clamp(rotationInput.y * rotationSpeed, minRotationY, maxRotationY);

        transform.RotateAround(target.position, Vector3.up, rotationX);

        Vector3 desiredPosition = target.position - transform.forward * Vector3.Distance(transform.position, target.position);

        transform.RotateAround(desiredPosition, transform.right, rotationY);

        transform.position = desiredPosition + transform.forward * Vector3.Distance(transform.position, desiredPosition);
    }
}