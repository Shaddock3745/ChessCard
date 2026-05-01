using System.Collections.Generic;
using UnityEngine;

public abstract class LogData
{
    protected static readonly string WHITE_TEXT = "白方", BLACK_TEXT = "黑方";
    protected string message;

    protected List<string> containedLinkIds = new List<string>();

    public string getMessage() => message;

    public List<string> getLinks() => containedLinkIds;

    protected string registerdata(object data)
    {
        return LogManager.rigisterLogData(data);
    }
}
