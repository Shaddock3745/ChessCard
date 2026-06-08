using System.Collections.Generic;

public abstract class LogData
{
    protected string message;

    protected List<string> containedLinkIds = new List<string>();

    public string getMessage() => message;

    public List<string> getLinks() => containedLinkIds;

    // 以目前語言取得白方／黑方文字
    protected static string playerText(bool isWhite) => LocalizationManager.get(isWhite ? "white" : "black");

    protected string registerdata(object data)
    {
        return LogManager.rigisterLogData(data);
    }
}
