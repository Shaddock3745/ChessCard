using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseStatus<TEnum> where TEnum : Enum
{
    private TEnum statusType;
    private int startTurn, endTurn;
    private bool isPermanetStatus;


    public BaseStatus(TEnum status, bool isPermanetStatus, int startTurn, int endTurn)
    {
        this.statusType = status;
        this.isPermanetStatus = isPermanetStatus;
        this.startTurn = startTurn;
        this.endTurn = endTurn;
    }

    public TEnum getStatusType() => statusType;

    public int getStartTurn() => startTurn;

    public int getEndTurn() => endTurn;

    public bool isPermanet() => isPermanetStatus;

    public bool isActive(int currentTurn) => isPermanetStatus || (currentTurn >= startTurn && currentTurn < endTurn);


    public abstract BaseStatus<TEnum> Clone();
}

public abstract class StatusContainer<TStatus, TEnum> : IDisposable
    where TStatus : BaseStatus<TEnum>
    where TEnum : Enum
{
    protected List<TStatus> activeStatuses = new List<TStatus>();

    public StatusContainer()
    {
        GameManager.OnTurnStart += triggerOnTurnStart;
    }

    public void addStatus(TStatus status)
    {
        activeStatuses.Add(status);
    }

    private void triggerOnTurnStart(int currentTurnCount, bool isWhiteTurn)
    {
        cleanUpExpiredStatuses(currentTurnCount);
    }

    public void cleanUpExpiredStatuses(int currentTotalTurn)
    {
        activeStatuses.RemoveAll(status => currentTotalTurn >= status.getEndTurn());
    }

    public bool hasActiveStatus(TEnum type, int currentTotalTurn)
    {
        return activeStatuses.Exists(s => s.getStatusType().Equals(type) && s.isActive(currentTotalTurn));
    }

    public List<TStatus> GetActiveStatuses(TEnum type, int currentTotalTurn)
    {
        return activeStatuses.FindAll(s => s.getStatusType().Equals(type) && s.isActive(currentTotalTurn));
    }

    public void DeepCopyStatuses(StatusContainer<TStatus, TEnum> source)
    {
        this.activeStatuses = new List<TStatus>();
        foreach (var status in source.activeStatuses)
        {
            this.activeStatuses.Add((TStatus)status.Clone());
        }
    }

    public void Dispose()
    {
        GameManager.OnTurnStart -= triggerOnTurnStart;
        activeStatuses.Clear();
    }
}