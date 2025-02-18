public enum TeamType {
    AutoAssign,
    Spectator,
    Blue,
    Red,
}

public static class TeamTypeExtensions {
    public static TeamType GetOpponentTeam(this TeamType teamType) => teamType == TeamType.Blue
        ? TeamType.Red
        : TeamType.Blue;
}
