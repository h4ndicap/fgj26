using UnityEngine;
using System;
using NaughtyAttributes;


namespace FGJ26
{

    public enum EnemyState
    {
        idle,
        attack,
        finishTurn,
        die
    }

    public class EnemyController : MonoBehaviour, ITurnControllable
    {

        [BoxGroup("Graphics")]
        public Texture2D enemyIdle;
        [BoxGroup("Graphics")]
        public float idleYOffset = 0.333f;
        [BoxGroup("Graphics")]
        public Texture2D enemyAttack;
        [BoxGroup("Graphics")]
        public float attackYOffset = 0.46f;
        [BoxGroup("Graphics")]
        public Texture2D enemyDie;
        [BoxGroup("Graphics")]
        public float dieYOffset = 0.489f;
        [BoxGroup("Graphics")]
        public Material enemyMaterial;
        [BoxGroup("Graphics")]
        public MeshRenderer enemyMeshRenderer;
        [BoxGroup("Graphics")]
        public GameObject enemyMainGraphicsObject;

        [BoxGroup("Graphics")]
        public AnimationCurve attackCurve;

        [BoxGroup("Graphics")]
        public float attackHitPhase = 0.25f;

        private bool attackHit = false;

        private float attackCurvePhase = 0f;

        private Material _enemyMaterialInstance;
        public GameObject enemyPivot;

        public Vector3 pivotBasePosition = Vector3.zero;
        public bool IsOwnTurn
        {
            get { return _isOwnTurn; }
            set { _isOwnTurn = value; }
        }
        private bool _isOwnTurn = false;

        private Vector3 idleScaleMultiplier = new Vector3(0.2f, -0.2f, 1f);

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
        private int _actionPoints = 3;
        private int _currentActionPoints = 0;

        public int AttackActionPoints = 1;

        public int MovementCost { get { return _movementCost; } }
        private int _movementCost = 1;

        public event Action OnActionPointsChanged;

        private EnemyState _enemyState = EnemyState.idle;

        public float currentAnimationSpeed = 1f;
        public float actionDelay = 1f;

        public float attackAnimationDelay = 2f;
        public float attackAnimationSpeed = 25f;
        public float finishTurnAnimationDelay = 2f;
        public float finishTurnAnimationSpeed = 1f;

        [SerializeField]
        private int _attackDamage = 1;
        public int AttackDamage
        {
            get { return _attackDamage; }
            set { _attackDamage = value; }
        }

        public float idleAnimationDelay = 1f;
        public float idleAnimationSpeed = 1f;

        private float actionDelayTimer = 0f;

        public bool executingAnimation = false;
        public float animationTimer = 0f;


        void Start()
        {
            // _currentActionPoints = _actionPoints;

            TurnControlSystem.instance.AddTurnControllable(this);
            enemyMeshRenderer.material = Instantiate(enemyMaterial);
            _enemyMaterialInstance = enemyMeshRenderer.material;
            _enemyMaterialInstance.SetTexture("_BaseMap", enemyIdle);
            enemyMainGraphicsObject.transform.localPosition = new Vector3(enemyMainGraphicsObject.transform.localPosition.x, idleYOffset, enemyMainGraphicsObject.transform.localPosition.z);
            pivotBasePosition = enemyPivot.transform.position;

            LevelTile tile = LevelTile.GetTileAtPosition(gameObject.transform.position);
            if (tile != null)
            {
                tile.Occupy();
            }
            // enemyMainGraphicsObject.transform.position = new Vector3(enemyMainGraphicsObject.transform.position.x, 1000f, enemyMainGraphicsObject.transform.position.z);
        }

        public void StartTurn()
        {
            // throw new System.NotImplementedException();
            IsOwnTurn = true;
            CurrentActionPoints = MaxActionPoints;
            Debug.Log("EnemyController started turn");
        }

        public event Action OnTurnEnded;

        void Update()
        {
            float sinTime = 1 + Mathf.Sin(Time.time * currentAnimationSpeed) * 0.5f;
            float sinTime2 = 1 + Mathf.Sin(Time.time * currentAnimationSpeed * 1.5f) * 0.5f;
            // Debug.Log("sinTime: " + sinTime);
            enemyPivot.transform.localScale = new Vector3(
                (1f - idleScaleMultiplier.x) + sinTime * idleScaleMultiplier.x,
                1.5f + sinTime * idleScaleMultiplier.y,
                1);

            PlayerController playerController = PlayerController.instance;
            if (playerController != null)
            {
                gameObject.transform.LookAt(playerController.transform.position);
            }
            if (IsOwnTurn)
            {
                if (!executingAnimation)
                {
                    actionDelayTimer += Time.deltaTime;
                    {
                        if (actionDelayTimer >= actionDelay)
                        {
                            actionDelayTimer = 0f;
                            // _enemyState = EnemyState.attack;
                            SelectAction();
                            Debug.Log("EnemyController selected action" + _enemyState);
                        }
                    }
                }
                if (executingAnimation)
                {
                    if (!attackHit)
                    {
                        if (attackCurvePhase >= attackHitPhase)
                        {
                            attackHit = true;
                            Debug.Log("EnemyController hit player");
                            // PlayerController.instance.Health -= AttackDamage;
                            PlayerController.instance.TakeDamage(AttackDamage);
                        }
                    }
                    actionDelayTimer += Time.deltaTime;

                    if (_enemyState == EnemyState.attack)
                    {
                        attackCurvePhase += Time.deltaTime / attackAnimationDelay;
                        float attackCurveValue = attackCurve.Evaluate(attackCurvePhase);
                        Vector3 newPosition = Vector3.Lerp(pivotBasePosition, PlayerController.instance.transform.position, attackCurveValue);
                        enemyPivot.transform.position = newPosition;
                    }

                    if (actionDelayTimer >= actionDelay)
                    {
                        executingAnimation = false;
                        attackCurvePhase = 0f;
                        enemyPivot.transform.position = pivotBasePosition;
                        attackHit = false;
                        if (_enemyState == EnemyState.attack)
                        {
                            Debug.Log("EnemyController finished attack action");
                        }
                        else if (_enemyState == EnemyState.finishTurn)
                        {
                            Debug.Log("EnemyController finished finish turn action");
                            OnTurnEnded?.Invoke();
                            IsOwnTurn = false;
                            SelectAction();
                        }
                    }
                }
            }
        }

        public void SelectAction()
        {
            if (!IsOwnTurn)
            {
                _enemyState = EnemyState.idle;
                currentAnimationSpeed = idleAnimationSpeed;
                actionDelay = idleAnimationDelay;
                // executingAnimation = true;
                _enemyMaterialInstance.SetTexture("_BaseMap", enemyIdle);
                enemyMainGraphicsObject.transform.localPosition = new Vector3(enemyMainGraphicsObject.transform.localPosition.x, idleYOffset, enemyMainGraphicsObject.transform.localPosition.z);
                return;
            }

            // go to idle after any action
            if (_enemyState != EnemyState.idle)
            {
                _enemyState = EnemyState.idle;
                currentAnimationSpeed = idleAnimationSpeed;
                actionDelay = idleAnimationDelay;
                // executingAnimation = true;
                _enemyMaterialInstance.SetTexture("_BaseMap", enemyIdle);
                enemyMainGraphicsObject.transform.localPosition = new Vector3(enemyMainGraphicsObject.transform.localPosition.x, idleYOffset, enemyMainGraphicsObject.transform.localPosition.z);
            }
            else if (CurrentActionPoints >= AttackActionPoints)
            {
                _enemyState = EnemyState.attack;
                currentAnimationSpeed = attackAnimationSpeed;
                actionDelay = attackAnimationDelay;
                CurrentActionPoints -= AttackActionPoints;
                executingAnimation = true;
                _enemyMaterialInstance.SetTexture("_BaseMap", enemyAttack);
                enemyMainGraphicsObject.transform.localPosition = new Vector3(enemyMainGraphicsObject.transform.localPosition.x, attackYOffset, enemyMainGraphicsObject.transform.localPosition.z);
                Debug.Log("EnemyController selected attack action");
            }
            else
            {
                _enemyState = EnemyState.finishTurn;
                currentAnimationSpeed = finishTurnAnimationSpeed;
                actionDelay = finishTurnAnimationDelay;
                CurrentActionPoints = 0;
                executingAnimation = true;
                _enemyMaterialInstance.SetTexture("_BaseMap", enemyDie);
                enemyMainGraphicsObject.transform.localPosition = new Vector3(enemyMainGraphicsObject.transform.localPosition.x, dieYOffset, enemyMainGraphicsObject.transform.localPosition.z);
                Debug.Log("EnemyController selected finish turn action");
                Debug.LogError("Not enough action points to attack");
            }
        }

    }
}
