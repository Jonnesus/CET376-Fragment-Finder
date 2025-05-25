using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class InputManager : MonoBehaviour
{
    [HideInInspector] public Vector2 move;
    [HideInInspector] public bool fire;
    [HideInInspector] public bool jump;
    [HideInInspector] public bool sprint;
    [HideInInspector] public bool crouch;
    [HideInInspector] public bool cancel;

    public void OnMove(InputValue value) => move = value.Get<Vector2>();
    public void OnFire(InputValue value) => fire = Convert.ToBoolean(value.Get<float>());
    public void OnJump(InputValue value) => jump = Convert.ToBoolean(value.Get<float>());
    public void OnSprint(InputValue value) => sprint = Convert.ToBoolean(value.Get<float>());
    public void OnCrouch(InputValue value) => crouch = Convert.ToBoolean(value.Get<float>());
    public void OnPause(InputValue value) => cancel = Convert.ToBoolean(value.Get<float>());
}