using System;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;


namespace FGJ26
{

    public enum ActionType
    {
        move,
        turn
    }

    public class PlayerController : MonoBehaviour, ITurnControllable
    {
        public static PlayerController instance;

        private InputAction moveAction;

        private Vector2 moveDirection;

        public Camera mainCamera;

        public AnimationCurve rotationCurve;

        public AnimationCurve movementCurve;

        private float currentAnimationRotationY = 0f;
        private float rotationStartY = 0f;
        private float rotationTargetY = 0f;
        private float rotationPhase = 0f;
        [SerializeField]
        private float rotationSpeed = 1.5f;

        private Vector3 forwardDirection = Vector3.forward;

        private bool rotationFired = false;

        private Vector3 movementCurrentAnimationPosition = Vector3.zero;
        private Vector3 movementStartPosition = Vector3.zero;
        private Vector3 movementTargetPosition = Vector3.zero;
        private float movementPhase = 0f;
        [SerializeField]
        private float movementSpeed = 1.5f;

        private int movementStep = 2;

        private bool movementFired = false;

        private bool transformFired = false;

        private LevelTile currentTile;

        public int MaxActionPoints { get { return _actionPoints; } }
        public int CurrentActionPoints
        {
            get { return _currentActionPoints; }
            set
            {
                _currentActionPoints = value;
            }
        }

        public int MovementCost { get { return _movementCost; } }


        [SerializeField]
        private int _actionPoints = 5000;
        [SerializeField]
        private int _movementCost = 2;
        [SerializeField]
        private int _turnCost = 1;

        private int _currentActionPoints = 0;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
            TurnControlSystem.instance.AddTurnControllable(this);
            moveAction = InputSystem.actions.FindAction("Move");
            CheckTile(gameObject.transform.position);
        }

        // Update is called once per frame
        void Update()
        {
            moveDirection = moveAction.ReadValue<Vector2>();

            if (moveDirection.x > 0 && !rotationFired && !transformFired && HasEnoughActionPoints(ActionType.turn))
            {
                rotationStartY = rotationTargetY;
                rotationTargetY += 90f;
                // forwardDirection = new Vector3();
                rotationFired = true;
                transformFired = true;
                CurrentActionPoints -= _turnCost;
            }
            else if (moveDirection.x < 0 && !rotationFired && !transformFired && HasEnoughActionPoints(ActionType.turn))
            {
                rotationStartY = rotationTargetY;
                rotationTargetY -= 90f;
                rotationFired = true;
                transformFired = true;
                CurrentActionPoints -= _turnCost;
            }
            else if (moveDirection.y > 0 && !movementFired && !transformFired && HasEnoughActionPoints(ActionType.move))
            {
                if (IsMovementValid(forwardDirection))
                {
                    movementStartPosition = gameObject.transform.position;
                    movementTargetPosition = gameObject.transform.position + forwardDirection * movementStep;
                    movementFired = true;
                    transformFired = true;
                    _currentActionPoints -= MovementCost;
                }
                else
                {
                    // Debug.Log("movement blocked");
                }
            }
            else if (moveDirection.y < 0 && !movementFired && !transformFired && HasEnoughActionPoints(ActionType.move))
            {
                if (IsMovementValid(forwardDirection))
                {
                    movementStartPosition = gameObject.transform.position;
                    movementTargetPosition = gameObject.transform.position - forwardDirection * movementStep;
                    movementFired = true;
                    transformFired = true;
                    _currentActionPoints -= MovementCost;
                }
                else
                {
                    // Debug.Log("movement blocked");
                }
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

        private bool IsMovementValid(Vector3 direction)
        {
            if (direction.z > 0.5)
            {
                if (currentTile.tileWallNorth.type == WallType.wall)
                {
                    return false;
                }
                if (currentTile.tileWallNorth.type == WallType.door)
                {
                    return false;
                }
            }
            if (direction.z < -0.5)
            {
                if (currentTile.tileWallSouth.type == WallType.wall)
                {
                    return false;
                }
                if (currentTile.tileWallSouth.type == WallType.door)
                {
                    return false;
                }
            }
            if (direction.x > 0.5)
            {
                if (currentTile.tileWallEast.type == WallType.wall)
                {
                    return false;
                }
                if (currentTile.tileWallEast.type == WallType.door)
                {
                    return false;
                }
            }
            if (direction.x < -0.5)
            {
                if (currentTile.tileWallWest.type == WallType.wall)
                {
                    return false;
                }
                if (currentTile.tileWallWest.type == WallType.door)
                {
                    return false;
                }
            }
            return true;
        }

        private bool HasEnoughActionPoints(ActionType action)
        {
            switch (action)
            {
                case ActionType.move:
                    return _currentActionPoints >= MovementCost;
                case ActionType.turn:
                    return _currentActionPoints >= _turnCost;
                default:
                    return false;
            }
        }

        private void GetNewForwardDirection()
        {
            Vector3 newForwardDirection = new Vector3(Mathf.Sin(currentAnimationRotationY * Mathf.Deg2Rad), 0, Mathf.Cos(currentAnimationRotationY * Mathf.Deg2Rad));
            forwardDirection.Set((float)Math.Round(newForwardDirection.x), (float)Math.Round(newForwardDirection.y), (float)Math.Round(newForwardDirection.z));
            // Debug.Log("new forward direction: " + forwardDirection);
        }

        private void CheckTile(Vector3 position)
        {

            // Debug.Log("checking tile at: " + position);
            RaycastHit hit;
            if (Physics.Raycast(position + new Vector3(0, 10, 0), new Vector3(0, -1, 0), out hit, 100f, LayerMask.GetMask("LevelTiles")))
            {
                // Debug.Log("hit: " + hit.collider.gameObject.name);
                currentTile = hit.collider.gameObject.GetComponent<LevelTile>();
                Debug.Log("current tile: " + currentTile);
            }
        }

        public void StartTurn()
        {
            _currentActionPoints = MaxActionPoints;
            // OnTurnEnded();
            Debug.Log("Starting player turn with AP:" + _currentActionPoints);
        }

        public event Action OnTurnEnded;

    }
}
