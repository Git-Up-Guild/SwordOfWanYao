namespace XClient.Login.State
{
    public class StateGatewayContext
    {
        public string serverAddr;
        public int serverPort;

        public void Reset()
        {
            serverAddr = null;
            serverPort = 0;
        }

        public void CopyTo(StateGatewayContext target)
        {
            target.serverAddr = serverAddr;
            target.serverPort = serverPort;
        }

    }

    public class StateGatewayShareContext : StateGatewayContext
    {
        private StateGatewayShareContext() { }
        public static StateGatewayContext instance = new StateGatewayShareContext();
    }
}
