using System.Data;

namespace OrderManagement.Application.Common;

public interface IDbSessionAccessor
{
    IDbConnection Connection { get; }
    IDbTransaction? Transaction { get; set; }
}
