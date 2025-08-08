using UnityEngine;
using Unity.Entities;

public partial class InputSystem : SystemBase
{
    private ControlECS controls;

    protected override void OnCreate()
    {
        if (!SystemAPI.TryGetSingleton(out InputComponent input))
        {
            EntityManager.CreateEntity(typeof(InputComponent));
        }

        controls = new ControlECS();
        controls.Enable();
    }

    protected override void OnUpdate()
    {
        Vector2 moveVector = controls.Player.Move.ReadValue<Vector2>();
        Vector2 movePosition = controls.Player.MousePos.ReadValue<Vector2>();
        bool shoot = controls.Player.Shoot.IsPressed();

        SystemAPI.SetSingleton(new InputComponent
        {
            Movement = moveVector,
            MousePosition = movePosition,
            Shoot = shoot
        });
    }
}
