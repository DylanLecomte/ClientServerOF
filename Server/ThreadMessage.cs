
namespace Server
{

    class ThreadMessage
    {
        public enum Action
        {
            Connection,
            Disconnection,
            Update
        }

        public ThreadMessage(Action ActionMsg, string Username, string Balance)
        {
            this.ActionMsg = ActionMsg;
            this.Username = Username;
            this.Balance = Balance;
        }

        public Action ActionMsg { get; set; }
        public string Username { get; set; }
        public string Balance { get; set; }
    }
}
