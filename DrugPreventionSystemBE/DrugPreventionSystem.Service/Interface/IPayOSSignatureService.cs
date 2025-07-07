namespace DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface
{
    public interface IPayOSSignatureService
    {
        bool VerifyPayOSSignature(string rawJsonPayload, string receivedSignature, string checksumKey);
    }
}
