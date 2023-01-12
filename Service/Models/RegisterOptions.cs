using Fido2NetLib;

namespace Service.Models
{
    public class ApiResponse<T>
    {
        public T Data { get; set; }
        public string SessionId { get; set; }

        public ApiResponse()
        {

        }
        public ApiResponse(T data)
        {
            Data = data;
        }
    }
}