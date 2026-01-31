using UnityEngine;
using System;

namespace FGJ26
{

    public interface ITurnControllable
    {
        public int MaxActionPoints { get; }
        public int MovementCost { get; }

        public int CurrentActionPoints { get; set; }

        public bool IsOwnTurn { get; set; }
        void StartTurn();
        // void OnTurnEnd();

        public event Action OnTurnEnded;
        public event Action OnActionPointsChanged;
    }
}