using Microsoft.Data.SqlClient;
using EmployeePortal.Models.Domain;
using EmployeePortal.Repositories.Interfaces;
using EmployeePortal.Data;
using System.Data;

namespace EmployeePortal.Repositories.Implementations
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly DbHelper _db;

        public EmployeeRepository(DbHelper db)
        {
            _db = db;
        }

        // 1. Get Count - Powers the "Total Records" badge and Pagination math
        public int GetCount(string? name, int? deptId, int? typeId)
        {
            using var conn = _db.GetConnection();
            string sql = @"SELECT COUNT(*) FROM Employees 
                           WHERE (@Name IS NULL OR FullName LIKE '%' + @Name + '%') 
                           AND (@DeptId IS NULL OR DepartmentId = @DeptId) 
                           AND (@TypeId IS NULL OR EmployeeTypeId = @TypeId)";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Name", (object?)name ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@DeptId", (object?)deptId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@TypeId", (object?)typeId ?? DBNull.Value);

            conn.Open();
            return (int)cmd.ExecuteScalar();
        }

        // 2. Get Filtered - Powers the Main Classy Table
        public IEnumerable<Employee> GetFiltered(string? name, int? deptId, int? typeId, int page, int size)
        {
            var list = new List<Employee>();
            if (page < 1) page = 1;

            using var conn = _db.GetConnection();
            string sql = @"SELECT * FROM Employees 
                           WHERE (@Name IS NULL OR FullName LIKE '%' + @Name + '%') 
                           AND (@DeptId IS NULL OR DepartmentId = @DeptId) 
                           AND (@TypeId IS NULL OR EmployeeTypeId = @TypeId)
                           ORDER BY EmployeeId DESC 
                           OFFSET @Offset ROWS FETCH NEXT @Size ROWS ONLY";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Name", (object?)name ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@DeptId", (object?)deptId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@TypeId", (object?)typeId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Offset", (page - 1) * size);
            cmd.Parameters.AddWithValue("@Size", size);

            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(MapEmployee(reader));
            }
            return list;
        }

        // 3. Lookup Methods for "Classy" Names in Dropdowns/Table
        public IEnumerable<dynamic> GetDepartments()
        {
            var list = new List<dynamic>();
            using var conn = _db.GetConnection();
            string sql = "SELECT DepartmentId AS Id, DepartmentName AS Name FROM Departments ORDER BY Name";
            using var cmd = new SqlCommand(sql, conn);
            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new { Id = (int)reader["Id"], Name = reader["Name"].ToString() });
            }
            return list;
        }

        public IEnumerable<dynamic> GetEmployeeTypes()
        {
            var list = new List<dynamic>();
            using var conn = _db.GetConnection();
            string sql = "SELECT EmployeeTypeId AS Id, TypeName AS Name FROM EmployeeTypes ORDER BY Name";
            using var cmd = new SqlCommand(sql, conn);
            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new { Id = (int)reader["Id"], Name = reader["Name"].ToString() });
            }
            return list;
        }

        // 4. CRUD Operations
        public int Add(Employee emp)
        {
            using var conn = _db.GetConnection();

            // 1. Add 'SELECT SCOPE_IDENTITY();' to get the ID back from SQL Server
            string sql = @"INSERT INTO Employees (FullName, Email, Phone, Designation, DepartmentId, EmployeeTypeId, Gender, DateOfBirth, HireDate, Salary, IsActive) 
                   VALUES (@Name, @Email, @Phone, @Desig, @DeptId, @TypeId, @Gender, @DOB, @Hire, @Salary, @Active);
                   SELECT SCOPE_IDENTITY();";

            using var cmd = new SqlCommand(sql, conn);
            MapParameters(cmd, emp);

            conn.Open();

            // 2. Use ExecuteScalar() instead of ExecuteNonQuery() to "catch" the new ID
            var result = cmd.ExecuteScalar();

            // 3. Convert and return the new ID back to the Controller
            return Convert.ToInt32(result);
        }
        public Employee? GetById(int id)
        {
            using var conn = _db.GetConnection();
            string sql = "SELECT * FROM Employees WHERE EmployeeId = @Id";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            conn.Open();
            using var reader = cmd.ExecuteReader();
            return reader.Read() ? MapEmployee(reader) : null;
        }

        public void Update(Employee emp)
        {
            using var conn = _db.GetConnection();
            string sql = @"UPDATE Employees SET FullName=@Name, Email=@Email, Phone=@Phone, Designation=@Desig, 
                           DepartmentId=@DeptId, EmployeeTypeId=@TypeId, Gender=@Gender, DateOfBirth=@DOB, 
                           HireDate=@Hire, Salary=@Salary, IsActive=@Active, UpdatedAt=GETDATE() 
                           WHERE EmployeeId=@Id";
            using var cmd = new SqlCommand(sql, conn);
            MapParameters(cmd, emp);
            cmd.Parameters.AddWithValue("@Id", emp.EmployeeId);
            conn.Open();
            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var conn = _db.GetConnection();
            string sql = "DELETE FROM Employees WHERE EmployeeId = @Id";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            conn.Open();
            cmd.ExecuteNonQuery();
        }

        public bool IsEmailUnique(string email, int? excludeId = null)
        {
            using var conn = _db.GetConnection();
            string sql = "SELECT COUNT(*) FROM Employees WHERE Email = @Email" +
                         (excludeId.HasValue ? " AND EmployeeId <> @Id" : "");
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Email", email);
            if (excludeId.HasValue) cmd.Parameters.AddWithValue("@Id", excludeId.Value);
            conn.Open();
            return (int)cmd.ExecuteScalar() == 0;
        }

        // --- Helper Methods ---

        private void MapParameters(SqlCommand cmd, Employee emp)
        {
            cmd.Parameters.AddWithValue("@Name", emp.FullName);
            cmd.Parameters.AddWithValue("@Email", emp.Email);
            cmd.Parameters.AddWithValue("@Phone", (object?)emp.Phone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Desig", emp.Designation);
            cmd.Parameters.AddWithValue("@DeptId", emp.DepartmentId);
            cmd.Parameters.AddWithValue("@TypeId", emp.EmployeeTypeId);
            cmd.Parameters.AddWithValue("@Gender", (object?)emp.Gender ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@DOB", (object?)emp.DateOfBirth ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Hire", emp.HireDate);
            cmd.Parameters.AddWithValue("@Salary", emp.Salary);
            cmd.Parameters.AddWithValue("@Active", emp.IsActive);
        }

        private Employee MapEmployee(SqlDataReader reader)
        {
            return new Employee
            {
                EmployeeId = (int)reader["EmployeeId"],
                FullName = reader["FullName"].ToString()!,
                Email = reader["Email"].ToString()!,
                Phone = reader["Phone"]?.ToString(),
                Designation = reader["Designation"].ToString()!,
                DepartmentId = (int)reader["DepartmentId"],
                EmployeeTypeId = (int)reader["EmployeeTypeId"],
                Gender = reader["Gender"]?.ToString(),
                // Correctly handles the non-nullable DateTime error
                DateOfBirth = reader["DateOfBirth"] != DBNull.Value ? (DateTime)reader["DateOfBirth"] : null,
                HireDate = (DateTime)reader["HireDate"],
                Salary = (decimal)reader["Salary"],
                IsActive = (bool)reader["IsActive"]
            };
        }
    }
}