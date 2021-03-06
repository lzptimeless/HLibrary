﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.Book
{
    public class ReadPropertyException : Exception
    {
        public ReadPropertyException(string propertyName, string msg, Exception innerException)
            : base(msg, innerException)
        {
            PropertyName = propertyName;
        }

        public string PropertyName { get; private set; }

        public override string Message
        {
            get
            {
                string msg = base.Message;
                string append = $"Propert: {PropertyName}";
                if (string.IsNullOrEmpty(msg))
                    return append;
                else
                    return msg + Environment.NewLine + append;
            }
        }
    }
}
