using System.Collections.Generic;
using System.Linq;

public class CompositePayload : EffectTargetPayload
{
    private List<EffectTargetPayload> payloads;

    public List<EffectTargetPayload> getPayLoads() => payloads;

    public EffectTargetPayload getPayLoad(int index) => payloads[index];

    public CompositePayload() : base()
    {
        payloads = new List<EffectTargetPayload>();
    }

    public CompositePayload(List<EffectTargetPayload> payloads) : base()
    {
        this.payloads = payloads;
    }

    public CompositePayload(params EffectTargetPayload[] payloads) : base()
    {
        this.payloads = payloads.ToList();
    }

    public CompositePayload(CompositePayload payload) : base()
    {
        payloads = new List<EffectTargetPayload>();
        payloads.AddRange(payload.getPayLoads());
    }

    public void addPayload(EffectTargetPayload payload)
    {
        payloads.Add(payload);
    }

    public override bool isEmpty()
    {
        if (payloads.Count == 0)
            return true;

        return payloads.Any(p => p == null);
    }
}
