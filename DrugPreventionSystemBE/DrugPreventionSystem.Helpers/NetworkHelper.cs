namespace DrugPreventionSystemBE.DrugPreventionSystem.Helpers
{
    public static class NetworkHelper
    {
        public static string GetIpAddress(HttpContext httpContext)
        {
            string ipAddress = string.Empty;
            if (httpContext.Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                ipAddress = httpContext.Request.Headers["X-Forwarded-For"].ToString().Split(',').FirstOrDefault();
            }
            else if (httpContext.Connection.RemoteIpAddress != null)
            {
                ipAddress = httpContext.Connection.RemoteIpAddress.ToString();
            }
            return ipAddress;
        }
    }
}
