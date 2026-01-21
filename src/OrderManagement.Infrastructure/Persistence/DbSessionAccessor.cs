using OrderManagement.Application.Common;
using System.Data;

namespace OrderManagement.Infrastructure.Persistence;

public class DbSessionAccessor(IDbConnection connection) : IDbSessionAccessor
{
    public IDbConnection Connection => connection;
    public IDbTransaction? Transaction { get; set; }
}
