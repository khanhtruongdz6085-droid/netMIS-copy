namespace NetCafeMis.Services;

internal static class ServiceChecks
{
    internal static T Required<T>(T? value, string field) where T : class =>
        value ?? throw new ValidationException(field + " is required");

    internal static void Unique<T>(List<T> list, string? id) where T : IIdentifiable
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ValidationException("ID is required");
        if (list.Any(old => old.Id == id.Trim()))
            throw new DuplicateIdException("Duplicate ID: " + id);
    }

    internal static void Add<T>(List<T> list, T? value) where T : class, IIdentifiable
    {
        T entity = Required(value, "entity");
        Unique(list, entity.Id);
        list.Add(entity);
    }

    internal static T Registered<T>(List<T> list, T? value) where T : class
    {
        T entity = Required(value, "reference");
        if (!list.Contains(entity)) throw new EntityNotFoundException("Reference is not registered");
        return entity;
    }

    internal static void ActiveEmployee(Employee employee)
    {
        if (employee.EmployeeStatus != EmployeeStatus.ACTIVE)
            throw new InactiveEmployeeException("Inactive employee: " + employee.Id);
    }
}
