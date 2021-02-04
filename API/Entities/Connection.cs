namespace API.Entities
{
    public class Connection
    {
        public Connection()
        {
        }

        // if we give it the name of the class then Id, entity framework knows it's the id
        public Connection(string connectionId, string username)
        {
            ConnectionId = connectionId;
            Username = username;

        }
        public string ConnectionId { get; set; }
        public string Username { get; set; }
    }
}