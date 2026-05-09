using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseStatus<TEnum> where TEnum :Enum
{
    public TEnum statusType;
    public int startTurn, endTurn;

    public BaseStatus(TEnum status, int startTurn, int endTurn)
    {
        this.statusType = status;
        this.startTurn = startTurn;
        this.endTurn = endTurn;
    }

    public TEnum getStatusType() => statusType;

    public int getStartTurn() => startTurn;

    public int getEndTurn() => endTurn;

    public bool isActive(int currentTurn) => currentTurn >= startTurn && currentTurn < endTurn;
}

public abstract class StatusContainer<TStatus, TEnum> : IDisposable
    where TStatus : BaseStatus<TEnum>
    where TEnum : Enum
{
    protected List<TStatus> activeStatuses = new List<TStatus>();

    public StatusContainer()
    {
        GameManager.OnTurnStart += cleanUpExpiredStatuses;
    }

    public void addStatus(TStatus status)
    {
        activeStatuses.Add(status);
    }

    public void cleanUpExpiredStatuses(int currentTotalTurn)
    {
        activeStatuses.RemoveAll(status => !status.isActive(currentTotalTurn));
    }

    public bool hasActiveStatus(TEnum type, int currentTotalTurn)
    {
        return activeStatuses.Exists(s => s.getStatusType().Equals(type) && s.isActive(currentTotalTurn));
    }

    public List<TStatus> GetActiveStatuses(TEnum type, int currentTotalTurn)
    {
        return activeStatuses.FindAll(s => s.getStatusType().Equals(type) && s.isActive(currentTotalTurn));
    }

    public void Dispose()
    {
        GameManager.OnTurnStart -= cleanUpExpiredStatuses;
        activeStatuses.Clear();
    }
}