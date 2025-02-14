namespace test.Models.Dto
{
    public class UpdateDevice<T>
    {
        public string Status { get; set; }
        public string Message { get; set; } 
        public T? Data { get; set; }
    }
}
