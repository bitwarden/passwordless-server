using System;
using System.Collections.Generic;
using System.Text;

namespace Service.Helpers
{
    public class ApiException : Exception
    {
        public int ErrorCode { get; set; } = 500;
        public ApiException(string message) : base(message)
        {

        }

        public ApiException(string message, int errorCode) : base(message)
        {
            ErrorCode = errorCode;
        }
    }
}
