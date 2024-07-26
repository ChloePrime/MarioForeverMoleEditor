using Godot;

namespace ChloePrime.MarioForever.Level;

public partial class GamepadManager : Node
{
    public static bool TryGetInstance(out GamepadManager instance)
    {
        return IsInstanceValid(instance = _instance);
    }
    
    public int? CurrentGamepad { get; private set; }

    public override void _Ready()
    {
        base._Ready();
        _instance = this;
    }

    public override void _EnterTree()
    {
        base._EnterTree();
        _instance = this;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey)
        {
            CurrentGamepad = -1;
            return;
        }
        if (@event is InputEventJoypadButton or InputEventJoypadMotion)
        {
            CurrentGamepad = @event.Device;
        }
    }

    private static GamepadManager _instance;
}