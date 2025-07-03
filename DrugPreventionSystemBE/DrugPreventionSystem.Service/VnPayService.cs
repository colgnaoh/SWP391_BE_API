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

            TimeZoneInfo vnTimeZone;
            try
            {
                vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            }
            catch (TimeZoneNotFoundException)
            {
                vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
            }

            var createDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);
            var expireDate = createDate.AddMinutes(15);

            pay.AddRequestData("vnp_Version", "2.1.0");
            pay.AddRequestData("vnp_Command", "pay");
            pay.AddRequestData("vnp_TmnCode", _tmnCode);
            pay.AddRequestData("vnp_Amount", amount.ToString(CultureInfo.InvariantCulture));
            pay.AddRequestData("vnp_CreateDate", createDate.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture));
            pay.AddRequestData("vnp_CurrCode", request.Currency.ToString());
            pay.AddRequestData("vnp_IpAddr", GetClientIp());
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
        // Đảm bảo bạn đã thêm: using System.Linq; ở đầu file VnPayService.cs
        // Thay thế hàm GetClientIp() hiện có trong class VnPayService bằng hàm này:

        private string GetClientIp()
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null) return "127.0.0.1";

                string ipAddress = null;

                if (httpContext.Request.Headers.ContainsKey("X-Forwarded-For"))
                {
                    ipAddress = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
                }

                if (string.IsNullOrEmpty(ipAddress))
                {
                    ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
                }

                if (!string.IsNullOrEmpty(ipAddress) && ipAddress.Contains(','))
                {
                    ipAddress = ipAddress.Split(',')[0].Trim();
                }

                if (ipAddress == "::1" || string.IsNullOrEmpty(ipAddress) || ipAddress == "0.0.0.0")
                {
                    return "127.0.0.1";
                }

                return ipAddress;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in GetClientIp: {ex.Message}");
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