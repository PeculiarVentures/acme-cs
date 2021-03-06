﻿using System;
using System.Net;
using PeculiarVentures.ACME.Web.Http;

namespace PeculiarVentures.ACME.Web
{
    public abstract class BaseAcmeResponse
    {
        public int StatusCode { get; set; } = 200;
        public HeaderCollection Headers { get; set; } = new HeaderCollection();
    }

    public class AcmeResponse : BaseAcmeResponse
    {
        public object Content { get; set; }

        /// <summary>
        /// Returns content in specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="InvalidCastException"/>
        public T GetContent<T>() where T : class
        {
            if (Content is T)
            {
                return (T)Content;
            }

            throw new InvalidCastException();
        }
    }

    public class AcmeResponse<T> : BaseAcmeResponse where T : class
    {
        public static implicit operator AcmeResponse<T>(AcmeResponse response)
        {
            return new AcmeResponse<T>
            {
                StatusCode = response.StatusCode,
                Headers = response.Headers,
                Content = (T)response.Content,
            };
        }

        public T Content { get; set; }
    }
}
