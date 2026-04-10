using EmployeePortal.Models.Domain;

namespace EmployeePortal.Repositories.Interfaces
{
    public interface IEmployeeRepository
    {
        // Fixes: No overload for method 'GetCount' takes 3 arguments
        int GetCount(string? name, int? deptId, int? typeId);
        bool IsEmailUnique(string email, int? excludeId = null);

        // Fixes: No overload for method 'GetFiltered' takes 5 arguments
        // Parameters: name, deptId, typeId, pageNumber, pageSize
        IEnumerable<Employee> GetFiltered(string? name, int? deptId, int? typeId, int page, int size);

       int Add(Employee emp);
        void Delete(int id);
        IEnumerable<dynamic> GetDepartments();
        IEnumerable<dynamic> GetEmployeeTypes();
        Employee? GetById(int id);
        void Update(Employee emp);
    }
}