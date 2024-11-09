using System;
using Unity.Netcode;
using Unity.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using HarmonyLib;

namespace MaskedRagdoll
{
    [Serializable]
    public class SyncedInstance<T>
    {
        internal static CustomMessagingManager MessageManager => NetworkManager.Singleton.CustomMessagingManager;
        internal static bool IsClient => NetworkManager.Singleton.IsClient;
        internal static bool IsHost => NetworkManager.Singleton.IsHost;

        [NonSerialized]
        protected static int IntSize = 4;

        public static T Default { get; private set; }
        public static T Instance { get; private set; }

        public static bool Synced { get; internal set; }

        //Initialization
        protected void InitInstance(T instance)
        {
            Default = instance;
            Instance = instance;
            IntSize = sizeof(int);
        }

        internal static void SyncInstance(byte[] data)
        {
            Instance = DeserializeFromBytes(data);
            Synced = true;
        }

        internal static void RevertSync()
        {
            Instance = Default;
            Synced = false;
        }

        public static byte[] SerializeToBytes(T val)
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();

            try
            {
                bf.Serialize(stream, val);
                return stream.ToArray();
            }
            catch (Exception e)
            {
                RagdollModBase.mls.LogError($"Error serializing instance: {e}");
                return null;
            }
        }

        public static T DeserializeFromBytes(byte[] data)
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream stream = new MemoryStream(data);

            try {
                return (T)bf.Deserialize(stream);
            } catch (Exception e) {
                RagdollModBase.mls.LogError($"Error deserializing instance: {e}");
                return default;
            }
        }

        //Syncing
        public static void RequestSync()
        {
            if (!IsClient) return;

            FastBufferWriter stream = new FastBufferWriter(IntSize, Allocator.Temp);
            MessageManager.SendNamedMessage("MaskedRagdoll_OnRequestConfigSync", 0uL, stream);
        }

        public static void OnRequestSync(ulong clientId, FastBufferReader _)
        {
            if (!IsHost) return;

            RagdollModBase.mls.LogInfo($"Config sync request received from client: {clientId}");

            byte[] array = SerializeToBytes(Instance);
            int value = array.Length;

            FastBufferWriter stream = new FastBufferWriter(value + IntSize, Allocator.Temp);

            try {
                stream.WriteValueSafe(in value, default);
                stream.WriteBytesSafe(array);
                MessageManager.SendNamedMessage("MaskedRagdoll_OnReceiveConfigSync", clientId, stream);
            } catch (Exception e) {
                RagdollModBase.mls.LogInfo($"Error occurred syncing config with client: {clientId}\n{e}");
            }
        }

        public static void OnReceiveSync(ulong _, FastBufferReader reader)
        {
            if (!reader.TryBeginRead(IntSize)) {
                RagdollModBase.mls.LogError("Config sync error: Could not begin reading buffer.");
                return;
            }

            reader.ReadValueSafe(out int val, default);
            if (!reader.TryBeginRead(val)) {
                RagdollModBase.mls.LogError("Config sync error: Host could not sync.");
                return;
            }

            byte[] data = new byte[val];
            reader.ReadBytesSafe(ref data, val);

            SyncInstance(data);

            RagdollModBase.mls.LogInfo("Successfully synced config with host.");
        }

        //Patches
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameNetworkManager), "SteamMatchmaking_OnLobbyMemberJoined")]
        public static void InitializeLocalPlayer()
        {
            if (IsHost) {
                MessageManager.RegisterNamedMessageHandler("MaskedRagdoll_OnRequestConfigSync", OnRequestSync);
                Synced = true;

                return;
            }

            Synced = false;
            MessageManager.RegisterNamedMessageHandler("MaskedRagdoll_OnReceiveConfigSync", OnReceiveSync);
            RequestSync();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameNetworkManager), "StartDisconnect")]
        public static void PlayerLeave()
        {
            Config.RevertSync();
        }
    }
}