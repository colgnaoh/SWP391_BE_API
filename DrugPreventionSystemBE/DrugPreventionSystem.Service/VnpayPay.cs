using DrugPreventionSystemBE.DrugPreventionSystem.Service;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Services
{
    public class VnpayPay
    {
        private SortedList<string, string> _requestData = new SortedList<string, string>(new VnPayCompare());
        private SortedList<string, string> _responseData = new SortedList<string, string>(new VnPayCompare());

        public void AddRequestData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _requestData.Add(key, value);
            }
        }

        public void AddResponseData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _responseData.Add(key, value);
            }
        }

        public void AddAllResponseData(object responseObj)
        {
            foreach (var prop in responseObj.GetType().GetProperties())
            {
                if (prop.GetValue(responseObj) is string value && !string.IsNullOrEmpty(value))
                {
                    AddResponseData(prop.Name, value);
                }
            }
        }

        // Thay thế toàn bộ hàm CreateRequestUrl() hiện có trong class VnpayPay bằng hàm này:

        public string CreateRequestUrl(string baseUrl, string hashSecret)
        {
            var data = new StringBuilder();
            var queryString = new StringBuilder();

            foreach (var kv in _requestData)
            {
                data.Append($"{kv.Key}={kv.Value}&");
                queryString.Append($"{kv.Key}={HttpUtility.UrlEncode(kv.Value, Encoding.UTF8)}&");
            }

            string rawData = data.ToString().TrimEnd('&');
            string finalQueryString = queryString.ToString().TrimEnd('&');

            Console.WriteLine($"DEBUG VNPAY RawData for HASH: {rawData}");
            Console.WriteLine($"DEBUG VNPAY Final Query String (encoded): {finalQueryString}");

            string vnp_SecureHash = HmacSHA512(hashSecret.Trim(), rawData);
            return $"{baseUrl}?{finalQueryString}&vnp_SecureHash={vnp_SecureHash}";
        }
        public bool ValidateSignature(string secureHash, string hashSecret)
        {
            var data = new StringBuilder();

            foreach (var kv in _responseData)
            {
                if (kv.Key != "vnp_SecureHash" && kv.Key != "vnp_SecureHashType")
                {
                    data.Append($"{kv.Key}={kv.Value}&");
                }
            }

            string rawData = data.ToString().TrimEnd('&');
            Console.WriteLine($"DEBUG VNPAY RawData for ValidateSignature: {rawData}");
            Console.WriteLine($"DEBUG VNPAY Provided SecureHash: {secureHash}");

            string calculatedHash = HmacSHA512(hashSecret.Trim(), rawData);
            Console.WriteLine($"DEBUG VNPAY Calculated SecureHash: {calculatedHash}");

            bool isValid = string.Equals(calculatedHash, secureHash, StringComparison.OrdinalIgnoreCase);
            Console.WriteLine($"DEBUG VNPAY Signature Validation Result: {isValid}");

            return isValid;
        }
        private string HmacSHA512(string key, string inputData)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] inputBytes = Encoding.UTF8.GetBytes(inputData);

            using var hmac = new HMACSHA512(keyBytes);
            byte[] hashBytes = hmac.ComputeHash(inputBytes);

            return BitConverter.ToString(hashBytes).Replace("-", "").ToUpper();
        }
    }
}