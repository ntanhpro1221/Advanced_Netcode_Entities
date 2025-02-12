using Unity.NetCode;

public class CustomBoostrap : ClientServerBootstrap {
    public override bool Initialize(string defaultWorldName) {
        CreateLocalWorld(defaultWorldName);
        return true;
    }
}
