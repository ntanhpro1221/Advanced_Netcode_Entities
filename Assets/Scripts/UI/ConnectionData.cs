using System;

[Serializable]
public struct ConnectionData {
    public Mode mode;
    public string ipAddress;
    public ushort port;
    public TeamType teamType;

    public readonly ConnectionData WithIpAddress(string ipAddress) {
        ConnectionData data = this;
        data.ipAddress = ipAddress;
        return data;
    }

    public enum Mode {
        Host,
        Client,
        Server,
    }
}
