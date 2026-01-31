using UnityEngine;
using System;

public interface ITurnControllable
{
    public int MaxActionPoints { get; }
    public int MovementCost { get; }

    public int CurrentActionPoints { get; set; }
    void StartTurn();
    // void OnTurnEnd();

    public event Action OnTurnEnded;
}
