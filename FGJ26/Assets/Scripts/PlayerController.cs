using System;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    private InputAction moveAction;

    private Vector2 moveDirection;

    public Camera mainCamera;

    private float currentRotationY = 0f;
    private float rotationTargetY = 0f;
    private float rotationPhase = 0f;
    private float rotationSpeed = 0.1f;

    private bool rotationFired = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        moveAction = InputSystem.actions.FindAction("Move");
    }

    // Update is called once per frame
    void Update()
    {
        moveDirection = moveAction.ReadValue<Vector2>();

        if (moveDirection.x > 0 && !rotationFired)
        {
            rotationTargetY += 90f;
            rotationFired = true;
        }
        else if (moveDirection.x < 0 && !rotationFired)
        {
            rotationTargetY -= 90f;
            rotationFired = true;
        }

        if (rotationFired)
        {
            rotationPhase += Time.deltaTime * rotationSpeed;
            currentRotationY = Mathf.LerpAngle(0, rotationTargetY, rotationPhase);
        }
        if (rotationPhase >= 1f)
        {
            rotationPhase = 0f;
            rotationFired = false;
            currentRotationY = rotationTargetY;
        }

        // gameObject.transform.rotation = Quaternion.Slerp(rotationStartQuaternion, rotationTargetQuaternion, Time.deltaTime * rotationSpeed);
        gameObject.transform.rotation = Quaternion.Euler(0, currentRotationY, 0);
    }
}
