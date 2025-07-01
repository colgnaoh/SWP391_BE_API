using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Service
{
    public class PayOSSignatureService : IPayOSSignatureService
    {
        private readonly ILogger<PayOSSignatureService> _logger;

        public PayOSSignatureService(ILogger<PayOSSignatureService> logger)
        {
            _logger = logger;
        }

        public bool VerifyPayOSSignature(string rawJsonPayload, string receivedSignature, string checksumKey)
        {
            try
            {
                using JsonDocument doc = JsonDocument.Parse(rawJsonPayload);
                JsonElement root = doc.RootElement;

                string dataToSign = GetCanonicalString(root);
                _logger.LogDebug("Canonical String: {CanonicalString}", dataToSign);

                byte[] keyBytes = Encoding.UTF8.GetBytes(checksumKey);
                byte[] dataBytes = Encoding.UTF8.GetBytes(dataToSign);

                using (var hmac = new HMACSHA256(keyBytes))
                {
                    byte[] hashBytes = hmac.ComputeHash(dataBytes);
                    string calculatedSignature = Convert.ToHexString(hashBytes).ToLower();
                    _logger.LogDebug("Calculated Signature: {CalculatedSignature}", calculatedSignature);
                    _logger.LogDebug("Received Signature: {ReceivedSignature}", receivedSignature);

                    return string.Equals(calculatedSignature, receivedSignature, StringComparison.OrdinalIgnoreCase);
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse JSON payload during signature verification.");
                return false;
            }
            catch (CryptographicException ex)
            {
                _logger.LogError(ex, "Cryptographic error during signature verification.");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during signature verification.");
                return false;
            }
        }

        private string GetCanonicalString(JsonElement element)
        {
            var options = new JsonSerializerOptions { WriteIndented = false };

            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream);

            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    writer.WriteStartObject();
                    foreach (var property in element.EnumerateObject().OrderBy(p => p.Name, StringComparer.Ordinal))
                    {
                        if (property.Name == "signature")
                        {
                            continue;
                        }
                        writer.WritePropertyName(property.Name);
                        GetCanonicalStringRecursive(property.Value, writer);
                    }
                    writer.WriteEndObject();
                    break;

                case JsonValueKind.Array:
                    writer.WriteStartArray();
                    foreach (var item in element.EnumerateArray())
                    {
                        GetCanonicalStringRecursive(item, writer);
                    }
                    writer.WriteEndArray();
                    break;

                default:
                    element.WriteTo(writer);
                    break;
            }

            writer.Flush();
            return Encoding.UTF8.GetString(stream.ToArray());
        }

        private void GetCanonicalStringRecursive(JsonElement element, Utf8JsonWriter writer)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    writer.WriteStartObject();
                    foreach (var property in element.EnumerateObject().OrderBy(p => p.Name, StringComparer.Ordinal))
                    {
                        writer.WritePropertyName(property.Name);
                        GetCanonicalStringRecursive(property.Value, writer);
                    }
                    writer.WriteEndObject();
                    break;

                case JsonValueKind.Array:
                    writer.WriteStartArray();
                    foreach (var item in element.EnumerateArray())
                    {
                        GetCanonicalStringRecursive(item, writer);
                    }
                    writer.WriteEndArray();
                    break;

                default:
                    element.WriteTo(writer);
                    break;
            }
        }
    }
}