using System;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using NaughtyAttributes;


namespace FGJ26
{

    public enum ActionType
    {
        move,
        turn,
        mask
    }

    public class PlayerController : MonoBehaviour, ITurnControllable
    {
        public static PlayerController instance;

        [BoxGroup("Sound")]
        public AudioSource step1;
        [BoxGroup("Sound")]
        public AudioSource step2;
        [BoxGroup("Sound")]
        public AudioSource step3;
        [BoxGroup("Sound")]
        public float stepInterval = 0.5f;

        [BoxGroup("Sound")]
        public AudioSource maskChangeSource;


        public MaskType maskType
        {
            get
            {
                return _maskType;
            }
            set
            {
                bool changed = value != _maskType;
                _maskType = value;
                if (changed)
                {
                    OnMaskTypeChanged?.Invoke();
                }
            }
        }

        private MaskType _targetMaskType = MaskType.Basic;
        private MaskType _maskType = MaskType.Basic;

        private InputAction moveAction;
        private InputAction endTurnAction;

        private InputAction mask1Action;
        private InputAction mask2Action;
        private InputAction mask3Action;
        private InputAction mask4Action;
        private float maskPhase = 0f;
        [SerializeField]
        private float maskChangeSpeed = 2f;

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
        private float stepTimer = 0f;
        private AudioSource nextStepSource;

        private int movementStep = 2;

        private bool movementFired = false;
        private bool MaskFired
        {
            get { return _maskFired; }
            set
            {
                if (value == true)
                {
                    Debug.Log("mask changing " + _targetMaskType);
                    maskChangeSource.Play();
                    CurrentActionPoints -= _maskCost;
                }
                _maskFired = value;
            }
        }
        private bool _maskFired = false;

        private bool inputActionInProgress = false;

        private LevelTile currentTile;

        public int MaxActionPoints { get { return _actionPoints; } }
        public int CurrentActionPoints
        {
            get { return _currentActionPoints; }
            set
            {
                bool changed = value != _currentActionPoints;
                _currentActionPoints = value;
                if (_currentActionPoints <= 0)
                {
                    // Debug.LogError("Out of action points!");
                }
                OnActionPointsChanged?.Invoke();
            }
        }
        private int _currentActionPoints = 0;
        public event Action OnActionPointsChanged;
        public event Action OnMaskTypeChanged;

        public int MovementCost { get { return _movementCost; } }

        public bool IsOwnTurn
        {
            get
            {
                return _isOwnTurn;
            }
            set
            {
                _isOwnTurn = value;
            }
        }


        private bool _isOwnTurn = false;


        [SerializeField]
        private int _actionPoints = 5;
        [SerializeField]
        private int _movementCost = 2;
        [SerializeField]
        private int _turnCost = 1;


        [SerializeField]
        private int _maskCost = 3;


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
            endTurnAction = InputSystem.actions.FindAction("EndTurn");
            mask1Action = InputSystem.actions.FindAction("Mask1");
            mask2Action = InputSystem.actions.FindAction("Mask2");
            mask3Action = InputSystem.actions.FindAction("Mask3");
            mask4Action = InputSystem.actions.FindAction("Mask4");

            CheckTile(gameObject.transform.position);
        }

        // Update is called once per frame
        void Update()
        {
            if (!IsOwnTurn) return;
            // if (inputActionInProgress) return;
            moveDirection = moveAction.ReadValue<Vector2>();
            bool turnEndInput = endTurnAction.triggered && !inputActionInProgress;


            if (turnEndInput)
            {
                IsOwnTurn = false;
                OnTurnEnded();
                return;
            }
            bool mask1Input = mask1Action.triggered && !inputActionInProgress;
            bool mask2Input = mask2Action.triggered && !inputActionInProgress;
            bool mask3Input = mask3Action.triggered && !inputActionInProgress;
            bool mask4Input = mask4Action.triggered && !inputActionInProgress;

            if (mask1Input && HasEnoughActionPoints(ActionType.mask))
            {
                _targetMaskType = MaskType.Basic;
                MaskFired = true;
            }
            else if (mask2Input && HasEnoughActionPoints(ActionType.mask))
            {
                _targetMaskType = MaskType.Advanced;
                MaskFired = true;
            }
            else if (mask3Input && HasEnoughActionPoints(ActionType.mask))
            {
                _targetMaskType = MaskType.Elite;
                MaskFired = true;
            }
            else if (mask4Input && HasEnoughActionPoints(ActionType.mask))
            {
                _targetMaskType = MaskType.Basic;
                MaskFired = true;
            }

            // Debug.Log("updateloop" + turnEndInput + rotationFired + inputActionInProgress + HasEnoughActionPoints(ActionType.turn));
            if (moveDirection.x > 0 && HasEnoughActionPoints(ActionType.turn) && !inputActionInProgress)
            {
                rotationStartY = rotationTargetY;
                rotationTargetY += 90f;
                // forwardDirection = new Vector3();
                rotationFired = true;
                inputActionInProgress = true;
                CurrentActionPoints -= _turnCost;
            }
            else if (moveDirection.x < 0 && HasEnoughActionPoints(ActionType.turn) && !inputActionInProgress)
            {
                rotationStartY = rotationTargetY;
                rotationTargetY -= 90f;
                rotationFired = true;
                inputActionInProgress = true;
                CurrentActionPoints -= _turnCost;
            }
            else if (moveDirection.y > 0 && HasEnoughActionPoints(ActionType.move) && !inputActionInProgress)
            {
                if (IsMovementValid(forwardDirection))
                {
                    movementStartPosition = gameObject.transform.position;
                    movementTargetPosition = gameObject.transform.position + forwardDirection * movementStep;
                    movementFired = true;
                    inputActionInProgress = true;
                    CurrentActionPoints -= MovementCost;
                    nextStepSource = step1;
                }
                else
                {
                    // Debug.Log("movement blocked");
                }
            }
            else if (moveDirection.y < 0 && HasEnoughActionPoints(ActionType.move) && !inputActionInProgress)
            {
                if (IsMovementValid(forwardDirection))
                {
                    movementStartPosition = gameObject.transform.position;
                    movementTargetPosition = gameObject.transform.position - forwardDirection * movementStep;
                    movementFired = true;
                    inputActionInProgress = true;
                    CurrentActionPoints -= MovementCost;
                    nextStepSource = step1;
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
            if (MaskFired)
            {
                maskPhase += Time.deltaTime * maskChangeSpeed;
            }
            if (movementFired)
            {
                stepTimer += Time.deltaTime;
                if (stepTimer >= stepInterval)
                {
                    stepTimer = 0f;
                    nextStepSource.Play();
                }
                movementPhase += Time.deltaTime * movementSpeed;
                movementCurrentAnimationPosition = Vector3.Lerp(movementStartPosition, movementTargetPosition, movementCurve.Evaluate(movementPhase));
            }
            if (rotationPhase >= 1f)
            {
                rotationPhase = 0f;
                rotationFired = false;
                currentAnimationRotationY = rotationTargetY;
                GetNewForwardDirection();
                inputActionInProgress = false;
            }
            if (movementPhase >= 1f)
            {
                movementPhase = 0f;
                movementFired = false;
                inputActionInProgress = false;
                movementCurrentAnimationPosition = movementTargetPosition;
                CheckTile(movementCurrentAnimationPosition);
            }

            if (maskPhase >= 1f)
            {
                maskPhase = 0f;
                maskType = _targetMaskType;
                MaskFired = false;
                inputActionInProgress = false;
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
            // Debug.Log(action + _currentActionPoints);
            switch (action)
            {
                case ActionType.move:
                    return CurrentActionPoints >= MovementCost;
                case ActionType.turn:
                    return CurrentActionPoints >= _turnCost;
                case ActionType.mask:
                    return CurrentActionPoints >= _maskCost;
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
            CurrentActionPoints = MaxActionPoints;
            // OnTurnEnded();
            IsOwnTurn = true;
            Debug.Log("Starting player turn with AP:" + CurrentActionPoints + IsOwnTurn);
        }

        public event Action OnTurnEnded;

    }
}
