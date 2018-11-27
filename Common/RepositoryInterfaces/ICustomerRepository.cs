﻿using Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.RepositoryInterfaces
{
    public interface ICustomerRepository : IRepository<Customer>
    {
        Customer GetActiveCustomerByIdCard(string idCard);
        Customer GetCustomerWithLinesAndPayments(string idCard);
    }
}
