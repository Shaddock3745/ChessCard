using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseStatus<TEnum> where TEnum :Enum
{
    public TEnum status;
    public int startTurn, endTurn;

    public BaseStatus(TEnum status, int startTurn, int endTurn)
    {
        this.status = status;
        this.startTurn = startTurn;
        this.endTurn = endTurn;
    }

    public TEnum getStatus() => status;

    public int getStartTurn() => startTurn;

    public int getEndTurn() => endTurn;

    public bool isActive(int currentTurn) => currentTurn >= startTurn && currentTurn <= endTurn;
}

public abstract class StatusContainer<TStatus, TEnum> : MonoBehaviour
    where TStatus : BaseStatus<TEnum>
    where TEnum : Enum
{
    protected List<TStatus> activeStatuses = new List<TStatus>();

    public void addStatus(TStatus status)
    {
        activeStatuses.Add(status);
    }

    private void OnEnable()
    {
        GameManager.OnTurnStart += cleanUpExpiredStatuses;
    }

    private void OnDisable()
    {
        GameManager.OnTurnStart -= cleanUpExpiredStatuses;
    }

    public void cleanUpExpiredStatuses(int currentTotalTurn)
    {
        activeStatuses.RemoveAll(status => !status.isActive(currentTotalTurn));
    }

    public bool hasActiveStatus(TEnum type, int currentTotalTurn)
    {
        return activeStatuses.Exists(s => s.GetType().Equals(type) && s.isActive(currentTotalTurn));
    }
}