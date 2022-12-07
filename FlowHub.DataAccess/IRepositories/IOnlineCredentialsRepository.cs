
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowHub.DataAccess.IRepositories;

public interface IOnlineCredentialsRepository
{
    public string APIKey { get; set; }
}
