﻿using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PeculiarVentures.ACME.Protocol
{
    public class Error : BaseObject
    {
        [JsonProperty("type")]
        [JsonRequired]
        [JsonConverter(typeof(StringEnumConverter))]
        public ErrorType Type { get; set; }

        [JsonProperty("detail")]
        public string Detail { get; set; }

#if DEBUG
        [JsonProperty("stack")]
        public string Stack { get; set; }
#endif

        [JsonProperty("subproblems")]
        public Error[] SubProblems { get; set; }

        public static implicit operator Exception(Error error)
        {
            return new Exception($"ACME error '{error.Type}'.{(error.Detail != null ? " " + error.Detail : "")}");
        }

        public static implicit operator Error(Exception exception)
        {
            return new Error
            {
                Type = ErrorType.ServerInternal,
                Detail = exception.Message,
#if DEBUG
                Stack = exception.StackTrace,
#endif
            };
        }

        public static implicit operator AcmeException(Error error)
        {
            return new AcmeException($"ACME error '{error.Type}'.{(error.Detail != null ? " " + error.Detail : "")}")
            {
                Type = error.Type,
            };
        }
        public static implicit operator Error(AcmeException exception)
        {
            return new Error
            {
                Type = exception.Type,
                Detail = exception.StackTrace,
#if DEBUG
                Stack = exception.StackTrace,
#endif
            };
        }
    }
}