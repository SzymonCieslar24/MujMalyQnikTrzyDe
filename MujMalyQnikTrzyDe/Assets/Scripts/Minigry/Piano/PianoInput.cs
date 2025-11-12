using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/**
 * @class PianoInput
 * @brief Wygodne API wejœcia: start, dowolny klawisz, oraz kolumna (Q,W,E,R,T -> 0..4).
 */
public static class PianoInput
{
    public static bool AnyKeyPressed()
    {
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb == null) return false;
        return kb.anyKey.wasPressedThisFrame;
#else
        return Input.anyKeyDown;
#endif
    }

    /// <summary> Zwraca indeks kolumny 0..4 dla Q,W,E,R,T. Gdy nic nie wciœniêto – zwraca -1. </summary>
    public static int GetPressedColumn()
    {
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb == null) return -1;
        if (kb.qKey.wasPressedThisFrame) return 0;
        if (kb.wKey.wasPressedThisFrame) return 1;
        if (kb.eKey.wasPressedThisFrame) return 2;
        if (kb.rKey.wasPressedThisFrame) return 3;
        if (kb.tKey.wasPressedThisFrame) return 4;
        return -1;
#else
        if (Input.GetKeyDown(KeyCode.Q)) return 0;
        if (Input.GetKeyDown(KeyCode.W)) return 1;
        if (Input.GetKeyDown(KeyCode.E)) return 2;
        if (Input.GetKeyDown(KeyCode.R)) return 3;
        if (Input.GetKeyDown(KeyCode.T)) return 4;
        return -1;
#endif
    }
}
