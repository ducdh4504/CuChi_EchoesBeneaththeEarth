using UnityEngine;
using UnityEngine.InputSystem;

public readonly struct AnInputSnapshot
{
    public AnInputSnapshot(Vector2 move, bool jumpPressed, bool sneakPressed, bool crawlPressed, bool sprintHeld, bool interactPressed, bool flashlightPressed)
    {
        Move = move;
        JumpPressed = jumpPressed;
        SneakPressed = sneakPressed;
        CrawlPressed = crawlPressed;
        SprintHeld = sprintHeld;
        InteractPressed = interactPressed;
        FlashlightPressed = flashlightPressed;
    }

    public Vector2 Move { get; }
    public bool JumpPressed { get; }
    public bool SneakPressed { get; }
    public bool CrawlPressed { get; }
    public bool SprintHeld { get; }
    public bool InteractPressed { get; }
    // xử lý đèn pin
    public bool FlashlightPressed { get; }
}

public sealed class AnInputHandler
{
    public AnInputSnapshot Read()
    {
        Vector2 move = ReadKeyboardMove();
        bool jumpPressed = false;
        bool sneakPressed = false;
        bool crawlPressed = false;
        bool sprintHeld = false;
        bool interactPressed = false;
        bool flashlightPressed = false;

        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            jumpPressed |= keyboard.spaceKey.wasPressedThisFrame;
            sneakPressed |= keyboard.cKey.wasPressedThisFrame;
            crawlPressed |= keyboard.zKey.wasPressedThisFrame;
            sprintHeld |= keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;
            interactPressed |= keyboard.eKey.wasPressedThisFrame;

            // xử lý đèn pin
            flashlightPressed |= keyboard.digit2Key.wasPressedThisFrame;
            flashlightPressed |= keyboard.numpad2Key.wasPressedThisFrame;
            flashlightPressed |= keyboard.fKey.wasPressedThisFrame;
        }


        return new AnInputSnapshot(
            Vector2.ClampMagnitude(move, 1f),
            jumpPressed,
            sneakPressed,
            crawlPressed,
            sprintHeld,
            interactPressed,
            flashlightPressed);
    }

    private static Vector2 ReadKeyboardMove()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return Vector2.zero;
        }

        Vector2 move = Vector2.zero;

        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
        {
            move.y += 1f;
        }

        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
        {
            move.y -= 1f;
        }

        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
        {
            move.x += 1f;
        }

        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
        {
            move.x -= 1f;
        }

        return Vector2.ClampMagnitude(move, 1f);
    }
}
