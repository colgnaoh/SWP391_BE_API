namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.AuthModel
{
    public class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
