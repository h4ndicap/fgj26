using UnityEngine;
using System.Collections.Generic;


namespace FGJ26
{

    public class TurnControlSystem : MonoBehaviour
    {

        [SerializeField]
        private int roundCount = 0;

        private int _currentTurnControllableIndex = 0;
        [SerializeField]
        private List<ITurnControllable> turnControllables = new List<ITurnControllable>();

        public static TurnControlSystem instance;

        public ITurnControllable currentActiveEntity;

        private bool _skippedFrame = false;
        private bool _initialized = false;

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
        }

        public void AddTurnControllable(ITurnControllable turnControllable)
        {
            turnControllables.Add(turnControllable);
            turnControllable.OnTurnEnded += OnControllableTurnEnded;
        }

        public void RemoveTurnControllable(ITurnControllable turnControllable)
        {
            turnControllables.Remove(turnControllable);

        }

        public void BeginRound()
        {
            Debug.Log("Beginning round" + roundCount + " with " + turnControllables.Count + " controllable entities");
            if (turnControllables.Count < 1)
            {
                Debug.LogError("No controllables");
            }
            else
            {
                _currentTurnControllableIndex = 0;
                turnControllables[_currentTurnControllableIndex].StartTurn();
            }
        }

        public void OnControllableTurnEnded()
        {
            _currentTurnControllableIndex++;
            if (_currentTurnControllableIndex < turnControllables.Count)
            {
                Debug.Log("Starting turn " + _currentTurnControllableIndex + " of " + turnControllables.Count);
                turnControllables[_currentTurnControllableIndex].StartTurn();
            }
            else
            {
                Debug.LogError("End of round " + roundCount);
                roundCount++;
                BeginRound();
            }
        }

        // Update is called once per frame
        void Update()
        {
            // hack, skip the first frame
            if (_skippedFrame && !_initialized)
            {
                BeginRound();
                _initialized = true;
            }
            _skippedFrame = true;
        }
    }
}
