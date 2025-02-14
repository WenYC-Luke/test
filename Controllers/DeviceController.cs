using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using test.Models.Dto;
using test.Service;

namespace test.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class DeviceController : ControllerBase
    {
        //注入DeviceCRUD service
        private readonly IDeviceCRUD _deviceCRUD;

        public DeviceController(IDeviceCRUD deviceCRUD)
        {
            _deviceCRUD = deviceCRUD;
        }

        [HttpPost]
        public async Task<IActionResult> CreateDevice([FromBody] Device device)
        {
            var result = await _deviceCRUD.CreateDevice(device);

            return Ok(result);
        }


        [HttpGet("{deviceId?}")]
        public async Task<IActionResult> GetDevices(string? deviceId)
        {
            var result = await _deviceCRUD.GetDevices(deviceId);

            return Ok(result);
        }

        [HttpPut("{deviceId}")]
        public async Task<IActionResult> UpdateDevice([FromRoute] Guid deviceId, [FromBody] Device device)
        {
            // 檢查 deviceId 是否有效
            if (deviceId == Guid.Empty)
            {
                return BadRequest(new { status = "error", message = "請提供有效的 DeviceId" });
            }

            // 檢查 request body 中的 device 是否有效
            if (device == null || device.DeviceId != deviceId)
            {
                return BadRequest(new { status = "error", message = "請提供與 URL 中一致的 DeviceId" });
            }

            var result = await _deviceCRUD.UpdateDevice(device);

            if (result.Status == "success")
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }

        [HttpDelete("{deviceId}")]
        public async Task<IActionResult> DeleteDevice([FromRoute] Guid deviceId)
        {

            try
            {
                // 驗證 deviceId 是否有效
                if (deviceId == Guid.Empty)
                {
                    return BadRequest(new { status = "error", message = "無效的 DeviceId" });
                }

                var result = await _deviceCRUD.DeleteDevice(deviceId);

                // 根據刪除結果回傳相對應的狀態
                if (result.Status == "success")
                {
                    return Ok(new { status = "success", message = result.Message });
                }
                else
                {
                    return BadRequest(new { status = "error", message = result.Message });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = "伺服器錯誤: " + ex.Message });
            }
        }
    }
}

