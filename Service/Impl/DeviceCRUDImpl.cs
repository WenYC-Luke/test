using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;
using test.Models.Dto;

namespace test.Service.Impl
{
    public class DeviceCRUDImpl : IDeviceCRUD
    {
        //連線資料庫
        private readonly string _sqlConnection;

        public DeviceCRUDImpl(IConfiguration configuration) 
        {
            // 從設定檔中取得連線字串
            _sqlConnection = configuration.GetConnectionString("DefaultConnection")
                            ?? throw new ArgumentNullException("DefaultConnection", "Connection string is not configured.");
        }

        //實作CRUD
        //新增裝置
        public async Task<Device> CreateDevice(Device device)
        {
            if (device == null) {
                throw new ArgumentNullException(nameof(device), "device can't be null");
            }

            // 建立一個新資料庫連線
            using (var conn = new SqlConnection(_sqlConnection))
            {
                await conn.OpenAsync();

                // 建立一個資料庫操作(調用預存函式)
                using (var command = new SqlCommand("usp_createDevice", conn))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    //添加參數
                    command.Parameters.Add("@DeviceName", SqlDbType.NVarChar).Value = device.DeviceName;
                    command.Parameters.Add("@DeviceType", SqlDbType.NVarChar).Value = device.DeviceType;
                    command.Parameters.Add("@ProduceDate", SqlDbType.Date).Value = device.ProduceDate;

                    string jsonString = await command.ExecuteScalarAsync() as string;

                    if (!string.IsNullOrEmpty(jsonString)) {
                        Device newDevice = JsonConvert.DeserializeObject<Device>(jsonString);
                        return newDevice;
                    }
                }
            }
            return null;
        }

        //搜尋裝置
        public async Task<IEnumerable<Device>> GetDevices(string? DeviceId = null)
        {
            Guid? validDeviceId = null;

            //判斷DeviceId是否有效
            if (!string.IsNullOrEmpty(DeviceId)) 
            {
                if (!Guid.TryParse(DeviceId, out Guid tempDeviceId))
                {
                    throw new ArgumentException("無效的 DeviceId");
                }
                validDeviceId = tempDeviceId;
            }


            // 建立一個裝置清單存資料庫返回的資料
            var devices = new List<Device>(); 

            using (var conn = new SqlConnection(_sqlConnection))
            {
                await conn.OpenAsync();

                using (var command = new SqlCommand("usp_getDevices", conn))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // 用 Add 的方式定義要傳入預存程序的參數
                    SqlParameter sql_DeviceId = new SqlParameter("@DeviceId", SqlDbType.UniqueIdentifier);
                    
                    // 根據 validDeviceId 的值來設置 SQL 參數
                    if (validDeviceId.HasValue)
                    {
                        sql_DeviceId.Value = validDeviceId.Value; // 查詢單一裝置
                    }
                    else
                    {
                        sql_DeviceId.Value = DBNull.Value; // 查詢所有裝置
                    }

                    command.Parameters.Add(sql_DeviceId);
            
                    // 使用 ExecuteScalarAsync 來取得單一返回值（JSON 字串）
                    var jsonResult = await command.ExecuteScalarAsync() as string;

                    if (!string.IsNullOrEmpty(jsonResult))
                    {
                        // 反序列化 JSON 字串，轉換為設備列表
                        var deviceList = JsonConvert.DeserializeObject<List<Device>>(jsonResult);
                        devices.AddRange(deviceList);
                    }
                }                
            }
            return devices;
        }

        //更新裝置
        public async Task<UpdateDevice<Device>> UpdateDevice(Device device)
        {
            
            using (var conn = new SqlConnection(_sqlConnection)) 
            {
                await conn.OpenAsync();

                using (var command = new SqlCommand("usp_updateDevice", conn)) 
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // 添加參數
                    command.Parameters.Add(new SqlParameter("@DeviceId", SqlDbType.UniqueIdentifier) { Value = device.DeviceId });
                    command.Parameters.Add(new SqlParameter("@DeviceName", SqlDbType.NVarChar, 100) { Value = (object?)device.DeviceName ?? DBNull.Value });
                    command.Parameters.Add(new SqlParameter("@DeviceType", SqlDbType.NVarChar, 50) { Value = (object?)device.DeviceType ?? DBNull.Value });
                    command.Parameters.Add(new SqlParameter("@ProduceDate", SqlDbType.Date) { Value = (object?)device.ProduceDate ?? DBNull.Value });

                    try
                    {
                        var jsonResult = await command.ExecuteScalarAsync() as string;

                        if (!string.IsNullOrEmpty(jsonResult))
                        {
                            var result = JsonConvert.DeserializeObject<UpdateDevice<Device>>(jsonResult);
                            return result;
                        }
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Number == 50000)
                        {
                            throw new Exception($"SQL 錯誤: {ex.Message}");
                        }
                        throw;
                    }
                }
            }
            return null;
        }

        //刪除裝置
        public async Task<DeleteDevice> DeleteDevice(Guid deviceId)
        {
            if (deviceId == Guid.Empty)
            {
                throw new ArgumentException("請提供有效的 DeviceId");
            }

            using (var conn = new SqlConnection(_sqlConnection))
            {
                await conn.OpenAsync();

                // 建立資料庫操作
                using (var command = new SqlCommand("usp_deleteDevice", conn))
                {
                    // 指令Type = 資料庫預存程序 
                    command.CommandType = CommandType.StoredProcedure;

                    // 傳入參數
                    command.Parameters.Add(new SqlParameter("@DeviceId", SqlDbType.UniqueIdentifier) { Value = deviceId });

                    try
                    {
                        // 接收資料庫返回的 Json 字串
                        var jsonResult = await command.ExecuteScalarAsync() as string;

                        if (!string.IsNullOrEmpty(jsonResult))
                        {
                            // Json 轉物件
                            return JsonConvert.DeserializeObject<DeleteDevice>(jsonResult);
                        }
                    }
                    catch (SqlException ex)
                    {
                        throw new Exception($"SQL 錯誤: {ex.Message}");
                    }
                }
            }
            return null;
        }

    }
}
