using DataTables.AspNet.AspNetCore;
using DataTables.AspNet.Core;
using IOWebApplication.Core.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace IOWebApplication.Extensions
{
    public static class DataTablesExtension
    {
        public static IActionResult GetResponseFetched<T>(this IDataTablesRequest request, IQueryable<T> data) where T : class
        {
            return GetResponse(request, data, null, null, false);
        }
        /// <summary>
        /// Генерира отговор на AJAX заявка на DataTables
        /// </summary>
        /// <typeparam name="T">Тип на изходните данни</typeparam>
        /// <param name="request">Заявка на DataTables</param>
        /// <param name="data">Пълен сет от данни</param>
        /// <param name="filteredData">Филтриран сет от данни</param>
        /// <returns></returns>
        public static IActionResult GetResponse<T>(this IDataTablesRequest request, IQueryable<T> data, IQueryable<T> filteredData = null, Dictionary<string, object> additionalParameters = null, bool fromDatabase = true) where T : class
        {
            if (filteredData == null)
            {
                filteredData = request.GetFilteredData(data, fromDatabase);
                if (!fromDatabase)
                {
                    filteredData = filteredData.ToList().AsQueryable();
                }
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

            int dataCount = calcCountInDataset<T>(data);
            int filteredCount = dataCount;
            if (request.Search.Value != null)
            {
                filteredCount = calcCountInDataset<T>(filteredData);
            }

            var dtResponse = DataTablesResponse.Create(request, dataCount, filteredCount, dataPage, additionalParameters);

            var settings = new System.Text.Json.JsonSerializerOptions()
            {
                PropertyNamingPolicy = new DataTablesResponseDataNamingPolicy()
            };

            return new JsonResult(dtResponse, settings);
        }

        private static int calcCountInDataset<T>(IQueryable<T> data) where T : class
        {
            try
            {
                //Съкращава резултата до основната таблица и пряко свързаните подчинени
                return data.Select(x => new { id = 1 }).Count();
            }
            catch
            {
                return data.Count();
            }
        }

        /// <summary>
        /// Използва текста в полето за търсене за филтрация на данните по колоните, 
        /// маркирани като колони за търсене
        /// </summary>
        /// <typeparam name="T">Тип на изходните данни</typeparam>
        /// <param name="request">Заявка на DataTables</param>
        /// <param name="data">Пълен сет от данни</param>
        /// <returns></returns>
        public static IQueryable<T> GetFilteredData<T>(this IDataTablesRequest request, IQueryable<T> data, bool fromDatabase = true)
        {
            var filteredData = data;

            if (request.Search.Value != null)
            {
                var searchColumns = request.Columns.Where(c => c.IsSearchable);
                if (fromDatabase)
                {
                    filteredData = data.SearchForNet8(searchColumns, request.Search.Value);
                }
                else
                {
                    filteredData = data.SearchFor(searchColumns, request.Search.Value);
                }
            }

            return filteredData;
        }

        /// <summary>
        /// Генерира отговор на AJAX заявка на DataTables
        /// </summary>
        /// <typeparam name="T">Тип на изходните данни</typeparam>
        /// <param name="request">Заявка на DataTables</param>
        /// <param name="data">Сет от данни за страницата</param>
        /// <param name="dataCount">Общ брой записи</param>
        /// <returns></returns>
        public static IActionResult GetResponseServerPaging<T>(this IDataTablesRequest request, List<T> data, int dataCount) where T : class
        {
            var dtResponse = DataTablesResponse.Create(request, dataCount, dataCount, data);

            var settings = new System.Text.Json.JsonSerializerOptions()
            {
                PropertyNamingPolicy = new DataTablesResponseDataNamingPolicy()
            };

            return new JsonResult(dtResponse, settings);
        }

        /// <summary>
        /// Генерира списък от колоните, по които ще се сортира
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static List<DataTablesSortColumnVM> GetSortedColumnsForOrderBy(this IDataTablesRequest request)
        {
            List<DataTablesSortColumnVM> result = new List<DataTablesSortColumnVM>();

            var orderColums = request.Columns.Where(x => x.Sort != null);
            foreach (var column in orderColums)
            {
                DataTablesSortColumnVM add = new DataTablesSortColumnVM()
                {
                    Name = column.Name,
                    IsAscending = column.Sort.Direction == DataTables.AspNet.Core.SortDirection.Ascending,
                };

                result.Add(add);
            }

            return result;
        }
    }
}
