using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.VnPayPayment;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Globalization;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Services
{
    public class VnPayService : IVnPayService
    {
        private readonly IConfiguration _configuration;
        private readonly string _tmnCode;
        private readonly string _hashSecret;
        private readonly string _apiUrl;
        private readonly string _returnUrl;
        private readonly string _ipnUrl;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public VnPayService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _tmnCode = _configuration["VnPay:TmnCode"];
            _hashSecret = _configuration["VnPay:HashSecret"];
            _apiUrl = _configuration["VnPay:ApiUrl"];
            _returnUrl = _configuration["VnPay:ReturnUrl"];
            _ipnUrl = _configuration["VnPay:IpnUrl"];

            Console.WriteLine($"DEBUG VNPAY Config: TmnCode = '{_tmnCode}'");
            Console.WriteLine($"DEBUG VNPAY Config: HashSecret = '{_hashSecret}'");
            Console.WriteLine($"DEBUG VNPAY Config: ApiUrl = '{_apiUrl}'");
            Console.WriteLine($"DEBUG VNPAY Config: ReturnUrl = '{_returnUrl}'");
            Console.WriteLine($"DEBUG VNPAY Config: IpnUrl = '{_ipnUrl}'");
        }

        public string GetPaymentUrl(PaymentRequestVnPay request)
        {
            var pay = new VnpayPay();
            long amount = (long)(request.Money * 100);

            // Sử dụng TimeZoneInfo để đảm bảo múi giờ GMT+7
            var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); // GMT+7
            var createDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);
            var expireDate = createDate.AddMinutes(15);

            pay.AddRequestData("vnp_Version", "2.1.0");
            pay.AddRequestData("vnp_Command", "pay");
            pay.AddRequestData("vnp_TmnCode", _tmnCode);
            pay.AddRequestData("vnp_Amount", amount.ToString(CultureInfo.InvariantCulture));
            pay.AddRequestData("vnp_CreateDate", createDate.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture));
            pay.AddRequestData("vnp_CurrCode", request.Currency.ToString());
            pay.AddRequestData("vnp_IpAddr", GetClientIp()); // Sử dụng _httpContextAccessor
            pay.AddRequestData("vnp_Locale", request.Language == Enum.DisplayLanguage.Vietnamese ? "vn" : "en");
            pay.AddRequestData("vnp_OrderInfo", string.IsNullOrEmpty(request.Description) ? "Thanh toan don hang" : request.Description);
            pay.AddRequestData("vnp_OrderType", "other");
            pay.AddRequestData("vnp_ReturnUrl", _returnUrl);
            pay.AddRequestData("vnp_TxnRef", request.PaymentId.ToString());
            pay.AddRequestData("vnp_ExpireDate", expireDate.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture));
            pay.AddRequestData("vnp_SecureHashType", "HmacSHA512");

            if (request.BankCode != Enum.BankCode.ANY)
            {
                pay.AddRequestData("vnp_BankCode", request.BankCode.ToString());
            }

            string paymentUrl = pay.CreateRequestUrl(_apiUrl, _hashSecret);
            Console.WriteLine($"DEBUG VNPAY Generated URL: {paymentUrl}");
            return paymentUrl;
        }

        // Phương thức lấy IP động từ client
        private string GetClientIp()
        {
            try
            {
                var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
                return string.IsNullOrEmpty(ipAddress) ? "127.0.0.1" : ipAddress;
            }
            catch
            {
                return "127.0.0.1";
            }
        }

        public bool ProcessVnPayReturn(VnPayReturnResponse response)
        {
            var pay = new VnpayPay();
            pay.AddAllResponseData(response);
            return pay.ValidateSignature(response.vnp_SecureHash, _hashSecret) &&
                   response.vnp_ResponseCode == "00" &&
                   response.vnp_TransactionStatus == "00";
        }

        public VnPayIpnResult ProcessVnPayIpn(VnPayIpnResponse response)
        {
            var pay = new VnpayPay();
            pay.AddAllResponseData(response);

            bool isValid = pay.ValidateSignature(response.vnp_SecureHash, _hashSecret);

            if (isValid)
            {
                if (response.vnp_ResponseCode == "00" && response.vnp_TransactionStatus == "00")
                {
                    return new VnPayIpnResult { RspCode = "00", Message = "Confirm Success" };
                }
                return new VnPayIpnResult { RspCode = "01", Message = "Confirm Failed" };
            }

            return new VnPayIpnResult { RspCode = "97", Message = "Invalid Signature" };
        }
    }
}