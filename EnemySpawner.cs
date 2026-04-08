using System.Collections.Generic;

namespace TmxProcessorLib;

internal class EnemySpawner
{
    public ushort x;
    public ushort y;
    public ushort width;
    public ushort height;

    public ushort attemptInterval = 150;
    public ushort attemptSuccessFraction = 10;

    public List<WorldEnemyBlueprint> enemies;

    public EnemySpawner(
        ushort x,
        ushort y,
        ushort width,
        ushort height,
        List<WorldEnemyBlueprint> enemies,
        ushort? attemptInterval = null,
        ushort? attemptSuccessFraction = null
    )
    {
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = height;
        this.enemies = enemies;
        this.attemptInterval = attemptInterval ?? this.attemptInterval;
        this.attemptSuccessFraction = attemptSuccessFraction ?? this.attemptSuccessFraction;
    }
}

internal class WorldEnemyBlueprint
{
    public ushort type;

    public List<ushort> battleEnemies;

    public WorldEnemyBlueprint(ushort type, List<ushort> battleEnemies)
    {
        this.type = type;
        this.battleEnemies = battleEnemies;
    }
}
