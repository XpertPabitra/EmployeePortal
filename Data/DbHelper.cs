namespace EmployeePortal.Data
{
    public class DbHelper
    {
        private readonly string _connectionString;

        public DbHelper(IConfiguration configuration)
        {
            // Ensures _connectionString is never null upon exiting the constructor
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        public Microsoft.Data.SqlClient.SqlConnection GetConnection()
            => new Microsoft.Data.SqlClient.SqlConnection(_connectionString);
    }
}