using System;
using System.Collections.Generic;
using System.Text;

namespace Limited.MicroService
{
    public class ServiceException : Exception
    {
        public int Code
        {
            get;
            set;
        } = 200;


        public new string Message
        {
            get;
            set;
        } = "";


        public ServiceException(string message)
            : this(200, message)
        {
            Message = message;
        }

        public ServiceException(int code, string message)
        {
            Code = code;
            Message = message;
        }
    }
}
