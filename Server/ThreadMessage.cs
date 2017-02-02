
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

        public Action ActionMsg { get; set; }
        public string Username { get; set; }
        public string Balance { get; set; }
    }
}
