namespace Challenge
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 5 && args.Length != 6)
            {
                Console.WriteLine();
                Console.WriteLine(" Command line parameters:");
                Console.WriteLine(" dotnet run <path> <username> <password> <database> <server> <delay>");
                Console.WriteLine();
                Console.WriteLine(" Example: dotnet.run path username database server delay");
                Console.WriteLine();
                Console.WriteLine(" path       - Path to create files (Example: c:\\)");
                Console.WriteLine(" username   - Geotab user name");
                Console.WriteLine(" password   - Geotab password");
                Console.WriteLine(" database   - Database name (Example: G560)");
                Console.WriteLine(" server     - Sever host name (Example: my.geotab.com)");
                Console.WriteLine(" delay      - (optional, 60000 by default) Time to search for changes in miliseconds.");

                return;
            }

            string filePath = args[0];
            string user = args[1];
            string password = args[2];
            string database = args[3];
            string server = args[4];
            int milisecondsDelay = args.Length == 6 ? Convert.ToInt32(args[5]) : 60000;

            Backup backup = new DatabaseBackup(filePath, user, password, database, server, milisecondsDelay);

            Task[] tasks = new Task[1];
            tasks[0] = Task.Run(async () => await backup.CreateBackup());
            Task.WaitAll(tasks);
        }
    }
}