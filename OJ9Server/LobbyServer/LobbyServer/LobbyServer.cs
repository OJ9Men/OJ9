public class LobbyServer
{
    public void Start()
    {
        try
        {
            StartDB();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private void StartDB()
    {
        
    }
}