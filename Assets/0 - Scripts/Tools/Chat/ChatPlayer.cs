using Mirror;

namespace Mirror.Examples.Chat
{
    public class ChatPlayer : NetworkBehaviour
    {
        [SyncVar]
        public string playerName;

        public override void OnStartServer()
        {
            playerName = (string)connectionToClient.authenticationData;
        }

        public override void OnStartLocalPlayer()
        {
            ChatUI_PrivateCommand.localPlayerName = playerName;
        }
    }
}
