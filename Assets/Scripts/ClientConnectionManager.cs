using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientConnectionManager : MonoBehaviour {
    private void Start() {
        ConnectionMenuUI.Instance.AddListener_ConnectButton(OnConnect);
    }

    private void OnDisable() {
        ConnectionMenuUI.Instance?.RemoveListener_ConnectButton(OnConnect);
    }

    private void OnConnect(ConnectionData data) {
        DestroyLocalSimulationWorld();
        SceneManager.LoadSceneAsync(SceneNameHelper.GameScene);

        switch (data.mode) {
            case ConnectionData.Mode.Host:
                StartHost(data);
                break;
            case ConnectionData.Mode.Client:
                StartClient(data);
                break;
            case ConnectionData.Mode.Server:
                StartServer(data);
                break;
            default:
                throw new System.Exception("Unknown connection mode");
        }

        World.DefaultGameObjectInjectionWorld.EntityManager.CreateSingleton(new ClientInitGameData() {
            teamType = data.teamType,
        });
    }

    private void DestroyLocalSimulationWorld() {
        foreach (World world in World.All) {
            if (world.Flags == WorldFlags.Game) {
                world.Dispose();
                break;
            }
        }
    }
    
    private void StartHost(in ConnectionData data) {
        StartServer(data);
        StartClient(data.WithIpAddress("127.0.0.1"));
        World.DefaultGameObjectInjectionWorld = ClientServerBootstrap.ClientWorld;
    }

    private void StartClient(in ConnectionData data) {
        var clientWorld = ClientServerBootstrap.CreateClientWorld("Client World");

        var connectionEndpoint = NetworkEndpoint.Parse(data.ipAddress, data.port);

        var networkStreamDriver = clientWorld.EntityManager.CreateEntityQuery(typeof(NetworkStreamDriver)).GetSingleton<NetworkStreamDriver>();

        networkStreamDriver.Connect(clientWorld.EntityManager, connectionEndpoint);
    }

    private void StartServer(in ConnectionData data) {
        var serverWorld = ClientServerBootstrap.CreateServerWorld("Server World");

        var serverEndpoint = NetworkEndpoint.AnyIpv4.WithPort(data.port);

        var networkStreamDriver = serverWorld.EntityManager.CreateEntityQuery(typeof(NetworkStreamDriver)).GetSingleton<NetworkStreamDriver>();
        
        networkStreamDriver.Listen(serverEndpoint);
    }
}
