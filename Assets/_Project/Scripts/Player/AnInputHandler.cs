using UnityEngine;
using UnityEngine.InputSystem;

public readonly struct AnInputSnapshot
{
    public AnInputSnapshot(
        Vector2 move,
        bool jumpPressed,
        bool sneakPressed,
        bool crawlPressed,
        bool sprintHeld,
        bool interactPressed,
        bool slot1Pressed,
        bool slot2Pressed,
        bool slot3Pressed)
    {
        Move = move;
        JumpPressed = jumpPressed;
        SneakPressed = sneakPressed;
        CrawlPressed = crawlPressed;
        SprintHeld = sprintHeld;
        InteractPressed = interactPressed;
        Slot1Pressed = slot1Pressed;
        Slot2Pressed = slot2Pressed;
        Slot3Pressed = slot3Pressed;
    }

    public Vector2 Move { get; }
    public bool JumpPressed { get; }
    public bool SneakPressed { get; }
    public bool CrawlPressed { get; }
    public bool SprintHeld { get; }
    public bool InteractPressed { get; }
    public bool Slot1Pressed { get; }
    public bool Slot2Pressed { get; }
    public bool Slot3Pressed { get; }
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
        bool slot1Pressed = false;
        bool slot2Pressed = false;
        bool slot3Pressed = false;

        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            //jumpPressed |= keyboard.spaceKey.wasPressedThisFrame;
            // xử lý space không nhảy khi đang sử dụng morse code
            if (!DocumentViewerUI.ShouldBlockGameplaySpace)
            {
                jumpPressed |= keyboard.spaceKey.wasPressedThisFrame;
            }

            sneakPressed |= keyboard.cKey.wasPressedThisFrame;
            crawlPressed |= keyboard.zKey.wasPressedThisFrame;
            sprintHeld |= keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;
            interactPressed |= keyboard.eKey.wasPressedThisFrame;

            slot1Pressed |= keyboard.digit1Key.wasPressedThisFrame;
            slot1Pressed |= keyboard.numpad1Key.wasPressedThisFrame;
            slot2Pressed |= keyboard.digit2Key.wasPressedThisFrame;
            slot2Pressed |= keyboard.numpad2Key.wasPressedThisFrame;
            slot3Pressed |= keyboard.digit3Key.wasPressedThisFrame;
            slot3Pressed |= keyboard.numpad3Key.wasPressedThisFrame;
        }


        return new AnInputSnapshot(
            Vector2.ClampMagnitude(move, 1f),
            jumpPressed,
            sneakPressed,
            crawlPressed,
            sprintHeld,
            interactPressed,
            slot1Pressed,
            slot2Pressed,
            slot3Pressed);
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
