using System;

namespace RevokeMsgPatcher
{
    class BusinessException : ApplicationException
    {
        public string ErrorCode { get; protected set; }

        public BusinessException(string errcode, string message) : base(message)
        {
            ErrorCode = errcode;
        }
    }
}
