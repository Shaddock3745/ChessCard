using System.Threading.Tasks;

public interface AiOption
{
    public float getScore();
    public Task execute(PlayerData ownerData);

}
