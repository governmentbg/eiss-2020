// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using DataTables.AspNet.AspNetCore;
using DataTables.AspNet.Core;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace IOWebApplication.Extensions
{
    public static class DataTablesExtension
    {
        /// <summary>
        /// Генерира отговор на AJAX заявка на DataTables
        /// </summary>
        /// <typeparam name="T">Тип на изходните данни</typeparam>
        /// <param name="request">Заявка на DataTables</param>
        /// <param name="data">Пълен сет от данни</param>
        /// <param name="filteredData">Филтриран сет от данни</param>
        /// <returns></returns>
        public static IActionResult GetResponse<T>(this IDataTablesRequest request, IQueryable<T> data, IQueryable<T> filteredData = null, Dictionary<string, object> additionalParameters = null)
        {
            if (filteredData == null)
            {
                filteredData = request.GetFilteredData(data);
            }

            var orderColums = request.Columns.Where(x => x.Sort != null);
            IQueryable<T> dataPage = null;

            if (request.Length < 0)
            {
                dataPage = filteredData.OrderBy(orderColums);
            }
            else
            {
                dataPage = filteredData.OrderBy(orderColums).Skip(request.Start).Take(request.Length);
            }

            var dtResponse = DataTablesResponse.Create(request, data.Count(), filteredData.Count(), dataPage, additionalParameters);
            var settings = new JsonSerializerSettings()
            {
                ContractResolver = DataTablesResponseDataContractResolver.Instance
            };

            return new JsonResult(dtResponse, settings);
        }

        /// <summary>
        /// Използва текста в полето за търсене за филтрация на данните по колоните, 
        /// маркирани като колони за търсене
        /// </summary>
        /// <typeparam name="T">Тип на изходните данни</typeparam>
        /// <param name="request">Заявка на DataTables</param>
        /// <param name="data">Пълен сет от данни</param>
        /// <returns></returns>
        public static IQueryable<T> GetFilteredData<T>(this IDataTablesRequest request, IQueryable<T> data)
        {
            var filteredData = data;

            if (request.Search.Value != null)
            {
                var searchColumns = request.Columns.Where(c => c.IsSearchable);
                filteredData = data.SearchFor(searchColumns, request.Search.Value);
            }
            
            return filteredData;
        }
    }
}
