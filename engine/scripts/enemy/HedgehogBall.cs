using ChloePrime.MarioForever.RPG;
using ChloePrime.MarioForever.Util;
using Godot;

namespace ChloePrime.MarioForever.Enemy;

[GlobalClass]
public partial class HedgehogBall : WalkableNpc
{
    [Export] public PackedScene Ontology { get; set; } = GD.Load<PackedScene>("res://engine/objects/enemies/O_mole.tscn");
    
    public override void _PhysicsProcess(double deltaD)
    {
        base._PhysicsProcess(deltaD);
        if (IsOnFloor())
        {
            TransformToOntology();
        }
    }

    public void TransformToOntology()
    {
        if (Ontology is { } prefab)
        {
            var ontology = prefab.Instantiate();
            if (ontology is IMarioForeverNpc npc)
            {
                npc.NpcData.CopyValueFrom(NpcData);
            }
            this.GetPreferredRoot().AddChild(ontology);
            if (ontology is Node2D o2d)
            {
                o2d.GlobalPosition = GlobalPosition;
            }
        }
        QueueFree();
    }
}