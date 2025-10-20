using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WinUINotifsMSIX.Services
{
    public class LogMaintenanceService
    {
        private readonly HttpClient _httpClient = new();

        public async Task TriggerHousekeepingAsync()
        {
            try
            {
                var response = await _httpClient.DeleteAsync("https://betting3.110q.org:7283/api/SerilogHkpg/-60");
                response.EnsureSuccessStatusCode();

                string content = await response.Content.ReadAsStringAsync();
                if (int.TryParse(content, out int rowsDeleted))
                {
                    Log.Information("Serilog housekeeping triggered successfully. Rows deleted: {Count}", rowsDeleted);
                }
                else
                {
                    Log.Warning("Serilog housekeeping succeeded but response was not a valid number: {Content}", content);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to trigger Serilog housekeeping.");
            }
        }
    }
}
