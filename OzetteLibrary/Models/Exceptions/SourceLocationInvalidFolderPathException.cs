﻿using System;
using System.Runtime.Serialization;

namespace OzetteLibrary.Models.Exceptions
{
    /// <summary>
    /// An exception for invalid source location folder path.
    /// </summary>
    public class SourceLocationInvalidFolderPathException : Exception
    {
        public SourceLocationInvalidFolderPathException()
        {
        }

        public SourceLocationInvalidFolderPathException(string message) : base(message)
        {
        }

        public SourceLocationInvalidFolderPathException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SourceLocationInvalidFolderPathException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}