using Zorbo.Interface;

namespace FloodGuard
{
    public class FloodGuard : IPlugin
    {
        IServer server;

        string mydir = string.Empty;

        public string Directory {
            get { return mydir; }
            set { mydir = value; }
        }

        public void OnPluginLoaded(IServer server) {
            this.server = server;
        }

        public void OnPluginKilled() {
        }

        public void OnCaptcha(IClient client, CaptchaEvent @event) {

        }

        public void OnJoinRejected(IClient client, RejectReason reason) {

        }

        public ServerFeatures OnSendFeatures(IClient client, ServerFeatures features) {
            return features;
        }

        public void OnSendJoin(IClient client) {

        }

        public bool OnJoinCheck(IClient client) {
            return true;
        }

        public void OnJoin(IClient client) {

        }

        public void OnPart(IClient client, object state) {

        }

        public bool OnVroomJoinCheck(IClient client, ushort vroom) {
            return true;
        }

        public void OnVroomJoin(IClient client) {

        }

        public void OnVroomPart(IClient client) {

        }

        public void OnHelp(IClient client) {

        }

        public void OnLogin(IClient client, IPassword password) {
        }

        public bool OnRegister(IClient client, IPassword password) {
            return true;
        }

        public bool OnFileReceived(IClient client, ISharedFile file) {
            return true;
        }

        public bool OnBeforePacket(IClient client, IPacket packet) {
            return true;
        }

        public void OnAfterPacket(IClient client, IPacket packet) {

        }

        public bool OnFlood(IClient client, IFloodRule rule, IPacket packet) {
            return true;
        }

        public void OnError(IErrorInfo error) {

        }
    }
}
