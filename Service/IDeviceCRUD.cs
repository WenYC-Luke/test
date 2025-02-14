using test.Models.Dto;

namespace test.Service
{
    public interface IDeviceCRUD
    {
        //建立裝置
        Task<Device> CreateDevice(Device device);
        
        //搜尋裝置
        Task<IEnumerable<Device>> GetDevices(string? DeviceId);
        
        //更新裝置
        Task<UpdateDevice<Device>> UpdateDevice(Device device);
        
        //刪除裝置
        Task<DeleteDevice> DeleteDevice(Guid DeviceId);
    }
}
