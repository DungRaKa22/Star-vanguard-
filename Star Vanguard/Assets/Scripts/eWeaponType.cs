/// <summary>
/// 4 core weapon archetypes for Star Vanguard:
/// 1. Red Laser (Default): Balanced fiery-red dual/quad laser bolts
/// 2. Green Neutron: Piercing emerald lance passing through small drones
/// 3. Yellow Vulcan: Rapid-fire kinetic amber stream with LCG PRNG spray
/// 4. Purple Plasma: Wide fan-spread violet energy wave
/// </summary>
public enum eWeaponType
{
    RedLaser = 0,
    IonBlaster = RedLaser,          // Backwards-compatible alias

    GreenNeutron = 1,
    NeutronPiercer = GreenNeutron,  // Backwards-compatible alias

    YellowVulcan = 2,
    VulcanChaingun = YellowVulcan,  // Backwards-compatible alias

    PurplePlasma = 3,
    PlasmaSwarm = PurplePlasma,     // Backwards-compatible alias

    LaserCannon = 4
}
