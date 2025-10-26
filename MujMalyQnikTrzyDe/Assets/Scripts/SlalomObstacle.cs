using System.Linq;
using UnityEngine;

public class SlalomObstacle : ObstacleBase
{
    [SerializeField] private string playerTag = "Player";

    public SlalomGate[] gates;

    private int nextIndex = 0;
    private bool failed = false;
    private bool finished = false;

    protected override void Awake()
    {
        base.Awake();

        gates = GetComponentsInChildren<SlalomGate>(includeInactive: false)
                    .OrderBy(g => g.Order)
                    .ToArray();

        for (int i = 0; i < gates.Length; i++)
        {
            gates[i].AttachParent(this);
            gates[i].ResetGate();
        }

        for (int i = 1; i < gates.Length; i++)
        {
            if (gates[i - 1].Side == gates[i].Side)
            {
                Debug.LogWarning(
                    $"[Slalom] Brak naprzemiennoœci miêdzy bramk¹ {gates[i - 1].Order} ({gates[i - 1].Side}) a {gates[i].Order} ({gates[i].Side}) w obiekcie {name}."
                );
            }
        }
    }

    private void Start()
    {
        RegisterSelf();
        ResetRun();
    }

    public override void SetActiveState(bool active)
    {
        base.SetActiveState(active);
        if (active)
            ResetRun();
    }

    private void ResetRun()
    {
        nextIndex = 0;
        failed = false;
        finished = false;

        if (gates == null || gates.Length == 0) return;
        foreach (var g in gates)
            g.ResetGate();
    }

    public void ReportGateEntered(SlalomGate gate, Collider who)
    {
        if (!isActive) return;
        if (finished || failed) return;
        if (!who.CompareTag(playerTag)) return;

        if (nextIndex >= gates.Length)
            return;

        var expected = gates[nextIndex];

        if (gate != expected)
        {
            failed = true;
            Debug.Log($"Slalom NIEZALICZONY: oczekiwana bramka #{expected.Order} ({expected.Side}), " +
                      $"a trafiono #{gate.Order} ({gate.Side}) w obiekcie {name}.");
            CompleteFail();
            return;
        }

        if (nextIndex > 0)
        {
            var prev = gates[nextIndex - 1];
            if (prev.Side == gate.Side)
            {
                failed = true;
                Debug.Log($"Slalom NIEZALICZONY: dwie kolejne bramki maj¹ tê sam¹ stronê ({gate.Side}). Obiekt: {name}.");
                CompleteFail();
                return;
            }
        }

        nextIndex++;

        if (nextIndex >= gates.Length)
        {
            finished = true;
            Debug.Log($"Slalom ZALICZONY! Obiekt: {name}.");
            CompleteSuccess();
        }
        else
        {
            var nxt = gates[nextIndex];
            Debug.Log($"Poprawnie min¹³eœ bramkê #{gate.Order} ({gate.Side}). " +
                      $"Nastêpna: #{nxt.Order} ({nxt.Side}).");
        }
    }

    private void OnBecameInvisible()
    {
        if (!isActive) return;
        if (!finished && !failed && nextIndex > 0)
        {
            failed = true;
            Debug.Log($"Slalom NIEZALICZONY: sekwencja przerwana przed koñcem. Obiekt: {name}.");
            CompleteFail();
        }
    }
}

