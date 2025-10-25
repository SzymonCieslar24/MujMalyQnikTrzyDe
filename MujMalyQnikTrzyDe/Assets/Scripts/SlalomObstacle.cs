using System.Linq;
using UnityEngine;

/// <summary>
/// Logika slalomu: weryfikuje kolejno�� przej�cia przez bramki (pacho�ki)
/// oraz naprzemienno�� stron (Left/Right). Dzieci posiadaj� SlalomGate.
/// </summary>
public class SlalomObstacle : ObstacleBase
{
    [SerializeField] private string playerTag = "Player";

    // Ustalana automatycznie z dzieci posiadaj�cych SlalomGate
    [Tooltip("Bramki slalomu pobierane automatycznie z dzieci.")]
    public SlalomGate[] gates;

    private int nextIndex = 0;
    private bool failed = false;
    private bool finished = false;

    private void Awake()
    {
        // Pobierz wszystkie bramki i u�� wg Order
        gates = GetComponentsInChildren<SlalomGate>(includeInactive: false)
                    .OrderBy(g => g.Order)
                    .ToArray();

        // Zabezpieczenie: przypisz parenta i zresetuj stan bramek
        for (int i = 0; i < gates.Length; i++)
        {
            gates[i].AttachParent(this);
            gates[i].ResetGate();
        }

        // Walidacja naprzemienno�ci deklaracji stron
        for (int i = 1; i < gates.Length; i++)
        {
            if (gates[i - 1].Side == gates[i].Side)
            {
                Debug.LogWarning(
                    $"[Slalom] Brak naprzemienno�ci mi�dzy bramk� {gates[i - 1].Order} ({gates[i - 1].Side}) a {gates[i].Order} ({gates[i].Side}) w obiekcie {name}."
                );
            }
        }
    }

    /// <summary>
    /// Wywo�ywane przez bramk�, gdy gracz wejdzie w jej trigger.
    /// </summary>
    public void ReportGateEntered(SlalomGate gate, Collider who)
    {
        if (finished || failed) return;
        if (!who.CompareTag(playerTag)) return;

        // Oczekiwana bramka:
        if (nextIndex >= gates.Length)
            return; // nadmiarowe wej�cie po zako�czeniu

        SlalomGate expected = gates[nextIndex];

        if (gate != expected)
        {
            // Gracz wszed� w z�� bramk� (z�a kolejno��)
            failed = true;
            Debug.Log($"Slalom NIEZALICZONY: oczekiwana bramka #{expected.Order} ({expected.Side}), " +
                      $"a trafiono #{gate.Order} ({gate.Side}) w obiekcie {name}.");
            return;
        }

        // Dodatkowa kontrola naprzemienno�ci (gdyby kto� b��dnie skonfigurowa� kolejno��/Side)
        if (nextIndex > 0)
        {
            var prev = gates[nextIndex - 1];
            if (prev.Side == gate.Side)
            {
                failed = true;
                Debug.Log($"Slalom NIEZALICZONY: dwie kolejne bramki maj� t� sam� stron� ({gate.Side}). Obiekt: {name}.");
                return;
            }
        }

        // OK � poprawna bramka w kolejno�ci
        nextIndex++;

        if (nextIndex >= gates.Length)
        {
            finished = true;
            Debug.Log($"Slalom ZALICZONY! Obiekt: {name}.");
        }
        else
        {
            // Informacja pomocnicza
            var nxt = gates[nextIndex];
            Debug.Log($"Poprawnie min��e� bramk� #{gate.Order} ({gate.Side}). " +
                      $"Nast�pna: #{nxt.Order} ({nxt.Side}).");
        }
    }

    /// <summary>
    /// Gdy ca�o�� minie pole widzenia i nie ma wyniku, uznaj za niezaliczone.
    /// (Opcjonalnie � mo�esz usun�� je�li nie chcesz takiej logiki.)
    /// </summary>
    private void OnBecameInvisible()
    {
        if (!finished && !failed && nextIndex > 0)
        {
            failed = true;
            Debug.Log($"Slalom NIEZALICZONY: sekwencja przerwana przed ko�cem. Obiekt: {name}.");
        }
    }
}
