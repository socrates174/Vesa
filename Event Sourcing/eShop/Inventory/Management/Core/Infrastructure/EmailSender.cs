using eShop.Inventory.Management.Core.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eShop.Inventory.Management.Core.Infrastructure;
public class EmailSender : IEmailSender
{
    public async Task SendAsync(IEmail email)
    {
        //TODO
    }
}
