using System;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;


namespace FGJ26
{

    public class PlayerController : MonoBehaviour
    {

        private InputAction moveAction;

        private Vector2 moveDirection;

        public Camera mainCamera;

        public AnimationCurve rotationCurve;

        public AnimationCurve movementCurve;

        private float currentAnimationRotationY = 0f;
        private float rotationStartY = 0f;
        private float rotationTargetY = 0f;
        private float rotationPhase = 0f;
        private float rotationSpeed = 1.5f;

        private Vector3 forwardDirection = Vector3.forward;

        private bool rotationFired = false;

        private Vector3 movementCurrentAnimationPosition = Vector3.zero;
        private Vector3 movementStartPosition = Vector3.zero;
        private Vector3 movementTargetPosition = Vector3.zero;
        private float movementPhase = 0f;
        private float movementSpeed = 1.5f;

        private int movementStep = 2;

        private bool movementFired = false;

        private bool transformFired = false;

        private LevelTile currentTile;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            moveAction = InputSystem.actions.FindAction("Move");
            CheckTile(gameObject.transform.position);
        }

        // Update is called once per frame
        void Update()
        {
            moveDirection = moveAction.ReadValue<Vector2>();

            if (moveDirection.x > 0 && !rotationFired && !transformFired)
            {
                rotationStartY = rotationTargetY;
                rotationTargetY += 90f;
                // forwardDirection = new Vector3();
                rotationFired = true;
                transformFired = true;
            }
            else if (moveDirection.x < 0 && !rotationFired && !transformFired)
            {
                rotationStartY = rotationTargetY;
                rotationTargetY -= 90f;
                rotationFired = true;
                transformFired = true;
            }
            else if (moveDirection.y > 0 && !movementFired && !transformFired)
            {
                movementStartPosition = gameObject.transform.position;
                movementTargetPosition = gameObject.transform.position + forwardDirection * movementStep;
                movementFired = true;
                transformFired = true;
            }
            else if (moveDirection.y < 0 && !movementFired && !transformFired)
            {
                movementStartPosition = gameObject.transform.position;
                movementTargetPosition = gameObject.transform.position - forwardDirection * movementStep;
                movementFired = true;
                transformFired = true;
            }

            if (rotationFired)
            {
                rotationPhase += Time.deltaTime * rotationSpeed;
                currentAnimationRotationY = Mathf.LerpAngle(rotationStartY, rotationTargetY, rotationCurve.Evaluate(rotationPhase));
            }
            else if (movementFired)
            {
                movementPhase += Time.deltaTime * movementSpeed;
                movementCurrentAnimationPosition = Vector3.Lerp(movementStartPosition, movementTargetPosition, movementCurve.Evaluate(movementPhase));
            }
            if (rotationPhase >= 1f)
            {
                rotationPhase = 0f;
                rotationFired = false;
                currentAnimationRotationY = rotationTargetY;
                GetNewForwardDirection();
                transformFired = false;
            }
            if (movementPhase >= 1f)
            {
                movementPhase = 0f;
                movementFired = false;
                transformFired = false;
                movementCurrentAnimationPosition = movementTargetPosition;
                CheckTile(movementCurrentAnimationPosition);
            }

            // gameObject.transform.rotation = Quaternion.Slerp(rotationStartQuaternion, rotationTargetQuaternion, Time.deltaTime * rotationSpeed);
            gameObject.transform.rotation = Quaternion.Euler(0, currentAnimationRotationY, 0);
            gameObject.transform.position = movementCurrentAnimationPosition;
        }

        private void GetNewForwardDirection()
        {
            Vector3 newForwardDirection = new Vector3(Mathf.Sin(currentAnimationRotationY * Mathf.Deg2Rad), 0, Mathf.Cos(currentAnimationRotationY * Mathf.Deg2Rad));
            forwardDirection.Set((float)Math.Round(newForwardDirection.x), (float)Math.Round(newForwardDirection.y), (float)Math.Round(newForwardDirection.z));
            // Debug.Log("new forward direction: " + forwardDirection);
        }

        private void CheckTile(Vector3 position)
        {

            Debug.Log("checking tile at: " + position);
            RaycastHit hit;
            if (Physics.Raycast(position + new Vector3(0, 10, 0), new Vector3(0, -1, 0), out hit, 100f, LayerMask.GetMask("LevelTiles")))
            {
                Debug.Log("hit: " + hit.collider.gameObject.name);
                currentTile = hit.collider.gameObject.GetComponent<LevelTile>();
                Debug.Log("current tile: " + currentTile);
            }
        }
    }
}
