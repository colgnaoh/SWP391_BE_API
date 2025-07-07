namespace DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string htmlMessage);
    }
}
