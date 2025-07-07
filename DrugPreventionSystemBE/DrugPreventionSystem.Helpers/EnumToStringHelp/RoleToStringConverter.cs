using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using DrugPreventionSystemBE.DrugPreventionSystem.Enum;

public class RoleToStringConverter : ValueConverter<Role, string>
{
    public RoleToStringConverter() : base(
        role => role.ToString(),
        str => Enum.Parse<Role>(str)) // Không dùng static, dùng Parse chuẩn
    {
    }
}
