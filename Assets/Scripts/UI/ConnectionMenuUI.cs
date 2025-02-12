using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using SingletonUtil;

public class ConnectionMenuUI : SceneSingleton<ConnectionMenuUI> {
    [SerializeField] private TMP_Dropdown modeDropdown;
    [SerializeField] private TMP_InputField ipAddressInput;
    [SerializeField] private TMP_InputField portInput;
    [SerializeField] private TMP_Dropdown teamTypeDropdown;
    [SerializeField] private Button connectBtn;

    public void SubscribeToConnectButton(UnityAction<ConnectionData> callback) {
        connectBtn.onClick.AddListener(() => callback.Invoke(GetConnectionData()));
    }

    private TeamType DropdownIdToTeamType(int id) => id switch {
        0 => TeamType.AutoAssign,
        1 => TeamType.Blue,
        2 => TeamType.Red,
        3 => TeamType.Spectator,
        _ => throw new System.Exception("Unknow team type's dropdown id value")
    };

    private ConnectionData.Mode DropdownIdToMode(int id) => id switch {
        0 => ConnectionData.Mode.Host,
        1 => ConnectionData.Mode.Client,
        2 => ConnectionData.Mode.Server,
        _ => throw new System.Exception("Unknow role's dropdown id value")
    };

    private ConnectionData GetConnectionData() => new() {
        mode = DropdownIdToMode(modeDropdown.value),
        ipAddress = ipAddressInput.text,
        port = ushort.Parse(
            portInput.text == "" 
            ? "0"
            : portInput.text),
        teamType = DropdownIdToTeamType(teamTypeDropdown.value),
    };
}
