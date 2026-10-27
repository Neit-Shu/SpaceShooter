using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace SpaceShooter
{
    // Единая точка ввода. Работает и со старым Input Manager,
    // и с новым Input System — какой бы ни был включён в проекте.
    public static class GameInput
    {
        public static float Horizontal()
        {
            float x = 0f;
#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            if (kb == null) return 0f;
            if (kb.leftArrowKey.isPressed || kb.aKey.isPressed) x -= 1f;
            if (kb.rightArrowKey.isPressed || kb.dKey.isPressed) x += 1f;
#else
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) x -= 1f;
            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) x += 1f;
#endif
            return x;
        }

        public static float Vertical()
        {
            float y = 0f;
#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            if (kb == null) return 0f;
            if (kb.downArrowKey.isPressed || kb.sKey.isPressed) y -= 1f;
            if (kb.upArrowKey.isPressed || kb.wKey.isPressed) y += 1f;
#else
            if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) y -= 1f;
            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) y += 1f;
#endif
            return y;
        }

        // Стрельба: пробел или левая кнопка мыши (можно удерживать)
        public static bool Fire()
        {
#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            var mouse = Mouse.current;
            return (kb != null && kb.spaceKey.isPressed)
                || (mouse != null && mouse.leftButton.isPressed);
#else
            return Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0);
#endif
        }

        // Перезапуск после проигрыша: клавиша R
        public static bool RestartPressed()
        {
#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            return kb != null && kb.rKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.R);
#endif
        }
    }
}
