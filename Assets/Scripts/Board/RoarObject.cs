using System.Threading.Tasks;
using UnityEngine;

public class RoarObject : MonoBehaviour
{
    [SerializeField] private SpriteRenderer first, second, third;
    private static readonly int WAIT_DURATION_MILLIS = 150;

    public async Task show()
    {
        hide(first);
        hide(second);
        hide(third);

        show(first);
        await Awaitable.WaitForSecondsAsync(WAIT_DURATION_MILLIS / 1000f);

        if (this == null)
            return;

        show(second);
        await Awaitable.WaitForSecondsAsync(WAIT_DURATION_MILLIS / 1000f);

        if (this == null)
            return;

        show(third);
        await Awaitable.WaitForSecondsAsync(WAIT_DURATION_MILLIS / 1000f);

        if (this == null)
            return;

        Destroy(gameObject);
    }

    private void show(SpriteRenderer renderer)
    {
        Color c = renderer.color;
        c.a = 1f;
        renderer.color = c;
    }

    private void hide(SpriteRenderer renderer)
    {
        Color c = renderer.color;
        c.a = 0f;
        renderer.color = c;
    }
}
