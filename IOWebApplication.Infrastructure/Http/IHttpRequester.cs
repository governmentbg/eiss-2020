// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System.Net.Http;
using System.Threading.Tasks;

namespace IOWebApplication.Infrastructure.Http
{
  public interface IHttpRequester
  {
    /// <summary>
    /// Authorization Key if needed
    /// </summary>
    string ApiKey { get; set; }

    /// <summary>
    /// If needed
    /// </summary>
    string CertificatePath { get; set; }

    /// <summary>
    /// If needed
    /// </summary>
    string CertificatePassword { get; set; }
    bool ValidateServerCertificate { get; set; }

    /// <summary>
    /// Get data from Rest service
    /// </summary>
    /// <typeparam name="T">Data type</typeparam>
    /// <param name="url">Service endpoint</param>
    /// <returns>Received data</returns>
    Task<T> GetAsync<T>(string url);

    Task<T> GetXmlAsync<T>(string url) where T : class;

    /// <summary>
    /// Get Data from Rest service
    /// </summary>
    /// <param name="url">Service endpoint</param>
    /// <returns>Service response</returns>
    Task<HttpResponseMessage> GetAsync(string url);

    /// <summary>
    /// Post data to Rest service
    /// </summary>
    /// <param name="url">Service endpoint</param>
    /// <param name="data">Data to send</param>
    /// <returns>Service response</returns>
    Task<HttpResponseMessage> PostAsync(string url, object data);

    /// <summary>
    /// Put request to Rest service
    /// </summary>
    /// <param name="url">Service endpoint</param>
    /// <param name="data">Data to send</param>
    /// <returns>Service response</returns>
    Task<HttpResponseMessage> PutAsync(string url, object data = null);

    /// <summary>
    /// Delete from Rest service
    /// </summary>
    /// <param name="url">Service endpoint</param>
    /// <returns>Service response</returns>
    Task<HttpResponseMessage> DeleteAsync(string url);
  }
}
