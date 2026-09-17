using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Application.Contracts.Services
{
    public interface ISqlUtility
    {
        Task<TReturn> ExecuteScalarAsync<TReturn>(string storedProcedureName, IDictionary<string, object> parameters = null);
        IDictionary<string, object> ExecuteStoredProcedure(string storedProcedureName, IDictionary<string, object> parameters = null, IDictionary<string, Type> outParameters = null);
        Task<IDictionary<string, object>> ExecuteStoredProcedureAsync(string storedProcedureName, IDictionary<string, object> parameters = null, IDictionary<string, Type> outParameters = null);
        Task<(IList<TReturn> result, IDictionary<string, object> outValues)> QueryWithStoredProcedureAsync<TReturn>(string storedProcedureName, IDictionary<string, object?>? parameters = null, IDictionary<string, Type>? outParameters = null) where TReturn : class, new();
    }
}
