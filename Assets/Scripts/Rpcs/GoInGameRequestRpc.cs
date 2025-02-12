using Unity.NetCode;

public struct GoInGameRequestRpc : IRpcCommand {
    public TeamType teamType;
}
